// Utilities/ExcelClipboardHelper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Utility trieda pre prácu s Excel formátovaným clipboard obsahom.
    /// Poskytuje konverzie medzi 2D dátami a TSV (Tab-Separated Values) formátom.
    /// </summary>
    internal static class ExcelClipboardHelper
    {
        #region Konštanty

        /// <summary>
        /// Oddeľovač stĺpcov v TSV formáte.
        /// </summary>
        public const string ColumnSeparator = "\t";

        /// <summary>
        /// Oddeľovač riadkov v TSV formáte.
        /// </summary>
        public const string RowSeparator = "\r\n";

        #endregion

        #region Export do TSV

        /// <summary>
        /// Konvertuje 2D pole dát na TSV string kompatibilný s Excel.
        /// </summary>
        /// <param name="data">2D pole hodnôt</param>
        /// <returns>TSV formátovaný string</returns>
        public static string ConvertToTsv(object?[,] data)
        {
            if (data == null)
                return string.Empty;

            var rows = data.GetLength(0);
            var cols = data.GetLength(1);

            if (rows == 0 || cols == 0)
                return string.Empty;

            var sb = new StringBuilder();

            for (int row = 0; row < rows; row++)
            {
                var rowValues = new List<string>();

                for (int col = 0; col < cols; col++)
                {
                    var cellValue = data[row, col];
                    var formattedValue = FormatCellForTsv(cellValue);
                    rowValues.Add(formattedValue);
                }

                sb.Append(string.Join(ColumnSeparator, rowValues));

                // Pridať row separator okrem posledného riadku
                if (row < rows - 1)
                {
                    sb.Append(RowSeparator);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Konvertuje List 2D štruktúru na TSV string.
        /// </summary>
        /// <param name="data">2D List dát</param>
        /// <returns>TSV formátovaný string</returns>
        public static string ConvertToTsv(IEnumerable<IEnumerable<object?>> data)
        {
            var dataArray = data.Select(row => row.ToArray()).ToArray();

            if (!dataArray.Any())
                return string.Empty;

            var maxColumns = dataArray.Max(row => row.Length);
            var result = new object?[dataArray.Length, maxColumns];

            for (int row = 0; row < dataArray.Length; row++)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    result[row, col] = col < dataArray[row].Length ? dataArray[row][col] : null;
                }
            }

            return ConvertToTsv(result);
        }

        #endregion

        #region Import z TSV

        /// <summary>
        /// Parsuje TSV string na 2D pole dát.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>2D pole hodnôt</returns>
        public static object?[,] ParseTsvData(string tsvData)
        {
            if (string.IsNullOrEmpty(tsvData))
                return new object?[0, 0];

            // Split riadky - handle rôzne line endings
            var lines = tsvData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            // Odstrániť prázdne riadky na konci
            while (lines.Length > 0 && string.IsNullOrEmpty(lines[^1]))
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            if (lines.Length == 0)
                return new object?[0, 0];

            // Určiť maximálny počet stĺpcov
            var maxColumns = lines.Max(line => line.Split('\t').Length);

            var result = new object?[lines.Length, maxColumns];

            for (int row = 0; row < lines.Length; row++)
            {
                var cells = lines[row].Split('\t');

                for (int col = 0; col < maxColumns; col++)
                {
                    if (col < cells.Length)
                    {
                        result[row, col] = ParseCellFromTsv(cells[col]);
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
        /// Parsuje TSV string na List 2D štruktúru.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>2D List dát</returns>
        public static List<List<object?>> ParseTsvDataToList(string tsvData)
        {
            var arrayData = ParseTsvData(tsvData);
            var result = new List<List<object?>>();

            var rows = arrayData.GetLength(0);
            var cols = arrayData.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                var rowList = new List<object?>();
                for (int col = 0; col < cols; col++)
                {
                    rowList.Add(arrayData[row, col]);
                }
                result.Add(rowList);
            }

            return result;
        }

        #endregion

        #region Rozmery a validácia

        /// <summary>
        /// Získa rozmery TSV dát.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>Tuple s počtom riadkov a stĺpcov</returns>
        public static (int rows, int columns) GetTsvDimensions(string tsvData)
        {
            if (string.IsNullOrEmpty(tsvData))
                return (0, 0);

            var lines = tsvData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return (0, 0);

            var rows = lines.Length;
            var columns = lines.Max(line => line.Split('\t').Length);

            return (rows, columns);
        }

        /// <summary>
        /// Validuje či je TSV string správne formátovaný.
        /// </summary>
        /// <param name="tsvData">TSV string na validáciu</param>
        /// <returns>True ak je TSV validný</returns>
        public static bool IsValidTsv(string tsvData)
        {
            if (string.IsNullOrEmpty(tsvData))
                return true; // Prázdny string je validný

            try
            {
                var dimensions = GetTsvDimensions(tsvData);
                return dimensions.rows > 0 && dimensions.columns > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Clipboard operácie

        /// <summary>
        /// Načíta TSV dáta z system clipboardu.
        /// </summary>
        /// <returns>TSV string alebo null ak clipboard neobsahuje text</returns>
        public static async Task<string?> GetClipboardTsvAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();

                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    var text = await dataPackageView.GetTextAsync();

                    // Skontrolovať či to vyzerá ako TSV (obsahuje taby)
                    if (!string.IsNullOrEmpty(text) && text.Contains('\t'))
                    {
                        return text;
                    }

                    // Ak neobsahuje taby, ale je to text, môže to byť single cell
                    return text;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri čítaní z clipboardu: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Nastaví TSV dáta do system clipboardu.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>Task pre asynchrónne nastavenie</returns>
        public static async Task SetClipboardTsvAsync(string tsvData)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(tsvData ?? string.Empty);

                Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri zápise do clipboardu: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Skontroluje či clipboard obsahuje Excel kompatibilné dáta.
        /// </summary>
        /// <returns>True ak clipboard obsahuje TSV alebo text dáta</returns>
        public static async Task<bool> HasClipboardDataAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                var hasData = dataPackageView.Contains(StandardDataFormats.Text);
                await Task.CompletedTask;
                return hasData;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Formátuje hodnotu bunky pre TSV export.
        /// </summary>
        /// <param name="cellValue">Hodnota bunky</param>
        /// <returns>Formátovaný string</returns>
        private static string FormatCellForTsv(object? cellValue)
        {
            if (cellValue == null)
                return string.Empty;

            var stringValue = cellValue.ToString() ?? string.Empty;

            // Escape špeciálne znaky
            stringValue = EscapeTsvValue(stringValue);

            return stringValue;
        }

        /// <summary>
        /// Parsuje hodnotu bunky z TSV importu.
        /// </summary>
        /// <param name="cellValue">String hodnota z TSV</param>
        /// <returns>Parsovaná hodnota</returns>
        private static object? ParseCellFromTsv(string cellValue)
        {
            if (string.IsNullOrEmpty(cellValue))
                return string.Empty;

            // Unescape TSV value
            return UnescapeTsvValue(cellValue);
        }

        /// <summary>
        /// Escape špeciálne znaky pre TSV formát.
        /// </summary>
        /// <param name="value">Pôvodná hodnota</param>
        /// <returns>Escaped hodnota</returns>
        private static string EscapeTsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Nahradiť tabs medzerami (tabs sú separátory)
            value = value.Replace("\t", "    ");

            // Nahradiť newlines medzerami (môžu narušiť štruktúru)
            value = value.Replace("\r\n", " ");
            value = value.Replace("\n", " ");
            value = value.Replace("\r", " ");

            return value;
        }

        /// <summary>
        /// Unescape TSV hodnotu.
        /// </summary>
        /// <param name="value">Escaped hodnota</param>
        /// <returns>Unescaped hodnota</returns>
        private static string UnescapeTsvValue(string value)
        {
            // V základnej implementácii nemáme špeciálne unescaping
            // Excel TSV formát je pomerne jednoduchý
            return value;
        }

        #endregion

        #region Utility metódy

        /// <summary>
        /// Kombinuje viacero TSV stringov do jedného.
        /// </summary>
        /// <param name="tsvDataList">Zoznam TSV stringov</param>
        /// <returns>Kombinovaný TSV string</returns>
        public static string CombineTsvData(IEnumerable<string> tsvDataList)
        {
            var validTsvData = tsvDataList.Where(tsv => !string.IsNullOrEmpty(tsv));
            return string.Join(RowSeparator, validTsvData);
        }

        /// <summary>
        /// Rozdelí TSV string na menšie bloky.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <param name="maxRowsPerBlock">Maximálny počet riadkov na blok</param>
        /// <returns>Zoznam TSV blokov</returns>
        public static List<string> SplitTsvData(string tsvData, int maxRowsPerBlock)
        {
            if (string.IsNullOrEmpty(tsvData) || maxRowsPerBlock <= 0)
                return new List<string> { tsvData };

            var lines = tsvData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var blocks = new List<string>();

            for (int i = 0; i < lines.Length; i += maxRowsPerBlock)
            {
                var blockLines = lines.Skip(i).Take(maxRowsPerBlock);
                var blockTsv = string.Join(RowSeparator, blockLines);
                blocks.Add(blockTsv);
            }

            return blocks;
        }

        /// <summary>
        /// Vytvorí prázdny TSV string s určenými rozmermi.
        /// </summary>
        /// <param name="rows">Počet riadkov</param>
        /// <param name="columns">Počet stĺpcov</param>
        /// <returns>Prázdny TSV string</returns>
        public static string CreateEmptyTsv(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
                return string.Empty;

            var emptyRow = string.Join(ColumnSeparator, Enumerable.Repeat(string.Empty, columns));
            var allRows = Enumerable.Repeat(emptyRow, rows);

            return string.Join(RowSeparator, allRows);
        }

        #endregion
    }
}