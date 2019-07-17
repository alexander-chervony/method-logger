using CodeCleaner;
using NUnit.Framework;
using Rhino.Mocks;

namespace Tests
{
    /// <summary>
    /// there can be 2 reasons why method is not logged:
    /// 1. It was not called
    /// 2. It's signature changed or it was moved to another class, renamed e.t.c - in other words it's the differences caused by code changes between weaving and cleanup processes
    /// To mitigate this situation we can
    /// 2.1 Run cleanup over the the old version (those that was used during the weaving) and the merge all the deletions into the new code - this is wery time consuming and still it needs to resolve conflicts
    /// --------------------------------------------------------------- BEST APPROACH -------------------------------------------------------------------------------------------------
    /// 2.2 It's better to analyze weaved libs during the cleanup and delete only those methods that found in weaved libs, but not logged and match methods found by Roslyn. todo: its good to mark weaved assemblies with some weawingGuid and log it with method and then on cleanup check that the logged and weaved guids match
    /// -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// All other methods: new or with changed signatures must be just listed as non-matched. In other words, only the intersection of Roslyn and Cecil methods must be processed.
    /// </summary>
    [TestFixture]
    public class DeletionEvaluatorTest
    {
        [Test]
        public void WhenLibsContainMoreMethodsThanLogged_ShouldEvaluateCorrectly()
        {
            var deleteEvaluator = new DeletionEvaluator(
                GetCodeCleanupRepository(new[] { "Method1", "Method2" }),
                GetWeavedDataProvider(new[] { "Method1", "Method2", "Method3" }));

            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method1"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method2"));
            Assert.IsTrue(deleteEvaluator.NeedToDeleteMethod("Method3"));
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method1"));
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method2"));
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method3"));
        }

        [Test]
        public void WhenLibsContainSameMethodsAsLogged_ShouldEvaluateCorrectly()
        {
            var deleteEvaluator = new DeletionEvaluator(GetCodeCleanupRepository(new[] { "Method1", "Method2" }), GetWeavedDataProvider(new[] { "Method1", "Method2" }));

            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method1"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method2"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method3")); // unknown method is not deleted
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method1"));
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method2"));
            Assert.IsFalse(deleteEvaluator.MethodFoundInWeavedLibs("Method3")); // unknown method is not found
        }

        [Test]
        [Description("Methods not found by Roslyn must be ignored during the deletion process.")]
        public void WhenLibsContainLessMethodsThanLogged_ShouldEvaluateCorrectly()
        {
            var deleteEvaluator = new DeletionEvaluator(
                GetCodeCleanupRepository(new[] { "Method1", "Method2" }),
                GetWeavedDataProvider(new[] { "Method1", "Method222", "Method333" }));

            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method1"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method2"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteMethod("Method3")); // the 2.2 case
            Assert.IsTrue(deleteEvaluator.NeedToDeleteMethod("Method222"));
            Assert.IsTrue(deleteEvaluator.NeedToDeleteMethod("Method333"));
            Assert.IsTrue(deleteEvaluator.MethodFoundInWeavedLibs("Method1"));
            Assert.IsFalse(deleteEvaluator.MethodFoundInWeavedLibs("Method2"));
        }

        [Test]
        public void WhenNoMethodsFromTypeCalled_ShouldDeleteType()
        {
            var deleteEvaluator = new DeletionEvaluator(
                GetCodeCleanupRepository(new[] { "T1::Method1", "T1::Method2" }),
                GetWeavedDataProvider(new[] { "T1::Method1", "T1::Method2", "T2::Method3" }));

            Assert.IsFalse(deleteEvaluator.NeedToDeleteType("T1"));
            Assert.IsTrue(deleteEvaluator.NeedToDeleteType("T2"));
        }

        [Test]
        public void WhenMethodsFromTypeCalled_ShouldNotDeleteType()
        {
            var deleteEvaluator = new DeletionEvaluator(
                GetCodeCleanupRepository(new[] { "T1::Method1", "T1::Method2", "T2::Method3" }),
                GetWeavedDataProvider(new[] { "T1::Method1", "T1::Method2", "T2::Method3", "T2::Method4" }));

            Assert.IsFalse(deleteEvaluator.NeedToDeleteType("T1"));
            Assert.IsFalse(deleteEvaluator.NeedToDeleteType("T2"));
        }

        private IWeawingDataProvider GetWeavedDataProvider(string[] methods)
        {
            var stub = MockRepository.GenerateMock<IWeawingDataProvider>();
            stub.Stub(x => x.GetMethodsFoundInWeavedLibs()).Return(methods);
            return stub;
        }

        private ICodeCleanupRepository GetCodeCleanupRepository(string[] methods)
        {
            var stub = MockRepository.GenerateMock<ICodeCleanupRepository>();
            stub.Stub(x => x.GetLoggedMethods()).Return(methods);
            return stub;
        }
    }
}