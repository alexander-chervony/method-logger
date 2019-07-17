using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CodeCleaner;
using Configuration;
using LibWeaver;
using MethodLogger;
using NUnit.Framework;
using Stringification;

namespace Tests
{
    [TestFixture]
    [Description("In order to get this test working correctly solution must be updated and built. Sources must be in sync with DLLs.")]
    public abstract class IntegrationTestsBase
    {
        protected abstract string SolutionFile { get; }
        protected abstract string SolutionProjectExcludeFilter { get; }
        protected abstract string WeaveLibrariesRootDir { get; }

        protected string[] GetRoslynListedMethods(string namespaceStartsWith = null)
        {
            return
                new RoslynMethodLister(
                    SolutionFile,
                    projectExcludeFilter: SolutionProjectExcludeFilter,
                    namespaceStartsWith: namespaceStartsWith)
                    .GetAllMethods()
                    .ToArray();
        }

        protected string[] GetWeavedMethods(string namespaceFilter = null)
        {
            IEnumerable<string> allMethods = new CecilMethodLister(
                LibWeaver.Program.GetLibsToWeave(WeaveLibrariesRootDir),
                typeDefinition => namespaceFilter == null || typeDefinition.Namespace.Contains(namespaceFilter))
                .GetAllMethods();

            return allMethods.ToArray();
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            // find VisualStudioVersion or MinimumVisualStudioVersion
            // if not escaped - fail test
            var slnLines = File.ReadAllLines(SolutionFile);
            if (slnLines.Any(q => q.Contains("VisualStudioVersion") && !q.StartsWith("#")))
            {
                Assert.Fail("Solution file parsing will fail. You should comment VisualStudioVersion and MinimumVisualStudioVersion lines with hash '#'. OR check for new Roslyn version.");
            }
        }

        [Test]
        [Description("In order to get this test working correctly activity solution must be updated and built. Sources must be in sync with DLLs.")]
        public void AllLoggedMethodsMustBeFoundInCode()
        {
            EnsureAllLoggableWeavedMethodsCanBeSavedIntoDbSuccessfully();

            var allLoggedMethods = GetLoggedMethods().WithoutCtors().ToArray();

            var allSolutionMethods = GetRoslynListedMethods();

            var notFound = allLoggedMethods.Except(allSolutionMethods).ToArray();

            Console.WriteLine("allLoggedMethods = {0}, allSolutionMethods = {1}, notFound = {2}\r\n", allLoggedMethods.Count(), allSolutionMethods.Count(), notFound.Count());

            Console.WriteLine("List of not foud methods:");
            foreach (var nf in notFound)
            {
                Console.WriteLine("\r\n" + nf);
            }

            CollectionAssert.IsEmpty(notFound);
        }

        [Test]
        public void MethodListsFoundByRoslynAndByCecilMustBeTheSame()
        {
            MethodListsFoundByRoslynAndByCecilMustBeTheSameEx(
                GetWeavedMethods().WithoutCtors().ToArray(),
                GetRoslynListedMethods());
        }

        [Test]
        [Ignore("There are controls referenced in different projects. this can't be issue in runtime since duplicetes are not added to the dictioonary. See ColorPicker. It shouldn't be the problem since Method logger operates with dictionary of already logged methods.")]
        public void EnsureThereAreNoEqualMethodsFound()
        {
            var allMethods = GetWeavedMethods().WithoutCtors();

            var duplicates = allMethods.GroupBy(name => name).Select(g => new { MethName = g.Key, Cnt = g.Count() }).Where(g => g.Cnt > 1).OrderBy(g => g.MethName).ToArray();

            Console.WriteLine("List of duplicates (count = {0}):\r\n", duplicates.Count());
            foreach (var d in duplicates)
            {
                Console.WriteLine("\r\nCount={0}, Name={1}", d.Cnt, d.MethName);
            }
            CollectionAssert.IsEmpty(duplicates);
        }

        [Test]
        public void EnsureAllLoggableWeavedMethodsCanBeSavedIntoDbSuccessfully()
        {
            DeleteAllLoggedMethods();

            // getting distinct since there are controls referenced in different projects. this can't be issue in runtime since duplicetes are not added to the dictioonary. see ColorPicker
            var methods = GetWeavedMethods().WithoutCtors().Distinct().ToArray();
            var config = new Config(new ErrorLogger());
            var repo = new MethodLoggerRepository(config);
            bool error = false;

            foreach (var partition in Partition(methods, config.ChunkSize))
            {
                try
                {
                    repo.AddMethods(InvocationLogger.ComposeRows(partition));
                }
                catch (Exception e)
                {
                    foreach (var method in partition)
                    {
                        try
                        {
                            repo.AddMethods(InvocationLogger.ComposeRows(new[] { method }));
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Problem saving method\r\n{0}\r\n{1}", method, exception);
                        }
                    }
                    Console.WriteLine("Problem saving method list\r\n{0}", e);
                    error = true;
                    throw;
                }
            }
            Assert.IsFalse(error);
        }

        [Test]
        public void PrintLongMethods()
        {
            foreach (var longMethod in GetWeavedMethods().WithoutCtors().Where(m => m.Length > 700))
            {
                Console.WriteLine(longMethod.Length);
                Console.WriteLine(longMethod);
                Console.WriteLine("\r\n\r\n\r\n");
            }
        }

        protected void MethodListsFoundByRoslynAndByCecilMustBeTheSameEx(string[] allWeavedMethods, string[] allSolutionMethods)
        {
            var weavedOnlyMethods = allWeavedMethods.Except(allSolutionMethods).ToArray();
            var solutionOnlyMethods = allSolutionMethods.Except(allWeavedMethods).ToArray();

            Console.WriteLine("allWeavedMethods = {0}, weavedOnlyMethods = {1}, solutionOnlyMethods = {2}\r\n", allWeavedMethods.Count(), weavedOnlyMethods.Count(), solutionOnlyMethods.Count());

            Console.WriteLine("List of weavedOnlyMethods:");
            foreach (var nf in weavedOnlyMethods)
            {
                Console.WriteLine("\r\n" + nf);
            }

            Console.WriteLine("\r\n\r\n\r\n\r\n\r\n\r\nList of solutionOnlyMethods:");
            foreach (var nf in solutionOnlyMethods)
            {
                Console.WriteLine("\r\n" + nf);
            }

            CollectionAssert.IsEmpty(weavedOnlyMethods);
            CollectionAssert.IsEmpty(solutionOnlyMethods);
        }

        /// <summary>
        /// Dont use anywhere else - it's suboptimal
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> items, int partitionSize)
        {
            int i = 0;
            return items.GroupBy(x => i++ / partitionSize).ToArray();
        }

        protected void DeleteAllLoggedMethods()
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand("delete from [LoggedMethods]", conn) { CommandType = CommandType.Text })
                    cmd.ExecuteNonQuery();
            }
        }

        protected static IEnumerable<string> GetLoggedMethods()
        {
            return new CodeCleanupRepository(Console.WriteLine).GetLoggedMethods();
        }

        private string GetConnectionString()
        {
            var config = new Config(new ErrorLogger());
            return config.ConnectionString;
        }

    }

    public static class TestExtensions
    {
        public static IEnumerable<string> WithoutCtors(this IEnumerable<string> methods)
        {
            return methods.Where(m => !m.Contains("::.ctor") && !m.Contains("::.cctor"));
        }
    }
}