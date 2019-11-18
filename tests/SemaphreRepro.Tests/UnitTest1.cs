using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true, MaxParallelThreads = 1)]

namespace SemaphoreRepro.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestSemaphoreSlim()
        {
            const int timesPerPeriod = 1;
            const int ms = 100;
            const int loops = 5;
            const double msMax = (double)ms * 1.5;
            const double msMin = (double)ms * (1.0 / 1.5);

            output.WriteLine("# SemaphoreSlim");
            using var semaphore = new SemaphoreSlim(timesPerPeriod, timesPerPeriod);
            var entered = await semaphore.WaitAsync(0);
            Assert.True(entered);
            var globalSw = Stopwatch.StartNew();
            var isWaiting = false;
            var isFinished = false;

            var t = new Thread(_ =>
            {
                while (!isFinished)
                {
                    output.WriteLine("[0][{0}] Sleeping. ", globalSw.Elapsed.TotalMilliseconds);

                    var timer = Stopwatch.StartNew();
                    Thread.Sleep(ms);
                    timer.Stop();

                    output.WriteLine("[0][{0}] Sleeped for {1}ms", globalSw.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds);

                    if (isWaiting)
                    {
                        semaphore.Release();
                        output.WriteLine("[0][{0}] Released.", globalSw.Elapsed.TotalMilliseconds);
                    }
                }
            })
            {
                IsBackground = true
            };
            t.Start();

            try
            {
                for (int i = 0; i < loops; i++)
                {
                    output.WriteLine("[1][{0}] Waiting {1}...", globalSw.Elapsed.TotalMilliseconds, i);
                    var timer = Stopwatch.StartNew();
                    isWaiting = true;
                    await semaphore.WaitAsync(-1);
                    isWaiting = false;
                    timer.Stop();
                    output.WriteLine("[1][{0}] Waited for {1}...", globalSw.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds);

                    if (i <= 0)
                        continue;

                    // check for too much elapsed time with a little fudge
                    Assert.True(
                        timer.Elapsed.TotalMilliseconds <= msMax,
                        "Rate gate took too long to wait in between calls: " + timer.Elapsed.TotalMilliseconds + "ms"
                    );
                    Assert.True(
                        timer.Elapsed.TotalMilliseconds >= msMin,
                        "Rate gate took too little to wait in between calls: " + timer.Elapsed.TotalMilliseconds + "ms"
                    );
                }
            }
            finally
            {
                isFinished = true;
            }
        }

        [Fact]
        public void TestSemaphore()
        {
            const int timesPerPeriod = 1;
            const int ms = 100;
            const int loops = 5;
            const double msMax = (double)ms * 1.5;
            const double msMin = (double)ms * (1.0 / 1.5);

            output.WriteLine("# Semaphore");
            using var semaphore = new Semaphore(timesPerPeriod, timesPerPeriod);
            var entered = semaphore.WaitOne(0);
            Assert.True(entered);
            var globalSw = Stopwatch.StartNew();
            var isWaiting = false;
            var isFinished = false;

            var t = new Thread(_ =>
            {
                while (!isFinished)
                {
                    output.WriteLine("[0][{0}] Sleeping. ", globalSw.Elapsed.TotalMilliseconds);

                    var timer = Stopwatch.StartNew();
                    Thread.Sleep(ms);
                    timer.Stop();

                    output.WriteLine("[0][{0}] Sleeped for {1}ms", globalSw.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds);

                    if (isWaiting)
                    {
                        semaphore.Release();
                        output.WriteLine("[0][{0}] Released.", globalSw.Elapsed.TotalMilliseconds);
                    }
                }
            })
            {
                IsBackground = true
            };
            t.Start();

            try
            {
                for (int i = 0; i < loops; i++)
                {
                    output.WriteLine("[1][{0}] Waiting {1}...", globalSw.Elapsed.TotalMilliseconds, i);
                    var timer = Stopwatch.StartNew();
                    isWaiting = true;
                    semaphore.WaitOne(-1);
                    isWaiting = false;
                    timer.Stop();
                    output.WriteLine("[1][{0}] Waited for {1}ms...", globalSw.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds);

                    if (i <= 0)
                        continue;

                    // check for too much elapsed time with a little fudge
                    Assert.True(
                        timer.Elapsed.TotalMilliseconds <= msMax,
                        "Rate gate took too long to wait in between calls: " + timer.Elapsed.TotalMilliseconds + "ms"
                    );
                    Assert.True(
                        timer.Elapsed.TotalMilliseconds >= msMin,
                        "Rate gate took too little to wait in between calls: " + timer.Elapsed.TotalMilliseconds + "ms"
                    );
                }
            }
            finally
            {
                isFinished = true;
            }
        }
    }
}
