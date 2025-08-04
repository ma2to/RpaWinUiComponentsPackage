// Services/UI/ResizeHandlingService.cs - ✅ NOVÝ: Service pre handling resize operácií
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre handling resize operácií - INTERNAL
    /// Zodpovedný za column resizing, auto-sizing, resize constraints
    /// </summary>
    internal class ResizeHandlingService
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        private List<Models.Grid.ColumnDefinition> _columns = new();
        private Dictionary<string, ResizeConstraints> _resizeConstraints = new();
        private bool _isResizing = false;
        private bool _isInitialized = false;

        // Resize tracking
        private string? _currentResizingColumn;
        private double _originalWidth;
        private double _startMouseX;

        #endregion

        #region Constructor

        public ResizeHandlingService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("📏 ResizeHandlingService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Service je inicializovaný
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Je práve v procese resizing
        /// </summary>
        public bool IsResizing => _isResizing;

        /// <summary>
        /// Názov stĺpca ktorý sa práve resize-uje
        /// </summary>
        public string? CurrentResizingColumn => _currentResizingColumn;

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje service s column definíciami
        /// </summary>
        public async Task InitializeAsync(List<Models.Grid.ColumnDefinition> columns)
        {
            try
            {
                _logger.LogInformation("📏 ResizeHandlingService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _columns = columns ?? throw new ArgumentNullException(nameof(columns));
                
                // Nastav default resize constraints
                SetupDefaultResizeConstraints();

                _isInitialized = true;

                _logger.LogInformation("✅ ResizeHandlingService INITIALIZED - InstanceId: {InstanceId}, Columns: {Count}", 
                    _serviceInstanceId, _columns.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ResizeHandlingService.InitializeAsync - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
                throw;
            }
        }

        #endregion

        #region Resize Operations

        /// <summary>
        /// Začne resize operáciu pre stĺpec
        /// </summary>
        public bool StartResize(string columnName, double mouseX)
        {
            try
            {
                if (string.IsNullOrEmpty(columnName) || _isResizing) return false;

                var column = _columns.FirstOrDefault(c => c.Name == columnName);
                if (column == null) return false;

                _currentResizingColumn = columnName;
                _originalWidth = column.Width;
                _startMouseX = mouseX;
                _isResizing = true;

                _logger.LogDebug("📏 Started resize - Column: {Column}, OriginalWidth: {Width}, MouseX: {MouseX}", 
                    columnName, _originalWidth, mouseX);

                OnResizeStarted(columnName, _originalWidth);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting resize - Column: {Column}", columnName);
                return false;
            }
        }

        /// <summary>
        /// Aktualizuje resize počas drag operácie
        /// </summary>
        public bool UpdateResize(double currentMouseX)
        {
            try
            {
                if (!_isResizing || string.IsNullOrEmpty(_currentResizingColumn)) return false;

                var column = _columns.FirstOrDefault(c => c.Name == _currentResizingColumn);
                if (column == null) return false;

                var delta = currentMouseX - _startMouseX;
                var newWidth = _originalWidth + delta;

                // Aplikuj constraints
                var constraints = GetResizeConstraints(_currentResizingColumn);
                newWidth = Math.Max(constraints.MinWidth, Math.Min(constraints.MaxWidth, newWidth));

                // Aktualizuj šírku stĺpca
                var oldWidth = column.Width;
                column.Width = newWidth;

                _logger.LogTrace("📏 Updating resize - Column: {Column}, NewWidth: {Width}, Delta: {Delta}", 
                    _currentResizingColumn, newWidth, delta);

                OnResizeUpdated(_currentResizingColumn, oldWidth, newWidth);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating resize");
                return false;
            }
        }

        /// <summary>
        /// Ukončí resize operáciu
        /// </summary>
        public bool EndResize()
        {
            try
            {
                if (!_isResizing || string.IsNullOrEmpty(_currentResizingColumn)) return false;

                var column = _columns.FirstOrDefault(c => c.Name == _currentResizingColumn);
                if (column == null) return false;

                var finalWidth = column.Width;

                _logger.LogDebug("📏 Ended resize - Column: {Column}, FinalWidth: {Width}", 
                    _currentResizingColumn, finalWidth);

                OnResizeCompleted(_currentResizingColumn, _originalWidth, finalWidth);

                // Reset resize state
                _currentResizingColumn = null;
                _originalWidth = 0;
                _startMouseX = 0;
                _isResizing = false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ending resize");
                return false;
            }
        }

        /// <summary>
        /// Zruší resize operáciu
        /// </summary>
        public bool CancelResize()
        {
            try
            {
                if (!_isResizing || string.IsNullOrEmpty(_currentResizingColumn)) return false;

                var column = _columns.FirstOrDefault(c => c.Name == _currentResizingColumn);
                if (column != null)
                {
                    // Vráť pôvodnú šírku
                    column.Width = _originalWidth;
                }

                _logger.LogDebug("📏 Canceled resize - Column: {Column}, RestoredWidth: {Width}", 
                    _currentResizingColumn, _originalWidth);

                OnResizeCanceled(_currentResizingColumn, _originalWidth);

                // Reset resize state
                _currentResizingColumn = null;
                _originalWidth = 0;
                _startMouseX = 0;
                _isResizing = false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error canceling resize");
                return false;
            }
        }

        #endregion

        #region Auto-sizing

        /// <summary>
        /// Auto-size stĺpca na základe obsahu
        /// </summary>
        public async Task<bool> AutoSizeColumnAsync(string columnName, List<object> sampleData, int maxSamples = 100)
        {
            try
            {
                if (string.IsNullOrEmpty(columnName) || sampleData == null) return false;

                var column = _columns.FirstOrDefault(c => c.Name == columnName);
                if (column == null) return false;

                var oldWidth = column.Width;
                var newWidth = CalculateOptimalWidth(columnName, sampleData, maxSamples);

                // Aplikuj constraints
                var constraints = GetResizeConstraints(columnName);
                newWidth = Math.Max(constraints.MinWidth, Math.Min(constraints.MaxWidth, newWidth));

                column.Width = newWidth;

                _logger.LogDebug("📐 Auto-sized column - Column: {Column}, OldWidth: {OldWidth}, NewWidth: {NewWidth}", 
                    columnName, oldWidth, newWidth);

                OnColumnAutoSized(columnName, oldWidth, newWidth);

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-sizing column - Column: {Column}", columnName);
                return false;
            }
        }

        /// <summary>
        /// Auto-size všetkých stĺpcov
        /// </summary>
        public async Task AutoSizeAllColumnsAsync(List<object> sampleData)
        {
            try
            {
                _logger.LogDebug("📐 Auto-sizing all columns...");

                foreach (var column in _columns)
                {
                    await AutoSizeColumnAsync(column.Name, sampleData);
                }

                _logger.LogDebug("✅ Auto-sizing all columns completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-sizing all columns");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Handle double-click auto-fit na resize grip
        /// </summary>
        public async Task<bool> HandleDoubleClickAutoFitAsync(string columnName, List<object> sampleData)
        {
            try
            {
                if (string.IsNullOrEmpty(columnName) || _isResizing) return false;

                _logger.LogInformation("🖱️ Double-click auto-fit triggered - Column: {Column}", columnName);

                // Auto-size stĺpec na základe obsahu
                var result = await AutoSizeColumnAsync(columnName, sampleData, maxSamples: 200); // Viac samples pre lepšiu presnosť

                if (result)
                {
                    _logger.LogInformation("✅ Double-click auto-fit completed - Column: {Column}", columnName);
                    OnDoubleClickAutoFit(columnName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in double-click auto-fit - Column: {Column}", columnName);
                return false;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Handle double-click auto-fit pre všetky stĺpce
        /// </summary>
        public async Task HandleDoubleClickAutoFitAllAsync(List<object> sampleData)
        {
            try
            {
                _logger.LogInformation("🖱️ Double-click auto-fit ALL columns triggered");

                await AutoSizeAllColumnsAsync(sampleData);

                _logger.LogInformation("✅ Double-click auto-fit ALL columns completed");
                OnDoubleClickAutoFitAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in double-click auto-fit all columns");
            }
        }

        #endregion

        #region Resize Constraints

        /// <summary>
        /// Nastaví resize constraints pre stĺpec
        /// </summary>
        public void SetResizeConstraints(string columnName, double minWidth, double maxWidth)
        {
            if (string.IsNullOrEmpty(columnName) || minWidth < 0 || maxWidth < minWidth) return;

            _resizeConstraints[columnName] = new ResizeConstraints(minWidth, maxWidth);
            
            _logger.LogDebug("📏 Set resize constraints - Column: {Column}, Min: {Min}, Max: {Max}", 
                columnName, minWidth, maxWidth);
        }

        /// <summary>
        /// Získa resize constraints pre stĺpec
        /// </summary>
        public ResizeConstraints GetResizeConstraints(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return ResizeConstraints.Default;

            return _resizeConstraints.TryGetValue(columnName, out var constraints) 
                ? constraints 
                : ResizeConstraints.Default;
        }

        /// <summary>
        /// Odstráni resize constraints pre stĺpec
        /// </summary>
        public void RemoveResizeConstraints(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return;

            _resizeConstraints.Remove(columnName);
            _logger.LogDebug("🗑️ Removed resize constraints - Column: {Column}", columnName);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Nastaví default resize constraints pre všetky stĺpce
        /// </summary>
        private void SetupDefaultResizeConstraints()
        {
            foreach (var column in _columns)
            {
                _resizeConstraints[column.Name] = ResizeConstraints.Default;
            }
        }

        /// <summary>
        /// Vypočíta optimálnu šírku stĺpca
        /// </summary>
        private double CalculateOptimalWidth(string columnName, List<object> sampleData, int maxSamples)
        {
            try
            {
                var samples = sampleData.Take(maxSamples);
                var maxLength = samples
                    .Select(item => item?.ToString()?.Length ?? 0)
                    .DefaultIfEmpty(0)
                    .Max();

                // Odhad: 8 pixelov na znak + 20 pixelov padding + 10 pixelov pre sort indicator
                var estimatedWidth = Math.Max(50, (maxLength * 8) + 30);
                
                return Math.Min(estimatedWidth, 400); // Max 400px
            }
            catch
            {
                return 100.0; // Default fallback
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event pre začiatok resize operácie
        /// </summary>
        public event EventHandler<ResizeStartedEventArgs>? ResizeStarted;

        /// <summary>
        /// Event pre aktualizáciu resize operácie
        /// </summary>
        public event EventHandler<ResizeUpdatedEventArgs>? ResizeUpdated;

        /// <summary>
        /// Event pre dokončenie resize operácie
        /// </summary>
        public event EventHandler<ResizeCompletedEventArgs>? ResizeCompleted;

        /// <summary>
        /// Event pre zrušenie resize operácie
        /// </summary>
        public event EventHandler<ResizeCanceledEventArgs>? ResizeCanceled;

        /// <summary>
        /// Event pre auto-sizing stĺpca
        /// </summary>
        public event EventHandler<ColumnAutoSizedEventArgs>? ColumnAutoSized;

        /// <summary>
        /// ✅ NOVÉ: Event pre double-click auto-fit jedného stĺpca
        /// </summary>
        public event EventHandler<DoubleClickAutoFitEventArgs>? DoubleClickAutoFit;

        /// <summary>
        /// ✅ NOVÉ: Event pre double-click auto-fit všetkých stĺpcov
        /// </summary>
        public event EventHandler<EventArgs>? DoubleClickAutoFitAll;

        protected virtual void OnResizeStarted(string columnName, double originalWidth)
        {
            ResizeStarted?.Invoke(this, new ResizeStartedEventArgs(columnName, originalWidth));
        }

        protected virtual void OnResizeUpdated(string columnName, double oldWidth, double newWidth)
        {
            ResizeUpdated?.Invoke(this, new ResizeUpdatedEventArgs(columnName, oldWidth, newWidth));
        }

        protected virtual void OnResizeCompleted(string columnName, double originalWidth, double finalWidth)
        {
            ResizeCompleted?.Invoke(this, new ResizeCompletedEventArgs(columnName, originalWidth, finalWidth));
        }

        protected virtual void OnResizeCanceled(string columnName, double originalWidth)
        {
            ResizeCanceled?.Invoke(this, new ResizeCanceledEventArgs(columnName, originalWidth));
        }

        protected virtual void OnColumnAutoSized(string columnName, double oldWidth, double newWidth)
        {
            ColumnAutoSized?.Invoke(this, new ColumnAutoSizedEventArgs(columnName, oldWidth, newWidth));
        }

        /// <summary>
        /// ✅ NOVÉ: Trigger double-click auto-fit event
        /// </summary>
        protected virtual void OnDoubleClickAutoFit(string columnName)
        {
            DoubleClickAutoFit?.Invoke(this, new DoubleClickAutoFitEventArgs(columnName));
        }

        /// <summary>
        /// ✅ NOVÉ: Trigger double-click auto-fit all event
        /// </summary>
        protected virtual void OnDoubleClickAutoFitAll()
        {
            DoubleClickAutoFitAll?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"ResizeHandlingService[{_serviceInstanceId}] - " +
                   $"Initialized: {_isInitialized}, IsResizing: {_isResizing}, " +
                   $"CurrentColumn: {_currentResizingColumn ?? "None"}, Constraints: {_resizeConstraints.Count}";
        }

        #endregion
    }

    #region Helper Classes & Event Args

    /// <summary>
    /// Resize constraints pre stĺpec
    /// </summary>
    internal class ResizeConstraints
    {
        public double MinWidth { get; }
        public double MaxWidth { get; }

        public ResizeConstraints(double minWidth, double maxWidth)
        {
            MinWidth = Math.Max(0, minWidth);
            MaxWidth = Math.Max(MinWidth, maxWidth);
        }

        public static ResizeConstraints Default => new ResizeConstraints(50, 500);
    }

    // Event Args classes
    internal class ResizeStartedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OriginalWidth { get; }

        public ResizeStartedEventArgs(string columnName, double originalWidth)
        {
            ColumnName = columnName;
            OriginalWidth = originalWidth;
        }
    }

    internal class ResizeUpdatedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OldWidth { get; }
        public double NewWidth { get; }

        public ResizeUpdatedEventArgs(string columnName, double oldWidth, double newWidth)
        {
            ColumnName = columnName;
            OldWidth = oldWidth;
            NewWidth = newWidth;
        }
    }

    internal class ResizeCompletedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OriginalWidth { get; }
        public double FinalWidth { get; }

        public ResizeCompletedEventArgs(string columnName, double originalWidth, double finalWidth)
        {
            ColumnName = columnName;
            OriginalWidth = originalWidth;
            FinalWidth = finalWidth;
        }
    }

    internal class ResizeCanceledEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OriginalWidth { get; }

        public ResizeCanceledEventArgs(string columnName, double originalWidth)
        {
            ColumnName = columnName;
            OriginalWidth = originalWidth;
        }
    }

    internal class ColumnAutoSizedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public double OldWidth { get; }
        public double NewWidth { get; }

        public ColumnAutoSizedEventArgs(string columnName, double oldWidth, double newWidth)
        {
            ColumnName = columnName;
            OldWidth = oldWidth;
            NewWidth = newWidth;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args pre double-click auto-fit
    /// </summary>
    internal class DoubleClickAutoFitEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public DateTime Timestamp { get; }

        public DoubleClickAutoFitEventArgs(string columnName)
        {
            ColumnName = columnName;
            Timestamp = DateTime.UtcNow;
        }
    }

    #endregion
}