// Services/Interfaces/ICopyPasteService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Rozhranie pre Copy/Paste/Cut operácie s Excel kompatibilitou.
    /// Podporuje TSV formát a viacbunkový výber.
    /// </summary>
    internal interface ICopyPasteService : IDisposable
    {
        #region Events

        /// <summary>
        /// Event vyvolaný keď sa dokončí copy operácia.
        /// </summary>
        event EventHandler<CopyCompletedEventArgs>? CopyCompleted;

        /// <summary>
        /// Event vyvolaný keď sa dokončí paste operácia.
        /// </summary>
        event EventHandler<PasteCompletedEventArgs>? PasteCompleted;

        /// <summary>
        /// Event vyvolaný keď sa dokončí cut operácia.
        /// </summary>
        event EventHandler<CutCompletedEventArgs>? CutCompleted;

        #endregion

        #region Inicializácia

        /// <summary>
        /// Inicializuje službu s konfiguráciou gridu.
        /// </summary>
        /// <param name="configuration">Konfigurácia gridu</param>
        /// <returns>Task pre asynchrónnu inicializáciu</returns>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Určuje či je služba inicializovaná.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Copy operácie

        /// <summary>
        /// Skopíruje hodnoty vybraných buniek do clipboardu v Excel TSV formáte.
        /// </summary>
        /// <param name="selectedCells">Zoznam vybraných buniek</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom skopírovaných buniek</returns>
        Task<int> CopySelectedCellsAsync(IEnumerable<CellSelection> selectedCells, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skopíruje celý riadok do clipboardu.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="includeSpecialColumns">Zahrnúť špeciálne stĺpce</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom skopírovaných buniek</returns>
        Task<int> CopyRowAsync(int rowIndex, bool includeSpecialColumns = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skopíruje celý stĺpec do clipboardu.
        /// </summary>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <param name="includeEmptyRows">Zahrnúť prázdne riadky</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom skopírovaných buniek</returns>
        Task<int> CopyColumnAsync(int columnIndex, bool includeEmptyRows = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skopíruje obdĺžnikovú oblasť buniek.
        /// </summary>
        /// <param name="startRow">Začiatočný riadok</param>
        /// <param name="startColumn">Začiatočný stĺpec</param>
        /// <param name="endRow">Koncový riadok</param>
        /// <param name="endColumn">Koncový stĺpec</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom skopírovaných buniek</returns>
        Task<int> CopyRangeAsync(int startRow, int startColumn, int endRow, int endColumn, CancellationToken cancellationToken = default);

        #endregion

        #region Paste operácie

        /// <summary>
        /// Vloží dáta z clipboardu od určenej pozície.
        /// </summary>
        /// <param name="startRow">Začiatočný riadok pre vloženie</param>
        /// <param name="startColumn">Začiatočný stĺpec pre vloženie</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s výsledkom paste operácie</returns>
        Task<PasteResult> PasteFromClipboardAsync(int startRow, int startColumn, CancellationToken cancellationToken = default);

        /// <summary>
        /// Vloží TSV dáta od určenej pozície.
        /// </summary>
        /// <param name="tsvData">TSV dáta na vloženie</param>
        /// <param name="startRow">Začiatočný riadok</param>
        /// <param name="startColumn">Začiatočný stĺpec</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s výsledkom paste operácie</returns>
        Task<PasteResult> PasteTsvDataAsync(string tsvData, int startRow, int startColumn, CancellationToken cancellationToken = default);

        #endregion

        #region Cut operácie

        /// <summary>
        /// Vystihne (copy + clear) vybrané bunky.
        /// </summary>
        /// <param name="selectedCells">Zoznam vybraných buniek</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom vystrihnutých buniek</returns>
        Task<int> CutSelectedCellsAsync(IEnumerable<CellSelection> selectedCells, CancellationToken cancellationToken = default);

        /// <summary>
        /// Vystihne celý riadok.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s počtom vystrihnutých buniek</returns>
        Task<int> CutRowAsync(int rowIndex, CancellationToken cancellationToken = default);

        #endregion

        #region Clipboard operations

        /// <summary>
        /// Načíta TSV dáta z clipboardu.
        /// </summary>
        /// <returns>TSV string alebo null ak clipboard neobsahuje text</returns>
        Task<string?> GetClipboardTsvAsync();

        /// <summary>
        /// Nastaví TSV dáta do clipboardu.
        /// </summary>
        /// <param name="tsvData">TSV dáta</param>
        /// <returns>Task pre asynchrónne nastavenie</returns>
        Task SetClipboardTsvAsync(string tsvData);

        /// <summary>
        /// Vymaže clipboard.
        /// </summary>
        /// <returns>Task pre asynchrónne vymazanie</returns>
        Task ClearClipboardAsync();

        /// <summary>
        /// Určuje či clipboard obsahuje Excel kompatibilné dáta.
        /// </summary>
        /// <returns>True ak clipboard obsahuje TSV dáta</returns>
        Task<bool> HasClipboardDataAsync();

        #endregion

        #region Formátovanie

        /// <summary>
        /// Konvertuje 2D pole dát na TSV string.
        /// </summary>
        /// <param name="data">2D pole hodnôt</param>
        /// <returns>TSV formátovaný string</returns>
        string ConvertToTsv(object?[,] data);

        /// <summary>
        /// Parsuje TSV string na 2D pole dát.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>2D pole hodnôt</returns>
        object?[,] ParseTsvData(string tsvData);

        /// <summary>
        /// Získa rozmery TSV dát.
        /// </summary>
        /// <param name="tsvData">TSV string</param>
        /// <returns>Tuple s počtom riadkov a stĺpcov</returns>
        (int rows, int columns) GetTsvDimensions(string tsvData);

        #endregion

        #region Validácia

        /// <summary>
        /// Skontroluje či je paste operácia možná na danej pozícii.
        /// </summary>
        /// <param name="startRow">Začiatočný riadok</param>
        /// <param name="startColumn">Začiatočný stĺpec</param>
        /// <param name="dataRows">Počet riadkov na vloženie</param>
        /// <param name="dataColumns">Počet stĺpcov na vloženie</param>
        /// <returns>Výsledok validácie</returns>
        PasteValidationResult ValidatePasteOperation(int startRow, int startColumn, int dataRows, int dataColumns);

        #endregion
    }

    #region Helper classes

    /// <summary>
    /// Reprezentuje výber bunky pre copy/cut operácie.
    /// </summary>
    internal class CellSelection
    {
        public CellSelection(int rowIndex, int columnIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        public int RowIndex { get; }
        public int ColumnIndex { get; }

        public override bool Equals(object? obj)
        {
            return obj is CellSelection other && RowIndex == other.RowIndex && ColumnIndex == other.ColumnIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RowIndex, ColumnIndex);
        }

        public override string ToString()
        {
            return $"Cell[{RowIndex},{ColumnIndex}]";
        }
    }

    /// <summary>
    /// Výsledok paste operácie.
    /// </summary>
    internal class PasteResult
    {
        public PasteResult(bool success, int cellsAffected, int rowsCreated = 0, string? errorMessage = null)
        {
            Success = success;
            CellsAffected = cellsAffected;
            RowsCreated = rowsCreated;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; }
        public int CellsAffected { get; }
        public int RowsCreated { get; }
        public string? ErrorMessage { get; }

        public static PasteResult CreateSuccess(int cellsAffected, int rowsCreated = 0)
            => new(true, cellsAffected, rowsCreated);

        public static PasteResult CreateError(string errorMessage)
            => new(false, 0, 0, errorMessage);
    }

    /// <summary>
    /// Výsledok validácie paste operácie.
    /// </summary>
    internal class PasteValidationResult
    {
        public PasteValidationResult(bool isValid, string? errorMessage = null, int additionalRowsNeeded = 0)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            AdditionalRowsNeeded = additionalRowsNeeded;
        }

        public bool IsValid { get; }
        public string? ErrorMessage { get; }
        public int AdditionalRowsNeeded { get; }

        public static PasteValidationResult Valid(int additionalRowsNeeded = 0)
            => new(true, null, additionalRowsNeeded);

        public static PasteValidationResult Invalid(string errorMessage)
            => new(false, errorMessage);
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Event args pre dokončenie copy operácie.
    /// </summary>
    internal class CopyCompletedEventArgs : EventArgs
    {
        public CopyCompletedEventArgs(int cellsCopied, string tsvData)
        {
            CellsCopied = cellsCopied;
            TsvData = tsvData;
        }

        public int CellsCopied { get; }
        public string TsvData { get; }
    }

    /// <summary>
    /// Event args pre dokončenie paste operácie.
    /// </summary>
    internal class PasteCompletedEventArgs : EventArgs
    {
        public PasteCompletedEventArgs(PasteResult result)
        {
            Result = result;
        }

        public PasteResult Result { get; }
    }

    /// <summary>
    /// Event args pre dokončenie cut operácie.
    /// </summary>
    internal class CutCompletedEventArgs : EventArgs
    {
        public CutCompletedEventArgs(int cellsCut, string tsvData)
        {
            CellsCut = cellsCut;
            TsvData = tsvData;
        }

        public int CellsCut { get; }
        public string TsvData { get; }
    }

    #endregion
}