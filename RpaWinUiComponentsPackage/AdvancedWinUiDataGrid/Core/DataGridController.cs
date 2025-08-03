// Core/DataGridController.cs - ✅ NOVÝ: Centrálny controller pre koordináciu services
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core
{
    /// <summary>
    /// Centrálny controller pre koordináciu všetkých DataGrid services - INTERNAL
    /// Implementuje Controller pattern pre separation of concerns
    /// </summary>
    internal class DataGridController
    {
        #region Private Fields

        private readonly NullLogger _logger;
        private readonly string _controllerInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        // Service references
        private IDataManagementService? _dataService;
        private IValidationService? _validationService;
        private ICopyPasteService? _copyPasteService;
        private IExportService? _exportService;
        private INavigationService? _navigationService;
        private IBackgroundProcessingService? _backgroundService;
        
        // Configuration
        private DataGridConfiguration? _configuration;
        private bool _isInitialized = false;

        #endregion

        #region Constructor

        public DataGridController()
        {
            _logger = NullLogger.Instance;
            _logger.LogDebug("🎛️ DataGridController created - InstanceId: {InstanceId}", _controllerInstanceId);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Controller je inicializovaný a pripravený na použitie
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Aktuálna konfigurácia controller-a
        /// </summary>
        public DataGridConfiguration? Configuration => _configuration;

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje controller s potrebnými services
        /// </summary>
        public async Task InitializeAsync(DataGridConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("🎛️ DataGridController.InitializeAsync START - InstanceId: {InstanceId}", 
                    _controllerInstanceId);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                // Inicializuj services v správnom poradí
                await InitializeServicesAsync();
                
                _isInitialized = true;
                
                _logger.LogInformation("✅ DataGridController INITIALIZED - InstanceId: {InstanceId}", 
                    _controllerInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in DataGridController.InitializeAsync - InstanceId: {InstanceId}", 
                    _controllerInstanceId);
                throw;
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// Inicializuje všetky potrebné services
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            _logger.LogDebug("🔧 Initializing services...");

            // TODO: Implement service initialization
            // _dataService = new DataManagementService(_logger);
            // _validationService = new ValidationService(_logger);
            // await _dataService.InitializeAsync();
            // await _validationService.InitializeAsync();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Registruje service do controller-a
        /// </summary>
        public void RegisterService<T>(T service) where T : class
        {
            switch (service)
            {
                case IDataManagementService dataService:
                    _dataService = dataService;
                    break;
                case IValidationService validationService:
                    _validationService = validationService;
                    break;
                case ICopyPasteService copyPasteService:
                    _copyPasteService = copyPasteService;
                    break;
                case IExportService exportService:
                    _exportService = exportService;
                    break;
                case INavigationService navigationService:
                    _navigationService = navigationService;
                    break;
                case IBackgroundProcessingService backgroundService:
                    _backgroundService = backgroundService;
                    break;
                default:
                    _logger.LogWarning("⚠️ Unknown service type: {ServiceType}", typeof(T).Name);
                    break;
            }
        }

        /// <summary>
        /// Získa registrovaný service
        /// </summary>
        public T? GetService<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(IDataManagementService) => _dataService as T,
                nameof(IValidationService) => _validationService as T,
                nameof(ICopyPasteService) => _copyPasteService as T,
                nameof(IExportService) => _exportService as T,
                nameof(INavigationService) => _navigationService as T,
                nameof(IBackgroundProcessingService) => _backgroundService as T,
                _ => null
            };
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Vyčistí resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            try
            {
                _logger.LogDebug("🧹 Disposing DataGridController - InstanceId: {InstanceId}", _controllerInstanceId);

                // Dispose services
                if (_backgroundService is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();

                _isInitialized = false;
                
                _logger.LogDebug("✅ DataGridController disposed - InstanceId: {InstanceId}", _controllerInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR disposing DataGridController - InstanceId: {InstanceId}", 
                    _controllerInstanceId);
            }
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostické informácie o controller-i
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var servicesCount = 0;
            if (_dataService != null) servicesCount++;
            if (_validationService != null) servicesCount++;
            if (_copyPasteService != null) servicesCount++;
            if (_exportService != null) servicesCount++;
            if (_navigationService != null) servicesCount++;
            if (_backgroundService != null) servicesCount++;

            return $"DataGridController[{_controllerInstanceId}] - " +
                   $"Initialized: {_isInitialized}, Services: {servicesCount}/6";
        }

        #endregion
    }
}