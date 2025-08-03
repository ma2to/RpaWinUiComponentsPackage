// Services/NavigationService.cs - ✅ OPRAVENÝ accessibility
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia navigačnej služby s komplexným UI state transition logovaním - INTERNAL
    /// </summary>
    internal class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> _logger;
        private bool _isInitialized = false;
        private INavigationCallback? _navigationCallback;

        // ✅ ROZŠÍRENÉ: UI state a performance tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private readonly Dictionary<string, object?> _currentCellState = new();
        private int _totalNavigationOperations = 0;
        private int _totalKeyPressesHandled = 0;
        private int _totalEditOperations = 0;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // UI State tracking
        private Models.Cell.CellPosition? _currentFocusedCell;
        private Models.Cell.CellPosition? _previousFocusedCell;
        private string? _currentEditValue;
        private string? _originalEditValue;
        private DateTime _lastNavigationTime = DateTime.MinValue;

        public NavigationService(ILogger<NavigationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("🔧 NavigationService created - InstanceId: {InstanceId}, LoggerType: {LoggerType}",
                _serviceInstanceId, _logger.GetType().Name);
        }

        /// <summary>
        /// Nastaví navigation callback pre komunikáciu s DataGrid
        /// </summary>
        public void SetNavigationCallback(INavigationCallback callback)
        {
            _navigationCallback = callback;
            _logger.LogDebug("🎮 Navigation callback set - Type: {CallbackType}", callback?.GetType().Name ?? "null");
        }

        /// <summary>
        /// Inicializuje navigačnú službu s UI state tracking systémom
        /// </summary>
        public Task InitializeAsync()
        {
            var operationId = StartOperation("InitializeAsync");
            
            try
            {
                _logger.LogInformation("🎮 NavigationService.InitializeAsync START - InstanceId: {InstanceId}",
                    _serviceInstanceId);

                // Initialize UI state tracking
                _currentCellState.Clear();
                _currentFocusedCell = null;
                _previousFocusedCell = null;
                _lastNavigationTime = DateTime.UtcNow;

                _isInitialized = true;

                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ NavigationService INITIALIZED - Duration: {Duration}ms, " +
                    "KeyboardShortcutsEnabled: {ShortcutsEnabled}, UIStateTrackingEnabled: {StateTracking}, " +
                    "SupportedKeys: [{SupportedKeys}]",
                    duration, true, true, "Tab, Shift+Tab, Enter, Shift+Enter, Esc, Ctrl+C/V/X");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in NavigationService.InitializeAsync - InstanceId: {InstanceId}",
                    _serviceInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Spracuje stlačenie klávesy v bunke s komplexným UI state transition logovaním
        /// </summary>
        public async Task HandleKeyDownAsync(object sender, KeyRoutedEventArgs e)
        {
            var operationId = StartOperation("HandleKeyDownAsync");
            _totalKeyPressesHandled++;
            
            try
            {
                if (!_isInitialized)
                {
                    _logger.LogWarning("🎮 HandleKeyDown called but NavigationService not initialized");
                    return;
                }

                if (sender is not TextBox textBox)
                {
                    _logger.LogDebug("🎮 HandleKeyDown: Sender is not TextBox, ignoring");
                    return;
                }

                // Analyze current UI state before processing
                var uiStateAnalysis = AnalyzeCurrentUIState(textBox);
                var keyAnalysis = AnalyzeKeyEvent(e);
                
                _logger.LogInformation("🎮 HandleKeyDown START - InstanceId: {InstanceId}, " +
                    "Key: {Key}, Modifiers: [{Modifiers}], TotalKeyPresses: {TotalPresses}, " +
                    "TextBoxContent: '{Content}', CursorPosition: {CursorPos}, " +
                    "UIState: {UIState}",
                    _serviceInstanceId, keyAnalysis.KeyName, string.Join(", ", keyAnalysis.Modifiers),
                    _totalKeyPressesHandled, uiStateAnalysis.TextContent, uiStateAnalysis.CursorPosition,
                    uiStateAnalysis.CurrentState);

                // Store previous state for comparison
                var previousState = CaptureUIState(textBox);
                
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Tab:
                        e.Handled = true;
                        if (IsShiftPressed())
                        {
                            _logger.LogDebug("🎮 Processing Shift+Tab navigation");
                            await HandleShiftTabAsync(textBox);
                        }
                        else
                        {
                            _logger.LogDebug("🎮 Processing Tab navigation");
                            await HandleTabAsync(textBox);
                        }
                        break;

                    case Windows.System.VirtualKey.Enter:
                        e.Handled = true;
                        if (IsShiftPressed())
                        {
                            _logger.LogDebug("🎮 Processing Shift+Enter (new line in cell)");
                            await HandleShiftEnterAsync(textBox);
                        }
                        else
                        {
                            _logger.LogDebug("🎮 Processing Enter (move to cell below)");
                            await HandleEnterAsync(textBox);
                        }
                        break;

                    case Windows.System.VirtualKey.Escape:
                        e.Handled = true;
                        _logger.LogDebug("🎮 Processing Escape (cancel cell edit)");
                        await HandleEscapeAsync(textBox);
                        break;

                    case Windows.System.VirtualKey.C when IsCtrlPressed():
                        _logger.LogInformation("📋 Copy operation detected - allowing system to handle, " +
                            "TextLength: {TextLength}, HasSelection: {HasSelection}",
                            textBox.Text?.Length ?? 0, textBox.SelectionLength > 0);
                        break;

                    case Windows.System.VirtualKey.V when IsCtrlPressed():
                        _logger.LogInformation("📋 Paste operation detected - allowing system to handle, " +
                            "CurrentTextLength: {TextLength}, CursorPos: {CursorPos}",
                            textBox.Text?.Length ?? 0, textBox.SelectionStart);
                        break;

                    case Windows.System.VirtualKey.X when IsCtrlPressed():
                        _logger.LogInformation("📋 Cut operation detected - allowing system to handle, " +
                            "TextLength: {TextLength}, SelectionLength: {SelectionLength}",
                            textBox.Text?.Length ?? 0, textBox.SelectionLength);
                        break;

                    // ✅ NOVÉ: Arrow keys navigation
                    case Windows.System.VirtualKey.Up:
                        if (_navigationCallback != null)
                        {
                            e.Handled = true;
                            var position = _navigationCallback.GetCellPosition(textBox);
                            int row = position.Row;
                            int column = position.Column;
                            if (IsShiftPressed())
                            {
                                // Extend selection upward
                                await _navigationCallback.ExtendSelectionAsync(row, column, row - 1, column);
                                _logger.LogDebug("🎮 Shift+Up: Extending selection upward");
                            }
                            else
                            {
                                await _navigationCallback.MoveToCellAboveAsync(row, column);
                                _logger.LogDebug("🎮 Up: Moving to cell above");
                            }
                        }
                        break;

                    case Windows.System.VirtualKey.Down:
                        if (_navigationCallback != null)
                        {
                            e.Handled = true;
                            var position = _navigationCallback.GetCellPosition(textBox);
                            int row = position.Row;
                            int column = position.Column;
                            if (IsShiftPressed())
                            {
                                // Extend selection downward
                                await _navigationCallback.ExtendSelectionAsync(row, column, row + 1, column);
                                _logger.LogDebug("🎮 Shift+Down: Extending selection downward");
                            }
                            else
                            {
                                await _navigationCallback.MoveToCellBelowAsync(row, column);
                                _logger.LogDebug("🎮 Down: Moving to cell below");
                            }
                        }
                        break;

                    case Windows.System.VirtualKey.Left:
                        if (_navigationCallback != null && textBox.SelectionStart == 0)
                        {
                            e.Handled = true;
                            var position = _navigationCallback.GetCellPosition(textBox);
                            int row = position.Row;
                            int column = position.Column;
                            if (IsShiftPressed())
                            {
                                // Extend selection left
                                await _navigationCallback.ExtendSelectionAsync(row, column, row, column - 1);
                                _logger.LogDebug("🎮 Shift+Left: Extending selection left");
                            }
                            else
                            {
                                await _navigationCallback.MoveToCellLeftAsync(row, column);
                                _logger.LogDebug("🎮 Left: Moving to cell left");
                            }
                        }
                        break;

                    case Windows.System.VirtualKey.Right:
                        if (_navigationCallback != null && textBox.SelectionStart == textBox.Text?.Length)
                        {
                            e.Handled = true;
                            var position = _navigationCallback.GetCellPosition(textBox);
                            int row = position.Row;
                            int column = position.Column;
                            if (IsShiftPressed())
                            {
                                // Extend selection right
                                await _navigationCallback.ExtendSelectionAsync(row, column, row, column + 1);
                                _logger.LogDebug("🎮 Shift+Right: Extending selection right");
                            }
                            else
                            {
                                await _navigationCallback.MoveToCellRightAsync(row, column);
                                _logger.LogDebug("🎮 Right: Moving to cell right");
                            }
                        }
                        break;

                    // ✅ NOVÉ: Ctrl+A pre Select All
                    case Windows.System.VirtualKey.A when IsCtrlPressed():
                        if (_navigationCallback != null)
                        {
                            e.Handled = true;
                            await _navigationCallback.SelectAllCellsAsync();
                            _logger.LogDebug("🎮 Ctrl+A: Selecting all cells");
                        }
                        break;
                }

                // Analyze state change after processing
                var newState = CaptureUIState(textBox);
                var stateChange = CompareUIStates(previousState, newState);
                
                var duration = EndOperation(operationId);
                var navigationRate = _totalNavigationOperations > 0 && duration > 0 ? 
                    _totalNavigationOperations / duration : 0;

                _logger.LogInformation("✅ HandleKeyDown COMPLETED - Duration: {Duration}ms, " +
                    "Key: {Key}, StateChange: {StateChange}, NavigationOps: {NavOps}, " +
                    "NavigationRate: {NavRate:F1} ops/ms, KeyPressRate: {KeyRate:F1} keys/ms",
                    duration, keyAnalysis.KeyName, stateChange, _totalNavigationOperations,
                    navigationRate, duration > 0 ? _totalKeyPressesHandled / duration : 0);

                // Update navigation timing
                _lastNavigationTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in HandleKeyDownAsync - InstanceId: {InstanceId}, " +
                    "Key: {Key}, TotalKeyPresses: {TotalPresses}",
                    _serviceInstanceId, e.Key, _totalKeyPressesHandled);
                throw;
            }
        }

        /// <summary>
        /// Presunie fokus na ďalšiu bunku (Tab) s komplexným navigation tracking
        /// </summary>
        public async Task MoveToNextCellAsync(int currentRow, int currentColumn)
        {
            var operationId = StartOperation("MoveToNextCellAsync");
            _totalNavigationOperations++;
            
            try
            {
                _logger.LogInformation("🎮 MoveToNextCell START - From: [{CurrentRow}, {CurrentColumn}], " +
                    "NavigationOps: {TotalOps}, InstanceId: {InstanceId}",
                    currentRow, currentColumn, _totalNavigationOperations, _serviceInstanceId);

                // Store previous position
                _previousFocusedCell = _currentFocusedCell;
                
                // Calculate next cell position (simple increment logic)
                var nextRow = currentRow;
                var nextColumn = currentColumn + 1;
                
                // TODO: Get actual grid bounds from configuration
                var maxColumns = _currentCellState.Count > 0 ? _currentCellState.Count : 10; // fallback
                
                if (nextColumn >= maxColumns)
                {
                    nextColumn = 0;
                    nextRow++;
                    _logger.LogDebug("🎮 Wrapping to next row - NewPosition: [{NewRow}, {NewColumn}]", 
                        nextRow, nextColumn);
                }

                // Update current focused cell
                _currentFocusedCell = new Models.Cell.CellPosition 
                { 
                    Row = nextRow, 
                    Column = nextColumn, 
                    ColumnName = $"Column_{nextColumn}" 
                };

                // TODO: Implementácia presunu fokusu
                // Táto metóda by mala interagovať s UI a presunúť fokus na ďalšiu bunku

                var duration = EndOperation(operationId);
                var timeSinceLastNav = (DateTime.UtcNow - _lastNavigationTime).TotalMilliseconds;

                _logger.LogInformation("✅ MoveToNextCell COMPLETED - Duration: {Duration}ms, " +
                    "From: [{FromRow}, {FromColumn}] → To: [{ToRow}, {ToColumn}], " +
                    "TimeSinceLastNav: {TimeSince:F0}ms, TotalNavOps: {TotalOps}",
                    duration, currentRow, currentColumn, nextRow, nextColumn, 
                    timeSinceLastNav, _totalNavigationOperations);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in MoveToNextCellAsync - From: [{CurrentRow}, {CurrentColumn}], " +
                    "InstanceId: {InstanceId}", currentRow, currentColumn, _serviceInstanceId);
                throw;
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
        /// Zruší editáciu bunky (Esc) s komplexným edit state tracking
        /// </summary>
        public async Task CancelCellEditAsync(object sender)
        {
            var operationId = StartOperation("CancelCellEditAsync");
            _totalEditOperations++;
            
            try
            {
                if (sender is TextBox textBox)
                {
                    var currentValue = textBox.Text ?? string.Empty;
                    var originalValue = _originalEditValue ?? string.Empty;
                    var hasChanges = currentValue != originalValue;
                    
                    _logger.LogInformation("🎮 CancelCellEdit START - InstanceId: {InstanceId}, " +
                        "CurrentValue: '{CurrentValue}', OriginalValue: '{OriginalValue}', " +
                        "HasChanges: {HasChanges}, EditOps: {EditOps}",
                        _serviceInstanceId, currentValue, originalValue, hasChanges, _totalEditOperations);

                    // Capture UI state before cancellation
                    var beforeState = CaptureUIState(textBox);

                    // TODO: Obnoviť pôvodnú hodnotu bunky
                    if (!string.IsNullOrEmpty(_originalEditValue))
                    {
                        textBox.Text = _originalEditValue;
                        _logger.LogDebug("🎮 Restored original value: '{OriginalValue}'", _originalEditValue);
                    }

                    // Presun fokus mimo TextBox (focus management)
                    textBox.IsEnabled = false;
                    textBox.IsEnabled = true;

                    // Capture UI state after cancellation
                    var afterState = CaptureUIState(textBox);
                    var stateChange = CompareUIStates(beforeState, afterState);

                    // Clear edit state
                    _currentEditValue = null;
                    _originalEditValue = null;

                    var duration = EndOperation(operationId);

                    _logger.LogInformation("✅ CancelCellEdit COMPLETED - Duration: {Duration}ms, " +
                        "ChangesDiscarded: {ChangesDiscarded}, StateChange: {StateChange}, " +
                        "TotalEditOps: {TotalEditOps}",
                        duration, hasChanges, stateChange, _totalEditOperations);
                }
                else
                {
                    _logger.LogWarning("🎮 CancelCellEdit: Sender is not TextBox - {SenderType}", 
                        sender?.GetType().Name ?? "null");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CancelCellEditAsync - InstanceId: {InstanceId}, " +
                    "EditOps: {EditOps}", _serviceInstanceId, _totalEditOperations);
                throw;
            }
        }

        /// <summary>
        /// Dokončí editáciu bunky s komplexnou value validation a change tracking
        /// </summary>
        public async Task FinishCellEditAsync(object sender)
        {
            var operationId = StartOperation("FinishCellEditAsync");
            _totalEditOperations++;
            
            try
            {
                if (sender is TextBox textBox)
                {
                    var newValue = textBox.Text ?? string.Empty;
                    var originalValue = _originalEditValue ?? string.Empty;
                    var hasChanges = newValue != originalValue;
                    var valueLength = newValue.Length;
                    
                    _logger.LogInformation("🎮 FinishCellEdit START - InstanceId: {InstanceId}, " +
                        "NewValue: '{NewValue}', OriginalValue: '{OriginalValue}', " +
                        "HasChanges: {HasChanges}, ValueLength: {ValueLength}, EditOps: {EditOps}",
                        _serviceInstanceId, newValue, originalValue, hasChanges, valueLength, _totalEditOperations);

                    // Capture UI state before finishing
                    var beforeState = CaptureUIState(textBox);

                    // Value validation
                    var validation = ValidateEditValue(newValue);
                    
                    if (!validation.IsValid)
                    {
                        _logger.LogWarning("🎮 FinishCellEdit: Validation failed - Issues: [{Issues}]",
                            string.Join(", ", validation.Issues));
                    }

                    // TODO: Uložiť hodnotu bunky a spustiť validáciu
                    // await _dataManagementService.SetCellValueAsync(currentRow, currentColumn, newValue);

                    // Update edit state
                    _currentEditValue = newValue;
                    if (hasChanges)
                    {
                        _logger.LogDebug("🎮 Cell value changed from '{OldValue}' to '{NewValue}'", 
                            originalValue, newValue);
                    }

                    // Capture UI state after finishing
                    var afterState = CaptureUIState(textBox);
                    var stateChange = CompareUIStates(beforeState, afterState);

                    var duration = EndOperation(operationId);
                    var editRate = _totalEditOperations > 0 && duration > 0 ? 
                        _totalEditOperations / duration : 0;

                    _logger.LogInformation("✅ FinishCellEdit COMPLETED - Duration: {Duration}ms, " +
                        "ValueSaved: '{SavedValue}', HasChanges: {HasChanges}, " +
                        "ValidationResult: {ValidationResult}, StateChange: {StateChange}, " +
                        "EditRate: {EditRate:F1} edits/ms, TotalEditOps: {TotalEditOps}",
                        duration, newValue, hasChanges, validation.IsValid ? "VALID" : "INVALID",
                        stateChange, editRate, _totalEditOperations);
                }
                else
                {
                    _logger.LogWarning("🎮 FinishCellEdit: Sender is not TextBox - {SenderType}", 
                        sender?.GetType().Name ?? "null");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in FinishCellEditAsync - InstanceId: {InstanceId}, " +
                    "EditOps: {EditOps}", _serviceInstanceId, _totalEditOperations);
                throw;
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
            
            if (_navigationCallback != null)
            {
                var position = _navigationCallback.GetCellPosition(textBox);
                int row = position.Row;
                int column = position.Column;
                await _navigationCallback.MoveToNextCellAsync(row, column);
                _logger.LogDebug("🎮 Tab: Moved to next cell from [{Row},{Column}]", row, column);
            }
            else
            {
                _logger.LogWarning("🎮 Tab: Navigation callback not set");
            }
        }

        private async Task HandleShiftTabAsync(TextBox textBox)
        {
            await FinishCellEditAsync(textBox);
            
            if (_navigationCallback != null)
            {
                var position = _navigationCallback.GetCellPosition(textBox);
                int row = position.Row;
                int column = position.Column;
                await _navigationCallback.MoveToPreviousCellAsync(row, column);
                _logger.LogDebug("🎮 Shift+Tab: Moved to previous cell from [{Row},{Column}]", row, column);
            }
            else
            {
                _logger.LogWarning("🎮 Shift+Tab: Navigation callback not set");
            }
        }

        private async Task HandleEnterAsync(TextBox textBox)
        {
            await FinishCellEditAsync(textBox);
            
            if (_navigationCallback != null)
            {
                var position = _navigationCallback.GetCellPosition(textBox);
                int row = position.Row;
                int column = position.Column;
                await _navigationCallback.MoveToCellBelowAsync(row, column);
                _logger.LogDebug("🎮 Enter: Moved to cell below from [{Row},{Column}]", row, column);
            }
            else
            {
                _logger.LogWarning("🎮 Enter: Navigation callback not set");
            }
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

        #region ✅ Performance Tracking & UI State Analysis Helper Methods

        /// <summary>
        /// Spustí sledovanie operácie a vráti jej ID
        /// </summary>
        private string StartOperation(string operationName)
        {
            var operationId = $"{operationName}_{Guid.NewGuid():N}"[..16];
            _operationStartTimes[operationId] = DateTime.UtcNow;
            _operationCounters[operationName] = _operationCounters.GetValueOrDefault(operationName, 0) + 1;
            
            _logger.LogTrace("⏱️ Navigation Operation START - {OperationName} (ID: {OperationId}), " +
                "TotalCalls: {TotalCalls}",
                operationName, operationId, _operationCounters[operationName]);
                
            return operationId;
        }

        /// <summary>
        /// Ukončí sledovanie operácie a vráti dobu trvania v ms
        /// </summary>
        private double EndOperation(string operationId)
        {
            if (_operationStartTimes.TryGetValue(operationId, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationId);
                
                _logger.LogTrace("⏱️ Navigation Operation END - ID: {OperationId}, Duration: {Duration:F2}ms", 
                    operationId, duration);
                    
                return duration;
            }
            
            _logger.LogWarning("⏱️ Navigation Operation END - Unknown operation ID: {OperationId}", operationId);
            return 0;
        }

        /// <summary>
        /// Analyzuje current UI state TextBoxu
        /// </summary>
        private UIStateAnalysis AnalyzeCurrentUIState(Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            var analysis = new UIStateAnalysis
            {
                TextContent = textBox.Text ?? string.Empty,
                CursorPosition = textBox.SelectionStart,
                SelectionLength = textBox.SelectionLength,
                IsEnabled = textBox.IsEnabled,
                IsFocused = textBox.FocusState != Microsoft.UI.Xaml.FocusState.Unfocused,
                MaxLength = textBox.MaxLength,
                IsReadOnly = textBox.IsReadOnly
            };

            analysis.CurrentState = $"Enabled:{analysis.IsEnabled}, Focused:{analysis.IsFocused}, " +
                $"ReadOnly:{analysis.IsReadOnly}, TextLen:{analysis.TextContent.Length}";

            return analysis;
        }

        /// <summary>
        /// Analyzuje key event properties
        /// </summary>
        private KeyAnalysis AnalyzeKeyEvent(KeyRoutedEventArgs e)
        {
            var analysis = new KeyAnalysis
            {
                KeyName = e.Key.ToString(),
                Modifiers = new List<string>()
            };

            if (IsCtrlPressed()) analysis.Modifiers.Add("Ctrl");
            if (IsShiftPressed()) analysis.Modifiers.Add("Shift");
            if (IsAltPressed()) analysis.Modifiers.Add("Alt");

            return analysis;
        }

        /// <summary>
        /// Zachytí aktuálny UI state pre porovnanie
        /// </summary>
        private UISnapshot CaptureUIState(Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            return new UISnapshot
            {
                TextContent = textBox.Text ?? string.Empty,
                CursorPosition = textBox.SelectionStart,
                SelectionLength = textBox.SelectionLength,
                FocusState = textBox.FocusState.ToString(),
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Porovná dva UI states a vráti opis zmien
        /// </summary>
        private string CompareUIStates(UISnapshot before, UISnapshot after)
        {
            var changes = new List<string>();

            if (before.TextContent != after.TextContent)
                changes.Add($"Text: '{before.TextContent}' → '{after.TextContent}'");
                
            if (before.CursorPosition != after.CursorPosition)
                changes.Add($"Cursor: {before.CursorPosition} → {after.CursorPosition}");
                
            if (before.SelectionLength != after.SelectionLength)
                changes.Add($"Selection: {before.SelectionLength} → {after.SelectionLength}");
                
            if (before.FocusState != after.FocusState)
                changes.Add($"Focus: {before.FocusState} → {after.FocusState}");

            return changes.Any() ? string.Join(", ", changes) : "No changes";
        }

        /// <summary>
        /// Overuje či je Alt stlačený
        /// </summary>
        private static bool IsAltPressed()
        {
            var altState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu);
            return (altState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        /// <summary>
        /// Validuje hodnotu pred uložením do bunky
        /// </summary>
        private EditValidation ValidateEditValue(string value)
        {
            var validation = new EditValidation { IsValid = true };

            if (value == null)
            {
                validation.Issues.Add("Value is null");
                validation.IsValid = false;
                return validation;
            }

            // Basic validation rules
            if (value.Length > 1000) // Arbitrary limit
            {
                validation.Issues.Add($"Value too long: {value.Length} characters (max: 1000)");
                validation.IsValid = false;
            }

            // Check for potentially dangerous content
            if (value.Contains("<script", StringComparison.OrdinalIgnoreCase))
            {
                validation.Issues.Add("Potentially dangerous content detected");
                validation.IsValid = false;
            }

            return validation;
        }

        #endregion
    }

    /// <summary>
    /// UI State analysis data - INTERNAL
    /// </summary>
    internal class UIStateAnalysis
    {
        public string TextContent { get; set; } = string.Empty;
        public int CursorPosition { get; set; }
        public int SelectionLength { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFocused { get; set; }
        public int MaxLength { get; set; }
        public bool IsReadOnly { get; set; }
        public string CurrentState { get; set; } = string.Empty;
    }

    /// <summary>
    /// Key analysis data - INTERNAL
    /// </summary>
    internal class KeyAnalysis
    {
        public string KeyName { get; set; } = string.Empty;
        public List<string> Modifiers { get; set; } = new();
    }

    /// <summary>
    /// UI snapshot for state comparison - INTERNAL
    /// </summary>
    internal class UISnapshot
    {
        public string TextContent { get; set; } = string.Empty;
        public int CursorPosition { get; set; }
        public int SelectionLength { get; set; }
        public string FocusState { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }


    /// <summary>
    /// Edit validation result - INTERNAL
    /// </summary>
    internal class EditValidation
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}