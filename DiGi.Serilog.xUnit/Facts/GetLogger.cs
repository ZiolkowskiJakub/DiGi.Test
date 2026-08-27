using DiGi.Serilog.Classes;
using Serilog.Core;
using System.Reflection;

namespace DiGi.Serilog.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Reproduces the concurrent first-time logger creation race. With a non-concurrent cache the
        /// check-then-create pattern lets racing callers corrupt the dictionary or throw, so the test
        /// must fail on the unmodified code and pass once the cache is made thread-safe.
        /// </summary>
        [Fact]
        public void GetLogger_ConcurrentCreation_ReturnsSingleLoggerPerPath()
        {
            const int threadCount = 16;
            const int roundCount = 25;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string baseDirectory = Path.Combine(Path.GetTempPath(), "DiGi.Serilog.xUnit", Guid.NewGuid().ToString("N"));

            try
            {
                for (int round = 0; round < roundCount; round++)
                {
                    LoggerManager loggerManager = new() { Directory = baseDirectory };
                    ManualResetEvent startGate = new(false);
                    CountdownEvent doneGate = new(threadCount);
                    Logger?[] loggers = new Logger?[threadCount];
                    Exception?[] exceptions = new Exception?[threadCount];

                    Thread[] threads = new Thread[threadCount];
                    for (int thread = 0; thread < threadCount; thread++)
                    {
                        int capturedThread = thread;
                        threads[thread] = new Thread(() =>
                        {
                            startGate.WaitOne();
                            try
                            {
                                loggers[capturedThread] = loggerManager.GetLogger(assembly);
                            }
                            catch (Exception exception)
                            {
                                exceptions[capturedThread] = exception;
                            }
                            finally
                            {
                                doneGate.Signal();
                            }
                        });
                        threads[thread].IsBackground = true;
                    }

                    foreach (Thread thread in threads)
                    {
                        thread.Start();
                    }

                    startGate.Set();

                    // Bounded wait: a corrupted non-concurrent cache can leave a worker spinning forever
                    // inside the dictionary, so the verdict must be a failure, never a hung test suite.
                    bool finished = doneGate.WaitHandle.WaitOne(10000);
                    Assert.True(finished, "Concurrent GetLogger calls did not complete within 10 s (racing writes on a non-concurrent cache)");

                    Exception? firstException = exceptions.FirstOrDefault();
                    Assert.Null(firstException);
                    Assert.All(loggers, logger => Assert.Same(loggers[0], logger));
                }
            }
            finally
            {
                if (Directory.Exists(baseDirectory))
                {
                    Directory.Delete(baseDirectory, true);
                }
            }
        }

        /// <summary>
        /// The explicit Directory override stays the source of the log location and the log file is
        /// created inside the logs folder of that directory.
        /// </summary>
        [Fact]
        public void GetLogger_DirectoryOverride_WritesBesideOverrideDirectory()
        {
            string baseDirectory = Path.Combine(Path.GetTempPath(), "DiGi.Serilog.xUnit", Guid.NewGuid().ToString("N"));

            try
            {
                LoggerManager loggerManager = new() { Directory = baseDirectory };
                Logger? logger = loggerManager.GetLogger(Assembly.GetExecutingAssembly());
                Assert.NotNull(logger);

                logger.Information("Directory override test");
                logger.Dispose();

                string logDirectory = Path.Combine(baseDirectory, "logs");
                Assert.True(Directory.Exists(logDirectory));
                Assert.Single(Directory.GetFiles(logDirectory, "log*.txt"));
            }
            finally
            {
                if (Directory.Exists(baseDirectory))
                {
                    Directory.Delete(baseDirectory, true);
                }
            }
        }
    }
}
