using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Configuration;
using MethodLogger;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public partial class IntegrationTestsYourSln1
    {
        [Test]
        public void RepositoryThrows_WhenSavingDuplicateMethods()
        {
            DeleteAllLoggedMethods();

            var config = new Config(new ErrorLogger());
            var repo = new MethodLoggerRepository(config);
            Assert.Throws<SqlException>(() => repo.AddMethods(InvocationLogger.ComposeRows(new[] { "Duplicate", "Duplicate" })));
        }

        [Test]
        public void MultithreadedWriteOfLoggableMethodsDoesntCauseThreadingIssues_AtRepositoryLevel()
        {
            DeleteAllLoggedMethods();
            EnsureAllLoggableWeavedMethodsCanBeSavedIntoDbSuccessfully();

            var allLoggedMethods = GetLoggedMethods().ToArray();

            DeleteAllLoggedMethods();

            var config = new Config(new ErrorLogger());

            // several threads write the same chunk 
            // (testing the situation when different applications create same classes/ call same methods and log them)
            Parallel.ForEach(
                Enumerable.Range(0, 4),
                i =>
                {
                    var chunk = allLoggedMethods.Take(config.ChunkSize).ToArray();
                    var repo = new MethodLoggerRepository(config);
                    Console.WriteLine("Writing " + i);
                    repo.AddMethods(InvocationLogger.ComposeRows(chunk));
                    Console.WriteLine("Finished " + i);
                });

            var loggedMethods = GetLoggedMethods().ToArray();

            Assert.That(loggedMethods.Count(), Is.EqualTo(config.ChunkSize));
        }

        [Test]
        public void MultithreadedWriteOfLoggableMethodsDoesntCauseThreadingIssues_AtInvocationLoggerLevel()
        {
            DeleteAllLoggedMethods();
            EnsureAllLoggableWeavedMethodsCanBeSavedIntoDbSuccessfully();

            var allLoggedMethods = GetLoggedMethods().ToArray();

            DeleteAllLoggedMethods();

            var errorLogger = new ErrorLogger();
            var config = new Config(errorLogger) { ApproximateLoggersCount = 1, PersistIntervalMs = 10, ChunkSize = 1000 };

            int methIndex = 0;
            var loggerChunk = allLoggedMethods.Take(config.ChunkSize).ToDictionary(m => m, m => methIndex++);

            var savedEvents = new List<ManualResetEvent>();

            // several threads write the same chunk (testing the situation when different applications create same classes/ call same methods and log them)
            Parallel.ForEach(
                Enumerable.Range(0, 4),
                loggerIndex =>
                {
                    Console.WriteLine("InvocationLogger #{0} Started", loggerIndex);
                    var invocationLogger = CreateInvocationLogger(config, errorLogger);

                    var evt = new ManualResetEvent(false);
                    savedEvents.Add(evt);
                    invocationLogger.AllNewMethodsLogged += () => evt.Set();

                    // each logger has 10 threads that try to log same methods
                    Parallel.ForEach(
                        Enumerable.Range(0, 10),
                        j =>
                        {
                            // lets say each thread calls 100 methods (and log them)
                            foreach (var threadChunk in Partition(loggerChunk, 100))
                            {
                                foreach (var method in threadChunk)
                                {
                                    invocationLogger.Log(method.Key, method.Value);
                                }
                            }
                        });

                    Console.WriteLine("InvocationLogger #{0} Finished", loggerIndex);
                });

            WaitHandle.WaitAll(savedEvents.ToArray());

            var loggedMethods = GetLoggedMethods().ToArray();

            Assert.That(loggedMethods.Count(), Is.EqualTo(config.ChunkSize));
        }

        private InvocationLogger CreateInvocationLogger(Config config, ErrorLogger errorLogger)
        {
            return new InvocationLogger(errorLogger, config, new MethodLoggerRepository(config));
        }
    }
}