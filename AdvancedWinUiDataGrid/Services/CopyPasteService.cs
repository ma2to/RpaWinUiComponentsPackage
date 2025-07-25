// Services/CopyPasteService.cs - ✅ OPRAVENÝ accessibility
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ OPRAVENÉ CS0051: INTERNAL CopyPasteService
    /// </summary>
    internal class CopyPasteService : ICopyPasteService
    {
        private readonly ILogger<CopyPasteService> _logger;
        private bool _isInitialized = false;

        public CopyPasteService(ILogger<CopyPasteService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inicializuje copy/paste službu
        /// </summary>
        public Task InitializeAsync()
        {
            _logger.LogInformation("CopyPasteService inicializovaný");
            _isInitialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Skopíruje označené bunky do clipboardu (Excel formát)
        /// </summary>
        public async Task CopySelectedCellsAsync(List<CellSelection> selectedCells)
        {
            try
            {
                if (!_isInitialized || selectedCells == null || !selectedCells.Any())
                {
                    _logger.LogWarning("Pokus o kopírovanie prázdneho výberu buniek");
                    return;
                }

                _logger.LogInformation("Kopírujem {CellCount} buniek", selectedCells.Count);

                // Zoradi bunky podľa pozície (riadok, stĺpec)
                var sortedCells = selectedCells
                    .OrderBy(c => c.RowIndex)
                    .ThenBy(c => c.ColumnIndex)
                    .ToList();

                // Vytvor 2D štruktúru dát
                var dataStructure = CreateDataStructureFromSelection(sortedCells);

                // Konvertuj na Excel formát
                var excelData = ExcelClipboardHelper.ConvertToExcelFormat(dataStructure);

                // Skopíruj do clipboardu
                await ExcelClipboardHelper.CopyToClipboardAsync(excelData);

                _logger.LogInformation("Bunky úspešne skopírované do clipboardu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri kopírovaní buniek");
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
    /// ✅ OPRAVENÉ CS0051: INTERNAL CellSelection class
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
}