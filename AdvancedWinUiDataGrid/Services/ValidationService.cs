// Services/ValidationService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Implementácia validačnej služby pre DataGrid komponent.
    /// Zabezpečuje asynchrónne validácie s throttling podporou a optimalizáciami.
    /// </summary>
    internal class ValidationService : IValidationService
    {
        #region Private fields

        private GridConfiguration? _configuration;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // Throttling & Performance
        private readonly ConcurrentDictionary<(int row, int col), Timer> _pendingValidations = new();
        private readonly SemaphoreSlim _validationSemaphore;
        private readonly ConcurrentDictionary<(int row, int col), ValidationResult> _validationCache = new();

        // Event handlers
        private readonly object _eventLock = new();

        #endregion

        #region Constructor

        public ValidationService()
        {
            _validationSemaphore = new SemaphoreSlim(3, 3); // Max 3 concurrent validations by default
        }

        #endregion

        #region Events

        public event EventHandler<CellValidationChangedEventArgs>? CellValidationChanged;
        public event EventHandler<RowValidationCompletedEventArgs>? RowValidationCompleted;

        #endregion

        #region IValidationService - Inicializácia

        public async Task InitializeAsync(GridConfiguration configuration)
        {
            if (_isInitialized)
                throw new InvalidOperationException("ValidationService je už inicializovaný");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Nastaviť semaphore na základe throttling config
            var maxConcurrency = _configuration.ThrottlingConfig.MaxConcurrentValidations;
            if (_validationSemaphore.CurrentCount != maxConcurrency)
            {
                // Recreate semaphore with correct count
                _validationSemaphore.Dispose();
                //_validationSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            }

            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        #endregion

        #region IValidationService - Validácia buniek

        public async Task<ValidationResult> ValidateCellAsync(CellData cellData, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            try
            {
                await _validationSemaphore.WaitAsync(cancellationToken);

                var startTime = DateTime.UtcNow;
                var result = await ValidateCellInternal(cellData);
                var duration = DateTime.UtcNow - startTime;

                // Cache result
                _validationCache.TryAdd((cellData.RowIndex, cellData.ColumnIndex), result);

                // Trigger event
                OnCellValidationChanged(cellData.RowIndex, cellData.ColumnIndex, result);

                return result;
            }
            finally
            {
                _validationSemaphore.Release();
            }
        }

        public async Task ValidateCellWithThrottlingAsync(CellData cellData, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (!_configuration!.ThrottlingConfig.EnableValidationThrottling)
            {
                await ValidateCellAsync(cellData, cancellationToken);
                return;
            }

            var cellKey = (cellData.RowIndex, cellData.ColumnIndex);
            var debounceMs = _configuration.ThrottlingConfig.ValidationDebounceMs;

            // Cancel existing timer for this cell
            if (_pendingValidations.TryRemove(cellKey, out var existingTimer))
            {
                existingTimer.Dispose();
            }

            // Create new throttled validation
            var timer = new Timer(async _ =>
            {
                try
                {
                    await ValidateCellAsync(cellData, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't crash
                    System.Diagnostics.Debug.WriteLine($"Throttled validation error: {ex.Message}");
                }
                finally
                {
                    _pendingValidations.TryRemove(cellKey, out _);
                }
            }, null, debounceMs, Timeout.Infinite);

            _pendingValidations.TryAdd(cellKey, timer);
        }

        public async Task<Dictionary<(int row, int col), ValidationResult>> ValidateMultipleCellsAsync(
            IEnumerable<CellData> cellDataList,
            CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var results = new Dictionary<(int row, int col), ValidationResult>();
            var batchSize = _configuration!.ThrottlingConfig.BatchSize;

            var cellsArray = cellDataList.ToArray();
            for (int i = 0; i < cellsArray.Length; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = cellsArray.Skip(i).Take(batchSize);
                var batchTasks = batch.Select(async cell =>
                {
                    var result = await ValidateCellAsync(cell, cancellationToken);
                    return new { Cell = cell, Result = result };
                });

                var batchResults = await Task.WhenAll(batchTasks);

                foreach (var item in batchResults)
                {
                    results[(item.Cell.RowIndex, item.Cell.ColumnIndex)] = item.Result;
                }
            }

            return results;
        }

        #endregion

        #region IValidationService - Validácia riadkov

        public async Task<RowValidationResult> ValidateRowAsync(int rowIndex, IEnumerable<CellData> rowData, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            // Skontrolovať či je riadok prázdny
            if (IsRowEmpty(rowData))
            {
                return new RowValidationResult(rowIndex, true, new Dictionary<int, ValidationResult>());
            }

            var cellResults = await ValidateMultipleCellsAsync(rowData, cancellationToken);
            var isRowValid = cellResults.Values.All(r => r.IsValid);

            var indexedResults = cellResults.ToDictionary(
                kvp => kvp.Key.col,
                kvp => kvp.Value
            );

            var result = new RowValidationResult(rowIndex, isRowValid, indexedResults);

            OnRowValidationCompleted(rowIndex, result);

            return result;
        }

        public async Task<GridValidationResult> ValidateAllNonEmptyRowsAsync(IEnumerable<IEnumerable<CellData>> allRowsData, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var rowResults = new Dictionary<int, RowValidationResult>();
            var rowsArray = allRowsData.ToArray();

            for (int rowIndex = 0; rowIndex < rowsArray.Length; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rowData = rowsArray[rowIndex];
                if (!IsRowEmpty(rowData))
                {
                    var rowResult = await ValidateRowAsync(rowIndex, rowData, cancellationToken);
                    rowResults[rowIndex] = rowResult;
                }
            }

            var isGridValid = rowResults.Values.All(r => r.IsValid);
            return new GridValidationResult(isGridValid, rowResults);
        }

        #endregion

        #region IValidationService - Kontrola prázdnych riadkov

        public bool IsRowEmpty(IEnumerable<CellData> rowData)
        {
            if (!_isInitialized || _configuration == null)
                return true;

            // Získať iba dátové stĺpce (nie špeciálne)
            var dataCells = rowData.Where(cell => cell.ColumnDefinition.IsDataColumn);

            // Riadok je prázdny ak sú všetky dátové bunky prázdne
            return dataCells.All(cell => cell.IsEmpty);
        }

        public List<int> GetNonEmptyRowIndices(IEnumerable<IEnumerable<CellData>> allRowsData)
        {
            var nonEmptyRows = new List<int>();
            var rowsArray = allRowsData.ToArray();

            for (int rowIndex = 0; rowIndex < rowsArray.Length; rowIndex++)
            {
                if (!IsRowEmpty(rowsArray[rowIndex]))
                {
                    nonEmptyRows.Add(rowIndex);
                }
            }

            return nonEmptyRows;
        }

        #endregion

        #region IValidationService - Throttling a výkon

        public async Task CancelPendingValidationsAsync()
        {
            var timers = _pendingValidations.Values.ToArray();
            _pendingValidations.Clear();

            foreach (var timer in timers)
            {
                timer.Dispose();
            }

            await Task.CompletedTask;
        }

        public int ActiveValidationsCount => _validationSemaphore.CurrentCount;

        public int PendingValidationsCount => _pendingValidations.Count;

        #endregion

        #region IValidationService - Konfigurácia

        public async Task UpdateThrottlingConfigAsync(ThrottlingConfig newConfig)
        {
            if (_configuration != null)
            {
                // Update existing configuration
                var oldConfig = _configuration.ThrottlingConfig;
                // Note: We can't directly replace the config object, but we can update our behavior

                // Cancel all pending validations if throttling is disabled
                if (!newConfig.EnableValidationThrottling)
                {
                    await CancelPendingValidationsAsync();
                }
            }

            await Task.CompletedTask;
        }

        public ThrottlingConfig CurrentThrottlingConfig => _configuration?.ThrottlingConfig ?? ThrottlingConfig.Default;

        #endregion

        #region Private - Core validation logic

        /// <summary>
        /// Hlavná validačná logika pre jednu bunku.
        /// </summary>
        private async Task<ValidationResult> ValidateCellInternal(CellData cellData)
        {
            var columnName = cellData.ColumnName;
            var value = cellData.Value;

            // Získať validačné pravidlá pre tento stĺpec
            var rules = _configuration!.GetValidationRulesForColumn(columnName);

            if (!rules.Any())
            {
                return ValidationResult.Success();
            }

            // Spustiť všetky validačné pravidlá
            var errors = new List<string>();

            foreach (var rule in rules.OrderByDescending(r => r.Priority))
            {
                try
                {
                    if (!rule.Validate(value))
                    {
                        errors.Add(rule.GetFormattedErrorMessage());
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{columnName}: Chyba pri validácii - {ex.Message}");
                }
            }

            await Task.CompletedTask; // Make it async-ready

            if (errors.Any())
            {
                return ValidationResult.Error(string.Join("; ", errors));
            }

            return ValidationResult.Success();
        }

        #endregion

        #region Private - Event helpers

        private void OnCellValidationChanged(int rowIndex, int columnIndex, ValidationResult result)
        {
            lock (_eventLock)
            {
                CellValidationChanged?.Invoke(this, new CellValidationChangedEventArgs(rowIndex, columnIndex, result));
            }
        }

        private void OnRowValidationCompleted(int rowIndex, RowValidationResult result)
        {
            lock (_eventLock)
            {
                RowValidationCompleted?.Invoke(this, new RowValidationCompletedEventArgs(rowIndex, result));
            }
        }

        #endregion

        #region Helper methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("ValidationService nie je inicializovaný");
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Cancel all pending validations
                var timers = _pendingValidations.Values.ToArray();
                _pendingValidations.Clear();

                foreach (var timer in timers)
                {
                    timer?.Dispose();
                }

                // Dispose semaphore
                _validationSemaphore?.Dispose();

                // Clear cache
                _validationCache.Clear();

                // Clear events
                lock (_eventLock)
                {
                    CellValidationChanged = null;
                    RowValidationCompleted = null;
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during ValidationService dispose: {ex.Message}");
            }
        }

        #endregion
    }
}