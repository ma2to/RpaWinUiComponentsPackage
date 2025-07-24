// Services/NavigationService.cs
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia navigačnej služby pre DataGrid
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> _logger;
        private bool _isInitialized = false;

        public NavigationService(ILogger<NavigationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Inicializuje navigačnú službu
        /// </summary>
        public Task InitializeAsync()
        {
            _logger.LogInformation("NavigationService inicializovaný");
            _isInitialized = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Spracuje stlačenie klávesy v bunke
        /// </summary>
        public async Task HandleKeyDownAsync(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized)
                    return;

                if (sender is not TextBox textBox)
                    return;

                _logger.LogDebug("Spracovávam klávesovú skratku: {Key}", e.Key);

                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Tab:
                        e.Handled = true;
                        if (IsShiftPressed())
                        {
                            await HandleShiftTabAsync(textBox);
                        }
                        else
                        {
                            await HandleTabAsync(textBox);
                        }
                        break;

                    case Windows.System.VirtualKey.Enter:
                        e.Handled = true;
                        if (IsShiftPressed())
                        {
                            await HandleShiftEnterAsync(textBox);
                        }
                        else
                        {
                            await HandleEnterAsync(textBox);
                        }
                        break;

                    case Windows.System.VirtualKey.Escape:
                        e.Handled = true;
                        await HandleEscapeAsync(textBox);
                        break;

                    case Windows.System.VirtualKey.C when IsCtrlPressed():
                        // Copy - nechaj systém spracovať, ale zalógujem
                        _logger.LogDebug("Copy operation detected");
                        break;

                    case Windows.System.VirtualKey.V when IsCtrlPressed():
                        // Paste - nechaj systém spracovať, ale zalógujem
                        _logger.LogDebug("Paste operation detected");
                        break;

                    case Windows.System.VirtualKey.X when IsCtrlPressed():
                        // Cut - nechaj systém spracovať, ale zalógujem
                        _logger.LogDebug("Cut operation detected");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri spracovaní KeyDown event");
            }
        }

        /// <summary>
        /// Presunie fokus na ďalšiu bunku (Tab)
        /// </summary>
        public async Task MoveToNextCellAsync(int currentRow, int currentColumn)
        {
            try
            {
                _logger.LogDebug("Presúvam na ďalšiu bunku z [{CurrentRow}, {CurrentColumn}]", currentRow, currentColumn);

                // TODO: Implementácia presunu fokusu
                // Táto metóda by mala interagovať s UI a presunúť fokus na ďalšiu bunku

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri presune na ďalšiu bunku");
            }
        }

        /// <summary>
        /// Presunie fokus na predchádzajúcu bunku (Shift+Tab)
        /// </summary>
        public async Task MoveToPreviousCellAsync(int currentRow, int currentColumn)
        {
            try
            {
                _logger.LogDebug("Presúvam na predchádzajúcu bunku z [{CurrentRow}, {CurrentColumn}]", currentRow, currentColumn);

                // TODO: Implementácia presunu fokusu

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri presune na predchádzajúcu bunku");
            }
        }

        /// <summary>
        /// Presunie fokus na bunku nižšie (Enter)
        /// </summary>
        public async Task MoveToCellBelowAsync(int currentRow, int currentColumn)
        {
            try
            {
                _logger.LogDebug("Presúvam na bunku nižšie z [{CurrentRow}, {CurrentColumn}]", currentRow, currentColumn);

                // TODO: Implementácia presunu fokusu

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri presune na bunku nižšie");
            }
        }

        /// <summary>
        /// Presunie fokus na bunku vyššie (Shift+Enter)
        /// </summary>
        public async Task MoveToCellAboveAsync(int currentRow, int currentColumn)
        {
            try
            {
                _logger.LogDebug("Presúvam na bunku vyššie z [{CurrentRow}, {CurrentColumn}]", currentRow, currentColumn);

                // TODO: Implementácia presunu fokusu

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri presune na bunku vyššie");
            }
        }

        /// <summary>
        /// Zruší editáciu bunky (Esc)
        /// </summary>
        public async Task CancelCellEditAsync(object sender)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    _logger.LogDebug("Zrušujem editáciu bunky");

                    // TODO: Obnoviť pôvodnú hodnotu bunky
                    // textBox.Text = originalValue;

                    // Presun fokus mimo TextBox
                    textBox.IsEnabled = false;
                    textBox.IsEnabled = true;
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri zrušovaní editácie bunky");
            }
        }

        /// <summary>
        /// Dokončí editáciu bunky
        /// </summary>
        public async Task FinishCellEditAsync(object sender)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    _logger.LogDebug("Dokončujem editáciu bunky s hodnotou: {Value}", textBox.Text);

                    // TODO: Uložiť hodnotu bunky a spustiť validáciu
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri dokončovaní editácie bunky");
            }
        }

        /// <summary>
        /// Pridá nový riadok v bunke (Shift+Enter)
        /// </summary>
        public async Task InsertNewLineInCellAsync(object sender)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    _logger.LogDebug("Pridávam nový riadok v bunke");

                    var currentPosition = textBox.SelectionStart;
                    var currentText = textBox.Text;

                    var newText = currentText.Insert(currentPosition, Environment.NewLine);
                    textBox.Text = newText;
                    textBox.SelectionStart = currentPosition + Environment.NewLine.Length;
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri pridávaní nového riadku v bunke");
            }
        }

        #region Private Helper Methods

        private async Task HandleTabAsync(TextBox textBox)
        {
            await FinishCellEditAsync(textBox);
            // TODO: Presun na ďalšiu bunku
            _logger.LogDebug("Tab: Presúvam na ďalšiu bunku");
        }

        private async Task HandleShiftTabAsync(TextBox textBox)
        {
            await FinishCellEditAsync(textBox);
            // TODO: Presun na predchádzajúcu bunku
            _logger.LogDebug("Shift+Tab: Presúvam na predchádzajúcu bunku");
        }

        private async Task HandleEnterAsync(TextBox textBox)
        {
            await FinishCellEditAsync(textBox);
            // TODO: Presun na bunku nižšie
            _logger.LogDebug("Enter: Presúvam na bunku nižšie");
        }

        private async Task HandleShiftEnterAsync(TextBox textBox)
        {
            await InsertNewLineInCellAsync(textBox);
            _logger.LogDebug("Shift+Enter: Pridaný nový riadok v bunke");
        }

        private async Task HandleEscapeAsync(TextBox textBox)
        {
            await CancelCellEditAsync(textBox);
            _logger.LogDebug("Escape: Zrušená editácia bunky");
        }

        private static bool IsCtrlPressed()
        {
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            return (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        private static bool IsShiftPressed()
        {
            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
            return (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion
    }
}