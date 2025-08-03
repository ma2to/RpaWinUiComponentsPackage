// Services/Operations/DataGridBindingService.cs - ✅ NOVÝ: Data Binding Service  
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Row;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations
{
    /// <summary>
    /// Service pre Data Binding - INTERNAL
    /// Zodpovedný za data loading, refresh, cell value binding, validation triggers
    /// </summary>
    internal class DataGridBindingService : INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // Referencia na hlavný DataGrid control
        private AdvancedDataGrid? _dataGrid;

        // Data management
        private readonly ObservableCollection<Dictionary<string, object?>> _gridData = new();
        private readonly List<Models.Grid.ColumnDefinition> _columns = new();
        private readonly Dictionary<string, Type> _columnTypes = new();

        // Binding state
        private bool _isBinding = false;
        private bool _isRefreshing = false;
        private bool _isUpdating = false;

        // Data source
        private object? _itemsSource;
        private DataTable? _dataTable;
        private bool _hasDataSource = false;

        // Auto-refresh configuration
        private bool _autoRefreshEnabled = false;
        private TimeSpan _autoRefreshInterval = TimeSpan.FromSeconds(30);
        private DispatcherTimer? _autoRefreshTimer;

        // Change tracking
        private readonly Dictionary<string, object?> _originalValues = new();
        private readonly HashSet<string> _changedCells = new();
        private bool _hasUnsavedChanges = false;

        // Validation integration
        private IValidationService? _validationService;
        private readonly Dictionary<string, List<string>> _cellValidationErrors = new();

        // Performance tracking
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private int _refreshCount = 0;
        private TimeSpan _averageRefreshTime = TimeSpan.Zero;

        // Initialization state
        private bool _isInitialized = false;

        #endregion

        #region Events

        /// <summary>
        /// Event pre zmenu dát
        /// </summary>
        public event EventHandler<DataChangedEventArgs>? DataChanged;

        /// <summary>
        /// Event pre zmenu bunky
        /// </summary>
        public event EventHandler<CellValueChangedEventArgs>? CellValueChanged;

        /// <summary>
        /// Event pre refresh completion
        /// </summary>
        public event EventHandler<RefreshCompletedEventArgs>? RefreshCompleted;


        /// <summary>
        /// INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Constructor

        public DataGridBindingService()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🏗️ DataGridBindingService created - InstanceId: {InstanceId}", _serviceInstanceId);

            // Setup data collection change monitoring
            _gridData.CollectionChanged += OnGridDataCollectionChanged;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje binding service s referenciou na DataGrid
        /// </summary>
        public async Task InitializeAsync(AdvancedDataGrid dataGrid, List<Models.Grid.ColumnDefinition> columns, IValidationService? validationService = null)
        {
            try
            {
                _logger.LogInformation("🚀 DataGridBindingService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _serviceInstanceId);

                _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
                _validationService = validationService;

                await InitializeColumnsAsync(columns);
                await SetupAutoRefreshAsync();
                await InitializeDataSourceAsync();

                _isInitialized = true;
                _logger.LogInformation("✅ DataGridBindingService initialized successfully - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataGridBindingService initialization");
                throw;
            }
        }

        /// <summary>
        /// Inicializuje columns
        /// </summary>
        private async Task InitializeColumnsAsync(List<Models.Grid.ColumnDefinition> columns)
        {
            try
            {
                await Task.Run(() =>
                {
                    _columns.Clear();
                    _columnTypes.Clear();

                    if (columns != null)
                    {
                        _columns.AddRange(columns);

                        foreach (var column in columns)
                        {
                            _columnTypes[column.Name] = column.DataType;
                            _logger.LogTrace("📊 Registered column: {Name} ({Type})", column.Name, column.DataType.Name);
                        }
                    }

                    _logger.LogDebug("📊 Initialized {Count} columns", _columns.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing columns");
                throw;
            }
        }

        /// <summary>
        /// Nastaví auto-refresh
        /// </summary>
        private async Task SetupAutoRefreshAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (_autoRefreshEnabled && _autoRefreshTimer == null)
                    {
                        _autoRefreshTimer = new DispatcherTimer
                        {
                            Interval = _autoRefreshInterval
                        };
                        _autoRefreshTimer.Tick += OnAutoRefreshTick;

                        _logger.LogDebug("⏰ Auto-refresh setup with interval: {Interval}", _autoRefreshInterval);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting up auto-refresh");
            }
        }

        /// <summary>
        /// Inicializuje data source
        /// </summary>
        private async Task InitializeDataSourceAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Create empty data structure
                    for (int i = 0; i < 10; i++) // Default empty rows
                    {
                        var row = CreateEmptyRow();
                        _gridData.Add(row);
                    }

                    _logger.LogDebug("📊 Initialized data source with {Count} empty rows", _gridData.Count);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing data source");
            }
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// Načíta dáta z DataTable
        /// </summary>
        public async Task LoadDataFromDataTableAsync(DataTable dataTable)
        {
            try
            {
                _logger.LogInformation("📥 Loading data from DataTable - Rows: {RowCount}, Columns: {ColumnCount}", 
                    dataTable.Rows.Count, dataTable.Columns.Count);

                _isBinding = true;
                var startTime = DateTime.Now;

                await Task.Run(() =>
                {
                    _dataTable = dataTable;
                    _gridData.Clear();

                    // Convert DataTable rows to dictionary format
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        var rowDict = new Dictionary<string, object?>();

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            var value = dataRow[column];
                            rowDict[column.ColumnName] = value == DBNull.Value ? null : value;
                        }

                        _gridData.Add(rowDict);
                    }

                    _hasDataSource = true;
                });

                var loadTime = DateTime.Now - startTime;
                _isBinding = false;

                await ValidateLoadedDataAsync();
                FireDataChangedEvent(DataChangeType.Loaded, _gridData.Count);

                _logger.LogInformation("✅ Data loaded successfully - {RowCount} rows in {LoadTime}ms", 
                    _gridData.Count, loadTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _isBinding = false;
                _logger.LogError(ex, "❌ Error loading data from DataTable");
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta z ObservableCollection
        /// </summary>
        public async Task LoadDataFromCollectionAsync<T>(ObservableCollection<T> collection)
        {
            try
            {
                _logger.LogInformation("📥 Loading data from ObservableCollection - Items: {ItemCount}", collection.Count);

                _isBinding = true;
                var startTime = DateTime.Now;

                await Task.Run(() =>
                {
                    _gridData.Clear();

                    foreach (var item in collection)
                    {
                        var rowDict = ConvertObjectToDictionary(item);
                        _gridData.Add(rowDict);
                    }

                    _hasDataSource = true;
                });

                var loadTime = DateTime.Now - startTime;
                _isBinding = false;

                await ValidateLoadedDataAsync();
                FireDataChangedEvent(DataChangeType.Loaded, _gridData.Count);

                _logger.LogInformation("✅ Data loaded from collection - {RowCount} rows in {LoadTime}ms", 
                    _gridData.Count, loadTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _isBinding = false;
                _logger.LogError(ex, "❌ Error loading data from collection");
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta z Dictionary list
        /// </summary>
        public async Task LoadDataFromDictionaryListAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                _logger.LogInformation("📥 Loading data from Dictionary list - Items: {ItemCount}", data.Count);

                _isBinding = true;
                var startTime = DateTime.Now;

                await Task.Run(() =>
                {
                    _gridData.Clear();

                    foreach (var item in data)
                    {
                        var rowDict = new Dictionary<string, object?>(item);
                        ProcessRowData(rowDict);
                        _gridData.Add(rowDict);
                    }

                    _hasDataSource = true;
                });

                var loadTime = DateTime.Now - startTime;
                _isBinding = false;

                await ValidateLoadedDataAsync();
                FireDataChangedEvent(DataChangeType.Loaded, _gridData.Count);

                _logger.LogInformation("✅ Data loaded from dictionary list - {RowCount} rows in {LoadTime}ms", 
                    _gridData.Count, loadTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _isBinding = false;
                _logger.LogError(ex, "❌ Error loading data from dictionary list");
                throw;
            }
        }

        #endregion

        #region Data Refresh

        /// <summary>
        /// Refresh všetkých dát
        /// </summary>
        public async Task RefreshDataAsync()
        {
            try
            {
                _logger.LogInformation("🔄 Refreshing all data");

                _isRefreshing = true;
                var startTime = DateTime.Now;

                await Task.Run(() =>
                {
                    // If we have a data source, reload from it
                    if (_dataTable != null)
                    {
                        LoadDataFromDataTableAsync(_dataTable).Wait();
                    }
                    else
                    {
                        // Refresh existing data in place
                        RefreshExistingData();
                    }
                });

                var refreshTime = DateTime.Now - startTime;
                _lastRefreshTime = DateTime.Now;
                _refreshCount++;
                UpdateAverageRefreshTime(refreshTime);

                _isRefreshing = false;

                FireRefreshCompletedEvent(refreshTime, _gridData.Count);
                _logger.LogInformation("✅ Data refresh completed - {RowCount} rows in {RefreshTime}ms", 
                    _gridData.Count, refreshTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _isRefreshing = false;
                _logger.LogError(ex, "❌ Error refreshing data");
                throw;
            }
        }

        /// <summary>
        /// Refresh konkrétneho riadku
        /// </summary>
        public async Task RefreshRowAsync(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _gridData.Count)
                {
                    _logger.LogWarning("⚠️ Invalid row index for refresh: {RowIndex}", rowIndex);
                    return;
                }

                _logger.LogDebug("🔄 Refreshing row: {RowIndex}", rowIndex);

                await Task.Run(() =>
                {
                    var row = _gridData[rowIndex];
                    ProcessRowData(row);
                });

                await ValidateRowAsync(rowIndex);
                FireDataChangedEvent(DataChangeType.RowUpdated, 1, rowIndex);

                _logger.LogDebug("✅ Row refreshed: {RowIndex}", rowIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error refreshing row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// Refresh konkrétnej bunky
        /// </summary>
        public async Task RefreshCellAsync(int rowIndex, string columnName)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _gridData.Count)
                {
                    _logger.LogWarning("⚠️ Invalid row index for cell refresh: {RowIndex}", rowIndex);
                    return;
                }

                if (!_columnTypes.ContainsKey(columnName))
                {
                    _logger.LogWarning("⚠️ Unknown column for cell refresh: {ColumnName}", columnName);
                    return;
                }

                _logger.LogDebug("🔄 Refreshing cell: [{RowIndex}, {ColumnName}]", rowIndex, columnName);

                await Task.Run(() =>
                {
                    var row = _gridData[rowIndex];
                    if (row.ContainsKey(columnName))
                    {
                        var value = row[columnName];
                        var processedValue = ProcessCellValue(columnName, value);
                        row[columnName] = processedValue;
                    }
                });

                await ValidateCellAsync(rowIndex, columnName);
                FireCellValueChangedEvent(rowIndex, columnName, _gridData[rowIndex][columnName]);

                _logger.LogDebug("✅ Cell refreshed: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error refreshing cell: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            }
        }

        #endregion

        #region Cell Value Management

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        public async Task SetCellValueAsync(int rowIndex, string columnName, object? value)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _gridData.Count)
                {
                    _logger.LogWarning("⚠️ Invalid row index for set cell value: {RowIndex}", rowIndex);
                    return;
                }

                if (!_columnTypes.ContainsKey(columnName))
                {
                    _logger.LogWarning("⚠️ Unknown column for set cell value: {ColumnName}", columnName);
                    return;
                }

                _logger.LogDebug("💾 Setting cell value: [{RowIndex}, {ColumnName}] = {Value}", 
                    rowIndex, columnName, value);

                _isUpdating = true;

                await Task.Run(() =>
                {
                    var row = _gridData[rowIndex];
                    var oldValue = row.ContainsKey(columnName) ? row[columnName] : null;
                    var processedValue = ProcessCellValue(columnName, value);

                    // Store original value for change tracking
                    var cellKey = $"{rowIndex}_{columnName}";
                    if (!_originalValues.ContainsKey(cellKey))
                    {
                        _originalValues[cellKey] = oldValue;
                    }

                    row[columnName] = processedValue;

                    // Track changes
                    if (!Equals(oldValue, processedValue))
                    {
                        _changedCells.Add(cellKey);
                        _hasUnsavedChanges = true;
                    }
                });

                await ValidateCellAsync(rowIndex, columnName);
                FireCellValueChangedEvent(rowIndex, columnName, value);

                _isUpdating = false;

                _logger.LogDebug("✅ Cell value set: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            }
            catch (Exception ex)
            {
                _isUpdating = false;
                _logger.LogError(ex, "❌ Error setting cell value: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            }
        }

        /// <summary>
        /// Získa hodnotu bunky
        /// </summary>
        public async Task<object?> GetCellValueAsync(int rowIndex, string columnName)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _gridData.Count)
                {
                    _logger.LogWarning("⚠️ Invalid row index for get cell value: {RowIndex}", rowIndex);
                    return null;
                }

                if (!_columnTypes.ContainsKey(columnName))
                {
                    _logger.LogWarning("⚠️ Unknown column for get cell value: {ColumnName}", columnName);
                    return null;
                }

                return await Task.Run(() =>
                {
                    var row = _gridData[rowIndex];
                    var value = row.ContainsKey(columnName) ? row[columnName] : null;

                    _logger.LogTrace("📤 Got cell value: [{RowIndex}, {ColumnName}] = {Value}", 
                        rowIndex, columnName, value);

                    return value;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting cell value: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
                return null;
            }
        }

        #endregion

        #region Row Management

        /// <summary>
        /// Pridá nový riadok
        /// </summary>
        public async Task<int> AddRowAsync(Dictionary<string, object?>? initialData = null)
        {
            try
            {
                _logger.LogDebug("➕ Adding new row");

                var newIndex = await Task.Run(() =>
                {
                    var row = initialData != null ? new Dictionary<string, object?>(initialData) : CreateEmptyRow();
                    ProcessRowData(row);
                    
                    _gridData.Add(row);
                    var index = _gridData.Count - 1;

                    _hasUnsavedChanges = true;
                    return index;
                });

                await ValidateRowAsync(newIndex);
                FireDataChangedEvent(DataChangeType.RowAdded, 1, newIndex);

                _logger.LogDebug("✅ Row added at index: {Index}", newIndex);
                return newIndex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error adding row");
                return -1;
            }
        }

        /// <summary>
        /// Odstráni riadok
        /// </summary>
        public async Task RemoveRowAsync(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= _gridData.Count)
                {
                    _logger.LogWarning("⚠️ Invalid row index for removal: {RowIndex}", rowIndex);
                    return;
                }

                _logger.LogDebug("🗑️ Removing row: {RowIndex}", rowIndex);

                await Task.Run(() =>
                {
                    _gridData.RemoveAt(rowIndex);
                    
                    // Remove change tracking for this row
                    var keysToRemove = _changedCells.Where(k => k.StartsWith($"{rowIndex}_")).ToList();
                    foreach (var key in keysToRemove)
                    {
                        _changedCells.Remove(key);
                        _originalValues.Remove(key);
                    }

                    _hasUnsavedChanges = true;
                });

                FireDataChangedEvent(DataChangeType.RowRemoved, 1, rowIndex);

                _logger.LogDebug("✅ Row removed: {RowIndex}", rowIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removing row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// Vymaže všetky dáta
        /// </summary>
        public async Task ClearDataAsync()
        {
            try
            {
                _logger.LogInformation("🧹 Clearing all data");

                var oldCount = _gridData.Count;

                await Task.Run(() =>
                {
                    _gridData.Clear();
                    _changedCells.Clear();
                    _originalValues.Clear();
                    _cellValidationErrors.Clear();
                    _hasUnsavedChanges = false;
                });

                FireDataChangedEvent(DataChangeType.Cleared, oldCount);

                _logger.LogInformation("✅ Data cleared - {OldCount} rows removed", oldCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing data");
            }
        }

        #endregion

        #region Data Processing

        /// <summary>
        /// Vytvorí prázdny riadok
        /// </summary>
        private Dictionary<string, object?> CreateEmptyRow()
        {
            var row = new Dictionary<string, object?>();

            foreach (var column in _columns)
            {
                row[column.Name] = GetDefaultValueForType(column.DataType);
            }

            return row;
        }

        /// <summary>
        /// Spracuje dáta riadku
        /// </summary>
        private void ProcessRowData(Dictionary<string, object?> row)
        {
            foreach (var column in _columns)
            {
                if (row.ContainsKey(column.Name))
                {
                    var value = row[column.Name];
                    row[column.Name] = ProcessCellValue(column.Name, value);
                }
                else
                {
                    row[column.Name] = GetDefaultValueForType(column.DataType);
                }
            }
        }

        /// <summary>
        /// Spracuje hodnotu bunky
        /// </summary>
        private object? ProcessCellValue(string columnName, object? value)
        {
            if (value == null) return null;

            if (_columnTypes.TryGetValue(columnName, out var targetType))
            {
                try
                {
                    return ConvertValueToType(value, targetType);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Type conversion failed for column {ColumnName}: {Value} → {TargetType}", 
                        columnName, value, targetType.Name);
                    return value; // Return original value if conversion fails
                }
            }

            return value;
        }

        /// <summary>
        /// Konvertuje hodnotu na cieľový typ
        /// </summary>
        private object? ConvertValueToType(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
                return value.ToString();
            if (underlyingType == typeof(int))
                return Convert.ToInt32(value);
            if (underlyingType == typeof(decimal))
                return Convert.ToDecimal(value);
            if (underlyingType == typeof(double))
                return Convert.ToDouble(value);
            if (underlyingType == typeof(DateTime))
                return Convert.ToDateTime(value);
            if (underlyingType == typeof(bool))
                return Convert.ToBoolean(value);

            return Convert.ChangeType(value, underlyingType);
        }

        /// <summary>
        /// Získa default hodnotu pre typ
        /// </summary>
        private object? GetDefaultValueForType(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                return Activator.CreateInstance(type);
            return null;
        }

        /// <summary>
        /// Konvertuje objekt na dictionary
        /// </summary>
        private Dictionary<string, object?> ConvertObjectToDictionary(object? obj)
        {
            var result = new Dictionary<string, object?>();

            if (obj == null) return result;

            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    result[prop.Name] = value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error reading property: {PropertyName}", prop.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Refresh existujúcich dát
        /// </summary>
        private void RefreshExistingData()
        {
            for (int i = 0; i < _gridData.Count; i++)
            {
                ProcessRowData(_gridData[i]);
            }
        }

        #endregion

        #region Validation Integration

        /// <summary>
        /// Validuje načítané dáta
        /// </summary>
        private async Task ValidateLoadedDataAsync()
        {
            try
            {
                if (_validationService == null) return;

                _logger.LogDebug("🔍 Validating loaded data");

                for (int i = 0; i < _gridData.Count; i++)
                {
                    await ValidateRowAsync(i);
                }

                _logger.LogDebug("✅ Data validation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating loaded data");
            }
        }

        /// <summary>
        /// Validuje riadok
        /// </summary>
        private async Task ValidateRowAsync(int rowIndex)
        {
            try
            {
                if (_validationService == null || rowIndex >= _gridData.Count) return;

                var row = _gridData[rowIndex];
                foreach (var column in _columns)
                {
                    await ValidateCellAsync(rowIndex, column.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating row: {RowIndex}", rowIndex);
            }
        }

        /// <summary>
        /// Validuje bunku
        /// </summary>
        private async Task ValidateCellAsync(int rowIndex, string columnName)
        {
            try
            {
                if (_validationService == null || rowIndex >= _gridData.Count) return;

                var value = await GetCellValueAsync(rowIndex, columnName);
                
                // Perform validation (placeholder implementation)
                // var isValid = await _validationService.ValidateCellAsync(columnName, value);
                
                var cellKey = $"{rowIndex}_{columnName}";
                if (_cellValidationErrors.ContainsKey(cellKey))
                {
                    _cellValidationErrors.Remove(cellKey);
                }

                // If validation fails, add error
                // if (!isValid) _cellValidationErrors[cellKey] = new List<string> { "Validation error" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error validating cell: [{RowIndex}, {ColumnName}]", rowIndex, columnName);
            }
        }

        #endregion

        #region Auto-Refresh

        /// <summary>
        /// Handler pre auto-refresh timer
        /// </summary>
        private async void OnAutoRefreshTick(object? sender, object e)
        {
            try
            {
                if (_isBinding || _isRefreshing || _isUpdating) return;

                _logger.LogDebug("⏰ Auto-refresh triggered");
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during auto-refresh");
            }
        }

        /// <summary>
        /// Spustí auto-refresh
        /// </summary>
        public void StartAutoRefresh()
        {
            try
            {
                if (_autoRefreshTimer != null)
                {
                    _autoRefreshTimer.Start();
                    _autoRefreshEnabled = true;
                    _logger.LogInformation("▶️ Auto-refresh started with interval: {Interval}", _autoRefreshInterval);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error starting auto-refresh");
            }
        }

        /// <summary>
        /// Zastaví auto-refresh
        /// </summary>
        public void StopAutoRefresh()
        {
            try
            {
                if (_autoRefreshTimer != null)
                {
                    _autoRefreshTimer.Stop();
                    _autoRefreshEnabled = false;
                    _logger.LogInformation("⏸️ Auto-refresh stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error stopping auto-refresh");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler pre zmenu grid data collection
        /// </summary>
        private void OnGridDataCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                _logger.LogTrace("🔄 Grid data collection changed: {Action}", e.Action);
                OnPropertyChanged(nameof(RowCount));
                OnPropertyChanged(nameof(HasData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling grid data collection change");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Aktualizuje priemerný refresh time
        /// </summary>
        private void UpdateAverageRefreshTime(TimeSpan refreshTime)
        {
            var total = _averageRefreshTime.TotalMilliseconds * (_refreshCount - 1) + refreshTime.TotalMilliseconds;
            _averageRefreshTime = TimeSpan.FromMilliseconds(total / _refreshCount);
        }

        /// <summary>
        /// Vyvolá DataChanged event
        /// </summary>
        private void FireDataChangedEvent(DataChangeType changeType, int affectedRows, int? affectedRowIndex = null)
        {
            try
            {
                DataChanged?.Invoke(this, new DataChangedEventArgs(changeType, affectedRows, affectedRowIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error firing data changed event");
            }
        }

        /// <summary>
        /// Vyvolá CellValueChanged event
        /// </summary>
        private void FireCellValueChangedEvent(int rowIndex, string columnName, object? newValue)
        {
            try
            {
                CellValueChanged?.Invoke(this, new CellValueChangedEventArgs(rowIndex, columnName, newValue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error firing cell value changed event");
            }
        }

        /// <summary>
        /// Vyvolá RefreshCompleted event
        /// </summary>
        private void FireRefreshCompletedEvent(TimeSpan refreshTime, int rowCount)
        {
            try
            {
                RefreshCompleted?.Invoke(this, new RefreshCompletedEventArgs(refreshTime, rowCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error firing refresh completed event");
            }
        }

        /// <summary>
        /// OnPropertyChanged implementation
        /// </summary>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Dáta gridu
        /// </summary>
        public ObservableCollection<Dictionary<string, object?>> GridData => _gridData;

        /// <summary>
        /// Počet riadkov
        /// </summary>
        public int RowCount => _gridData.Count;

        /// <summary>
        /// Počet stĺpcov
        /// </summary>
        public int ColumnCount => _columns.Count;

        /// <summary>
        /// Či má dáta
        /// </summary>
        public bool HasData => _gridData.Count > 0;

        /// <summary>
        /// Či má data source
        /// </summary>
        public bool HasDataSource => _hasDataSource;

        /// <summary>
        /// Či má neuložené zmeny
        /// </summary>
        public bool HasUnsavedChanges => _hasUnsavedChanges;

        /// <summary>
        /// Či je binding
        /// </summary>
        public bool IsBinding => _isBinding;

        /// <summary>
        /// Či je refreshing
        /// </summary>
        public bool IsRefreshing => _isRefreshing;

        /// <summary>
        /// Posledný refresh time
        /// </summary>
        public DateTime LastRefreshTime => _lastRefreshTime;

        /// <summary>
        /// Počet refresh operácií
        /// </summary>
        public int RefreshCount => _refreshCount;

        /// <summary>
        /// Priemerný refresh time
        /// </summary>
        public TimeSpan AverageRefreshTime => _averageRefreshTime;

        /// <summary>
        /// Či je inicializovaný
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                _logger.LogInformation("🧹 Disposing DataGridBindingService - InstanceId: {InstanceId}", _serviceInstanceId);

                // Stop auto-refresh
                StopAutoRefresh();
                _autoRefreshTimer?.Stop();
                _autoRefreshTimer = null;

                // Cleanup collections
                _gridData.CollectionChanged -= OnGridDataCollectionChanged;
                _gridData.Clear();
                _columns.Clear();
                _columnTypes.Clear();
                _originalValues.Clear();
                _changedCells.Clear();
                _cellValidationErrors.Clear();

                // Clear references
                _dataGrid = null;
                _validationService = null;
                _itemsSource = null;
                _dataTable = null;

                _logger.LogInformation("✅ DataGridBindingService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing DataGridBindingService");
            }
        }

        #endregion
    }

    #region Event Args Classes

    /// <summary>
    /// Event args pre zmenu dát
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        public DataChangeType ChangeType { get; }
        public int AffectedRows { get; }
        public int? AffectedRowIndex { get; }

        public DataChangedEventArgs(DataChangeType changeType, int affectedRows, int? affectedRowIndex = null)
        {
            ChangeType = changeType;
            AffectedRows = affectedRows;
            AffectedRowIndex = affectedRowIndex;
        }
    }

    /// <summary>
    /// Event args pre zmenu hodnoty bunky
    /// </summary>
    public class CellValueChangedEventArgs : EventArgs
    {
        public int RowIndex { get; }
        public string ColumnName { get; }
        public object? NewValue { get; }

        public CellValueChangedEventArgs(int rowIndex, string columnName, object? newValue)
        {
            RowIndex = rowIndex;
            ColumnName = columnName;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event args pre dokončenie refresh
    /// </summary>
    public class RefreshCompletedEventArgs : EventArgs
    {
        public TimeSpan RefreshTime { get; }
        public int RowCount { get; }

        public RefreshCompletedEventArgs(TimeSpan refreshTime, int rowCount)
        {
            RefreshTime = refreshTime;
            RowCount = rowCount;
        }
    }

    /// <summary>
    /// Event args pre validation error
    /// </summary>
    public class ValidationErrorEventArgs : EventArgs
    {
        public int RowIndex { get; }
        public string ColumnName { get; }
        public string ErrorMessage { get; }

        public ValidationErrorEventArgs(int rowIndex, string columnName, string errorMessage)
        {
            RowIndex = rowIndex;
            ColumnName = columnName;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Typ zmeny dát
    /// </summary>
    public enum DataChangeType
    {
        Loaded,
        RowAdded,
        RowRemoved,
        RowUpdated,
        CellUpdated,
        Cleared,
        Refreshed
    }

    #endregion
}