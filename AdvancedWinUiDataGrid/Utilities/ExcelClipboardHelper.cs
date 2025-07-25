// Utilities/ExcelClipboardHelper.cs - ✅ KOMPLETNÝ s všetkými metódami
using Windows.ApplicationModel.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre Excel clipboard operácie - ✅ INTERNAL
    /// </summary>
    internal static class ExcelClipboardHelper
    {
        private const string TabSeparator = "\t";
        private const string LineSeparator = "\r\n";

        #region ✅ NOVÉ: Export/Convert metódy

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

        #endregion

        #region ✅ NOVÉ: Clipboard operácie

        /// <summary>
        /// Skopíruje Excel formátované dáta do clipboardu
        /// </summary>
        public static async Task CopyToClipboardAsync(string excelFormatData)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(excelFormatData);

                // Nastav špecifické formáty pre Excel
                dataPackage.Properties.ApplicationName = "Advanced DataGrid";
                dataPackage.Properties.Description = "DataGrid Excel Data";

                Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri kopírovaní do clipboardu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Získa dáta z clipboardu ako string
        /// </summary>
        public static async Task<string> GetFromClipboardAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();

                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return await dataPackageView.GetTextAsync();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri čítaní z clipboardu: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Kontroluje či clipboard obsahuje Excel dáta
        /// </summary>
        public static async Task<bool> HasExcelDataInClipboardAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();

                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    var text = await dataPackageView.GetTextAsync();

                    // Kontroluj či text obsahuje tabuľkové dáta (tab-separated values)
                    return !string.IsNullOrWhiteSpace(text) &&
                           (text.Contains('\t') || text.Contains('\n') || text.Contains('\r'));
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri kontrole clipboardu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Získa náhľad dát z clipboardu (prvých 100 znakov)
        /// </summary>
        public static async Task<string> GetClipboardPreviewAsync()
        {
            try
            {
                var data = await GetFromClipboardAsync();

                if (string.IsNullOrWhiteSpace(data))
                    return "Clipboard je prázdny";

                // Vráť prvých 100 znakov s info o veľkosti
                var preview = data.Length > 100 ? data.Substring(0, 100) + "..." : data;
                var lineCount = data.Split('\n').Length;
                var tabCount = data.Count(c => c == '\t');

                return $"Clipboard: {data.Length} znakov, {lineCount} riadkov, {tabCount} tabulátorov\n\nNáhľad:\n{preview}";
            }
            catch (Exception ex)
            {
                return $"Chyba pri náhľade: {ex.Message}";
            }
        }

        #endregion

        #region ✅ NOVÉ: Parse metódy

        /// <summary>
        /// Parsuje Excel formát na List<List<object?>>
        /// </summary>
        public static List<List<object?>> ParseExcelFormatToList(string excelData)
        {
            var result = new List<List<object?>>();

            if (string.IsNullOrWhiteSpace(excelData))
                return result;

            try
            {
                // Rozdel na riadky
                var lines = excelData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Rozdel na bunky podľa tabov
                    var cells = line.Split('\t');
                    var rowData = new List<object?>();

                    foreach (var cell in cells)
                    {
                        var unescapedCell = UnescapeExcelCell(cell);
                        rowData.Add(ConvertToAppropriateType(unescapedCell));
                    }

                    result.Add(rowData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri parsovaní Excel dát: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Parsuje Excel formát na 2D array
        /// </summary>
        public static object?[,] ParseExcelFormatToArray(string excelData)
        {
            var listData = ParseExcelFormatToList(excelData);

            if (!listData.Any())
                return new object[0, 0];

            var rows = listData.Count;
            var cols = listData.Max(row => row.Count);
            var result = new object?[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                var rowData = listData[row];
                for (int col = 0; col < cols; col++)
                {
                    result[row, col] = col < rowData.Count ? rowData[col] : null;
                }
            }

            return result;
        }

        #endregion

        #region ✅ OPRAVENÉ: Helper metódy

        /// <summary>
        /// Escapuje bunku pre Excel TSV formát
        /// </summary>
        private static string EscapeExcelCell(string cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            // Ak obsahuje tab, newline alebo úvodzovky, obal do úvodzoviek
            if (cell.Contains('\t') || cell.Contains('\n') || cell.Contains('\r') || cell.Contains('"'))
            {
                var escaped = cell.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }

            return cell;
        }

        /// <summary>
        /// Unescapuje bunku z Excel TSV formátu
        /// </summary>
        private static string UnescapeExcelCell(string cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            // Ak začína a končí úvodzovkami, odstráň ich
            if (cell.StartsWith("\"") && cell.EndsWith("\"") && cell.Length >= 2)
            {
                var unquoted = cell.Substring(1, cell.Length - 2);
                return unquoted.Replace("\"\"", "\"");
            }

            return cell;
        }

        /// <summary>
        /// Pokúsi sa skonvertovať string na vhodný typ
        /// </summary>
        private static object? ConvertToAppropriateType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Pokús sa skonvertovať na čísla
            if (int.TryParse(value, out var intValue))
                return intValue;

            if (decimal.TryParse(value, out var decimalValue))
                return decimalValue;

            if (DateTime.TryParse(value, out var dateValue))
                return dateValue;

            if (bool.TryParse(value, out var boolValue))
                return boolValue;

            // Vráť ako string
            return value;
        }

        #endregion

        #region ✅ NOVÉ: Validation metódy

        /// <summary>
        /// Kontroluje či je text validný Excel TSV formát
        /// </summary>
        public static bool IsValidExcelFormat(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            try
            {
                var parsed = ParseExcelFormatToList(data);
                return parsed.Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Získa info o Excel dátach
        /// </summary>
        public static ExcelDataInfo GetExcelDataInfo(string excelData)
        {
            var info = new ExcelDataInfo();

            if (string.IsNullOrWhiteSpace(excelData))
                return info;

            try
            {
                var lines = excelData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                info.RowCount = lines.Length;

                if (lines.Length > 0)
                {
                    var firstLineColumns = lines[0].Split('\t').Length;
                    info.ColumnCount = firstLineColumns;
                }

                info.TotalCells = info.RowCount * info.ColumnCount;
                info.EstimatedSizeBytes = Encoding.UTF8.GetByteCount(excelData);
            }
            catch (Exception ex)
            {
                info.HasError = true;
                info.ErrorMessage = ex.Message;
            }

            return info;
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÁ: Informácie o Excel dátach
    /// </summary>
    internal class ExcelDataInfo
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int TotalCells { get; set; }
        public long EstimatedSizeBytes { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public override string ToString()
        {
            return HasError
                ? $"Error: {ErrorMessage}"
                : $"{RowCount}x{ColumnCount} ({TotalCells} cells, {EstimatedSizeBytes} bytes)";
        }
    }
}