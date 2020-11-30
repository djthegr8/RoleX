using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoleX.Utilities {
    public static class ExtensionMethod {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> enumerable) {
            foreach (var obj in enumerable)
                collection.Add(obj);
        }
        
        public static Task SafeDelete(this IMessage? message) {
            try {
                return message?.DeleteAsync();
            }
            catch (Exception) {
                return Task.CompletedTask;
            }
        }
        
        public static void DelayedDelete(this IMessage message, TimeSpan span) {
            Task.Run(async () => {
                await Task.Delay(span);
                message.SafeDelete();
            });
        }

        public static void DelayedDelete(this Task<IUserMessage> message, TimeSpan span) {
            Task.Run(async () => {
                await Task.Delay(span);
                (await message.ConfigureAwait(false)).SafeDelete();
            });
        }
        
        public static int Normalize(this int value, int min, int max) {
            return Math.Max(min, Math.Min(max, value));
        }
        
        /// <summary>
        /// Groups contiguous elements into a list based on a predicate.
        /// The predicate decides whether the next element should be added to the current group.
        /// If the predicate fails, the current group is closed and a new one, containing this element, is created.
        /// </summary>
        [return: NotNull]
        public static IEnumerable<IReadOnlyList<T>> GroupContiguous<T>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] Func<IReadOnlyList<T>, T, bool> groupPredicate)
        {
            List<T> source1 = new List<T>();
            foreach (T obj in source)
            {
                T element = obj;
                if (source1.Any<T>() && !groupPredicate((IReadOnlyList<T>) source1, element))
                {
                    yield return (IReadOnlyList<T>) source1;
                    source1 = new List<T>();
                }
                source1.Add(element);
                element = default (T);
            }
            if (source1.Any<T>())
                yield return (IReadOnlyList<T>) source1;
        }
    }
}