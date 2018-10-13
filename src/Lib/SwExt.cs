using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;

namespace YoutubeCollector.Lib {
    public static class SwExt {
        public static TimeSpan CalculateTimeLeft(this TimeSpan elapsed, long stepsDone, long allSteps) {
            if (stepsDone > 0) {
                var minutesElapsed = elapsed.TotalMinutes;
                var oneStep = minutesElapsed / stepsDone;
                var stepsLeft = allSteps - stepsDone;
                var minutesLeft = stepsLeft * oneStep;
                return TimeSpan.FromMinutes(minutesLeft);
            }
            else {
                return TimeSpan.MaxValue;
            }
        }

        public static TimeSpan CalculateTimeLeft(this Stopwatch sw, long stepsDone, long allSteps) => CalculateTimeLeft(sw.Elapsed, stepsDone, allSteps);
        public static TimeSpan CalculateTimeLeft(this Stopwatch sw, long percentDone) => CalculateTimeLeft(sw.Elapsed, percentDone, 100);

        public static Stopwatch Run(this Stopwatch sw) {
            sw.Start();
            return sw;
        }

        public static async Task<T> TryHarder<T,TL>(this Task<T> task, ILogger<TL> logger, int howHard = 5, int millisecondsDelay = 5000, CancellationToken ct = default(CancellationToken)) {
            void LogException(Exception exception) {
                logger.LogWarning(exception, task.GetType().FullName);
            }
            return await task.TryHarder(howHard, LogException, millisecondsDelay, ct);
        }

        public static async Task<T> TryHarder<T>(this Task<T> task, int howHard = 5, Action<Exception> skippedException = null, int millisecondsDelay = 5000, CancellationToken ct = default(CancellationToken)) {
            while (true) {
                try {
                    return await task;
                }
                catch (Exception ex) {
                    howHard--;
                    if (howHard <= 0) throw;
                    skippedException?.Invoke(ex);
                    await Task.Delay(millisecondsDelay, ct);
                }
            }
        }
    }
}
