// Utilities/ClipboardHelper.cs - ✅ INTERNAL Clipboard helper pre range operations
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper pre clipboard operations - INTERNAL
    /// </summary>
    internal static class ClipboardHelper
    {
        /// <summary>
        /// Nastaví text do clipboardu
        /// </summary>
        public static async Task SetClipboardTextAsync(string text)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(text);
                Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                // Ignore clipboard errors
            }
        }

        /// <summary>
        /// Získa text z clipboardu
        /// </summary>
        public static async Task<string> GetClipboardTextAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return await dataPackageView.GetTextAsync() ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Nastaví multiple formáty do clipboardu
        /// </summary>
        public static async Task SetClipboardDataAsync(string tabData, string csvData)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(tabData);
                
                // Add CSV format if different
                if (tabData != csvData)
                {
                    dataPackage.SetData("CSV", csvData);
                }
                
                Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                // Fallback to simple text
                await SetClipboardTextAsync(tabData);
            }
        }
    }
}