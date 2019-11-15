using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemaphoreRepro.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            const int timesPerPeriod = 1;
            const int ms = 100;
            const int loops = 5;
            const double msMax = (double)ms * 1.5;
            const double msMin = (double)ms * (1.0 / 1.5);

            var semaphore = new SemaphoreSlim(timesPerPeriod, timesPerPeriod);
            var entered = await semaphore.WaitAsync(0);

            var t = new Thread(_ =>
            {
                while (true)
                {
                    Thread.Sleep(ms);
                    semaphore.Release();
                }
            })
            {
                IsBackground = true
            };
            t.Start();

            for (int i = 0; i < loops; i++)
            {
                Console.WriteLine("Waiting {0}...", i);
                var timer = Stopwatch.StartNew();
                await semaphore.WaitAsync(-1);
                timer.Stop();
                Console.WriteLine("Waited for {0}...", timer.Elapsed.TotalMilliseconds);

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
    }
}
