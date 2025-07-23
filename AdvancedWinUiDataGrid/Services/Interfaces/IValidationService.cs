// Services/Interfaces/IValidationService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Rozhranie pre službu validácie buniek a riadkov v DataGrid.
    /// Poskytuje asynchrónne validácie s throttling podporou.
    /// </summary>
    internal interface IValidationService : IDisposable
    {
        #region Events

        /// <summary>
        /// Event vyvolaný keď sa zmení validačný stav bunky.
        /// </summary>
        event EventHandler<CellValidationChangedEventArgs>? CellValidationChanged;

        /// <summary>
        /// Event vyvolaný keď sa dokončí validácia riadku.
        /// </summary>
        event EventHandler<RowValidationCompletedEventArgs>? RowValidationCompleted;

        #endregion

        #region Inicializácia

        /// <summary>
        /// Inicializuje validačnú službu s konfiguráciou.
        /// </summary>
        /// <param name="configuration">Konfigurácia gridu</param>
        /// <returns>Task pre asynchrónnu inicializáciu</returns>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Určuje či je služba inicializovaná.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Validácia buniek

        /// <summary>
        /// Validuje jednu bunku asynchrónne.
        /// </summary>
        /// <param name="cellData">Dáta bunky na validáciu</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Výsledok validácie</returns>
        Task<ValidationResult> ValidateCellAsync(CellData cellData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validuje bunku s throttling-om (debounce).
        /// </summary>
        /// <param name="cellData">Dáta bunky na validáciu</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónnu validáciu</returns>
        Task ValidateCellWithThrottlingAsync(CellData cellData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validuje viacero buniek naraz.
        /// </summary>
        /// <param name="cellDataList">Zoznam buniek na validáciu</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Slovník výsledkov validácie (kľúč = pozícia bunky)</returns>
        Task<Dictionary<(int row, int col), ValidationResult>> ValidateMultipleCellsAsync(
            IEnumerable<CellData> cellDataList,
            CancellationToken cancellationToken = default);

        #endregion

        #region Validácia riadkov

        /// <summary>
        /// Validuje celý riadok (všetky dátové bunky).
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="rowData">Dáta riadku (bunky)</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Výsledok validácie riadku</returns>
        Task<RowValidationResult> ValidateRowAsync(int rowIndex, IEnumerable<CellData> rowData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validuje všetky neprázdne riadky.
        /// </summary>
        /// <param name="allRowsData">Všetky dáta gridu</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Celkový výsledok validácie</returns>
        Task<GridValidationResult> ValidateAllNonEmptyRowsAsync(IEnumerable<IEnumerable<CellData>> allRowsData, CancellationToken cancellationToken = default);

        #endregion

        #region Kontrola prázdnych riadkov

        /// <summary>
        /// Určuje či je riadok úplne prázdny (všetky dátové bunky sú prázdne).
        /// </summary>
        /// <param name="rowData">Dáta riadku</param>
        /// <returns>True ak je riadok prázdny</returns>
        bool IsRowEmpty(IEnumerable<CellData> rowData);

        /// <summary>
        /// Vráti zoznam indexov neprázdnych riadkov.
        /// </summary>
        /// <param name="allRowsData">Všetky dáta gridu</param>
        /// <returns>Zoznam indexov neprázdnych riadkov</returns>
        List<int> GetNonEmptyRowIndices(IEnumerable<IEnumerable<CellData>> allRowsData);

        #endregion

        #region Throttling a výkon

        /// <summary>
        /// Zruší všetky pending validácie.
        /// </summary>
        Task CancelPendingValidationsAsync();

        /// <summary>
        /// Počet aktuálne bežiacich validácií.
        /// </summary>
        int ActiveValidationsCount { get; }

        /// <summary>
        /// Počet pending validácií (čakajúcich na vykonanie).
        /// </summary>
        int PendingValidationsCount { get; }

        #endregion

        #region Konfigurácia

        /// <summary>
        /// Aktualizuje throttling konfiguráciu.
        /// </summary>
        /// <param name="newConfig">Nová konfigurácia</param>
        Task UpdateThrottlingConfigAsync(ThrottlingConfig newConfig);

        /// <summary>
        /// Aktuálna throttling konfigurácia.
        /// </summary>
        ThrottlingConfig CurrentThrottlingConfig { get; }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event args pre zmenu validačného stavu bunky.
    /// </summary>
    internal class CellValidationChangedEventArgs : EventArgs
    {
        public CellValidationChangedEventArgs(int rowIndex, int columnIndex, ValidationResult result)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ValidationResult = result;
        }

        public int RowIndex { get; }
        public int ColumnIndex { get; }
        public ValidationResult ValidationResult { get; }
    }

    /// <summary>
    /// Event args pre dokončenie validácie riadku.
    /// </summary>
    internal class RowValidationCompletedEventArgs : EventArgs
    {
        public RowValidationCompletedEventArgs(int rowIndex, RowValidationResult result)
        {
            RowIndex = rowIndex;
            ValidationResult = result;
        }

        public int RowIndex { get; }
        public RowValidationResult ValidationResult { get; }
    }

    #endregion

    #region Výsledky validácie

    /// <summary>
    /// Výsledok validácie jednej bunky.
    /// </summary>
    internal class ValidationResult
    {
        public ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; }
        public string? ErrorMessage { get; }

        public static ValidationResult Success() => new(true);
        public static ValidationResult Error(string message) => new(false, message);
    }

    /// <summary>
    /// Výsledok validácie celého riadku.
    /// </summary>
    internal class RowValidationResult
    {
        public RowValidationResult(int rowIndex, bool isValid, Dictionary<int, ValidationResult> cellResults)
        {
            RowIndex = rowIndex;
            IsValid = isValid;
            CellResults = cellResults ?? new Dictionary<int, ValidationResult>();
        }

        public int RowIndex { get; }
        public bool IsValid { get; }
        public Dictionary<int, ValidationResult> CellResults { get; }

        /// <summary>
        /// Všetky chybové správy v riadku.
        /// </summary>
        public IEnumerable<string> ErrorMessages => CellResults.Values
            .Where(r => !r.IsValid && !string.IsNullOrEmpty(r.ErrorMessage))
            .Select(r => r.ErrorMessage!);

        /// <summary>
        /// Formátovaná chybová správa pre ValidAlerts stĺpec.
        /// </summary>
        public string FormattedErrorMessage => string.Join("; ", ErrorMessages);
    }

    /// <summary>
    /// Výsledok validácie celého gridu.
    /// </summary>
    internal class GridValidationResult
    {
        public GridValidationResult(bool isValid, Dictionary<int, RowValidationResult> rowResults)
        {
            IsValid = isValid;
            RowResults = rowResults ?? new Dictionary<int, RowValidationResult>();
        }

        public bool IsValid { get; }
        public Dictionary<int, RowValidationResult> RowResults { get; }

        /// <summary>
        /// Počet validných riadkov.
        /// </summary>
        public int ValidRowsCount => RowResults.Values.Count(r => r.IsValid);

        /// <summary>
        /// Počet nevalidných riadkov.
        /// </summary>
        public int InvalidRowsCount => RowResults.Values.Count(r => !r.IsValid);

        /// <summary>
        /// Celkový počet validovaných riadkov.
        /// </summary>
        public int TotalRowsCount => RowResults.Count;

        /// <summary>
        /// Indexy nevalidných riadkov.
        /// </summary>
        public IEnumerable<int> InvalidRowIndices => RowResults.Values
            .Where(r => !r.IsValid)
            .Select(r => r.RowIndex);
    }

    #endregion
}