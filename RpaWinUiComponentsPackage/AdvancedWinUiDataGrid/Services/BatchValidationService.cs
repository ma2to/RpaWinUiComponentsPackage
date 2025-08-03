// Services/BatchValidationService.cs - ✅ NOVÉ: Batch Validation Service s parallel processing
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Service pre batch validation s parallel processing a progress reporting
    /// </summary>
    public class BatchValidationService : IDisposable
    {
        private readonly ILogger _logger;
        private BatchValidationConfiguration _config;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;
        private CancellationTokenSource? _cancellationTokenSource;

        public BatchValidationService(BatchValidationConfiguration? config = null, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? BatchValidationConfiguration.Default;
            
            _logger.LogDebug("🚀 BatchValidationService initialized with config: {Config}", 
                $"Enabled={_config.IsEnabled}, BatchSize={_config.BatchSize}, MaxConcurrency={_config.MaxConcurrency}");
        }

        /// <summary>
        /// Event pre progress reporting
        /// </summary>
        public event EventHandler<BatchValidationProgress>? ProgressChanged;

        /// <summary>
        /// Aktualizuje konfiguráciu batch validation
        /// </summary>
        public void UpdateConfiguration(BatchValidationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Batch validation configuration updated");
            }
        }

        /// <summary>
        /// Spustí batch validation pre zoznam riadkov
        /// </summary>
        public async Task<BatchValidationResult> ValidateRowsAsync(
            List<Dictionary<string, object?>> rows,
            List<Models.Grid.ColumnDefinition> columns,
            List<ValidationRule> validationRules,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(BatchValidationService));

            try
            {
                _logger.LogInformation("🚀 BatchValidateRowsAsync START - Rows: {RowCount}, Rules: {RuleCount}, " +
                    "BatchSize: {BatchSize}, MaxConcurrency: {MaxConcurrency}",
                    rows.Count, validationRules.Count, _config.BatchSize, _config.MaxConcurrency);

                var stopwatch = Stopwatch.StartNew();
                var result = new BatchValidationResult
                {
                    TotalRows = rows.Count,
                    StartTime = DateTime.Now
                };

                var progress = new BatchValidationProgress
                {
                    TotalRows = rows.Count,
                    StartTime = DateTime.Now,
                    TotalBatches = (int)Math.Ceiling((double)rows.Count / _config.BatchSize)
                };

                if (!_config.IsEnabled || rows.Count == 0)
                {
                    _logger.LogDebug("⏭️ Batch validation skipped - Enabled: {Enabled}, RowCount: {RowCount}",
                        _config.IsEnabled, rows.Count);
                    return result;
                }

                // Ak je dataset malý, použij sekvenčnú validáciu
                if (rows.Count < _config.BatchSize)
                {
                    return await ValidateRowsSequentiallyAsync(rows, columns, validationRules, result, progress, cancellationToken);
                }

                // Pre veľké datasety použij parallel batch processing
                return await ValidateRowsInParallelBatchesAsync(rows, columns, validationRules, result, progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ Batch validation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ValidateRowsAsync");
                throw;
            }
        }

        /// <summary>
        /// Sekvenčná validácia pre malé datasety
        /// </summary>
        private async Task<BatchValidationResult> ValidateRowsSequentiallyAsync(
            List<Dictionary<string, object?>> rows,
            List<Models.Grid.ColumnDefinition> columns,
            List<ValidationRule> validationRules,
            BatchValidationResult result,
            BatchValidationProgress progress,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("🔄 Using sequential validation for {RowCount} rows", rows.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rowResult = await ValidateRowAsync(rows[i], i, columns, validationRules);
                result.RowResults.Add(rowResult);

                if (rowResult.IsValid)
                    result.ValidRows++;
                else
                    result.InvalidRows++;

                // Progress reporting
                if (_config.EnableProgressReporting && i % 10 == 0)
                {
                    progress.ProcessedRows = i + 1;
                    progress.ValidRows = result.ValidRows;
                    progress.InvalidRows = result.InvalidRows;
                    ProgressChanged?.Invoke(this, progress);
                }
            }

            result.ProcessedRows = rows.Count;
            result.EndTime = DateTime.Now;
            return result;
        }

        /// <summary>
        /// Parallel batch processing pre veľké datasety
        /// </summary>
        private async Task<BatchValidationResult> ValidateRowsInParallelBatchesAsync(
            List<Dictionary<string, object?>> rows,
            List<Models.Grid.ColumnDefinition> columns,
            List<ValidationRule> validationRules,
            BatchValidationResult result,
            BatchValidationProgress progress,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("🔄 Using parallel batch validation for {RowCount} rows", rows.Count);

            var batches = CreateBatches(rows);
            var semaphore = new SemaphoreSlim(_config.MaxConcurrency);
            var resultLock = new object();
            var progressLock = new object();

            var tasks = batches.Select(async (batch, batchIndex) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var batchResults = await ProcessBatchAsync(batch.rows, batch.startIndex, columns, validationRules, cancellationToken);
                    
                    lock (resultLock)
                    {
                        result.RowResults.AddRange(batchResults);
                        result.ValidRows += batchResults.Count(r => r.IsValid);
                        result.InvalidRows += batchResults.Count(r => !r.IsValid);
                        result.ProcessedRows += batchResults.Count;
                    }

                    // Progress reporting
                    if (_config.EnableProgressReporting)
                    {
                        lock (progressLock)
                        {
                            progress.ProcessedRows = result.ProcessedRows;
                            progress.ValidRows = result.ValidRows;
                            progress.InvalidRows = result.InvalidRows;
                            progress.CurrentBatch = batchIndex + 1;
                            ProgressChanged?.Invoke(this, progress);
                        }
                    }

                    _logger.LogTrace("✅ Batch {BatchIndex}/{TotalBatches} completed - " +
                        "Rows: {BatchRows}, Valid: {Valid}, Invalid: {Invalid}",
                        batchIndex + 1, batches.Count, batchResults.Count,
                        batchResults.Count(r => r.IsValid), batchResults.Count(r => !r.IsValid));
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            result.EndTime = DateTime.Now;
            result.RowResults = result.RowResults.OrderBy(r => r.RowIndex).ToList();

            _logger.LogInformation("✅ Parallel batch validation completed - " +
                "Duration: {Duration}ms, Valid: {Valid}, Invalid: {Invalid}",
                (result.EndTime - result.StartTime).TotalMilliseconds, result.ValidRows, result.InvalidRows);

            return result;
        }

        /// <summary>
        /// Vytvorí batch-e z riadkov
        /// </summary>
        private List<(List<Dictionary<string, object?>> rows, int startIndex)> CreateBatches(List<Dictionary<string, object?>> rows)
        {
            var batches = new List<(List<Dictionary<string, object?>> rows, int startIndex)>();
            
            for (int i = 0; i < rows.Count; i += _config.BatchSize)
            {
                var batchSize = Math.Min(_config.BatchSize, rows.Count - i);
                var batchRows = rows.Skip(i).Take(batchSize).ToList();
                batches.Add((batchRows, i));
            }

            return batches;
        }

        /// <summary>
        /// Spracuje jeden batch riadkov
        /// </summary>
        private async Task<List<RowValidationResult>> ProcessBatchAsync(
            List<Dictionary<string, object?>> batchRows,
            int startIndex,
            List<Models.Grid.ColumnDefinition> columns,
            List<ValidationRule> validationRules,
            CancellationToken cancellationToken)
        {
            var results = new List<RowValidationResult>();
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_config.BatchTimeoutMs);

            for (int i = 0; i < batchRows.Count; i++)
            {
                cts.Token.ThrowIfCancellationRequested();
                
                var rowResult = await ValidateRowAsync(batchRows[i], startIndex + i, columns, validationRules);
                results.Add(rowResult);
            }

            return results;
        }

        /// <summary>
        /// Validuje jeden riadok
        /// </summary>
        private async Task<RowValidationResult> ValidateRowAsync(
            Dictionary<string, object?> rowData,
            int rowIndex,
            List<Models.Grid.ColumnDefinition> columns,
            List<ValidationRule> validationRules)
        {
            var result = new RowValidationResult
            {
                RowIndex = rowIndex,
                IsValid = true
            };

            foreach (var rule in validationRules)
            {
                if (!rowData.ContainsKey(rule.ColumnName)) continue;

                var value = rowData[rule.ColumnName];
                var column = columns.FirstOrDefault(c => c.Name == rule.ColumnName);
                
                if (column != null)
                {
                    var cellResult = await ValidateCellAsync(value, rule, column);
                    result.CellResults.Add(cellResult);
                    
                    if (!cellResult.IsValid)
                        result.IsValid = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Validuje jednu bunku
        /// </summary>
        private async Task<CellValidationResult> ValidateCellAsync(
            object? value,
            ValidationRule rule,
            Models.Grid.ColumnDefinition column)
        {
            // Simulácia async validácie
            await Task.Yield();

            var result = new CellValidationResult
            {
                ColumnName = rule.ColumnName,
                Value = value,
                IsValid = true
            };

            try
            {
                // Tu by bola skutočná validácia podľa rule.ValidationFunction
                // Pre demo účely jednoducho skontrolujeme či nie je null pre required fields
                if (rule.IsRequired && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Field {rule.ColumnName} is required";
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
                _logger.LogWarning(ex, "⚠️ Validation error for column {ColumnName}", rule.ColumnName);
            }

            return result;
        }

        /// <summary>
        /// Zruší prebiehajúcu validáciu
        /// </summary>
        public void CancelValidation()
        {
            _cancellationTokenSource?.Cancel();
            _logger.LogInformation("🛑 Batch validation cancelled");
        }

        /// <summary>
        /// Vráti konfiguráciu
        /// </summary>
        public BatchValidationConfiguration GetConfiguration()
        {
            lock (_lockObject)
            {
                return _config.Clone();
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _isDisposed = true;

            _logger.LogDebug("🗑️ BatchValidationService disposed");
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledok batch validation
    /// </summary>
    public class BatchValidationResult
    {
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<RowValidationResult> RowResults { get; set; } = new();
        
        public TimeSpan Duration => EndTime - StartTime;
        public double ProcessingRate => Duration.TotalSeconds > 0 ? ProcessedRows / Duration.TotalSeconds : 0;
        public bool IsSuccessful => ProcessedRows == TotalRows;
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledok validácie riadku
    /// </summary>
    public class RowValidationResult
    {
        public int RowIndex { get; set; }
        public bool IsValid { get; set; }
        public List<CellValidationResult> CellResults { get; set; } = new();
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledok validácie bunky
    /// </summary>
    public class CellValidationResult
    {
        public string ColumnName { get; set; } = string.Empty;
        public object? Value { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}