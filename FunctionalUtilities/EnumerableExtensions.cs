using System;
using System.Collections.Generic;


namespace FunctionalUtilities
{
    /// <summary>
    /// custom extension methods to IENumerable
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// ForEach can be used instead of foreach loop like List ForEach method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> range, Action<T> action)
        {
            foreach(var i in range)
            {
                action(i);
            }
        }

        /// <summary>
        /// Calls f on x but ignores, does not return the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="f"></param>
        /// <returns>void</returns>
        public static Action<T> Ignore<T,TOut>(this Func<T,TOut> f)
        {
            return
                x => { f(x);
                        // ReSharper disable once RedundantJumpStatement 
                        /*  we want to ignore the result of executing f *
                        * , we are only interested in the side effect   */
                        return;
                };
            
        }
    }
}
