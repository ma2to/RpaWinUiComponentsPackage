// Services/NavigationService.cs
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Služba pre klávesovú navigáciu v DataGrid komponente.
    /// Zabezpečuje Tab/Enter/Esc/Shift+Enter správanie.
    /// </summary>
    internal class NavigationService : INavigationService
    {
        #region Private fields

        private GridConfiguration? _configuration;
        private AdvancedDataGrid? _dataGrid;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // Aktuálna pozícia
        private (int row, int col)? _currentPosition = null;
        private bool _isEditing = false;

        #endregion

        #region Events

        public event EventHandler<NavigationEventArgs>? CellNavigated;
        public event EventHandler<EditModeChangedEventArgs>? EditModeChanged;

        #endregion

        #region INavigationService - Inicializácia

        public async Task InitializeAsync(GridConfiguration configuration, AdvancedDataGrid dataGrid)
        {
            if (_isInitialized)
                throw new InvalidOperationException("NavigationService je už inicializovaný");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));

            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        #endregion

        #region INavigationService - Klávesová navigácia

        public void HandleKeyDown(KeyRoutedEventArgs e)
        {
            if (!_isInitialized || _dataGrid == null)
                return;

            var key = e.Key;
            var isShiftPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            var isCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            try
            {
                switch (key)
                {
                    case VirtualKey.Tab:
                        if (!isShiftPressed)
                        {
                            HandleTabNavigation();
                            e.Handled = true;
                        }
                        else
                        {
                            HandleShiftTabNavigation();
                            e.Handled = true;
                        }
                        break;

                    case VirtualKey.Enter:
                        if (!isShiftPressed)
                        {
                            HandleEnterNavigation();
                            e.Handled = true;
                        }
                        else
                        {
                            HandleShiftEnterNavigation();
                            e.Handled = true;
                        }
                        break;

                    case VirtualKey.Escape:
                        HandleEscapeNavigation();
                        e.Handled = true;
                        break;

                    case VirtualKey.F2:
                        HandleF2Edit();
                        e.Handled = true;
                        break;

                    // Copy/Paste shortcuts
                    case VirtualKey.C when isCtrlPressed:
                        _ = HandleCopyAsync();
                        e.Handled = true;
                        break;

                    case VirtualKey.V when isCtrlPressed:
                        _ = HandlePasteAsync();
                        e.Handled = true;
                        break;

                    case VirtualKey.X when isCtrlPressed:
                        _ = HandleCutAsync();
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri klávesovej navigácii: {ex.Message}");
            }
        }

        #endregion

        #region INavigationService - Navigačné operácie

        public bool MoveToCell(int rowIndex, int columnIndex)
        {
            if (!IsValidPosition(rowIndex, columnIndex))
                return false;

            var oldPosition = _currentPosition;
            _currentPosition = (rowIndex, columnIndex);

            OnCellNavigated(oldPosition, _currentPosition);
            return true;
        }

        public bool MoveNext()
        {
            if (_currentPosition == null)
            {
                return MoveToCell(0, 0);
            }

            var (row, col) = _currentPosition.Value;
            var dataColumnCount = _configuration?.DataColumnCount ?? 0;

            // Ďalší stĺpec v riadku
            if (col + 1 < dataColumnCount)
            {
                return MoveToCell(row, col + 1);
            }

            // Ďalší riadok, prvý stĺpec
            return MoveToCell(row + 1, 0);
        }

        public bool MovePrevious()
        {
            if (_currentPosition == null)
                return false;

            var (row, col) = _currentPosition.Value;
            var dataColumnCount = _configuration?.DataColumnCount ?? 0;

            // Predchádzajúci stĺpec v riadku
            if (col > 0)
            {
                return MoveToCell(row, col - 1);
            }

            // Predchádzajúci riadok, posledný stĺpec
            if (row > 0)
            {
                return MoveToCell(row - 1, dataColumnCount - 1);
            }

            return false;
        }

        public bool MoveUp()
        {
            if (_currentPosition == null)
                return false;

            var (row, col) = _currentPosition.Value;
            return MoveToCell(row - 1, col);
        }

        public bool MoveDown()
        {
            if (_currentPosition == null)
                return false;

            var (row, col) = _currentPosition.Value;
            return MoveToCell(row + 1, col);
        }

        #endregion

        #region INavigationService - Edit mode

        public void StartEdit()
        {
            if (_isEditing)
                return;

            _isEditing = true;
            OnEditModeChanged(true);
        }

        public void StopEdit(bool commitChanges = true)
        {
            if (!_isEditing)
                return;

            _isEditing = false;

            if (commitChanges && _currentPosition.HasValue)
            {
                _ = CommitCurrentCellAsync();
            }
            else if (!commitChanges && _currentPosition.HasValue)
            {
                _ = RevertCurrentCellAsync();
            }

            OnEditModeChanged(false);
        }

        public bool IsEditing => _isEditing;

        public (int row, int col)? CurrentPosition => _currentPosition;

        #endregion

        #region Private - Klávesové operácie

        private void HandleTabNavigation()
        {
            if (_isEditing)
            {
                StopEdit(commitChanges: true);
            }

            MoveNext();
        }

        private void HandleShiftTabNavigation()
        {
            if (_isEditing)
            {
                StopEdit(commitChanges: true);
            }

            MovePrevious();
        }

        private void HandleEnterNavigation()
        {
            if (_isEditing)
            {
                StopEdit(commitChanges: true);
                MoveDown();
            }
            else
            {
                StartEdit();
            }
        }

        private void HandleShiftEnterNavigation()
        {
            if (_isEditing)
            {
                // Shift+Enter v edit mode = nový riadok v bunke
                // Toto by malo byť handled by TextBox directly
                return;
            }

            MoveUp();
        }

        private void HandleEscapeNavigation()
        {
            if (_isEditing)
            {
                StopEdit(commitChanges: false);
            }
        }

        private void HandleF2Edit()
        {
            if (!_isEditing)
            {
                StartEdit();
            }
        }

        #endregion

        #region Private - Copy/Paste operácie

        private async Task HandleCopyAsync()
        {
            // Implementation bude závisieť od selection service
            await Task.CompletedTask;
        }

        private async Task HandlePasteAsync()
        {
            // Implementation bude závisieť od copy/paste service
            await Task.CompletedTask;
        }

        private async Task HandleCutAsync()
        {
            // Implementation bude závisieť od copy/paste service
            await Task.CompletedTask;
        }

        #endregion

        #region Private - Helper methods

        private bool IsValidPosition(int rowIndex, int columnIndex)
        {
            if (_configuration == null)
                return false;

            var dataColumnCount = _configuration.DataColumnCount;

            return rowIndex >= 0 &&
                   columnIndex >= 0 &&
                   columnIndex < dataColumnCount;
        }

        private async Task CommitCurrentCellAsync()
        {
            if (!_currentPosition.HasValue)
                return;

            // Implementation by delegoval na DataManagementService
            await Task.CompletedTask;
        }

        private async Task RevertCurrentCellAsync()
        {
            if (!_currentPosition.HasValue)
                return;

            // Implementation by delegoval na DataManagementService  
            await Task.CompletedTask;
        }

        #endregion

        #region Event helpers

        private void OnCellNavigated((int row, int col)? oldPosition, (int row, int col)? newPosition)
        {
            CellNavigated?.Invoke(this, new NavigationEventArgs(oldPosition, newPosition));
        }

        private void OnEditModeChanged(bool isEditing)
        {
            EditModeChanged?.Invoke(this, new EditModeChangedEventArgs(isEditing));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                CellNavigated = null;
                EditModeChanged = null;
                _dataGrid = null;
                _configuration = null;

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during NavigationService dispose: {ex.Message}");
            }
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event args pre navigáciu medzi bunkami.
    /// </summary>
    internal class NavigationEventArgs : EventArgs
    {
        public NavigationEventArgs((int row, int col)? oldPosition, (int row, int col)? newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public (int row, int col)? OldPosition { get; }
        public (int row, int col)? NewPosition { get; }
    }

    /// <summary>
    /// Event args pre zmenu edit mode.
    /// </summary>
    internal class EditModeChangedEventArgs : EventArgs
    {
        public EditModeChangedEventArgs(bool isEditing)
        {
            IsEditing = isEditing;
        }

        public bool IsEditing { get; }
    }

    #endregion
}