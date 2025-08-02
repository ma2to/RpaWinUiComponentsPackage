// Services/Interfaces/INavigationCallback.cs - ✅ INTERNAL callback interface pre navigation
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Callback interface pre navigation medzi bunkami - INTERNAL
    /// </summary>
    internal interface INavigationCallback
    {
        /// <summary>
        /// Presunie focus na ďalšiu bunku (Tab)
        /// </summary>
        Task MoveToNextCellAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie focus na predchádzajúcu bunku (Shift+Tab)
        /// </summary>
        Task MoveToPreviousCellAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie focus na bunku nižšie (Enter)
        /// </summary>
        Task MoveToCellBelowAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie focus na bunku vyššie (Shift+Enter alebo Arrow Up)
        /// </summary>
        Task MoveToCellAboveAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie focus na bunku vľavo (Arrow Left)
        /// </summary>
        Task MoveToCellLeftAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Presunie focus na bunku vpravo (Arrow Right)
        /// </summary>
        Task MoveToCellRightAsync(int currentRow, int currentColumn);

        /// <summary>
        /// Rozšíri selection s Shift+Arrow
        /// </summary>
        Task ExtendSelectionAsync(int fromRow, int fromColumn, int toRow, int toColumn);

        /// <summary>
        /// Vyberie všetky bunky (Ctrl+A)
        /// </summary>
        Task SelectAllCellsAsync();

        /// <summary>
        /// Získa aktuálnu pozíciu bunky z UI elementu
        /// </summary>
        (int Row, int Column) GetCellPosition(object uiElement);

        /// <summary>
        /// Získa UI element pre bunku na pozícii
        /// </summary>
        object? GetCellUIElement(int row, int column);
    }
}