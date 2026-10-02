using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DiGi.YOLO.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, System.Threading.CancellationToken)"/> ends a process that stops producing output once the inactivity limit is exceeded, reports it as stalled and names the reason in the standard error tail, leaves a process that keeps producing lines alone, and waits for a process it is given no limit for.
        /// <para>Medium test (10 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void Query_ExecuteProcess_Inactivity()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi_YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                //One flushed line, then silence for far longer than any limit under test
                string path_Script_Silent = Path.Combine(directory, "silent.py");
                File.WriteAllLines(path_Script_Silent,
                [
                    "import time",
                    "print('one line', flush=True)",
                    "time.sleep(60)",
                ]);

                string arguments_Silent = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", path_Script_Silent);

                //A silent process is ended well before its own sleep would end it, with a non-zero exit code and the reason in the standard error tail
                Stopwatch stopwatch_Stalled = Stopwatch.StartNew();
                (int exitCode_Stalled, _, List<string> standardError_Stalled, bool stalled) = Query.ExecuteProcess(pythonPath!, arguments_Silent, directory, null, TimeSpan.FromSeconds(2));
                stopwatch_Stalled.Stop();

                Assert.True(stalled);
                Assert.NotEqual(0, exitCode_Stalled);
                Assert.True(stopwatch_Stalled.Elapsed < TimeSpan.FromSeconds(30), string.Format(CultureInfo.InvariantCulture, "A silent process under a 2 s limit took {0} to end", stopwatch_Stalled.Elapsed));
                Assert.Contains(standardError_Stalled, line => line.StartsWith("Ended after", StringComparison.Ordinal));

                //A process that keeps writing is never ended, even under a limit shorter than its lifetime
                string path_Script_Chatty = Path.Combine(directory, "chatty.py");
                File.WriteAllLines(path_Script_Chatty,
                [
                    "import time",
                    "for i in range(5):",
                    "    print(i, flush=True)",
                    "    time.sleep(1)",
                ]);

                Stopwatch stopwatch_Chatty = Stopwatch.StartNew();
                (int exitCode_Chatty, _, _, bool stalled_Chatty) = Query.ExecuteProcess(pythonPath!, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", path_Script_Chatty), directory, null, TimeSpan.FromSeconds(2));
                stopwatch_Chatty.Stop();

                Assert.False(stalled_Chatty);
                Assert.Equal(0, exitCode_Chatty);
                Assert.True(stopwatch_Chatty.Elapsed >= TimeSpan.FromSeconds(4));

                //No limit: the call waits for the process to end by itself
                string path_Script_Short = Path.Combine(directory, "short.py");
                File.WriteAllLines(path_Script_Short,
                [
                    "import time",
                    "print('one line', flush=True)",
                    "time.sleep(3)",
                ]);

                Stopwatch stopwatch_Short = Stopwatch.StartNew();
                (int exitCode_Short, _, _, bool stalled_Short) = Query.ExecuteProcess(pythonPath!, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", path_Script_Short), directory, null, null);
                stopwatch_Short.Stop();

                Assert.False(stalled_Short);
                Assert.Equal(0, exitCode_Short);
                Assert.True(stopwatch_Short.Elapsed >= TimeSpan.FromSeconds(3));
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
        /// Verifies that <see cref="Query.ExecuteProcess(string, string, string, Dictionary{string, string}?, TimeSpan?, System.Threading.CancellationToken)"/> ends a child process the interpreter started, both when the parent is ended for inactivity and when it is cancelled, so no worker survives either ending.
        /// <para>Medium test (4 s): runs when DIGI_TEST_MAX_DURATION is Medium (the default) or Long.</para>
        /// </summary>
        [MediumFact]
        public void Query_ExecuteProcess_ProcessTree()
        {
            string? pythonPath = PythonPath_Runnable();
            if (string.IsNullOrWhiteSpace(pythonPath))
            {
                return;
            }

            string directory = Path.Combine(Path.GetTempPath(), "DiGi.YOLO_Test_" + Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);

                //The parent prints the pid of a child that would outlive a plain Kill(), then goes silent long enough to be ended by the limit
                string path_Script_Parent = Path.Combine(directory, "parent.py");
                File.WriteAllLines(path_Script_Parent,
                [
                    "import subprocess, sys, time",
                    "child = subprocess.Popen([sys.executable, '-c', 'import time; time.sleep(60)'])",
                    "print(child.pid, flush=True)",
                    "time.sleep(60)",
                ]);

                static bool ProcessRunning(int processId)
                {
                    try
                    {
                        using Process _ = Process.GetProcessById(processId);
                        return true;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                }

                //A killed tree takes a moment to wind down, so the assertion waits for the disappearance rather than expecting it instantly
                static void WaitUntilNotRunning(int processId)
                {
                    DateTime dateTime_End = DateTime.Now.AddSeconds(10);
                    while (DateTime.Now < dateTime_End && ProcessRunning(processId))
                    {
                        Thread.Sleep(250);
                    }
                }

                static int ChildProcessId(List<string> standardOutput)
                {
                    foreach (string line in standardOutput)
                    {
                        if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int processId))
                        {
                            return processId;
                        }
                    }

                    return -1;
                }

                string arguments_Parent = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", path_Script_Parent);

                //Ended for inactivity: the child dies with the parent
                (int exitCode_Stalled, List<string> standardOutput_Stalled, _, bool stalled) = Query.ExecuteProcess(pythonPath!, arguments_Parent, directory, null, TimeSpan.FromSeconds(2));

                Assert.True(stalled);
                Assert.NotEqual(0, exitCode_Stalled);

                int processId_Child_Stalled = ChildProcessId(standardOutput_Stalled);
                Assert.True(processId_Child_Stalled > 0, string.Join(Environment.NewLine, standardOutput_Stalled));

                WaitUntilNotRunning(processId_Child_Stalled);
                Assert.False(ProcessRunning(processId_Child_Stalled), "The child of a parent ended for inactivity is still running");

                //Cancelled: the child dies with the parent as well, and the ending is not reported as a stall
                using (CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(2)))
                {
                    (int exitCode_Cancelled, List<string> standardOutput_Cancelled, _, bool stalled_Cancelled) = Query.ExecuteProcess(pythonPath!, arguments_Parent, directory, null, null, cancellationTokenSource.Token);

                    Assert.False(stalled_Cancelled);
                    Assert.Equal(-1, exitCode_Cancelled);

                    int processId_Child_Cancelled = ChildProcessId(standardOutput_Cancelled);
                    Assert.True(processId_Child_Cancelled > 0, string.Join(Environment.NewLine, standardOutput_Cancelled));

                    WaitUntilNotRunning(processId_Child_Cancelled);
                    Assert.False(ProcessRunning(processId_Child_Cancelled), "The child of a cancelled parent is still running");
                }
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
