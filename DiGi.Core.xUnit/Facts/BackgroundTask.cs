using DiGi.Core.Classes;
using DiGi.Core.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        private class TestBackgroundTask : BackgroundTask
        {
            private readonly int delayMs;
            private readonly bool shouldSucceed;

            public TestBackgroundTask(int delayMs = 10, bool shouldSucceed = true)
            {
                this.delayMs = delayMs;
                this.shouldSucceed = shouldSucceed;
            }

            protected override async Task<bool> ExecuteAsync()
            {
                await Task.Delay(delayMs);
                if (!shouldSucceed)
                {
                    throw new InvalidOperationException("Simulated task failure");
                }
                return true;
            }
        }

        private class TestCancelableBackgroundTask : CancelableBackgroundTask
        {
            private readonly int delayMs;

            public TestCancelableBackgroundTask(int delayMs = 100)
            {
                this.delayMs = delayMs;
            }

            protected override async Task<bool> ExecuteAsync(CancellationToken token)
            {
                // Simple loop simulating cancellation awareness
                for (int i = 0; i < delayMs / 10; i++)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(10, token);
                }
                return true;
            }
        }

        [Fact]
        public async Task BackgroundTask_Lifecycle_Success()
        {
            var task = new TestBackgroundTask(delayMs: 20, shouldSucceed: true);

            bool startingFired = false;
            bool startedFired = false;
            bool stoppingFired = false;
            bool stoppedFired = false;

            task.Starting += (s, e) => startingFired = true;
            task.Started += (s, e) => startedFired = true;
            task.Stopping += (s, e) => stoppingFired = true;
            task.Stopped += (s, e) => stoppedFired = true;

            Assert.Equal(BackgroundTaskStatus.Idle, task.BackgroundTaskStatus);
            Assert.False(task.IsRunning);
            Assert.False(task.IsCompleted);

            task.Start();

            // Should transition to Running immediately
            Assert.True(task.IsRunning || task.IsCompleted);

            // Wait for completion
            int timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.False(task.IsRunning);
            Assert.True(task.IsSucceeded);
            Assert.Null(task.Exception);
            Assert.Equal(BackgroundTaskStatus.Completed, task.BackgroundTaskStatus);

            Assert.True(startingFired);
            Assert.True(startedFired);
            Assert.True(stoppingFired);
            Assert.True(stoppedFired);
            Assert.True(task.ExecutionTimeSpan > System.TimeSpan.Zero);
        }

        [Fact]
        public async Task BackgroundTask_Lifecycle_Failure()
        {
            var task = new TestBackgroundTask(delayMs: 10, shouldSucceed: false);

            task.Start();

            int timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.Equal(BackgroundTaskStatus.Failed, task.BackgroundTaskStatus);
            Assert.NotNull(task.Exception);
            Assert.IsType<InvalidOperationException>(task.Exception);
        }

        [Fact]
        public async Task CancelableBackgroundTask_Cancellation()
        {
            TestCancelableBackgroundTask task = new TestCancelableBackgroundTask(delayMs: 2000);

            bool canceledFired = false;
            task.Canceled += (s, e) => canceledFired = true;

            task.Start();

            // Allow task to run briefly
            await Task.Delay(50);
            Assert.Equal(CancelableBackgroundTaskStatus.Running, task.CancelableBackgroundTaskStatus);

            // Stop the task (this requests cancellation and awaits task completion)
            await task.StopAsync();

            // Since production code's StopAsync calls Cleanup() which sets the Task reference to null,
            // IsCompleted returns false (as Task is null), IsRunning is false, and Status resets to Idle.
            Assert.False(task.IsRunning);
            Assert.Equal(CancelableBackgroundTaskStatus.Idle, task.CancelableBackgroundTaskStatus);
            Assert.True(canceledFired);
        }
    }
}