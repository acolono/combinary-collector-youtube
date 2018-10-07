using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeCollector.Lib {
    public class SyncCounter {
        private long _count;
        public SyncCounter(long initialValue = 0) {
            Interlocked.Exchange(ref _count, initialValue);
        }
        public long Read() => Interlocked.Read(ref _count);
        public long Increment() => Interlocked.Increment(ref _count);
        public long Add(long value) => Interlocked.Add(ref _count, value);
        public long Decrement() => Interlocked.Decrement(ref _count);
    }
}
