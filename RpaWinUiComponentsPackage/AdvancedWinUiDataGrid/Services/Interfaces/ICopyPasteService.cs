// Services/Interfaces/ICopyPasteService.cs - ✅ OPRAVENÝ - len ICopyPasteService
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre Copy/Paste funkcionalitu (Excel kompatibilita) - INTERNAL
    /// </summary>
    internal interface ICopyPasteService
    {
        /// <summary>
        /// Inicializuje copy/paste službu
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Skopíruje označené bunky do clipboardu (Excel formát)
        /// </summary>
        Task CopySelectedCellsAsync(List<CellSelection> selectedCells);

        /// <summary>
        /// Vloží dáta z clipboardu do označených buniek
        /// </summary>
        Task PasteFromClipboardAsync(int startRowIndex, int startColumnIndex);

        /// <summary>
        /// Vystrihne označené bunky (copy + clear)
        /// </summary>
        Task CutSelectedCellsAsync(List<CellSelection> selectedCells);

        /// <summary>
        /// Spracuje klávesové skratky (Ctrl+C, Ctrl+V, Ctrl+X)
        /// </summary>
        Task HandleKeyboardShortcutAsync(KeyRoutedEventArgs e);

        /// <summary>
        /// Kontroluje či je možné vložiť dáta z clipboardu
        /// </summary>
        Task<bool> CanPasteAsync();

        /// <summary>
        /// Získa náhľad dát z clipboardu
        /// </summary>
        Task<string> GetClipboardPreviewAsync();
    }
}