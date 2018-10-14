using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCollector.Lib {
    public class SyncHashSet<T> : IEnumerable<T> {
        private readonly HashSet<T> _hashSet;
        private readonly object _sync = new object();

        public SyncHashSet() {
            _hashSet = new HashSet<T>();
        }

        public SyncHashSet(IEqualityComparer<T> equalityComparer) {
            _hashSet = new HashSet<T>(equalityComparer);
        }

        public int Count() {
            lock (_sync) {
                return _hashSet.Count;
            }
        }

        public bool Add(T item) {
            lock (_sync) {
                return _hashSet.Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator() {
            lock (_sync) {
                return _hashSet.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
