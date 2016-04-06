using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    static class Combinatorics
    {
        /*  
        * Combination with repetition (IT : Disposizioni con ripetizione)
        * n = input.size
        * k = length
        * 
        * output.size =  n^k
        * 
        * ES.
        * input = {0,1,2}
        * lenght = 2
        * output = {(0,0),(0,1),(0,2),(1,0),(1,1),(1,2),(2,0),(2,1),(2,2)}         
        */
        public static IEnumerable<String> CombinationsWithRepetition(IEnumerable<string> input, int length)
        {
            if (length <= 0)
                yield return "";
            else
            {
                foreach (var i in input)
                    foreach (var c in CombinationsWithRepetition(input, length - 1))
                        yield return i.ToString() + c;
            }
        }


        /* 
         * k-permutations of n item (IT: Disposizione semplice )
         * n = items.size
         * k = count 
         * 
         * output.size = n!/[(n-k)!k!]
         * 
         * ES:
         * items = {0,1,2,3,4}
         * count = 2
         * 
         * output = {(0,1),(0,2),(0,3),(0,4),(1,2),(1,3),(1,4),(2,3),(2,4),(3,4)}         
         */
        public static IEnumerable<IEnumerable<T>> GetDispositions<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetDispositions(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }


        /*
         * Permutations (IT: Permutazioni)
         * n = items.size
         *
         * output.size = n!
         * 
         * ES:
         * items = {0,1,2}
         * 
         * output = {(0,1,2),(0,2,1),(1,0,2),(1,2,0),(2,0,1),(2,1,0)}        
         * 
         */
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
