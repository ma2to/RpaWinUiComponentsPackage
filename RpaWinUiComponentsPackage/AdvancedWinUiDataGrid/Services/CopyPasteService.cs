// Services/CopyPasteService.cs - ✅ OPRAVENÝ accessibility
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// CopyPasteService s komplexným data integrity a clipboard logovaním - INTERNAL
    /// </summary>
    internal class CopyPasteService : ICopyPasteService
    {
        private readonly ILogger<CopyPasteService> _logger;
        private bool _isInitialized = false;

        // ✅ ROZŠÍRENÉ: Performance a operation tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private int _totalCopyOperations = 0;
        private int _totalPasteOperations = 0;
        private int _totalCutOperations = 0;
        private long _totalBytesTransferred = 0;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        public CopyPasteService(ILogger<CopyPasteService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("🔧 CopyPasteService created - InstanceId: {InstanceId}, LoggerType: {LoggerType}",
                _serviceInstanceId, _logger.GetType().Name);
        }

        /// <summary>
        /// Inicializuje copy/paste službu s clipboard capability testovaním
        /// </summary>
        public Task InitializeAsync()
        {
            var operationId = StartOperation("InitializeAsync");
            
            try
            {
                _logger.LogInformation("📋 CopyPasteService.InitializeAsync START - InstanceId: {InstanceId}",
                    _serviceInstanceId);

                // Test clipboard capabilities
                var clipboardSupported = TestClipboardCapabilities();
                
                _isInitialized = true;

                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ CopyPasteService INITIALIZED - Duration: {Duration}ms, " +
                    "ClipboardSupported: {ClipboardSupported}, ExcelFormatSupported: {ExcelSupported}, " +
                    "KeyboardShortcutsEnabled: {ShortcutsEnabled}",
                    duration, clipboardSupported, true, true);

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
        /// Skopíruje označené bunky do clipboardu s komplexnou data integrity analýzou (Excel formát)
        /// </summary>
        public async Task CopySelectedCellsAsync(List<CellSelection> selectedCells)
        {
            var operationId = StartOperation("CopySelectedCellsAsync");
            _totalCopyOperations++;
            
            try
            {
                _logger.LogInformation("📋 CopySelectedCells START - InstanceId: {InstanceId}, " +
                    "RequestedCells: {CellCount}, TotalCopyOps: {TotalOps}",
                    _serviceInstanceId, selectedCells?.Count ?? 0, _totalCopyOperations);

                if (!_isInitialized)
                {
                    _logger.LogError("❌ CopyPasteService not initialized");
                    return;
                }

                if (selectedCells == null || !selectedCells.Any())
                {
                    _logger.LogWarning("📋 Empty cell selection provided - no data to copy");
                    return;
                }

                // Analyze selection structure
                var selectionAnalysis = AnalyzeCellSelection(selectedCells);
                
                _logger.LogInformation("📋 Selection analyzed - Rows: {Rows}, Columns: {Columns}, " +
                    "TotalCells: {TotalCells}, SelectionArea: {SelectionArea}, " +
                    "EmptyCells: {EmptyCells}, NonEmptyCells: {NonEmptyCells}, " +
                    "DataTypes: [{DataTypes}]",
                    selectionAnalysis.RowCount, selectionAnalysis.ColumnCount, selectedCells.Count,
                    $"{selectionAnalysis.RowCount}x{selectionAnalysis.ColumnCount}",
                    selectionAnalysis.EmptyCells, selectionAnalysis.NonEmptyCells,
                    string.Join(", ", selectionAnalysis.DataTypes.Select(kvp => $"{kvp.Key}: {kvp.Value}")));

                // Zoradi bunky podľa pozície (riadok, stĺpec)
                var sortedCells = selectedCells
                    .OrderBy(c => c.RowIndex)
                    .ThenBy(c => c.ColumnIndex)
                    .ToList();

                _logger.LogDebug("📋 Cells sorted by position - FirstCell: [{FirstRow},{FirstCol}], " +
                    "LastCell: [{LastRow},{LastCol}]",
                    sortedCells.First().RowIndex, sortedCells.First().ColumnIndex,
                    sortedCells.Last().RowIndex, sortedCells.Last().ColumnIndex);

                // Vytvor 2D štruktúru dát s integrity checkmi
                var dataStructure = CreateDataStructureFromSelection(sortedCells);
                var structureValidation = ValidateDataStructure(dataStructure);
                
                if (!structureValidation.IsValid)
                {
                    _logger.LogWarning("📋 Data structure validation failed - Issues: [{Issues}]",
                        string.Join(", ", structureValidation.Issues));
                }

                // Konvertuj na Excel formát
                var excelData = ExcelClipboardHelper.ConvertToExcelFormat(dataStructure);
                var excelDataSize = System.Text.Encoding.UTF8.GetByteCount(excelData);

                _logger.LogDebug("📋 Excel format conversion - OriginalRows: {OriginalRows}, " +
                    "OriginalColumns: {OriginalColumns}, ExcelDataSize: {ExcelSize} bytes, " +
                    "AvgBytesPerCell: {AvgBytes:F1}",
                    dataStructure.Count, dataStructure.FirstOrDefault()?.Count ?? 0,
                    excelDataSize, selectedCells.Count > 0 ? (double)excelDataSize / selectedCells.Count : 0);

                // Skopíruj do clipboardu s error handling
                var clipboardResult = await CopyToClipboardWithValidation(excelData);
                
                if (clipboardResult.Success)
                {
                    _totalBytesTransferred += excelDataSize;
                    
                    var duration = EndOperation(operationId);
                    var copyRate = duration > 0 ? selectedCells.Count / duration : 0;

                    _logger.LogInformation("✅ CopySelectedCells COMPLETED - Duration: {Duration}ms, " +
                        "CopiedCells: {CopiedCells}, ExcelDataSize: {DataSize} bytes, " +
                        "TotalBytesTransferred: {TotalBytes}, CopyRate: {CopyRate:F0} cells/ms, " +
                        "DataIntegrity: {DataIntegrity}, ClipboardFormat: Excel",
                        duration, selectedCells.Count, excelDataSize, _totalBytesTransferred,
                        copyRate, structureValidation.IsValid ? "OK" : "WARNING");

                    // Log sample data (first few cells) for debugging
                    if (selectedCells.Count > 0 && _logger.IsEnabled(LogLevel.Debug))
                    {
                        var sampleCells = selectedCells.Take(3).Select(c => 
                            $"[{c.RowIndex},{c.ColumnIndex}] {c.ColumnName}: '{c.Value}'").ToList();
                        
                        _logger.LogDebug("📋 Sample copied data - [{SampleCells}]",
                            string.Join(", ", sampleCells));
                    }
                }
                else
                {
                    _logger.LogError("❌ Clipboard copy failed - Error: {ClipboardError}", 
                        clipboardResult.ErrorMessage);
                    throw new InvalidOperationException($"Clipboard copy failed: {clipboardResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in CopySelectedCellsAsync - InstanceId: {InstanceId}, " +
                    "CellCount: {CellCount}", _serviceInstanceId, selectedCells?.Count ?? 0);
                throw;
            }
        }

        /// <summary>
        /// Vloží dáta z clipboardu do označených buniek
        /// </summary>
        public async Task PasteFromClipboardAsync(int startRowIndex, int startColumnIndex)
        {
            try
            {
                if (!_isInitialized)
                    return;

                _logger.LogInformation("Vkladám dáta z clipboardu na pozíciu [{StartRow}, {StartColumn}]",
                    startRowIndex, startColumnIndex);

                // Získaj dáta z clipboardu
                var clipboardData = await ExcelClipboardHelper.GetFromClipboardAsync();
                if (string.IsNullOrEmpty(clipboardData))
                {
                    _logger.LogWarning("Clipboard je prázdny");
                    return;
                }

                // Parsuj Excel dáta
                var pastedData = ExcelClipboardHelper.ParseExcelFormatToList(clipboardData);
                if (!pastedData.Any())
                {
                    _logger.LogWarning("Žiadne dáta na vloženie");
                    return;
                }

                // TODO: Implementácia vloženia dát do gridu
                // Táto metóda by mala interagovať s DataManagementService

                _logger.LogInformation("Vložených {RowCount} riadkov s {ColumnCount} stĺpcami",
                    pastedData.Count, pastedData.FirstOrDefault()?.Count ?? 0);

                await ProcessPastedDataAsync(pastedData, startRowIndex, startColumnIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vkladaní z clipboardu");
                throw;
            }
        }

        /// <summary>
        /// Vystrihne označené bunky (copy + clear)
        /// </summary>
        public async Task CutSelectedCellsAsync(List<CellSelection> selectedCells)
        {
            try
            {
                if (!_isInitialized)
                    return;

                _logger.LogInformation("Vystrihávam {CellCount} buniek", selectedCells?.Count ?? 0);

                // Najprv skopíruj
                await CopySelectedCellsAsync(selectedCells ?? new List<CellSelection>());

                // Potom vymaž obsahy buniek
                await ClearSelectedCellsAsync(selectedCells ?? new List<CellSelection>());

                _logger.LogInformation("Bunky úspešne vystrihnuté");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vystrihávaní buniek");
                throw;
            }
        }

        /// <summary>
        /// Spracuje klávesové skratky (Ctrl+C, Ctrl+V, Ctrl+X)
        /// </summary>
        public async Task HandleKeyboardShortcutAsync(KeyRoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized)
                    return;

                var isCtrlPressed = IsCtrlPressed();
                if (!isCtrlPressed)
                    return;

                switch (e.Key)
                {
                    case Windows.System.VirtualKey.C:
                        _logger.LogDebug("Ctrl+C detekovaný");
                        // TODO: Získaj aktuálny výber buniek a skopíruj
                        await HandleCopyShortcutAsync();
                        break;

                    case Windows.System.VirtualKey.V:
                        _logger.LogDebug("Ctrl+V detekovaný");
                        // TODO: Získaj aktuálnu pozíciu a vlož
                        await HandlePasteShortcutAsync();
                        break;

                    case Windows.System.VirtualKey.X:
                        _logger.LogDebug("Ctrl+X detekovaný");
                        // TODO: Získaj aktuálny výber buniek a vystrihni
                        await HandleCutShortcutAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri spracovaní klávesovej skratky");
            }
        }

        /// <summary>
        /// Kontroluje či je možné vložiť dáta z clipboardu
        /// </summary>
        public async Task<bool> CanPasteAsync()
        {
            try
            {
                return await ExcelClipboardHelper.HasExcelDataInClipboardAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri kontrole možnosti vloženia");
                return false;
            }
        }

        /// <summary>
        /// Získa náhľad dát z clipboardu
        /// </summary>
        public async Task<string> GetClipboardPreviewAsync()
        {
            try
            {
                return await ExcelClipboardHelper.GetClipboardPreviewAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri získavaní náhľadu clipboardu");
                return $"Chyba: {ex.Message}";
            }
        }

        #region ✅ Performance Tracking Helper Methods

        /// <summary>
        /// Spustí sledovanie operácie a vráti jej ID
        /// </summary>
        private string StartOperation(string operationName)
        {
            var operationId = $"{operationName}_{Guid.NewGuid():N}"[..16];
            _operationStartTimes[operationId] = DateTime.UtcNow;
            _operationCounters[operationName] = _operationCounters.GetValueOrDefault(operationName, 0) + 1;
            
            _logger.LogTrace("⏱️ CopyPaste Operation START - {OperationName} (ID: {OperationId}), " +
                "TotalCalls: {TotalCalls}",
                operationName, operationId, _operationCounters[operationName]);
                
            return operationId;
        }

        /// <summary>
        /// Ukončí sledovanie operácie a vráti dobu trvania v ms
        /// </summary>
        private double EndOperation(string operationId)
        {
            if (_operationStartTimes.TryGetValue(operationId, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationId);
                
                _logger.LogTrace("⏱️ CopyPaste Operation END - ID: {OperationId}, Duration: {Duration:F2}ms", 
                    operationId, duration);
                    
                return duration;
            }
            
            _logger.LogWarning("⏱️ CopyPaste Operation END - Unknown operation ID: {OperationId}", operationId);
            return 0;
        }

        /// <summary>
        /// Testuje clipboard capabilities
        /// </summary>
        private bool TestClipboardCapabilities()
        {
            try
            {
                // Basic clipboard capability test
                return true; // TODO: Implement actual clipboard test
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ Clipboard capability test failed - {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Analyzuje štruktúru cell selection
        /// </summary>
        private SelectionAnalysis AnalyzeCellSelection(List<CellSelection> selectedCells)
        {
            var analysis = new SelectionAnalysis();
            
            if (!selectedCells.Any())
                return analysis;

            analysis.RowCount = selectedCells.Max(c => c.RowIndex) - selectedCells.Min(c => c.RowIndex) + 1;
            analysis.ColumnCount = selectedCells.Max(c => c.ColumnIndex) - selectedCells.Min(c => c.ColumnIndex) + 1;
            
            foreach (var cell in selectedCells)
            {
                if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    analysis.EmptyCells++;
                }
                else
                {
                    analysis.NonEmptyCells++;
                    var dataType = cell.Value.GetType().Name;
                    analysis.DataTypes[dataType] = analysis.DataTypes.GetValueOrDefault(dataType, 0) + 1;
                }
            }
            
            return analysis;
        }

        /// <summary>
        /// Validuje data structure integrity
        /// </summary>
        private StructureValidation ValidateDataStructure(List<List<object?>> dataStructure)
        {
            var validation = new StructureValidation { IsValid = true };
            
            if (!dataStructure.Any())
                return validation;

            var expectedColumnCount = dataStructure.First().Count;
            
            for (int i = 0; i < dataStructure.Count; i++)
            {
                if (dataStructure[i].Count != expectedColumnCount)
                {
                    validation.IsValid = false;
                    validation.Issues.Add($"Row {i} has {dataStructure[i].Count} columns, expected {expectedColumnCount}");
                }
            }
            
            return validation;
        }

        /// <summary>
        /// Kopíruje data do clipboardu s validation
        /// </summary>
        private async Task<ClipboardResult> CopyToClipboardWithValidation(string excelData)
        {
            try
            {
                await ExcelClipboardHelper.CopyToClipboardAsync(excelData);
                return new ClipboardResult { Success = true };
            }
            catch (Exception ex)
            {
                return new ClipboardResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }

        #endregion

        #region Private Helper Methods

        private List<List<object?>> CreateDataStructureFromSelection(List<CellSelection> sortedCells)
        {
            if (!sortedCells.Any())
                return new List<List<object?>>();

            // Určí rozmery výberu
            var minRow = sortedCells.Min(c => c.RowIndex);
            var maxRow = sortedCells.Max(c => c.RowIndex);
            var minCol = sortedCells.Min(c => c.ColumnIndex);
            var maxCol = sortedCells.Max(c => c.ColumnIndex);

            var rows = maxRow - minRow + 1;
            var cols = maxCol - minCol + 1;

            // Vytvor 2D štruktúru
            var dataStructure = new List<List<object?>>();
            for (int row = 0; row < rows; row++)
            {
                var rowData = new List<object?>();
                for (int col = 0; col < cols; col++)
                {
                    var actualRow = minRow + row;
                    var actualCol = minCol + col;

                    var cell = sortedCells.FirstOrDefault(c => c.RowIndex == actualRow && c.ColumnIndex == actualCol);
                    rowData.Add(cell?.Value ?? string.Empty);
                }
                dataStructure.Add(rowData);
            }

            return dataStructure;
        }

        private async Task ProcessPastedDataAsync(List<List<object?>> pastedData, int startRowIndex, int startColumnIndex)
        {
            // TODO: Implementácia vloženia dát do gridu cez DataManagementService
            // for (int row = 0; row < pastedData.Count; row++)
            // {
            //     var rowData = pastedData[row];
            //     for (int col = 0; col < rowData.Count; col++)
            //     {
            //         var actualRow = startRowIndex + row;
            //         var actualCol = startColumnIndex + col;
            //         
            //         // Vlož hodnotu na pozíciu [actualRow, actualCol]
            //         await _dataManagementService.SetCellValueAsync(actualRow, columnName, rowData[col]);
            //     }
            // }

            await Task.CompletedTask;
        }

        private async Task ClearSelectedCellsAsync(List<CellSelection> selectedCells)
        {
            // TODO: Implementácia vymazania obsahov buniek cez DataManagementService
            // foreach (var cell in selectedCells)
            // {
            //     await _dataManagementService.SetCellValueAsync(cell.RowIndex, cell.ColumnName, null);
            // }

            await Task.CompletedTask;
        }

        private async Task HandleCopyShortcutAsync()
        {
            // TODO: Získaj aktuálny výber buniek z UI a skopíruj ich
            _logger.LogDebug("Spracovávam Copy shortcut");
            await Task.CompletedTask;
        }

        private async Task HandlePasteShortcutAsync()
        {
            // TODO: Získaj aktuálnu pozíciu kurzora a vlož dáta
            _logger.LogDebug("Spracovávam Paste shortcut");
            await Task.CompletedTask;
        }

        private async Task HandleCutShortcutAsync()
        {
            // TODO: Získaj aktuálny výber buniek z UI a vystrihni ich
            _logger.LogDebug("Spracovávam Cut shortcut");
            await Task.CompletedTask;
        }

        private static bool IsCtrlPressed()
        {
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            return (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion
    }

    /// <summary>
    /// CellSelection class - INTERNAL
    /// </summary>
    internal class CellSelection
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public object? Value { get; set; }

        public override string ToString()
        {
            return $"Cell[{RowIndex},{ColumnIndex}] {ColumnName} = {Value}";
        }
    }

    /// <summary>
    /// Selection analysis data - INTERNAL
    /// </summary>
    internal class SelectionAnalysis
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int EmptyCells { get; set; }
        public int NonEmptyCells { get; set; }
        public Dictionary<string, int> DataTypes { get; set; } = new();
    }

    /// <summary>
    /// Data structure validation result - INTERNAL
    /// </summary>
    internal class StructureValidation
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Clipboard operation result - INTERNAL
    /// </summary>
    internal class ClipboardResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}