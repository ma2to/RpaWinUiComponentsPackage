// Services/DataManagementService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Implementácia služby pre správu dát v DataGrid komponente.
    /// Zabezpečuje CRUD operácie, správu riadkov a optimalizované načítanie dát.
    /// </summary>
    internal class DataManagementService : IDataManagementService
    {
        #region Private fields

        private GridConfiguration? _configuration;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // Dátové štruktúry
        private readonly List<List<CellData>> _dataRows = new();
        private readonly object _dataLock = new();

        // Event handling
        private readonly object _eventLock = new();

        #endregion

        #region Events

        public event EventHandler<DataChangedEventArgs>? DataChanged;
        public event EventHandler<RowAddedEventArgs>? RowAdded;
        public event EventHandler<RowDeletedEventArgs>? RowDeleted;

        #endregion

        #region IDataManagementService - Inicializácia

        public async Task InitializeAsync(GridConfiguration configuration)
        {
            if (_isInitialized)
                throw new InvalidOperationException("DataManagementService je už inicializovaný");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            lock (_dataLock)
            {
                _dataRows.Clear();
            }

            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        public GridConfiguration? Configuration => _configuration;

        #endregion

        #region IDataManagementService - Základné operácie s dátami

        public async Task LoadDataAsync(IEnumerable<Dictionary<string, object?>> data, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var dataArray = data.ToArray();
            var batchSize = _configuration!.ThrottlingConfig.BatchSize;

            lock (_dataLock)
            {
                _dataRows.Clear();
            }

            // Spracovať data po batch-och
            for (int i = 0; i < dataArray.Length; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = dataArray.Skip(i).Take(batchSize);
                await ProcessDataBatch(batch, i);

                // Krátka pauza pre UI responsiveness
                if (_configuration.ThrottlingConfig.EnableBatchProcessing)
                {
                    await Task.Delay(1, cancellationToken);
                }
            }

            OnDataChanged(-1, -1, null, null); // Signal complete data reload
        }

        public async Task LoadDataAsync(DataTable dataTable, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var dictionaries = new List<Dictionary<string, object?>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object?>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    dict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                }
                dictionaries.Add(dict);
            }

            await LoadDataAsync(dictionaries, cancellationToken);
        }

        public async Task ClearAllDataAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            lock (_dataLock)
            {
                // Dispose všetkých CellData objektov
                foreach (var row in _dataRows)
                {
                    foreach (var cell in row)
                    {
                        cell?.Dispose();
                    }
                }

                _dataRows.Clear();
            }

            // Force garbage collection po veľkom cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            OnDataChanged(-1, -1, null, null); // Signal complete clear
            await Task.CompletedTask;
        }

        public async Task CreateEmptyRowsAsync(int rowCount)
        {
            EnsureInitialized();

            lock (_dataLock)
            {
                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    var row = CreateEmptyRow(rowIndex);
                    _dataRows.Add(row);
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region IDataManagementService - Operácie s riadkami

        public IEnumerable<IEnumerable<CellData>> GetAllRowsData()
        {
            lock (_dataLock)
            {
                return _dataRows.Select(row => row.AsEnumerable()).ToList();
            }
        }

        public IEnumerable<CellData>? GetRowData(int rowIndex)
        {
            lock (_dataLock)
            {
                if (rowIndex < 0 || rowIndex >= _dataRows.Count)
                    return null;

                return _dataRows[rowIndex].AsEnumerable();
            }
        }

        public async Task<int> AddEmptyRowAsync()
        {
            EnsureInitialized();

            int newRowIndex;
            lock (_dataLock)
            {
                newRowIndex = _dataRows.Count;
                var newRow = CreateEmptyRow(newRowIndex);
                _dataRows.Add(newRow);
            }

            OnRowAdded(newRowIndex);
            await Task.CompletedTask;
            return newRowIndex;
        }

        public async Task DeleteRowContentAsync(int rowIndex)
        {
            EnsureInitialized();

            lock (_dataLock)
            {
                if (rowIndex < 0 || rowIndex >= _dataRows.Count)
                    return;

                var row = _dataRows[rowIndex];

                // Vyčistiť iba dátové bunky
                foreach (var cell in row.Where(c => c.ColumnDefinition.IsDataColumn))
                {
                    cell.Clear();
                }
            }

            // Posunúť riadky nahor
            await CompactRowsAsync(rowIndex);

            OnRowDeleted(rowIndex);
        }

        public async Task CompactRowsAsync(int fromRowIndex)
        {
            lock (_dataLock)
            {
                bool anyMove = false;

                for (int i = fromRowIndex; i < _dataRows.Count - 1; i++)
                {
                    var currentRow = _dataRows[i];
                    var nextRow = _dataRows[i + 1];

                    // Ak je aktuálny riadok prázdny a ďalší nie, presuň dáta
                    if (IsRowEmptyInternal(currentRow) && !IsRowEmptyInternal(nextRow))
                    {
                        MoveRowData(nextRow, currentRow);
                        ClearRowData(nextRow);
                        anyMove = true;
                    }
                }

                // Aktualizovať row indexy
                if (anyMove)
                {
                    UpdateRowIndices();
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region IDataManagementService - Operácie s bunkami

        public CellData? GetCellData(int rowIndex, int columnIndex)
        {
            lock (_dataLock)
            {
                if (rowIndex < 0 || rowIndex >= _dataRows.Count)
                    return null;

                var row = _dataRows[rowIndex];
                if (columnIndex < 0 || columnIndex >= row.Count)
                    return null;

                return row[columnIndex];
            }
        }

        public async Task SetCellValueAsync(int rowIndex, int columnIndex, object? value)
        {
            var cellData = GetCellData(rowIndex, columnIndex);
            if (cellData == null) return;

            var oldValue = cellData.Value;
            cellData.Value = value;

            OnDataChanged(rowIndex, columnIndex, oldValue, value);
            await Task.CompletedTask;
        }

        public async Task CommitCellChangesAsync(int rowIndex, int columnIndex)
        {
            var cellData = GetCellData(rowIndex, columnIndex);
            if (cellData == null) return;

            cellData.CommitChanges();
            await Task.CompletedTask;
        }

        public async Task RevertCellChangesAsync(int rowIndex, int columnIndex)
        {
            var cellData = GetCellData(rowIndex, columnIndex);
            if (cellData == null) return;

            var oldValue = cellData.Value;
            cellData.RevertChanges();
            var newValue = cellData.Value;

            OnDataChanged(rowIndex, columnIndex, oldValue, newValue);
            await Task.CompletedTask;
        }

        #endregion

        #region IDataManagementService - Bulk operácie

        public async Task SetMultipleCellValuesAsync(Dictionary<(int row, int col), object?> cellUpdates, CancellationToken cancellationToken = default)
        {
            var batchSize = _configuration!.ThrottlingConfig.BatchSize;
            var updates = cellUpdates.ToArray();

            for (int i = 0; i < updates.Length; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = updates.Skip(i).Take(batchSize);

                foreach (var update in batch)
                {
                    await SetCellValueAsync(update.Key.row, update.Key.col, update.Value);
                }

                // Pauza pre responsiveness
                if (_configuration.ThrottlingConfig.EnableBatchProcessing)
                {
                    await Task.Delay(1, cancellationToken);
                }
            }
        }

        public async Task EnsureCapacityAsync(int requiredRows, int requiredColumns)
        {
            EnsureInitialized();

            lock (_dataLock)
            {
                // Pridať riadky ak je potreba
                while (_dataRows.Count < requiredRows)
                {
                    var newRow = CreateEmptyRow(_dataRows.Count);
                    _dataRows.Add(newRow);
                }

                // Stĺpce sa nedajú dynamicky pridávať (sú definované konfiguráciou)
                // Toto je len validácia
                var maxDataColumns = _configuration!.DataColumnCount;
                if (requiredColumns > maxDataColumns)
                {
                    throw new InvalidOperationException($"Požadované stĺpce ({requiredColumns}) prekračujú konfiguráciu ({maxDataColumns})");
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region IDataManagementService - Štatistiky a info

        public int RowCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _dataRows.Count;
                }
            }
        }

        public int ColumnCount => _configuration?.TotalColumnCount ?? 0;

        public int DataColumnCount => _configuration?.DataColumnCount ?? 0;

        public int NonEmptyRowCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _dataRows.Count(row => !IsRowEmptyInternal(row));
                }
            }
        }

        public bool HasPendingChanges
        {
            get
            {
                lock (_dataLock)
                {
                    return _dataRows.SelectMany(row => row).Any(cell => cell.HasChanges);
                }
            }
        }

        #endregion

        #region IDataManagementService - Memory management

        public async Task CleanupUnusedResourcesAsync()
        {
            lock (_dataLock)
            {
                // Nájsť prázdne riadky na konci a odstrániť ich
                while (_dataRows.Count > 0 && IsRowEmptyInternal(_dataRows.Last()))
                {
                    var lastRow = _dataRows.Last();
                    foreach (var cell in lastRow)
                    {
                        cell?.Dispose();
                    }
                    _dataRows.RemoveAt(_dataRows.Count - 1);
                }
            }

            // Force garbage collection
            GC.Collect();
            await Task.CompletedTask;
        }

        public MemoryUsageInfo GetMemoryUsage()
        {
            lock (_dataLock)
            {
                var totalCells = _dataRows.SelectMany(row => row).Count();
                var activeCells = _dataRows.SelectMany(row => row).Count(cell => !cell.IsEmpty);
                var estimatedBytes = totalCells * 100; // Rough estimate

                return new MemoryUsageInfo
                {
                    TotalCellsAllocated = totalCells,
                    ActiveCellsCount = activeCells,
                    EstimatedMemoryUsageBytes = estimatedBytes,
                    CachedUIElementsCount = 0 // Would need UI reference
                };
            }
        }

        #endregion

        #region Private helper methods

        private async Task ProcessDataBatch(IEnumerable<Dictionary<string, object?>> batch, int startIndex)
        {
            var batchArray = batch.ToArray();

            for (int i = 0; i < batchArray.Length; i++)
            {
                var rowData = batchArray[i];
                var rowIndex = startIndex + i;

                var row = CreateRowFromDictionary(rowData, rowIndex);

                lock (_dataLock)
                {
                    _dataRows.Add(row);
                }
            }

            await Task.CompletedTask;
        }

        private List<CellData> CreateEmptyRow(int rowIndex)
        {
            var row = new List<CellData>();

            foreach (var column in _configuration!.Columns)
            {
                var cell = new CellData(rowIndex, row.Count, column);
                row.Add(cell);
            }

            return row;
        }

        private List<CellData> CreateRowFromDictionary(Dictionary<string, object?> data, int rowIndex)
        {
            var row = new List<CellData>();

            foreach (var column in _configuration!.Columns)
            {
                var cell = new CellData(rowIndex, row.Count, column);

                // Nastaviť hodnotu ak existuje v dátach
                if (data.TryGetValue(column.Name, out var value))
                {
                    if (cell.TryConvertValue(value, out var convertedValue))
                    {
                        cell.SetValueWithoutChange(convertedValue);
                    }
                }

                row.Add(cell);
            }

            return row;
        }

        private bool IsRowEmptyInternal(List<CellData> row)
        {
            // Riadok je prázdny ak sú všetky dátové bunky prázdne
            return row.Where(cell => cell.ColumnDefinition.IsDataColumn)
                      .All(cell => cell.IsEmpty);
        }

        private void MoveRowData(List<CellData> sourceRow, List<CellData> targetRow)
        {
            for (int i = 0; i < Math.Min(sourceRow.Count, targetRow.Count); i++)
            {
                var sourceCell = sourceRow[i];
                var targetCell = targetRow[i];

                if (sourceCell.ColumnDefinition.IsDataColumn && targetCell.ColumnDefinition.IsDataColumn)
                {
                    targetCell.SetValueWithoutChange(sourceCell.Value);
                }
            }
        }

        private void ClearRowData(List<CellData> row)
        {
            foreach (var cell in row.Where(c => c.ColumnDefinition.IsDataColumn))
            {
                cell.Clear();
            }
        }

        private void UpdateRowIndices()
        {
            // CellData má readonly RowIndex, takže by sme museli recreate cells
            // Pre jednoduchosť zatiaľ nechávame staré indexy
            // V produkčnej verzii by sme mali recreate all cells s novými indexmi
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataManagementService nie je inicializovaný");
        }

        #endregion

        #region Event helpers

        private void OnDataChanged(int rowIndex, int columnIndex, object? oldValue, object? newValue)
        {
            lock (_eventLock)
            {
                DataChanged?.Invoke(this, new DataChangedEventArgs(rowIndex, columnIndex, oldValue, newValue));
            }
        }

        private void OnRowAdded(int rowIndex)
        {
            lock (_eventLock)
            {
                RowAdded?.Invoke(this, new RowAddedEventArgs(rowIndex));
            }
        }

        private void OnRowDeleted(int rowIndex)
        {
            lock (_eventLock)
            {
                RowDeleted?.Invoke(this, new RowDeletedEventArgs(rowIndex));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                lock (_dataLock)
                {
                    // Dispose všetkých buniek
                    foreach (var row in _dataRows)
                    {
                        foreach (var cell in row)
                        {
                            cell?.Dispose();
                        }
                    }
                    _dataRows.Clear();
                }

                // Clear events
                lock (_eventLock)
                {
                    DataChanged = null;
                    RowAdded = null;
                    RowDeleted = null;
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during DataManagementService dispose: {ex.Message}");
            }
        }

        #endregion
    }
}