// Services/Interfaces/INavigationService.cs
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Rozhranie pre klávesovú navigáciu v DataGrid komponente.
    /// Zabezpečuje Tab/Enter/Esc/Shift+Enter správanie.
    /// </summary>
    internal interface INavigationService : IDisposable
    {
        #region Events

        /// <summary>
        /// Event vyvolaný keď sa zmení aktuálna pozícia v gridi.
        /// </summary>
        event EventHandler<NavigationEventArgs>? CellNavigated;

        /// <summary>
        /// Event vyvolaný keď sa zmení edit mode.
        /// </summary>
        event EventHandler<EditModeChangedEventArgs>? EditModeChanged;

        #endregion

        #region Inicializácia

        /// <summary>
        /// Inicializuje navigačnú službu s konfiguráciou a referenciou na DataGrid.
        /// </summary>
        /// <param name="configuration">Konfigurácia gridu</param>
        /// <param name="dataGrid">Reference na DataGrid komponent</param>
        /// <returns>Task pre asynchrónnu inicializáciu</returns>
        Task InitializeAsync(GridConfiguration configuration, AdvancedDataGrid dataGrid);

        /// <summary>
        /// Určuje či je služba inicializovaná.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Klávesová navigácia

        /// <summary>
        /// Spracuje klávesové skratky a navigáciu.
        /// </summary>
        /// <param name="e">Klávesový event</param>
        void HandleKeyDown(KeyRoutedEventArgs e);

        #endregion

        #region Navigačné operácie

        /// <summary>
        /// Presunie kurzor na konkrétnu bunku.
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="columnIndex">Index stĺpca</param>
        /// <returns>True ak sa operácia podarila</returns>
        bool MoveToCell(int rowIndex, int columnIndex);

        /// <summary>
        /// Presunie kurzor na ďalšiu bunku (Tab správanie).
        /// </summary>
        /// <returns>True ak sa operácia podarila</returns>
        bool MoveNext();

        /// <summary>
        /// Presunie kurzor na predchádzajúcu bunku (Shift+Tab správanie).
        /// </summary>
        /// <returns>True ak sa operácia podarila</returns>
        bool MovePrevious();

        /// <summary>
        /// Presunie kurzor o riadok vyššie.
        /// </summary>
        /// <returns>True ak sa operácia podarila</returns>
        bool MoveUp();

        /// <summary>
        /// Presunie kurzor o riadok nižšie.
        /// </summary>
        /// <returns>True ak sa operácia podarila</returns>
        bool MoveDown();

        #endregion

        #region Edit mode

        /// <summary>
        /// Spustí edit mode pre aktuálnu bunku.
        /// </summary>
        void StartEdit();

        /// <summary>
        /// Ukončí edit mode.
        /// </summary>
        /// <param name="commitChanges">Či sa majú zmeny potvrdiť alebo zahodiť</param>
        void StopEdit(bool commitChanges = true);

        /// <summary>
        /// Určuje či je aktuálne aktívny edit mode.
        /// </summary>
        bool IsEditing { get; }

        /// <summary>
        /// Aktuálna pozícia kurzora v gridi.
        /// </summary>
        (int row, int col)? CurrentPosition { get; }

        #endregion
    }
}