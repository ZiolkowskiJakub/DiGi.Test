using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, CancellationToken)"/> captures standard output and standard error separately, propagates a non-zero exit code, and layers environment variables on the inherited environment.
        /// <para>Drives cmd.exe, so it needs no Python interpreter.</para>
        /// </summary>
        [Fact]
        public void ExecuteProcess()
        {
            string directory = Path.GetTempPath();

            Classes.PythonProcessResult pythonProcessResult = Query.ExecuteProcess("cmd.exe", "/d /c \"echo out&(echo err)1>&2&exit /b 3\"", directory);

            Assert.Equal(3, pythonProcessResult.ExitCode);
            Assert.Equal(["out"], pythonProcessResult.StandardOutput!);
            Assert.Equal(["err"], pythonProcessResult.StandardError!);
            Assert.False(pythonProcessResult.Stalled);
            Assert.False(pythonProcessResult.Cancelled);

            Classes.PythonProcessResult pythonProcessResult_Success = Query.ExecuteProcess("cmd.exe", "/d /c \"echo ok&exit /b 0\"", directory);

            Assert.Equal(0, pythonProcessResult_Success.ExitCode);
            Assert.Equal(["ok"], pythonProcessResult_Success.StandardOutput!);
            Assert.Empty(pythonProcessResult_Success.StandardError!);

            //The added variable is visible to the process, and the inherited environment (SystemRoot) is still there
            Dictionary<string, string> environmentVariables = new() { { "DIGI_PYTHON_TEST", "layered" } };
            Classes.PythonProcessResult pythonProcessResult_Environment = Query.ExecuteProcess("cmd.exe", "/d /c \"echo %DIGI_PYTHON_TEST%&echo %SystemRoot%\"", directory, environmentVariables);

            Assert.Equal(0, pythonProcessResult_Environment.ExitCode);
            Assert.Equal(2, pythonProcessResult_Environment.StandardOutput!.Count);
            Assert.Equal("layered", pythonProcessResult_Environment.StandardOutput[0]);
            Assert.Equal(Environment.GetEnvironmentVariable("SystemRoot"), pythonProcessResult_Environment.StandardOutput[1]);
        }

        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, CancellationToken)"/> ends a process that stops producing output once a 1 s inactivity limit is exceeded, together with the child process it started, reports it as stalled with exit code -1 and the reason in the standard error tail, and leaves a process that keeps producing lines alone.
        /// <para>Medium test (5 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long. Drives powershell.exe and cmd.exe, so it needs no Python interpreter.</para>
        /// </summary>
        [MediumFact]
        public void ExecuteProcess_Inactivity()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi_Python_Test_" + Path.GetRandomFileName());

            static bool HasExited(int processId)
            {
                try
                {
                    using Process process = Process.GetProcessById(processId);
                    return process.HasExited;
                }
                catch (ArgumentException)
                {
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            }

            try
            {
                Directory.CreateDirectory(directory);

                //The parent prints its own id and the id of a silent child (its output goes to a file, not to the parent's streams), then waits for the child without writing anything
                string path_ChildOutput = Path.Combine(directory, "child.txt");
                string command = string.Format(CultureInfo.InvariantCulture, "$p = Start-Process -FilePath ping.exe -ArgumentList '-n','60','127.0.0.1' -PassThru -WindowStyle Hidden -RedirectStandardOutput '{0}'; [Console]::Out.WriteLine($PID); [Console]::Out.WriteLine($p.Id); [Console]::Out.Flush(); $p.WaitForExit()", path_ChildOutput);
                string arguments = string.Format(CultureInfo.InvariantCulture, "-NoProfile -NonInteractive -Command \"{0}\"", command);

                Stopwatch stopwatch_Stalled = Stopwatch.StartNew();
                Classes.PythonProcessResult pythonProcessResult_Stalled = Query.ExecuteProcess("powershell.exe", arguments, directory, null, TimeSpan.FromSeconds(1));
                stopwatch_Stalled.Stop();

                Assert.True(pythonProcessResult_Stalled.Stalled);
                Assert.False(pythonProcessResult_Stalled.Cancelled);
                Assert.Equal(-1, pythonProcessResult_Stalled.ExitCode);
                Assert.True(stopwatch_Stalled.Elapsed < TimeSpan.FromSeconds(20), string.Format(CultureInfo.InvariantCulture, "A silent process under a 1 s limit took {0} to end", stopwatch_Stalled.Elapsed));
                Assert.Contains(pythonProcessResult_Stalled.StandardError!, line => line.StartsWith("Ended after", StringComparison.Ordinal));

                Assert.Equal(2, pythonProcessResult_Stalled.StandardOutput!.Count);
                int processId_Parent = int.Parse(pythonProcessResult_Stalled.StandardOutput[0], CultureInfo.InvariantCulture);
                int processId_Child = int.Parse(pythonProcessResult_Stalled.StandardOutput[1], CultureInfo.InvariantCulture);

                //Process.Kill(true) returns once termination is requested, so allow the processes a moment to go
                Stopwatch stopwatch_Gone = Stopwatch.StartNew();
                while ((!HasExited(processId_Parent) || !HasExited(processId_Child)) && stopwatch_Gone.Elapsed < TimeSpan.FromSeconds(5))
                {
                    Thread.Sleep(100);
                }

                Assert.True(HasExited(processId_Parent), "The parent process is still running");
                Assert.True(HasExited(processId_Child), "The child process survived the parent being ended for inactivity");

                //A process writing a line about every second is never ended under a 2 s limit, although it lives longer than that
                Classes.PythonProcessResult pythonProcessResult_Chatty = Query.ExecuteProcess("cmd.exe", "/d /c \"for /L %i in (1,1,4) do @(echo %i&ping -n 2 127.0.0.1 >nul)\"", directory, null, TimeSpan.FromSeconds(2));

                Assert.False(pythonProcessResult_Chatty.Stalled);
                Assert.Equal(0, pythonProcessResult_Chatty.ExitCode);
                Assert.Equal(["1", "2", "3", "4"], pythonProcessResult_Chatty.StandardOutput!);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, CancellationToken)"/> ends a running process tree promptly once its token is cancelled, reporting it as cancelled with exit code -1.
        /// <para>The process is cmd.exe waiting on a child ping.exe whose standard error is the redirected pipe, so the call returns only if the child is ended too.</para>
        /// </summary>
        [Fact]
        public void ExecuteProcess_Cancellation()
        {
            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500));

            Stopwatch stopwatch = Stopwatch.StartNew();
            Classes.PythonProcessResult pythonProcessResult = Query.ExecuteProcess("cmd.exe", "/d /c \"ping -n 60 127.0.0.1 >nul\"", Path.GetTempPath(), cancellationToken: cancellationTokenSource.Token);
            stopwatch.Stop();

            Assert.True(pythonProcessResult.Cancelled);
            Assert.False(pythonProcessResult.Stalled);
            Assert.Equal(-1, pythonProcessResult.ExitCode);

            //Cancelled at 0.5 s; the wait loop polls every 0.25 s
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), string.Format(CultureInfo.InvariantCulture, "A run cancelled after 0.5 s took {0} to return", stopwatch.Elapsed));
        }

        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, CancellationToken)"/> keeps exactly the last <see cref="Constants.Count.OutputLines"/> lines of a stream, in order, when a process prints more.
        /// </summary>
        [Fact]
        public void ExecuteProcess_OutputLines()
        {
            Classes.PythonProcessResult pythonProcessResult = Query.ExecuteProcess("cmd.exe", "/d /c \"for /L %i in (1,1,500) do @echo %i\"", Path.GetTempPath());

            Assert.Equal(0, pythonProcessResult.ExitCode);
            Assert.Equal(Constants.Count.OutputLines, pythonProcessResult.StandardOutput!.Count);

            for (int i = 0; i < pythonProcessResult.StandardOutput.Count; i++)
            {
                Assert.Equal((500 - Constants.Count.OutputLines + 1 + i).ToString(CultureInfo.InvariantCulture), pythonProcessResult.StandardOutput[i]);
            }
        }

        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, CancellationToken)"/> reports an executable that cannot be started with exit code -1 and the exception message as the only standard error line, rather than throwing.
        /// </summary>
        [Fact]
        public void ExecuteProcess_StartFailure()
        {
            string path = Path.Combine(Path.GetTempPath(), "no_such_executable_" + Path.GetRandomFileName() + ".exe");

            Classes.PythonProcessResult pythonProcessResult = Query.ExecuteProcess(path, string.Empty, Path.GetTempPath());

            Assert.Equal(-1, pythonProcessResult.ExitCode);
            Assert.Empty(pythonProcessResult.StandardOutput!);
            Assert.Single(pythonProcessResult.StandardError!);
            Assert.False(string.IsNullOrWhiteSpace(pythonProcessResult.StandardError![0]));
            Assert.False(pythonProcessResult.Stalled);
            Assert.False(pythonProcessResult.Cancelled);
        }
    }
}
