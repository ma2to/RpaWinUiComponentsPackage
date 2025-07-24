// Services/Interfaces/ICopyPasteService.cs
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre Copy/Paste funkcionalitu (Excel kompatibilita)
    /// </summary>
    public interface ICopyPasteService
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

    /// <summary>
    /// Reprezentuje výber bunky pre copy/paste operácie
    /// </summary>
    public class CellSelection
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public object? Value { get; set; }
    }
}