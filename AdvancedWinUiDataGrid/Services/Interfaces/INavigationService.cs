// Services/Interfaces/INavigationService.cs - ✅ OPRAVENÝ - len INavigationService

using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre navigáciu v DataGrid (Tab, Enter, Esc, atď.) - INTERNAL
    /// </summary>
    internal interface INavigationService
    {
        /// <summary>
        /// Inicializuje navigačnú službu
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Spracuje stlačenie klávesy v bunke
        /// </summary>
        Task HandleKeyDownAsync(object sender, KeyRoutedEventArgs e);

        /// <summary>
        /// Presunie fokus na ďalšiu bunku (Tab)
        /// </summary>
        Task MoveToNextCellAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie fokus na predchádzajúcu bunku (Shift+Tab)
        /// </summary>
        Task MoveToPreviousCellAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie fokus na bunku nižšie (Enter)
        /// </summary>
        Task MoveToCellBelowAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie fokus na bunku vyššie (Shift+Enter)
        /// </summary>
        Task MoveToCellAboveAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Zruší editáciu bunky (Esc)
        /// </summary>
        Task CancelCellEditAsync(object sender);

        /// <summary>
        /// Dokončí editáciu bunky
        /// </summary>
        Task FinishCellEditAsync(object sender);

        /// <summary>
        /// Pridá nový riadok v bunke (Shift+Enter)
        /// </summary>
        Task InsertNewLineInCellAsync(object sender);
    }
}