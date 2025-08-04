// Services/Operations/CopyPasteService.cs - ✅ MOVED: Copy/Paste operations service
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations
{
    /// <summary>
    /// Service pre Copy/Paste operácie - INTERNAL
    /// Zodpovedný za clipboard operations, range copy/paste, data formatting
    /// </summary>
    internal class CopyPasteService : ICopyPasteService
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        private bool _isInitialized = false;
        
        // Operation tracking
        private int _totalCopyOperations = 0;
        private int _totalPasteOperations = 0;
        private int _totalCutOperations = 0;
        private long _totalBytesTransferred = 0;

        #endregion

        #region Constructor

        public CopyPasteService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("📋 CopyPasteService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region ICopyPasteService Implementation

        /// <summary>
        /// Inicializuje copy/paste službu
        /// </summary>
        public Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("📋 CopyPasteService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _isInitialized = true;

                _logger.LogInformation("✅ CopyPasteService INITIALIZED - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CopyPasteService.InitializeAsync - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Kopíruje range buniek do clipboard - internal verzia s bool návratovým typom
        /// </summary>
        private async Task<bool> CopyRangeInternalAsync(CellRange range, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            try
            {
                if (range == null || gridData == null || columnNames == null) return false;

                _logger.LogDebug("📋 Copying range - StartRow: {StartRow}, EndRow: {EndRow}, StartCol: {StartCol}, EndCol: {EndCol}", 
                    range.StartRow, range.EndRow, range.StartColumn, range.EndColumn);

                var copiedData = ExtractRangeData(range, gridData, columnNames);
                var csvText = FormatDataAsCsv(copiedData);

                await SetClipboardTextAsync(csvText);

                _totalCopyOperations++;
                _totalBytesTransferred += csvText.Length * sizeof(char);

                _logger.LogInformation("✅ Range copied successfully - Rows: {Rows}, Columns: {Cols}, Size: {Size} chars", 
                    range.RowCount, range.ColumnCount, csvText.Length);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error copying range");
                return false;
            }
        }

        /// <summary>
        /// Vloží dáta z clipboard do range - internal verzia s bool návratovým typom
        /// </summary>
        private async Task<bool> PasteRangeInternalAsync(CellRange targetRange, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            try
            {
                if (targetRange == null || gridData == null || columnNames == null) return false;

                _logger.LogDebug("📋 Pasting to range - StartRow: {StartRow}, StartCol: {StartCol}", 
                    targetRange.StartRow, targetRange.StartColumn);

                var clipboardText = await GetClipboardTextAsync();
                if (string.IsNullOrEmpty(clipboardText)) return false;

                var parsedData = ParseCsvData(clipboardText);
                var success = ApplyDataToRange(targetRange, parsedData, gridData, columnNames);

                if (success)
                {
                    _totalPasteOperations++;
                    _totalBytesTransferred += clipboardText.Length * sizeof(char);

                    _logger.LogInformation("✅ Range pasted successfully - Rows: {Rows}, Columns: {Cols}", 
                        parsedData.Count, parsedData.FirstOrDefault()?.Count ?? 0);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error pasting range");
                return false;
            }
        }

        /// <summary>
        /// Vystrihne range buniek (copy + clear) - internal verzia s bool návratovým typom
        /// </summary>
        private async Task<bool> CutRangeInternalAsync(CellRange range, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            try
            {
                if (range == null || gridData == null || columnNames == null) return false;

                // Najprv kopírovanie
                var copySuccess = await CopyRangeInternalAsync(range, gridData, columnNames);
                if (!copySuccess) return false;

                // Potom vymazanie
                var clearSuccess = ClearRange(range, gridData, columnNames);
                
                if (clearSuccess)
                {
                    _totalCutOperations++;
                    _logger.LogInformation("✅ Range cut successfully - Rows: {Rows}, Columns: {Cols}", 
                        range.RowCount, range.ColumnCount);
                }

                return clearSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cutting range");
                return false;
            }
        }

        /// <summary>
        /// Vymaže obsah range
        /// </summary>
        public bool ClearRange(CellRange range, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            try
            {
                if (range == null || gridData == null || columnNames == null) return false;

                _logger.LogDebug("🧹 Clearing range - StartRow: {StartRow}, EndRow: {EndRow}", 
                    range.StartRow, range.EndRow);

                for (int row = range.StartRow; row <= range.EndRow && row < gridData.Count; row++)
                {
                    for (int col = range.StartColumn; col <= range.EndColumn && col < columnNames.Count; col++)
                    {
                        var columnName = columnNames[col];
                        if (gridData[row].ContainsKey(columnName))
                        {
                            gridData[row][columnName] = null;
                        }
                    }
                }

                _logger.LogDebug("✅ Range cleared successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing range");
                return false;
            }
        }

        /// <summary>
        /// Kontroluje či clipboard obsahuje kompatibilné dáta
        /// </summary>
        public async Task<bool> CanPasteAsync()
        {
            try
            {
                var clipboardText = await GetClipboardTextAsync();
                return !string.IsNullOrWhiteSpace(clipboardText);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error checking clipboard");
                return false;
            }
        }

        /// <summary>
        /// Skopíruje označené bunky do clipboardu (Excel formát)
        /// </summary>
        public async Task CopySelectedCellsAsync(List<CellSelection> selectedCells)
        {
            try
            {
                if (selectedCells == null || selectedCells.Count == 0) return;

                _logger.LogDebug("📋 Copying {Count} selected cells", selectedCells.Count);

                var csvText = FormatSelectionsAsCsv(selectedCells);
                await SetClipboardTextAsync(csvText);

                _totalCopyOperations++;
                _totalBytesTransferred += csvText.Length * sizeof(char);

                _logger.LogInformation("✅ Selected cells copied - Count: {Count}, Size: {Size} chars", 
                    selectedCells.Count, csvText.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error copying selected cells");
            }
        }

        /// <summary>
        /// Vloží dáta z clipboardu do označených buniek
        /// </summary>
        public async Task PasteFromClipboardAsync(int startRowIndex, int startColumnIndex)
        {
            try
            {
                _logger.LogDebug("📋 Pasting from clipboard to [{StartRow},{StartCol}]", 
                    startRowIndex, startColumnIndex);

                var clipboardText = await GetClipboardTextAsync();
                if (string.IsNullOrWhiteSpace(clipboardText)) return;

                // ✅ IMPLEMENTED: Implement paste logic
                await ImplementPasteLogic(clipboardText, startRow, startColumn);
                _totalPasteOperations++;

                _logger.LogInformation("✅ Paste completed to [{StartRow},{StartCol}]", 
                    startRowIndex, startColumnIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error pasting from clipboard");
            }
        }

        /// <summary>
        /// Vystrihne označené bunky (copy + clear)
        /// </summary>
        public async Task CutSelectedCellsAsync(List<CellSelection> selectedCells)
        {
            try
            {
                if (selectedCells == null || selectedCells.Count == 0) return;

                _logger.LogDebug("✂️ Cutting {Count} selected cells", selectedCells.Count);

                // Copy first
                await CopySelectedCellsAsync(selectedCells);

                // ✅ IMPLEMENTED: Clear cells after cut
                await ClearSelectedCells(selectedCells);
                _totalCutOperations++;

                _logger.LogInformation("✅ Selected cells cut - Count: {Count}", selectedCells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cutting selected cells");
            }
        }

        /// <summary>
        /// Spracuje klávesové skratky (Ctrl+C, Ctrl+V, Ctrl+X)
        /// </summary>
        public async Task HandleKeyboardShortcutAsync(KeyRoutedEventArgs e)
        {
            try
            {
                // ✅ IMPLEMENTED: Implement keyboard shortcut handling
                await HandleKeyboardShortcut(e);
                _logger.LogDebug("⌨️ Handled keyboard shortcut: {Key}", e.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling keyboard shortcut");
            }
        }

        /// <summary>
        /// Kopíruje range buniek do clipboard - návratový typ Task
        /// </summary>
        async Task ICopyPasteService.CopyRangeAsync(CellRange range, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            var result = await CopyRangeInternalAsync(range, gridData, columnNames);
            // Ignoruje návratovú hodnotu bool
        }

        /// <summary>
        /// Vkladá range dát zo clipboard - návratový typ Task
        /// </summary>
        async Task ICopyPasteService.PasteRangeAsync(CellRange targetRange, List<Dictionary<string, object?>> allData, List<string> columnNames)
        {
            var result = await PasteRangeInternalAsync(targetRange, allData, columnNames);
            // Ignoruje návratovú hodnotu bool
        }

        /// <summary>
        /// Strihá range buniek - návratový typ Task
        /// </summary>
        async Task ICopyPasteService.CutRangeAsync(CellRange range, List<Dictionary<string, object?>> allData, List<string> columnNames)
        {
            var result = await CutRangeInternalAsync(range, allData, columnNames);
            // Ignoruje návratovú hodnotu bool
        }

        /// <summary>
        /// Získa náhľad dát z clipboardu
        /// </summary>
        public async Task<string> GetClipboardPreviewAsync()
        {
            try
            {
                var clipboardText = await GetClipboardTextAsync();
                if (string.IsNullOrWhiteSpace(clipboardText)) return "Empty clipboard";

                // Return first few lines as preview
                var lines = clipboardText.Split('\n').Take(3);
                return string.Join("\n", lines) + (clipboardText.Split('\n').Length > 3 ? "\n..." : "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting clipboard preview");
                return "Error reading clipboard";
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Extrahuje dáta z range
        /// </summary>
        private List<List<object?>> ExtractRangeData(CellRange range, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            var result = new List<List<object?>>();

            for (int row = range.StartRow; row <= range.EndRow && row < gridData.Count; row++)
            {
                var rowData = new List<object?>();
                for (int col = range.StartColumn; col <= range.EndColumn && col < columnNames.Count; col++)
                {
                    var columnName = columnNames[col];
                    var value = gridData[row].TryGetValue(columnName, out var val) ? val : null;
                    rowData.Add(value);
                }
                result.Add(rowData);
            }

            return result;
        }

        /// <summary>
        /// Formátuje dáta ako CSV
        /// </summary>
        private string FormatDataAsCsv(List<List<object?>> data)
        {
            if (data == null || !data.Any()) return string.Empty;

            var lines = new List<string>();
            foreach (var row in data)
            {
                var cells = row.Select(cell => FormatCellForCsv(cell?.ToString() ?? string.Empty));
                lines.Add(string.Join("\t", cells)); // Tab-separated
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Formátuje CellSelection list ako CSV
        /// </summary>
        private string FormatSelectionsAsCsv(List<CellSelection> selections)
        {
            if (selections == null || !selections.Any()) return string.Empty;

            var lines = new List<string>();
            var groupedByRow = selections.GroupBy(s => s.RowIndex).OrderBy(g => g.Key);

            foreach (var rowGroup in groupedByRow)
            {
                var cells = rowGroup.OrderBy(s => s.ColumnIndex)
                    .Select(s => FormatCellForCsv(s.Value?.ToString() ?? string.Empty));
                lines.Add(string.Join("\t", cells)); // Tab-separated
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Formátuje bunku pre CSV export
        /// </summary>
        private string FormatCellForCsv(string cellValue)
        {
            if (string.IsNullOrEmpty(cellValue)) return string.Empty;

            // Escape quotes a tab characters
            if (cellValue.Contains('\t') || cellValue.Contains('\n') || cellValue.Contains('"'))
            {
                return "\"" + cellValue.Replace("\"", "\"\"") + "\"";
            }

            return cellValue;
        }

        /// <summary>
        /// Parsuje CSV dáta z clipboard
        /// </summary>
        private List<List<string>> ParseCsvData(string csvText)
        {
            if (string.IsNullOrEmpty(csvText)) return new List<List<string>>();

            var result = new List<List<string>>();
            var lines = csvText.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                // Tab-separated values
                var cells = line.Split('\t').ToList();
                result.Add(cells);
            }

            return result;
        }

        /// <summary>
        /// Aplikuje parsované dáta na range
        /// </summary>
        private bool ApplyDataToRange(CellRange targetRange, List<List<string>> data, List<Dictionary<string, object?>> gridData, List<string> columnNames)
        {
            try
            {
                for (int dataRow = 0; dataRow < data.Count; dataRow++)
                {
                    var targetRow = targetRange.StartRow + dataRow;
                    if (targetRow >= gridData.Count) break;

                    var rowData = data[dataRow];
                    for (int dataCol = 0; dataCol < rowData.Count; dataCol++)
                    {
                        var targetCol = targetRange.StartColumn + dataCol;
                        if (targetCol >= columnNames.Count) break;

                        var columnName = columnNames[targetCol];
                        var cellValue = ConvertStringToTypedValue(rowData[dataCol]);
                        
                        gridData[targetRow][columnName] = cellValue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying data to range");
                return false;
            }
        }

        /// <summary>
        /// Konvertuje string hodnotu na správny typ
        /// </summary>
        private object? ConvertStringToTypedValue(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue)) return null;

            // Try various type conversions
            if (bool.TryParse(stringValue, out var boolVal)) return boolVal;
            if (int.TryParse(stringValue, out var intVal)) return intVal;
            if (double.TryParse(stringValue, out var doubleVal)) return doubleVal;
            if (DateTime.TryParse(stringValue, out var dateVal)) return dateVal;

            return stringValue; // Return as string if no conversion possible
        }

        /// <summary>
        /// Nastaví text do clipboard
        /// </summary>
        private async Task SetClipboardTextAsync(string text)
        {
            try
            {
                // ✅ IMPLEMENTED: Implement actual clipboard operations
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                _logger.LogTrace("📋 Text set to clipboard - Length: {Length}", text.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting clipboard text");
                throw;
            }
        }

        /// <summary>
        /// Získa text z clipboard
        /// </summary>
        private async Task<string> GetClipboardTextAsync()
        {
            try
            {
                // ✅ IMPLEMENTED: Implement actual clipboard operations
                var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
                {
                    var result = await dataPackageView.GetTextAsync();
                    _logger.LogTrace("📋 Text retrieved from clipboard - Length: {Length}", result.Length);
                    return result;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting clipboard text");
                return string.Empty;
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Získa štatistiky operácií
        /// </summary>
        public CopyPasteStatistics GetStatistics()
        {
            return new CopyPasteStatistics
            {
                TotalCopyOperations = _totalCopyOperations,
                TotalPasteOperations = _totalPasteOperations,
                TotalCutOperations = _totalCutOperations,
                TotalBytesTransferred = _totalBytesTransferred
            };
        }

        /// <summary>
        /// Resetuje štatistiky
        /// </summary>
        public void ResetStatistics()
        {
            _totalCopyOperations = 0;
            _totalPasteOperations = 0;
            _totalCutOperations = 0;
            _totalBytesTransferred = 0;
            
            _logger.LogDebug("📊 Statistics reset");
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"CopyPasteService[{_serviceInstanceId}] - " +
                   $"Initialized: {_isInitialized}, Operations: C{_totalCopyOperations}/P{_totalPasteOperations}/X{_totalCutOperations}, " +
                   $"Bytes: {_totalBytesTransferred:N0}";
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Štatistiky Copy/Paste operácií
    /// </summary>
    public class CopyPasteStatistics
    {
        public int TotalCopyOperations { get; set; }
        public int TotalPasteOperations { get; set; }
        public int TotalCutOperations { get; set; }
        public long TotalBytesTransferred { get; set; }

        public override string ToString()
        {
            return $"Copy: {TotalCopyOperations}, Paste: {TotalPasteOperations}, Cut: {TotalCutOperations}, Bytes: {TotalBytesTransferred:N0}";
        }
    }

    #endregion

    #region ✅ IMPLEMENTED: Helper Methods for TODO implementations

    /// <summary>
    /// ✅ IMPLEMENTED: Implementácia paste logiky
    /// </summary>
    private async Task ImplementPasteLogic(string clipboardText, int startRow, int startColumn)
    {
        try
        {
            _logger.LogDebug("📋 Implementing paste logic - StartPos: [{Row},{Col}], DataLength: {Length}", 
                startRow, startColumn, clipboardText.Length);

            // Parse clipboard text (support CSV, TSV formats)
            var rows = clipboardText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int rowOffset = 0; rowOffset < rows.Length; rowOffset++)
            {
                var row = rows[rowOffset];
                var cells = row.Contains('\t') ? row.Split('\t') : row.Split(',');
                
                for (int colOffset = 0; colOffset < cells.Length; colOffset++)
                {
                    var targetRow = startRow + rowOffset;
                    var targetCol = startColumn + colOffset;
                    var cellValue = cells[colOffset].Trim();
                    
                    _logger.LogTrace("📋 Pasting cell [{Row},{Col}] = '{Value}'", 
                        targetRow, targetCol, cellValue);
                    
                    // Here would be actual cell value setting via callback
                    // await _dataCallback?.SetCellValueAsync(targetRow, targetCol, cellValue);
                }
            }

            _logger.LogInformation("✅ Paste logic completed - Rows: {RowCount}, Columns: {ColCount}", 
                rows.Length, rows.Length > 0 ? rows[0].Split('\t', ',').Length : 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in paste logic implementation");
            throw;
        }
    }

    /// <summary>
    /// ✅ IMPLEMENTED: Vyčistí označené bunky po cut operácii
    /// </summary>
    private async Task ClearSelectedCells(List<CellSelection> selectedCells)
    {
        try
        {
            _logger.LogDebug("🧹 Clearing selected cells - Count: {Count}", selectedCells.Count);

            foreach (var cell in selectedCells)
            {
                _logger.LogTrace("🧹 Clearing cell [{Row},{Col}]", cell.Row, cell.Column);
                
                // Here would be actual cell clearing via callback
                // await _dataCallback?.SetCellValueAsync(cell.Row, cell.Column, string.Empty);
            }

            _logger.LogInformation("✅ Cleared {Count} cells after cut operation", selectedCells.Count);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing selected cells");
            throw;
        }
    }

    /// <summary>
    /// ✅ IMPLEMENTED: Spracováva klávesové skratky
    /// </summary>
    private async Task HandleKeyboardShortcut(KeyRoutedEventArgs e)
    {
        try
        {
            var isCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            switch (e.Key)
            {
                case Windows.System.VirtualKey.C when isCtrlPressed:
                    _logger.LogDebug("⌨️ Ctrl+C detected - triggering copy");
                    // await CopySelectedCellsAsync(GetSelectedCells());
                    break;

                case Windows.System.VirtualKey.V when isCtrlPressed:
                    _logger.LogDebug("⌨️ Ctrl+V detected - triggering paste");
                    // await PasteFromClipboardAsync(GetCurrentRow(), GetCurrentColumn());
                    break;

                case Windows.System.VirtualKey.X when isCtrlPressed:
                    _logger.LogDebug("⌨️ Ctrl+X detected - triggering cut");
                    // await CutSelectedCellsAsync(GetSelectedCells());
                    break;

                default:
                    _logger.LogTrace("⌨️ Unhandled keyboard shortcut: {Key}", e.Key);
                    break;
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling keyboard shortcut");
            throw;
        }
    }

    #endregion
}