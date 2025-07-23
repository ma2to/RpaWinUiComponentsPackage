// Services/Interfaces/IDataManagementService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Rozhranie pre správu dát v DataGrid komponente.
    /// Zabezpečuje CRUD operácie, správu riadkov a optimalizované načítanie dát.
    /// </summary>
    internal interface IDataManagementService : IDisposable
    {
        #region Events

        /// <summary>
        /// Event vyvolaný keď sa zmenia dáta v gridi.
        /// </summary>
        event EventHandler<DataChangedEventArgs>? DataChanged;

        /// <summary>
        /// Event vyvolaný keď sa pridá nový riadok.
        /// </summary>
        event EventHandler<RowAddedEventArgs>? RowAdded;

        /// <summary>
        /// Event vyvolaný keď sa vymaže riadok.
        /// </summary>
        event EventHandler<RowDeletedEventArgs>? RowDeleted;

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

        /// <summary>
        /// Aktuálna konfigurácia gridu.
        /// </summary>
        GridConfiguration? Configuration { get; }

        #endregion

        #region Základné operácie s dátami

        /// <summary>
        /// Načíta dáta z Dictionary kolekcie.
        /// </summary>
        /// <param name="data">Kolekcia dát pre načítanie</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne načítanie</returns>
        Task LoadDataAsync(IEnumerable<Dictionary<string, object?>> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Načíta dáta z DataTable.
        /// </summary>
        /// <param name="dataTable">DataTable s dátami</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne načítanie</returns>
        Task LoadDataAsync(DataTable dataTable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Vymaže všetky dáta z gridu a uvoľní zdroje.
        /// </summary>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne vymazanie</returns>
        Task ClearAllDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Vytvorí prázdne riadky podľa konfigurácie.
        /// </summary>
        /// <param name="rowCount">Počet prázdnych riadkov</param>
        /// <returns>Task pre asynchrónne vytvorenie</returns>
        Task CreateEmptyRowsAsync(int rowCount);

        #endregion

        #region Operácie s riadkami

        /// <summary>
        /// Vráti všetky dáta gridu ako kolekciu riadkov.
        /// </summary>
        /// <returns>Kolekcia riadkov (každý riadok je kolekcia CellData)</returns>
        IEnumerable<IEnumerable<CellData>> GetAllRowsData();

        /// <summary>
        /// Vráti dáta konkrétneho riadku.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <returns>Kolekcia CellData pre daný riadok alebo null ak neexistuje</returns>
        IEnumerable<CellData>? GetRowData(int rowIndex);

        /// <summary>
        /// Pridá nový prázdny riadok na koniec gridu.
        /// </summary>
        /// <returns>Index nového riadku</returns>
        Task<int> AddEmptyRowAsync();

        /// <summary>
        /// Vymaže obsah riadku (nie fyzicky - iba vyčistí bunky).
        /// </summary>
        /// <param name="rowIndex">Index riadku na vymazanie</param>
        /// <returns>Task pre asynchrónne vymazanie</returns>
        Task DeleteRowContentAsync(int rowIndex);

        /// <summary>
        /// Posunie riadky nahor od určeného indexu (po vymazaní riadku).
        /// </summary>
        /// <param name="fromRowIndex">Index od ktorého sa začne posúvanie</param>
        /// <returns>Task pre asynchrónne posúvanie</returns>
        Task CompactRowsAsync(int fromRowIndex);

        #endregion

        #region Operácie s bunkami

        /// <summary>
        /// Vráti dáta konkrétnej bunky.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <returns>CellData alebo null ak bunka neexistuje</returns>
        CellData? GetCellData(int rowIndex, int columnIndex);

        /// <summary>
        /// Nastaví hodnotu bunky.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <param name="value">Nová hodnota</param>
        /// <returns>Task pre asynchrónne nastavenie</returns>
        Task SetCellValueAsync(int rowIndex, int columnIndex, object? value);

        /// <summary>
        /// Potvrdí zmeny v bunke.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <returns>Task pre asynchrónne potvrdenie</returns>
        Task CommitCellChangesAsync(int rowIndex, int columnIndex);

        /// <summary>
        /// Zruší zmeny v bunke.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <returns>Task pre asynchrónne zrušenie</returns>
        Task RevertCellChangesAsync(int rowIndex, int columnIndex);

        #endregion

        #region Bulk operácie

        /// <summary>
        /// Nastaví hodnoty viacerých buniek naraz.
        /// </summary>
        /// <param name="cellUpdates">Slovník updates (kľúč = pozícia bunky, hodnota = nová hodnota)</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne nastavenie</returns>
        Task SetMultipleCellValuesAsync(Dictionary<(int row, int col), object?> cellUpdates, CancellationToken cancellationToken = default);

        /// <summary>
        /// Automaticky rozšíri grid ak je potreba (pri paste operáciách).
        /// </summary>
        /// <param name="requiredRows">Minimálny počet riadkov</param>
        /// <param name="requiredColumns">Minimálny počet stĺpcov (iba dátové)</param>
        /// <returns>Task pre asynchrónne rozšírenie</returns>
        Task EnsureCapacityAsync(int requiredRows, int requiredColumns);

        #endregion

        #region Štatistiky a info

        /// <summary>
        /// Aktuálny počet riadkov v gridi.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Aktuálny počet stĺpcov v gridi (vrátane špeciálnych).
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Počet dátových stĺpcov (bez špeciálnych).
        /// </summary>
        int DataColumnCount { get; }

        /// <summary>
        /// Počet neprázdnych riadkov.
        /// </summary>
        int NonEmptyRowCount { get; }

        /// <summary>
        /// Určuje či má grid nepotvrdené zmeny.
        /// </summary>
        bool HasPendingChanges { get; }

        #endregion

        #region Memory management

        /// <summary>
        /// Vyčistí nepoužívané zdroje a optimalizuje pamäť.
        /// </summary>
        /// <returns>Task pre asynchrónne čistenie</returns>
        Task CleanupUnusedResourcesAsync();

        /// <summary>
        /// Vráti informácie o využití pamäte.
        /// </summary>
        /// <returns>Informácie o pamäti</returns>
        MemoryUsageInfo GetMemoryUsage();

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event args pre zmenu dát.
    /// </summary>
    internal class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs(int rowIndex, int columnIndex, object? oldValue, object? newValue)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int RowIndex { get; }
        public int ColumnIndex { get; }
        public object? OldValue { get; }
        public object? NewValue { get; }
    }

    /// <summary>
    /// Event args pre pridanie riadku.
    /// </summary>
    internal class RowAddedEventArgs : EventArgs
    {
        public RowAddedEventArgs(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        public int RowIndex { get; }
    }

    /// <summary>
    /// Event args pre vymazanie riadku.
    /// </summary>
    internal class RowDeletedEventArgs : EventArgs
    {
        public RowDeletedEventArgs(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        public int RowIndex { get; }
    }

    #endregion

    #region Helper classes

    /// <summary>
    /// Informácie o využití pamäte.
    /// </summary>
    internal class MemoryUsageInfo
    {
        public long TotalCellsAllocated { get; set; }
        public long ActiveCellsCount { get; set; }
        public long EstimatedMemoryUsageBytes { get; set; }
        public int CachedUIElementsCount { get; set; }

        public override string ToString()
        {
            return $"Cells: {ActiveCellsCount}/{TotalCellsAllocated}, Memory: {EstimatedMemoryUsageBytes / 1024}KB, UI Cache: {CachedUIElementsCount}";
        }
    }

    #endregion
}