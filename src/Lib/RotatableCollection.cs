using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCollector.Lib {
    public class RotatableReadOnlyCollection<T> : IEnumerable<T>
    {
        private readonly IList<T> _collection;
        private int _index = -1;

        public RotatableReadOnlyCollection(IEnumerable<T> collection) {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            _collection = new List<T>(collection);
        }

        public T Next() {
            if (_collection.Count < 1) return default(T);
            _index = ++_index % _collection.Count;
            return _collection[_index];
        }

        public IEnumerator<T> GetEnumerator() {
            while (true) {
                yield return Next();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public static partial class Enumerable {
        public static RotatableReadOnlyCollection<T> ToRotatableReadOnlyCollection<T>(this IEnumerable<T> collection) {
            return new RotatableReadOnlyCollection<T>(collection);
        }
    }
}
