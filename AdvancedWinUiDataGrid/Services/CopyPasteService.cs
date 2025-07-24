// Services/CopyPasteService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Implementácia služby pre Copy/Paste/Cut operácie s Excel kompatibilitou.
    /// Podporuje TSV formát a viacbunkový výber.
    /// </summary>
    internal class CopyPasteService : ICopyPasteService
    {
        #region Private fields

        private GridConfiguration? _configuration;
        private IDataManagementService? _dataService;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // Event handling
        private readonly object _eventLock = new();

        #endregion

        #region Events

        public event EventHandler<CopyCompletedEventArgs>? CopyCompleted;
        public event EventHandler<PasteCompletedEventArgs>? PasteCompleted;
        public event EventHandler<CutCompletedEventArgs>? CutCompleted;

        #endregion

        #region ICopyPasteService - Inicializácia

        public async Task InitializeAsync(GridConfiguration configuration)
        {
            if (_isInitialized)
                throw new InvalidOperationException("CopyPasteService je už inicializovaný");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        #endregion

        #region ICopyPasteService - Copy operácie

        public async Task<int> CopySelectedCellsAsync(IEnumerable<CellSelection> selectedCells, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var selections = selectedCells.OrderBy(s => s.RowIndex).ThenBy(s => s.ColumnIndex).ToArray();
            if (!selections.Any()) return 0;

            // Zistiť rozmery výberu
            var minRow = selections.Min(s => s.RowIndex);
            var maxRow = selections.Max(s => s.RowIndex);
            var minCol = selections.Min(s => s.ColumnIndex);
            var maxCol = selections.Max(s => s.ColumnIndex);

            var rows = maxRow - minRow + 1;
            var cols = maxCol - minCol + 1;

            // Vytvoriť 2D pole dát
            var data = new object?[rows, cols];

            foreach (var selection in selections)
            {
                var cell = _dataService?.GetCellData(selection.RowIndex, selection.ColumnIndex);
                var relativeRow = selection.RowIndex - minRow;
                var relativeCol = selection.ColumnIndex - minCol;

                data[relativeRow, relativeCol] = cell?.DisplayValue ?? "";
            }

            // Konvertovať na TSV a skopírovať do clipboardu
            var tsvData = ConvertToTsv(data);
            await SetClipboardTsvAsync(tsvData);

            OnCopyCompleted(selections.Length, tsvData);
            return selections.Length;
        }

        public async Task<int> CopyRowAsync(int rowIndex, bool includeSpecialColumns = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var rowData = _dataService?.GetRowData(rowIndex);
            if (rowData == null) return 0;

            var cellsToCopy = includeSpecialColumns
                ? rowData
                : rowData.Where(cell => cell.ColumnDefinition.IsDataColumn);

            var data = new object?[1, cellsToCopy.Count()];
            int colIndex = 0;

            foreach (var cell in cellsToCopy)
            {
                data[0, colIndex] = cell.DisplayValue;
                colIndex++;
            }

            var tsvData = ConvertToTsv(data);
            await SetClipboardTsvAsync(tsvData);

            OnCopyCompleted(cellsToCopy.Count(), tsvData);
            return cellsToCopy.Count();
        }

        public async Task<int> CopyColumnAsync(int columnIndex, bool includeEmptyRows = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var allRows = _dataService?.GetAllRowsData();
            if (allRows == null) return 0;

            var cellsToCopy = new List<object?>();

            foreach (var row in allRows)
            {
                var cell = row.ElementAtOrDefault(columnIndex);
                if (cell != null && (includeEmptyRows || !cell.IsEmpty))
                {
                    cellsToCopy.Add(cell.DisplayValue);
                }
            }

            if (!cellsToCopy.Any()) return 0;

            var data = new object?[cellsToCopy.Count, 1];
            for (int i = 0; i < cellsToCopy.Count; i++)
            {
                data[i, 0] = cellsToCopy[i];
            }

            var tsvData = ConvertToTsv(data);
            await SetClipboardTsvAsync(tsvData);

            OnCopyCompleted(cellsToCopy.Count, tsvData);
            return cellsToCopy.Count;
        }

        public async Task<int> CopyRangeAsync(int startRow, int startColumn, int endRow, int endColumn, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var selections = new List<CellSelection>();

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startColumn; col <= endColumn; col++)
                {
                    selections.Add(new CellSelection(row, col));
                }
            }

            return await CopySelectedCellsAsync(selections, cancellationToken);
        }

        #endregion

        #region ICopyPasteService - Paste operácie

        public async Task<PasteResult> PasteFromClipboardAsync(int startRow, int startColumn, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            try
            {
                var tsvData = await GetClipboardTsvAsync();
                if (string.IsNullOrEmpty(tsvData))
                {
                    return PasteResult.CreateError("Clipboard neobsahuje vhodné dáta");
                }

                return await PasteTsvDataAsync(tsvData, startRow, startColumn, cancellationToken);
            }
            catch (Exception ex)
            {
                return PasteResult.CreateError($"Chyba pri paste operácii: {ex.Message}");
            }
        }

        public async Task<PasteResult> PasteTsvDataAsync(string tsvData, int startRow, int startColumn, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            try
            {
                var parsedData = ParseTsvData(tsvData);
                var dimensions = GetTsvDimensions(tsvData);

                // Validovať paste operáciu
                var validation = ValidatePasteOperation(startRow, startColumn, dimensions.rows, dimensions.columns);
                if (!validation.IsValid)
                {
                    return PasteResult.CreateError(validation.ErrorMessage!);
                }

                // Rozšíriť grid ak je potreba
                var requiredRows = startRow + dimensions.rows;
                var maxDataColumns = _configuration!.DataColumnCount;
                var requiredColumns = Math.Min(startColumn + dimensions.columns, maxDataColumns);

                await _dataService!.EnsureCapacityAsync(requiredRows, requiredColumns);

                // Vykonať paste operáciu
                var cellsAffected = 0;
                var updates = new Dictionary<(int row, int col), object?>();

                for (int row = 0; row < dimensions.rows; row++)
                {
                    for (int col = 0; col < dimensions.columns; col++)
                    {
                        var targetRow = startRow + row;
                        var targetCol = startColumn + col;

                        // Skipovaňť ak by sme išli za posledný dátový stĺpec
                        if (targetCol >= maxDataColumns) continue;

                        var value = parsedData[row, col];
                        updates[(targetRow, targetCol)] = value;
                        cellsAffected++;
                    }
                }

                await _dataService.SetMultipleCellValuesAsync(updates, cancellationToken);

                var result = PasteResult.CreateSuccess(cellsAffected, validation.AdditionalRowsNeeded);
                OnPasteCompleted(result);
                return result;
            }
            catch (Exception ex)
            {
                var result = PasteResult.CreateError($"Chyba pri paste: {ex.Message}");
                OnPasteCompleted(result);
                return result;
            }
        }

        #endregion

        #region ICopyPasteService - Cut operácie

        public async Task<int> CutSelectedCellsAsync(IEnumerable<CellSelection> selectedCells, CancellationToken cancellationToken = default)
        {
            // Najprv copy
            var copiedCount = await CopySelectedCellsAsync(selectedCells, cancellationToken);

            // Potom clear values
            var updates = new Dictionary<(int row, int col), object?>();
            foreach (var selection in selectedCells)
            {
                updates[(selection.RowIndex, selection.ColumnIndex)] = null;
            }

            await _dataService!.SetMultipleCellValuesAsync(updates, cancellationToken);

            var tsvData = await GetClipboardTsvAsync();
            OnCutCompleted(copiedCount, tsvData ?? "");
            return copiedCount;
        }

        public async Task<int> CutRowAsync(int rowIndex, CancellationToken cancellationToken = default)
        {
            // Copy row
            var copiedCount = await CopyRowAsync(rowIndex, false, cancellationToken);

            // Clear row content
            await _dataService!.DeleteRowContentAsync(rowIndex);

            var tsvData = await GetClipboardTsvAsync();
            OnCutCompleted(copiedCount, tsvData ?? "");
            return copiedCount;
        }

        #endregion

        #region ICopyPasteService - Clipboard operations

        public async Task<string?> GetClipboardTsvAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return await dataPackageView.GetTextAsync();
                }
            }
            catch
            {
                // Ignore clipboard errors
            }
            return null;
        }

        public async Task SetClipboardTsvAsync(string tsvData)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(tsvData);
                Clipboard.SetContent(dataPackage);
                await Task.CompletedTask;
            }
            catch
            {
                // Ignore clipboard errors
            }
        }

        public async Task ClearClipboardAsync()
        {
            try
            {
                Clipboard.Clear();
                await Task.CompletedTask;
            }
            catch
            {
                // Ignore clipboard errors
            }
        }

        public async Task<bool> HasClipboardDataAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                var result = dataPackageView.Contains(StandardDataFormats.Text);
                await Task.CompletedTask;
                return result;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ICopyPasteService - Formátovanie

        public string ConvertToTsv(object?[,] data)
        {
            var rows = data.GetLength(0);
            var cols = data.GetLength(1);
            var sb = new StringBuilder();

            for (int row = 0; row < rows; row++)
            {
                var cellValues = new List<string>();

                for (int col = 0; col < cols; col++)
                {
                    var value = data[row, col];
                    var stringValue = value?.ToString() ?? "";

                    // Escape tabs and newlines
                    stringValue = stringValue.Replace("\t", " ").Replace("\r", "").Replace("\n", " ");
                    cellValues.Add(stringValue);
                }

                sb.AppendLine(string.Join("\t", cellValues));
            }

            return sb.ToString().TrimEnd();
        }

        public object?[,] ParseTsvData(string tsvData)
        {
            if (string.IsNullOrEmpty(tsvData))
                return new object?[0, 0];

            var lines = tsvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (!lines.Any()) return new object?[0, 0];

            var maxColumns = lines.Max(line => line.Split('\t').Length);
            var data = new object?[lines.Length, maxColumns];

            for (int row = 0; row < lines.Length; row++)
            {
                var cells = lines[row].Split('\t');
                for (int col = 0; col < maxColumns; col++)
                {
                    data[row, col] = col < cells.Length ? cells[col] : "";
                }
            }

            return data;
        }

        public (int rows, int columns) GetTsvDimensions(string tsvData)
        {
            if (string.IsNullOrEmpty(tsvData))
                return (0, 0);

            var lines = tsvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (!lines.Any()) return (0, 0);

            var rows = lines.Length;
            var columns = lines.Max(line => line.Split('\t').Length);

            return (rows, columns);
        }

        #endregion

        #region ICopyPasteService - Validácia

        public PasteValidationResult ValidatePasteOperation(int startRow, int startColumn, int dataRows, int dataColumns)
        {
            if (startRow < 0 || startColumn < 0)
                return PasteValidationResult.Invalid("Neplatná pozícia pre paste");

            if (dataRows <= 0 || dataColumns <= 0)
                return PasteValidationResult.Invalid("Neplatné rozmery dát");

            // Skontrolovať či neprekračujeme maximálny počet dátových stĺpcov
            var maxDataColumns = _configuration?.DataColumnCount ?? 0;
            if (startColumn >= maxDataColumns)
                return PasteValidationResult.Invalid("Paste pozícia prekračuje dostupné stĺpce");

            // Vypočítať koľko riadkov treba pridať
            var currentRows = _dataService?.RowCount ?? 0;
            var requiredRows = startRow + dataRows;
            var additionalRows = Math.Max(0, requiredRows - currentRows);

            return PasteValidationResult.Valid(additionalRows);
        }

        #endregion

        #region Dependency injection

        /// <summary>
        /// Nastaví referenciu na DataManagementService (dependency injection).
        /// </summary>
        public void SetDataService(IDataManagementService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        #endregion

        #region Helper methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("CopyPasteService nie je inicializovaný");
        }

        #endregion

        #region Event helpers

        private void OnCopyCompleted(int cellsCopied, string tsvData)
        {
            lock (_eventLock)
            {
                CopyCompleted?.Invoke(this, new CopyCompletedEventArgs(cellsCopied, tsvData));
            }
        }

        private void OnPasteCompleted(PasteResult result)
        {
            lock (_eventLock)
            {
                PasteCompleted?.Invoke(this, new PasteCompletedEventArgs(result));
            }
        }

        private void OnCutCompleted(int cellsCut, string tsvData)
        {
            lock (_eventLock)
            {
                CutCompleted?.Invoke(this, new CutCompletedEventArgs(cellsCut, tsvData));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Clear events
                lock (_eventLock)
                {
                    CopyCompleted = null;
                    PasteCompleted = null;
                    CutCompleted = null;
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during CopyPasteService dispose: {ex.Message}");
            }
        }

        #endregion
    }
}