using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCollector.Lib {
    public static partial class Enumerable {
        /// <summary>
        /// Partition into N groups
        /// </summary>
        public static IList<List<T>> Partition<T>(this IEnumerable<T> collection, int totalPartitions)
        {

            if (collection == null) throw new ArgumentNullException(nameof(collection));
            var list = collection.ToList();
            
            var partitions = new List<T>[totalPartitions]; 

            var maxSize = (int)Math.Ceiling(list.Count / (double)totalPartitions);
            var k = 0;

            for (var i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (var j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count) break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }

            return partitions.ToList();
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range) {
            foreach (var r in range) {
                hashSet.Add(r);
            }
        }
    }
}
