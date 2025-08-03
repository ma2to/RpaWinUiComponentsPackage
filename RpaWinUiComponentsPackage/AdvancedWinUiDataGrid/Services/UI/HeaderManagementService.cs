// Services/UI/HeaderManagementService.cs - ✅ NOVÝ: Service pre správu header-ov
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre správu DataGrid header-ov - INTERNAL
    /// Zodpovedný za header rendering, sizing, sorting indicators, resize handles
    /// </summary>
    internal class HeaderManagementService
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        private List<Models.Grid.ColumnDefinition> _columns = new();
        private Dictionary<string, SortDirection> _currentSortStates = new();
        private bool _isInitialized = false;

        #endregion

        #region Constructor

        public HeaderManagementService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🏗️ HeaderManagementService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Service je inicializovaný
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Aktuálne stĺpce
        /// </summary>
        public IReadOnlyList<Models.Grid.ColumnDefinition> Columns => _columns.AsReadOnly();

        /// <summary>
        /// Aktuálne sort states
        /// </summary>
        public IReadOnlyDictionary<string, SortDirection> CurrentSortStates => _currentSortStates;

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje service s column definíciami
        /// </summary>
        public async Task InitializeAsync(List<Models.Grid.ColumnDefinition> columns)
        {
            try
            {
                _logger.LogInformation("🏗️ HeaderManagementService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _columns = columns ?? throw new ArgumentNullException(nameof(columns));
                _currentSortStates.Clear();

                // Inicializuj default sort states
                foreach (var column in _columns)
                {
                    _currentSortStates[column.Name] = SortDirection.None;
                }

                _isInitialized = true;

                _logger.LogInformation("✅ HeaderManagementService INITIALIZED - InstanceId: {InstanceId}, Columns: {Count}", 
                    _serviceInstanceId, _columns.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in HeaderManagementService.InitializeAsync - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
                throw;
            }
        }

        #endregion

        #region Header Operations

        /// <summary>
        /// Aktualizuje sort indikátor pre stĺpec
        /// </summary>
        public void UpdateSortIndicator(string columnName, SortDirection sortDirection)
        {
            if (string.IsNullOrEmpty(columnName)) return;

            _logger.LogDebug("🔄 Updating sort indicator - Column: {Column}, Direction: {Direction}", 
                columnName, sortDirection);

            _currentSortStates[columnName] = sortDirection;
        }

        /// <summary>
        /// Získa sort indikátor pre stĺpec
        /// </summary>
        public SortDirection GetSortIndicator(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return SortDirection.None;

            return _currentSortStates.TryGetValue(columnName, out var direction) ? direction : SortDirection.None;
        }

        /// <summary>
        /// Vymaže všetky sort indikátory
        /// </summary>
        public void ClearAllSortIndicators()
        {
            _logger.LogDebug("🧹 Clearing all sort indicators");

            foreach (var key in _currentSortStates.Keys.ToList())
            {
                _currentSortStates[key] = SortDirection.None;
            }
        }

        /// <summary>
        /// Aktualizuje šírku stĺpca
        /// </summary>
        public bool UpdateColumnWidth(string columnName, double newWidth)
        {
            if (string.IsNullOrEmpty(columnName) || newWidth <= 0) return false;

            var column = _columns.FirstOrDefault(c => c.Name == columnName);
            if (column == null) return false;

            _logger.LogDebug("📏 Updating column width - Column: {Column}, Width: {Width}", 
                columnName, newWidth);

            column.Width = newWidth;
            return true;
        }

        /// <summary>
        /// Získa šírku stĺpca
        /// </summary>
        public double GetColumnWidth(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return 100.0;

            var column = _columns.FirstOrDefault(c => c.Name == columnName);
            return column?.Width ?? 100.0;
        }

        #endregion

        #region Header Layout

        /// <summary>
        /// Vypočíta optimálnu šírku stĺpca na základe obsahu
        /// </summary>
        public double CalculateOptimalColumnWidth(string columnName, List<object> sampleData, int maxSamples = 100)
        {
            if (string.IsNullOrEmpty(columnName) || sampleData == null || !sampleData.Any())
                return 100.0;

            try
            {
                // TODO: Implement actual text measurement logic
                // Pre teraz vrátime základnú hodnotu
                var samples = sampleData.Take(maxSamples);
                var maxLength = samples
                    .Select(item => item?.ToString()?.Length ?? 0)
                    .DefaultIfEmpty(0)
                    .Max();

                // Odhad: 8 pixelov na znak + 20 pixelov padding
                var estimatedWidth = Math.Max(100, (maxLength * 8) + 20);
                
                _logger.LogDebug("📐 Calculated optimal width - Column: {Column}, Width: {Width}", 
                    columnName, estimatedWidth);

                return Math.Min(estimatedWidth, 300); // Max 300px
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error calculating optimal width - Column: {Column}", columnName);
                return 100.0;
            }
        }

        /// <summary>
        /// Auto-size všetkých stĺpcov
        /// </summary>
        public async Task AutoSizeAllColumnsAsync(List<object> sampleData)
        {
            try
            {
                _logger.LogDebug("📏 Auto-sizing all columns...");

                foreach (var column in _columns)
                {
                    var optimalWidth = CalculateOptimalColumnWidth(column.Name, sampleData);
                    column.Width = optimalWidth;
                }

                _logger.LogDebug("✅ Auto-sizing completed");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in auto-sizing columns");
            }
        }

        #endregion

        #region Events & Notifications

        /// <summary>
        /// Event pre zmenu sort indikátora
        /// </summary>
        public event EventHandler<SortIndicatorChangedEventArgs>? SortIndicatorChanged;

        /// <summary>
        /// Event pre zmenu šírky stĺpca
        /// </summary>
        public event EventHandler<ColumnWidthChangedEventArgs>? ColumnWidthChanged;

        /// <summary>
        /// Vyvolá event pre zmenu sort indikátora
        /// </summary>
        protected virtual void OnSortIndicatorChanged(string columnName, SortDirection direction)
        {
            SortIndicatorChanged?.Invoke(this, new SortIndicatorChangedEventArgs(columnName, direction));
        }

        /// <summary>
        /// Vyvolá event pre zmenu šírky stĺpca
        /// </summary>
        protected virtual void OnColumnWidthChanged(string columnName, double oldWidth, double newWidth)
        {
            ColumnWidthChanged?.Invoke(this, new ColumnWidthChangedEventArgs(columnName, oldWidth, newWidth));
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var sortedColumns = _currentSortStates.Count(kvp => kvp.Value != SortDirection.None);
            return $"HeaderManagementService[{_serviceInstanceId}] - " +
                   $"Initialized: {_isInitialized}, Columns: {_columns.Count}, SortedColumns: {sortedColumns}";
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event args pre zmenu sort indikátora
    /// </summary>
    internal class SortIndicatorChangedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public SortDirection Direction { get; }

        public SortIndicatorChangedEventArgs(string columnName, SortDirection direction)
        {
            ColumnName = columnName;
            Direction = direction;
        }
    }

    /// <summary>
    /// Event args pre zmenu šírky stĺpca
    /// </summary>
    internal class ColumnWidthChangedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OldWidth { get; }
        public double NewWidth { get; }

        public ColumnWidthChangedEventArgs(string columnName, double oldWidth, double newWidth)
        {
            ColumnName = columnName;
            OldWidth = oldWidth;
            NewWidth = newWidth;
        }
    }

    #endregion
}