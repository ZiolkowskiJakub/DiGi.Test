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

        private class TestRefusingBackgroundTask : BackgroundTask
        {
            protected override Task<bool> ExecuteAsync()
            {
                // A deliberate refusal: reports failure without throwing, the way a task declining
                // to act does - and the situation the fallback wrap exists for.
                return Task.FromResult(false);
            }
        }

        private class TestConfigurableBackgroundTask : BackgroundTask
        {
            public bool ShouldSucceed { get; set; } = true;

            protected override Task<bool> ExecuteAsync()
            {
                return Task.FromResult(ShouldSucceed);
            }
        }

        private class TestThrowingRefusalBackgroundTask : BackgroundTask
        {
            protected override Task<bool> ExecuteAsync()
            {
                throw new BackgroundTaskFailureException("A user with the email user@digiproject.uk already exists - no user was created");
            }
        }

        /// <summary>
        /// Tests the successful execution lifecycle of a background task, verifying all state transitions and events.
        /// </summary>
        [Fact]
        public async Task BackgroundTask_Lifecycle_Success()
        {
            TestBackgroundTask task = new(delayMs: 20, shouldSucceed: true);

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

        /// <summary>
        /// Tests the failed execution lifecycle of a background task, verifying exception capture and final failed state.
        /// </summary>
        [Fact]
        public async Task BackgroundTask_Lifecycle_Failure()
        {
            TestBackgroundTask task = new(delayMs: 10, shouldSucceed: false);

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

        /// <summary>
        /// Tests that a task reporting failure by returning false without an exception is wrapped in a <see cref="BackgroundTaskFailureException"/>, so its run carries a message the task row can show.
        /// </summary>
        [Fact]
        public async Task BackgroundTask_FailureWithoutException()
        {
            TestRefusingBackgroundTask task = new();

            task.Start();

            int timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.False(task.IsRunning);
            Assert.False(task.IsSucceeded);
            Assert.Equal(BackgroundTaskStatus.Failed, task.BackgroundTaskStatus);

            Assert.NotNull(task.Exception);
            Assert.IsType<BackgroundTaskFailureException>(task.Exception);
            Assert.False(string.IsNullOrWhiteSpace(task.Exception.Message));
        }

        /// <summary>
        /// Tests that a task throwing a <see cref="BackgroundTaskFailureException"/> keeps exactly that instance as its exception, rather than being wrapped a second time.
        /// </summary>
        [Fact]
        public async Task BackgroundTask_RefusalException()
        {
            TestThrowingRefusalBackgroundTask task = new();

            task.Start();

            int timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.Equal(BackgroundTaskStatus.Failed, task.BackgroundTaskStatus);

            BackgroundTaskFailureException? backgroundTaskFailureException = Assert.IsType<BackgroundTaskFailureException>(task.Exception);
            Assert.Equal("A user with the email user@digiproject.uk already exists - no user was created", backgroundTaskFailureException.Message);
        }

        /// <summary>
        /// Tests that a failed run leaves no failure behind after the same task succeeds on a restart, so a row never shows the reason of a run before the last one.
        /// </summary>
        [Fact]
        public async Task BackgroundTask_RestartClearsFailure()
        {
            TestConfigurableBackgroundTask task = new() { ShouldSucceed = false };

            task.Start();

            int timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.NotNull(task.Exception);
            Assert.IsType<BackgroundTaskFailureException>(task.Exception);
            Assert.Equal(BackgroundTaskStatus.Failed, task.BackgroundTaskStatus);

            task.ShouldSucceed = true;

            task.Start();

            timeoutMs = 1000;
            while (!task.IsCompleted && timeoutMs > 0)
            {
                await Task.Delay(10);
                timeoutMs -= 10;
            }

            Assert.True(task.IsCompleted);
            Assert.Null(task.Exception);
            Assert.True(task.IsSucceeded);
            Assert.Equal(BackgroundTaskStatus.Completed, task.BackgroundTaskStatus);
        }

        /// <summary>
        /// Tests that a run stopped by cancelling its task is not wrapped as a <see cref="BackgroundTaskFailureException"/>: it returned false only because its operator asked it to stop, and a stop is not a failure to report.
        /// </summary>
        [Fact]
        public async Task CancelableBackgroundTask_Cancellation_NoFailureException()
        {
            TestCancelableBackgroundTask task = new(delayMs: 2000);

            task.Start();

            // Allow task to run briefly, then stop it - the stop requests cancellation and awaits completion
            await Task.Delay(50);
            await task.StopAsync();

            Assert.Null(task.Exception);
        }

        /// <summary>
        /// Tests the cancelable lifecycle of a background task, verifying cancellation triggers and event firing.
        /// </summary>
        [Fact]
        public async Task CancelableBackgroundTask_Cancellation()
        {
            TestCancelableBackgroundTask task = new(delayMs: 2000);

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