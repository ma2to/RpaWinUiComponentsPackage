// Services/UI/DataGridCoreService.cs - ✅ NOVÝ: Core UI Component Service
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.UI
{
    /// <summary>
    /// Service pre Core UI Component - INTERNAL
    /// Zodpovedný za základnú inicializáciu, konfiguráciu, XAML element access, stav UI
    /// </summary>
    internal class DataGridCoreService : INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Referencia na hlavný DataGrid control
        private AdvancedDataGrid? _dataGrid;
        
        // Configuration management
        private DataGridConfiguration? _configuration;
        private readonly Dictionary<string, object?> _configurationState = new();
        
        // UI state tracking
        private readonly Dictionary<string, object?> _uiStateSnapshot = new();
        private bool _isInitialized = false;
        private bool _isDisposed = false;
        
        // Performance tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, double> _operationDurations = new();
        private readonly Dictionary<string, int> _operationCounters = new();

        // Component state
        private string _componentInstanceId = string.Empty;
        private int _totalCellsRendered = 0;
        private int _totalValidationErrors = 0;
        private DateTime _lastDataUpdate = DateTime.MinValue;

        #endregion

        #region Constructor

        public DataGridCoreService()
        {
            _logger = NullLogger.Instance;
            _componentInstanceId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🏗️ DataGridCoreService created - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<string>? ConfigurationChanged;
        public event EventHandler<Dictionary<string, object?>>? StateChanged;

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje core service s referenciou na DataGrid
        /// </summary>
        public async Task InitializeAsync(AdvancedDataGrid dataGrid, DataGridConfiguration? configuration = null)
        {
            try
            {
                _logger.LogInformation("🚀 DataGridCoreService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
                _configuration = configuration ?? new DataGridConfiguration();

                // Inicializuj komponenty
                await InitializeComponentStateAsync();
                await InitializeConfigurationAsync();
                await InitializePerformanceTrackingAsync();
                await InitializeUIStateTrackingAsync();

                _isInitialized = true;
                _logger.LogInformation("✅ DataGridCoreService initialized successfully - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataGridCoreService initialization");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje stav komponentu
        /// </summary>
        private async Task InitializeComponentStateAsync()
        {
            try
            {
                _logger.LogDebug("🔧 Initializing component state");
                
                await Task.Run(() =>
                {
                    _componentInstanceId = Guid.NewGuid().ToString("N")[..8];
                    _totalCellsRendered = 0;
                    _totalValidationErrors = 0;
                    _lastDataUpdate = DateTime.UtcNow;
                    
                    LogSystemInfo();
                });

                _logger.LogInformation("✅ Component state initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing component state");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje konfiguráciu
        /// </summary>
        private async Task InitializeConfigurationAsync()
        {
            try
            {
                _logger.LogDebug("⚙️ Initializing configuration");
                
                await Task.Run(() =>
                {
                    if (_configuration != null)
                    {
                        _configurationState["AutoAddEnabled"] = _configuration.AutoAddEnabled;
                        _configurationState["UnifiedRowCount"] = _configuration.UnifiedRowCount;
                        _configurationState["CheckBoxColumnEnabled"] = _configuration.CheckBoxColumnEnabled;
                        _configurationState["ValidAlertsMinWidth"] = _configuration.ValidAlertsMinWidth;
                    }
                });

                OnConfigurationChanged("Initial");
                _logger.LogInformation("✅ Configuration initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing configuration");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje performance tracking
        /// </summary>
        private async Task InitializePerformanceTrackingAsync()
        {
            try
            {
                _logger.LogDebug("📊 Initializing performance tracking");
                
                await Task.Run(() =>
                {
                    _operationStartTimes.Clear();
                    _operationDurations.Clear();
                    _operationCounters.Clear();
                });

                _logger.LogInformation("✅ Performance tracking initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing performance tracking");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje UI state tracking
        /// </summary>
        private async Task InitializeUIStateTrackingAsync()
        {
            try
            {
                _logger.LogDebug("🖥️ Initializing UI state tracking");
                
                await Task.Run(() =>
                {
                    _uiStateSnapshot.Clear();
                    UpdateUIStateSnapshot("Initialization");
                });

                _logger.LogInformation("✅ UI state tracking initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing UI state tracking");
                throw;
            }
        }

        #endregion

        #region System Information

        /// <summary>
        /// Loguje systémové informácie
        /// </summary>
        private void LogSystemInfo()
        {
            try
            {
                var systemInfo = new
                {
                    OS = Environment.OSVersion.ToString(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    CLRVersion = Environment.Version.ToString(),
                    Is64BitProcess = Environment.Is64BitProcess,
                    TickCount = Environment.TickCount
                };

                _logger.LogInformation("🖥️ System Info - OS: {OS}, Machine: {MachineName}, User: {UserName}, " +
                    "Cores: {ProcessorCount}, Memory: {WorkingSet} bytes, CLR: {CLRVersion}, x64: {Is64BitProcess}",
                    systemInfo.OS, systemInfo.MachineName, systemInfo.UserName, 
                    systemInfo.ProcessorCount, systemInfo.WorkingSet, systemInfo.CLRVersion, systemInfo.Is64BitProcess);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not log system information");
            }
        }

        #endregion

        #region XAML Element Access

        /// <summary>
        /// Získa HeaderStackPanel element
        /// </summary>
        public StackPanel? GetHeaderStackPanel()
        {
            try
            {
                var element = _dataGrid?.FindName("HeaderStackPanel") as StackPanel;
                _logger.LogTrace("🔍 GetHeaderStackPanel - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting HeaderStackPanel");
                return null;
            }
        }

        /// <summary>
        /// Získa HeaderScrollViewer element
        /// </summary>
        public ScrollViewer? GetHeaderScrollViewer()
        {
            try
            {
                var element = _dataGrid?.FindName("HeaderScrollViewer") as ScrollViewer;
                _logger.LogTrace("🔍 GetHeaderScrollViewer - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting HeaderScrollViewer");
                return null;
            }
        }

        /// <summary>
        /// Získa DataGridScrollViewer element
        /// </summary>
        public ScrollViewer? GetDataGridScrollViewer()
        {
            try
            {
                var element = _dataGrid?.FindName("DataGridScrollViewer") as ScrollViewer;
                _logger.LogTrace("🔍 GetDataGridScrollViewer - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting DataGridScrollViewer");
                return null;
            }
        }

        /// <summary>
        /// Získa DataRowsContainer element
        /// </summary>
        public ItemsControl? GetDataRowsContainer()
        {
            try
            {
                var element = _dataGrid?.FindName("DataRowsContainer") as ItemsControl;
                _logger.LogTrace("🔍 GetDataRowsContainer - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting DataRowsContainer");
                return null;
            }
        }

        /// <summary>
        /// Získa MainContentGrid element
        /// </summary>
        public Grid? GetMainContentGrid()
        {
            try
            {
                var element = _dataGrid?.FindName("MainContentGrid") as Grid;
                _logger.LogTrace("🔍 GetMainContentGrid - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting MainContentGrid");
                return null;
            }
        }

        /// <summary>
        /// Získa LoadingOverlay element
        /// </summary>
        public Border? GetLoadingOverlay()
        {
            try
            {
                var element = _dataGrid?.FindName("LoadingOverlay") as Border;
                _logger.LogTrace("🔍 GetLoadingOverlay - Found: {Found}", element != null);
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting LoadingOverlay");
                return null;
            }
        }

        #endregion

        #region Configuration Management

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public async Task UpdateConfigurationAsync(DataGridConfiguration newConfiguration)
        {
            try
            {
                _logger.LogInformation("⚙️ Updating configuration");

                await Task.Run(() =>
                {
                    _configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
                    
                    _configurationState["AutoAddEnabled"] = _configuration.AutoAddEnabled;
                    _configurationState["UnifiedRowCount"] = _configuration.UnifiedRowCount;
                    _configurationState["CheckBoxColumnEnabled"] = _configuration.CheckBoxColumnEnabled;
                    _configurationState["ValidAlertsMinWidth"] = _configuration.ValidAlertsMinWidth;
                });

                OnConfigurationChanged("Update");
                _logger.LogInformation("✅ Configuration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating configuration");
                throw;
            }
        }

        /// <summary>
        /// Získa aktuálnu konfiguráciu
        /// </summary>
        public DataGridConfiguration? GetConfiguration()
        {
            return _configuration;
        }

        /// <summary>
        /// Aktivuje zmenu konfigurácie
        /// </summary>
        private void OnConfigurationChanged(string reason)
        {
            try
            {
                ConfigurationChanged?.Invoke(this, reason);
                _logger.LogDebug("⚙️ Configuration changed - Reason: {Reason}", reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error notifying configuration change");
            }
        }

        #endregion

        #region Performance Tracking

        /// <summary>
        /// Začne sledovanie operácie
        /// </summary>
        public void StartOperation(string operationName)
        {
            try
            {
                _operationStartTimes[operationName] = DateTime.UtcNow;
                _operationCounters[operationName] = _operationCounters.GetValueOrDefault(operationName, 0) + 1;
                
                _logger.LogTrace("⏱️ Started operation: {OperationName} (#{Count})", operationName, _operationCounters[operationName]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting operation tracking: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// Ukončí sledovanie operácie a vráti trvanie v ms
        /// </summary>
        public double EndOperation(string operationName)
        {
            try
            {
                if (_operationStartTimes.TryGetValue(operationName, out var startTime))
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _operationDurations[operationName] = duration;
                    _operationStartTimes.Remove(operationName);
                    
                    _logger.LogTrace("⏱️ Completed operation: {OperationName} in {Duration}ms", operationName, duration);
                    return duration;
                }
                else
                {
                    _logger.LogWarning("⚠️ End operation called without start: {OperationName}", operationName);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ending operation tracking: {OperationName}", operationName);
                return 0;
            }
        }

        /// <summary>
        /// Získa štatistiky výkonu
        /// </summary>
        public Dictionary<string, object> GetPerformanceStatistics()
        {
            try
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalOperations"] = _operationCounters.Values.Sum(),
                    ["OperationCounts"] = new Dictionary<string, int>(_operationCounters),
                    ["OperationDurations"] = new Dictionary<string, double>(_operationDurations),
                    ["TotalCellsRendered"] = _totalCellsRendered,
                    ["TotalValidationErrors"] = _totalValidationErrors,
                    ["LastDataUpdate"] = _lastDataUpdate,
                    ["ComponentInstanceId"] = _componentInstanceId
                };

                _logger.LogDebug("📊 Performance statistics retrieved - Total operations: {TotalOperations}", 
                    stats["TotalOperations"]);
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting performance statistics");
                return new Dictionary<string, object>();
            }
        }

        #endregion

        #region UI State Management

        /// <summary>
        /// Aktualizuje snapshot UI stavu
        /// </summary>
        public void UpdateUIStateSnapshot(string reason)
        {
            try
            {
                _uiStateSnapshot["Timestamp"] = DateTime.UtcNow;
                _uiStateSnapshot["Reason"] = reason;
                _uiStateSnapshot["TotalCellsRendered"] = _totalCellsRendered;
                _uiStateSnapshot["TotalValidationErrors"] = _totalValidationErrors;
                _uiStateSnapshot["LastDataUpdate"] = _lastDataUpdate;
                _uiStateSnapshot["IsInitialized"] = _isInitialized;
                _uiStateSnapshot["ComponentInstanceId"] = _componentInstanceId;

                OnStateChanged();
                _logger.LogTrace("📸 UI state snapshot updated - Reason: {Reason}", reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating UI state snapshot");
            }
        }

        /// <summary>
        /// Získa aktuálny UI state snapshot
        /// </summary>
        public Dictionary<string, object?> GetUIStateSnapshot()
        {
            return new Dictionary<string, object?>(_uiStateSnapshot);
        }

        /// <summary>
        /// Loguje stav komponentu
        /// </summary>
        public void LogComponentState(string context)
        {
            try
            {
                _logger.LogInformation("📊 Component State [{Context}] - InstanceId: {InstanceId}, " +
                    "Initialized: {IsInitialized}, CellsRendered: {TotalCellsRendered}, " +
                    "ValidationErrors: {TotalValidationErrors}, LastUpdate: {LastDataUpdate}",
                    context, _componentInstanceId, _isInitialized, _totalCellsRendered, 
                    _totalValidationErrors, _lastDataUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error logging component state");
            }
        }

        /// <summary>
        /// Aktivuje zmenu stavu
        /// </summary>
        private void OnStateChanged()
        {
            try
            {
                StateChanged?.Invoke(this, GetUIStateSnapshot());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error notifying state change");
            }
        }

        #endregion

        #region Loading State Management

        /// <summary>
        /// Zobrazí loading overlay
        /// </summary>
        public async Task ShowLoadingAsync(string message = "Loading...")
        {
            try
            {
                var loadingOverlay = GetLoadingOverlay();
                if (loadingOverlay != null)
                {
                    _dataGrid?.DispatcherQueue.TryEnqueue(() =>
                    {
                        loadingOverlay.Visibility = Visibility.Visible;
                        
                        // Update loading message if TextBlock exists
                        if (loadingOverlay.Child is TextBlock textBlock)
                        {
                            textBlock.Text = message;
                        }
                    });

                    _logger.LogDebug("🔄 Loading overlay shown - Message: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error showing loading overlay");
            }
        }

        /// <summary>
        /// Skryje loading overlay
        /// </summary>
        public async Task HideLoadingAsync()
        {
            try
            {
                var loadingOverlay = GetLoadingOverlay();
                if (loadingOverlay != null)
                {
                    _dataGrid?.DispatcherQueue.TryEnqueue(() =>
                    {
                        loadingOverlay.Visibility = Visibility.Collapsed;
                    });

                    _logger.LogDebug("✅ Loading overlay hidden");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error hiding loading overlay");
            }
        }

        #endregion

        #region Cell and Data Statistics

        /// <summary>
        /// Aktualizuje počet renderovaných buniek
        /// </summary>
        public void UpdateCellsRendered(int count)
        {
            _totalCellsRendered = count;
            UpdateUIStateSnapshot("CellsRendered");
        }

        /// <summary>
        /// Aktualizuje počet validačných chýb
        /// </summary>
        public void UpdateValidationErrors(int count)
        {
            _totalValidationErrors = count;
            UpdateUIStateSnapshot("ValidationErrors");
        }

        /// <summary>
        /// Označí poslednú aktualizáciu dát
        /// </summary>
        public void MarkDataUpdate()
        {
            _lastDataUpdate = DateTime.UtcNow;
            UpdateUIStateSnapshot("DataUpdate");
        }

        #endregion

        #region Properties

        /// <summary>
        /// ID inštancie komponentu
        /// </summary>
        public string ComponentInstanceId => _componentInstanceId;

        /// <summary>
        /// Či je služba inicializovaná
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Celkový počet renderovaných buniek
        /// </summary>
        public int TotalCellsRendered => _totalCellsRendered;

        /// <summary>
        /// Celkový počet validačných chýb
        /// </summary>
        public int TotalValidationErrors => _totalValidationErrors;

        /// <summary>
        /// Čas poslednej aktualizácie dát
        /// </summary>
        public DateTime LastDataUpdate => _lastDataUpdate;

        #endregion

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                if (_isDisposed) return;

                _logger.LogInformation("🧹 Disposing DataGridCoreService - InstanceId: {InstanceId}", _serviceInstanceId);

                // Clear collections
                _configurationState.Clear();
                _uiStateSnapshot.Clear();
                _operationStartTimes.Clear();
                _operationDurations.Clear();
                _operationCounters.Clear();

                // Reset references
                _dataGrid = null;
                _configuration = null;

                _isDisposed = true;
                _logger.LogInformation("✅ DataGridCoreService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing DataGridCoreService");
            }
        }

        #endregion
    }
}