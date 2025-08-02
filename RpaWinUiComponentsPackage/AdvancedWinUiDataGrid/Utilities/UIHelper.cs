// AdvancedWinUiDataGrid/Utilities/UIHelper.cs - ✅ NOVÝ SÚBOR
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.Common.SharedUtilities.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre UI operácie - INTERNAL
    /// </summary>
    internal static class UIHelper
    {
        /// <summary>
        /// Bezpečne vykoná operáciu na UI thread
        /// </summary>
        public static async Task RunOnUIThreadAsync(Action action, ILogger? logger = null)
        {
            try
            {
                var dispatcher = DispatcherQueue.GetForCurrentThread();

                if (dispatcher?.HasThreadAccess == true)
                {
                    // Už sme na UI thread
                    action();
                }
                else if (dispatcher != null)
                {
                    // Musíme sa prepnúť na UI thread
                    await dispatcher.EnqueueAsync(action);
                }
                else
                {
                    logger?.LogWarning("⚠️ UIHelper: No UI dispatcher available");
                    // Skús vykonať aj tak (môže skončiť chybou)  
                    action();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ UIHelper: Error in RunOnUIThreadAsync");
                throw;
            }
        }

        /// <summary>
        /// Bezpečne vykoná async operáciu na UI thread
        /// </summary>
        public static async Task RunOnUIThreadAsync(Func<Task> asyncAction, ILogger? logger = null)
        {
            try
            {
                var dispatcher = DispatcherQueue.GetForCurrentThread();

                if (dispatcher?.HasThreadAccess == true)
                {
                    // Už sme na UI thread
                    await asyncAction();
                }
                else if (dispatcher != null)
                {
                    // Musíme sa prepnúť na UI thread
                    await dispatcher.EnqueueAsync(asyncAction);
                }
                else
                {
                    logger?.LogWarning("⚠️ UIHelper: No UI dispatcher available for async operation");
                    // Skús vykonať aj tak
                    await asyncAction();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ UIHelper: Error in RunOnUIThreadAsync (async)");
                throw;
            }
        }

        /// <summary>
        /// Nájde child element v UI strome
        /// </summary>
        public static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && ((FrameworkElement)child).Name == childName)
                {
                    return typedChild;
                }

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }

        /// <summary>
        /// Nájde parent element v UI strome
        /// </summary>
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            if (parent is T typedParent)
                return typedParent;

            return FindParent<T>(parent);
        }

        /// <summary>
        /// Bezpečne nastaví visibility elementu
        /// </summary>
        public static async Task SetVisibilityAsync(FrameworkElement element, Visibility visibility, ILogger? logger = null)
        {
            if (element == null)
            {
                logger?.LogWarning("⚠️ UIHelper: Attempt to set visibility on null element");
                return;
            }

            await RunOnUIThreadAsync(() =>
            {
                element.Visibility = visibility;
                logger?.LogTrace("👁️ UIHelper: Set {ElementType}.Visibility = {Visibility}",
                    element.GetType().Name, visibility);
            }, logger);
        }

        /// <summary>
        /// Bezpečne nastaví text elementu
        /// </summary>
        public static async Task SetTextAsync(TextBlock textBlock, string text, ILogger? logger = null)
        {
            if (textBlock == null)
            {
                logger?.LogWarning("⚠️ UIHelper: Attempt to set text on null TextBlock");
                return;
            }

            await RunOnUIThreadAsync(() =>
            {
                textBlock.Text = text ?? string.Empty;
                logger?.LogTrace("📝 UIHelper: Set TextBlock.Text = '{Text}'", text);
            }, logger);
        }

        /// <summary>
        /// Animuje fade in/out elementu
        /// </summary>
        public static async Task FadeElementAsync(FrameworkElement element, double targetOpacity, TimeSpan duration, ILogger? logger = null)
        {
            if (element == null) return;

            await RunOnUIThreadAsync(async () =>
            {
                var currentOpacity = element.Opacity;
                var steps = 20;
                var stepDuration = duration.TotalMilliseconds / steps;
                var opacityStep = (targetOpacity - currentOpacity) / steps;

                for (int i = 0; i < steps; i++)
                {
                    element.Opacity = currentOpacity + (opacityStep * (i + 1));
                    await Task.Delay(TimeSpan.FromMilliseconds(stepDuration));
                }

                element.Opacity = targetOpacity;
                logger?.LogTrace("✨ UIHelper: Faded element to opacity {Opacity}", targetOpacity);
            }, logger);
        }

        /// <summary>
        /// Kontroluje či je element viditeľný na obrazovke
        /// </summary>
        public static bool IsElementVisible(FrameworkElement element)
        {
            if (element == null) return false;

            return element.Visibility == Visibility.Visible &&
                   element.Opacity > 0 &&
                   element.ActualWidth > 0 &&
                   element.ActualHeight > 0;
        }

        /// <summary>
        /// Získa skutočnú pozíciu elementu relatívne k rodičovi
        /// </summary>
        public static Windows.Foundation.Point GetRelativePosition(FrameworkElement element, FrameworkElement relativeTo)
        {
            try
            {
                var transform = element.TransformToVisual(relativeTo);
                return transform.TransformPoint(new Windows.Foundation.Point(0, 0));
            }
            catch
            {
                return new Windows.Foundation.Point(0, 0);
            }
        }

        /// <summary>
        /// Diagnostické informácie o elemente
        /// </summary>
        public static string GetElementDiagnostics(FrameworkElement element)
        {
            if (element == null) return "null";

            return $"{element.GetType().Name}[{element.Name}]: " +
                   $"Size({element.ActualWidth:F0}x{element.ActualHeight:F0}), " +
                   $"Visible={IsElementVisible(element)}, " +
                   $"Opacity={element.Opacity:F2}";
        }
    }
}