// Services/Interfaces/IBackgroundProcessingService.cs - ✅ NOVÝ: Background Processing Service Interface
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre background processing služby - INTERNAL
    /// </summary>
    internal interface IBackgroundProcessingService
    {
        #region Events

        /// <summary>
        /// Event pre progress reporting
        /// </summary>
        event EventHandler<AsyncLoadingProgress>? ProgressChanged;

        /// <summary>
        /// Event pre completion notification
        /// </summary>
        event EventHandler<AsyncLoadingResult<object>>? OperationCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Či je momentálne spustená async operácia
        /// </summary>
        bool IsOperationRunning { get; }

        /// <summary>
        /// Aktuálny progress poslednej operácie
        /// </summary>
        AsyncLoadingProgress? CurrentProgress { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje service s konfiguráciou
        /// </summary>
        Task InitializeAsync(BackgroundProcessingConfiguration configuration);

        #endregion

        #region Async Data Loading

        /// <summary>
        /// Asynchrónne načíta dáta zo zdroja
        /// </summary>
        Task<AsyncLoadingResult<List<Dictionary<string, object>>>> LoadDataAsync<T>(
            Func<IProgress<AsyncLoadingProgress>, CancellationToken, Task<T>> dataLoader,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchrónne načíta dáta s batch processing
        /// </summary>
        Task<AsyncLoadingResult<List<Dictionary<string, object>>>> LoadDataBatchAsync<T>(
            Func<int, int, IProgress<AsyncLoadingProgress>, CancellationToken, Task<IEnumerable<T>>> batchLoader,
            int totalCount,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchrónne načíta dáta so streaming support
        /// </summary>
        Task<AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>> LoadDataStreamAsync<T>(
            Func<IProgress<AsyncLoadingProgress>, CancellationToken, IAsyncEnumerable<T>> streamLoader,
            CancellationToken cancellationToken = default);

        #endregion

        #region Data Processing

        /// <summary>
        /// Asynchrónne spracuje dáta na pozadí
        /// </summary>
        Task<AsyncLoadingResult<TResult>> ProcessDataAsync<TInput, TResult>(
            TInput inputData,
            Func<TInput, IProgress<AsyncLoadingProgress>, CancellationToken, Task<TResult>> processor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Parallel processing viacerých items
        /// </summary>
        Task<AsyncLoadingResult<List<TResult>>> ProcessDataParallelAsync<TInput, TResult>(
            IEnumerable<TInput> inputData,
            Func<TInput, CancellationToken, Task<TResult>> processor,
            CancellationToken cancellationToken = default);

        #endregion

        #region Operation Management

        /// <summary>
        /// Zruší aktuálne bežiacu operáciu
        /// </summary>
        Task CancelCurrentOperationAsync();

        /// <summary>
        /// Čaká na dokončenie všetkých operácií
        /// </summary>
        Task WaitForCompletionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Získa stav aktuálnej operácie
        /// </summary>
        AsyncLoadingProgress? GetCurrentOperationStatus();

        #endregion

        #region Resource Management

        /// <summary>
        /// Vyčistí resources a pozastaví všetky operácie
        /// </summary>
        ValueTask DisposeAsync();

        #endregion
    }
}