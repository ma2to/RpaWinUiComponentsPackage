// Utilities/ExcelClipboardHelper.cs - ✅ INTERNAL
namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre Excel clipboard operácie - ✅ INTERNAL
    /// </summary>
    internal static class ExcelClipboardHelper  // ✅ CHANGED: public -> internal
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

        // ... rest of methods stay the same but class is internal ...

        private static string EscapeExcelCell(string cell)
        {
            if (string.IsNullOrEmpty(cell))
                return string.Empty;

            if (cell.Contains('\t') || cell.Contains('\n') || cell.Contains('\r') || cell.Contains('"'))
            {
                var escaped = cell.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }

            return cell;
        }
    }
}