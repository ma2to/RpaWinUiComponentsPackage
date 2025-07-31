using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities.Extensions
{
    /// <summary>
    /// Extension metódy pre DispatcherQueue - ✅ INTERNAL (zdieľané medzi komponentmi)
    /// </summary>
    internal static class DispatcherQueueExtensions
    {
        /// <summary>
        /// Async EnqueueAsync pre DispatcherQueue
        /// </summary>
        public static Task EnqueueAsync(this DispatcherQueue dispatcher, Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();

            var result = dispatcher.TryEnqueue(() =>
            {
                try
                {
                    callback();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!result)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation"));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Async EnqueueAsync s return value
        /// </summary>
        public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<T> callback)
        {
            var tcs = new TaskCompletionSource<T>();

            var result = dispatcher.TryEnqueue(() =>
            {
                try
                {
                    var returnValue = callback();
                    tcs.SetResult(returnValue);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!result)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation"));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Async EnqueueAsync s async callback
        /// </summary>
        public static Task EnqueueAsync(this DispatcherQueue dispatcher, Func<Task> asyncCallback)
        {
            var tcs = new TaskCompletionSource<bool>();

            var result = dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    await asyncCallback();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!result)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation"));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Skontroluje či sme na UI thread
        /// </summary>
        public static bool HasThreadAccess(this DispatcherQueue dispatcher)
        {
            try
            {
                return dispatcher.HasThreadAccess;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enqueue s prioritou
        /// </summary>
        public static Task EnqueueAsync(this DispatcherQueue dispatcher, DispatcherQueuePriority priority, Action callback)
        {
            var tcs = new TaskCompletionSource<bool>();

            var result = dispatcher.TryEnqueue(priority, () =>
            {
                try
                {
                    callback();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!result)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation"));
            }

            return tcs.Task;
        }
    }
}