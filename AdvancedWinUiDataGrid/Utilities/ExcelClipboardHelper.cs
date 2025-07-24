// Utilities/ExcelClipboardHelper.cs
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre Excel clipboard operácie
    /// </summary>
    public static class ExcelClipboardHelper
    {
        private const string TabSeparator = "\t";
        private const string LineSeparator = "\r\n";

        /// <summary>
        /// Konvertuje 2D array dát na Excel TSV formát
        /// </summary>
        public static string ConvertToExcelFormat(object?[,] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var rows = data.GetLength(0);
            var cols = data.GetLength(1);
            var lines = new List<string>();

            for (int row = 0; row < rows; row++)
            {
                var cells = new List<string>();
                for (int col = 0; col < cols; col++)
                {
                    var value = data[row, col];
                    cells.Add(EscapeExcelCell(value?.ToString() ?? string.Empty));
                }
                lines.Add(string.Join(TabSeparator, cells));
            }

            return string.Join(LineSeparator, lines);
        }

        /// <summary>
        /// Konvertuje List<List<object>> na Excel formát
        /// </summary>
        public static string ConvertToExcelFormat(List<List<object?>> data)
        {
            if (data == null || !data.Any())
                return string.Empty;

            var lines = new List<string>();

            foreach (var row in data)
            {
                var cells = row.Select(cell => EscapeExcelCell(cell?.ToString() ?? string.Empty));
                lines.Add(string.Join(TabSeparator, cells));
            }

            return string.Join(LineSeparator, lines);
        }

        /// <summary>
        /// Parsuje Excel TSV formát na 2D array
        /// </summary>
        public static object?[,] ParseExcelFormat(string excelData)
        {
            if (string.IsNullOrEmpty(excelData))
                return new object[0, 0];

            var lines = excelData.Split(new[] { LineSeparator, "\n" }, StringSplitOptions.None);
            if (!lines.Any())
                return new object[0, 0];

            // Určí maximálny počet stĺpcov
            var maxCols = lines.Max(line => line.Split('\t').Length);
            var rows = lines.Length;

            var result = new object?[rows, maxCols];

            for (int row = 0; row < rows; row++)
            {
                var cells = lines[row].Split('\t');
                for (int col = 0; col < maxCols; col++)
                {
                    if (col < cells.Length)
                    {
                        result[row, col] = UnescapeExcelCell(cells[col]);
                    }
                    else
                    {
                        result[row, col] = string.Empty;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parsuje Excel formát na List<List<object>>
        /// </summary>
        public static List<List<object?>> ParseExcelFormatToList(string excelData)
        {
            var result = new List<List<object?>>();

            if (string.IsNullOrEmpty(excelData))
                return result;

            var lines = excelData.Split(new[] { LineSeparator, "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var cells = line.Split('\t').Select(cell => (object?)UnescapeExcelCell(cell)).ToList();
                result.Add(cells);
            }

            return result;
        }

        /// <summary>
        /// Skopíruje dáta do Windows clipboardu
        /// </summary>
        public static async Task CopyToClipboardAsync(string excelData)
        {
            try
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(excelData);

                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                // Clipboard operations can fail, ignore silently
            }
        }

        /// <summary>
        /// Získa dáta z Windows clipboardu
        /// </summary>
        public static async Task<string> GetFromClipboardAsync()
        {
            try
            {
                var clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();

                if (clipboardContent.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
                {
                    return await clipboardContent.GetTextAsync();
                }
            }
            catch (Exception)
            {
                // Clipboard operations can fail, return empty
            }

            return string.Empty;
        }

        /// <summary>
        /// Kontroluje či je v clipboarde Excel-kompatibilný obsah
        /// </summary>
        public static async Task<bool> HasExcelDataInClipboardAsync()
        {
            try
            {
                var clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();

                if (clipboardContent.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
                {
                    var text = await clipboardContent.GetTextAsync();
                    return !string.IsNullOrEmpty(text) && (text.Contains('\t') || text.Contains('\n'));
                }
            }
            catch (Exception)
            {
                // Clipboard operations can fail
            }

            return false;
        }

        /// <summary>
        /// Získa náhľad dát z clipboardu (prvých 3 riadkov)
        /// </summary>
        public static async Task<string> GetClipboardPreviewAsync(int maxRows = 3)
        {
            try
            {
                var clipboardData = await GetFromClipboardAsync();
                if (string.IsNullOrEmpty(clipboardData))
                    return "Clipboard je prázdny";

                var lines = clipboardData.Split(new[] { LineSeparator, "\n" }, StringSplitOptions.None);
                var previewLines = lines.Take(maxRows);

                var preview = string.Join(Environment.NewLine, previewLines);
                if (lines.Length > maxRows)
                {
                    preview += $"{Environment.NewLine}... (celkom {lines.Length} riadkov)";
                }

                return preview;
            }
            catch (Exception ex)
            {
                return $"Chyba pri čítaní clipboardu: {ex.Message}";
            }
        }

        #region Private Helper Methods

        private static string EscapeExcelCell(string cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            // Ak obsahuje tab alebo newline, obal do úvodzoviek
            if (cell.Contains('\t') || cell.Contains('\n') || cell.Contains('\r') || cell.Contains('"'))
            {
                // Zdvojnásob úvodzovky
                var escaped = cell.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }

            return cell;
        }

        private static string UnescapeExcelCell(string cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            // Ak je obalené úvodzovkami, odstráň ich a un-escape vnútorné úvodzovky
            if (cell.StartsWith("\"") && cell.EndsWith("\"") && cell.Length >= 2)
            {
                var unquoted = cell.Substring(1, cell.Length - 2);
                return unquoted.Replace("\"\"", "\"");
            }

            return cell;
        }

        #endregion
    }
}