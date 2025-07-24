// Controls/SpecialColumns/ValidationAlertsColumn.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Špeciálny stĺpec pre zobrazovanie validačných upozornení
    /// </summary>
    public sealed partial class ValidationAlertsColumn : UserControl, INotifyPropertyChanged
    {
        #region Private fields

        private string _validationErrors = string.Empty;
        private List<ValidationAlert> _alerts = new();

        #endregion

        #region Constructor

        public ValidationAlertsColumn()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Validačné chyby ako string (oddelené ';')
        /// </summary>
        public string ValidationErrors
        {
            get => _validationErrors;
            set
            {
                if (SetProperty(ref _validationErrors, value))
                {
                    ParseValidationErrors(value);
                    UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// Zoznam parsovaných validačných alertov
        /// </summary>
        public List<ValidationAlert> Alerts
        {
            get => _alerts;
            private set => SetProperty(ref _alerts, value);
        }

        /// <summary>
        /// Či má bunka validačné chyby
        /// </summary>
        public bool HasErrors => Alerts.Any();

        /// <summary>
        /// Počet validačných chýb
        /// </summary>
        public int ErrorCount => Alerts.Count;

        /// <summary>
        /// Formátovaný text pre zobrazenie
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (!HasErrors) return string.Empty;

                return string.Join("\n", Alerts.Select(a => $"• {a.ColumnName}: {a.Message}"));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parsuje validation errors string na zoznam alertov
        /// </summary>
        private void ParseValidationErrors(string errorsString)
        {
            var newAlerts = new List<ValidationAlert>();

            if (string.IsNullOrWhiteSpace(errorsString))
            {
                Alerts = newAlerts;
                return;
            }

            // Rozdel na jednotlivé chyby (oddelené ';')
            var errors = errorsString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var error in errors)
            {
                var trimmedError = error.Trim();
                if (string.IsNullOrEmpty(trimmedError)) continue;

                // Pokús sa parsovať formát "ColumnName: Message"
                var colonIndex = trimmedError.IndexOf(':');

                if (colonIndex > 0 && colonIndex < trimmedError.Length - 1)
                {
                    var columnName = trimmedError.Substring(0, colonIndex).Trim();
                    var message = trimmedError.Substring(colonIndex + 1).Trim();

                    newAlerts.Add(new ValidationAlert
                    {
                        ColumnName = columnName,
                        Message = message,
                        Severity = ValidationSeverity.Error
                    });
                }
                else
                {
                    // Ak nie je v štandardnom formáte, použi celý text ako message
                    newAlerts.Add(new ValidationAlert
                    {
                        ColumnName = "General",
                        Message = trimmedError,
                        Severity = ValidationSeverity.Error
                    });
                }
            }

            Alerts = newAlerts;
        }

        /// <summary>
        /// Aktualizuje zobrazenie v UI
        /// </summary>
        private void UpdateDisplay()
        {
            if (AlertsTextBlock != null)
            {
                AlertsTextBlock.Text = DisplayText;

                // Nastav farbu podľa toho či sú chyby
                if (HasErrors)
                {
                    AlertsTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    AlertsCellBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 255, 0, 0)); // Jemne červené pozadie
                }
                else
                {
                    AlertsTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    AlertsCellBorder.Background = Application.Current.Resources["LayerFillColorDefaultBrush"] as SolidColorBrush
                                                ?? new SolidColorBrush(Microsoft.UI.Colors.White);
                }
            }

            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(ErrorCount));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Pridá validačnú chybu
        /// </summary>
        public void AddValidationError(string columnName, string message, ValidationSeverity severity = ValidationSeverity.Error)
        {
            var newAlert = new ValidationAlert
            {
                ColumnName = columnName,
                Message = message,
                Severity = severity
            };

            var currentAlerts = Alerts.ToList();

            // Odstráň existujúcu chybu pre ten istý stĺpec (ak existuje)
            currentAlerts.RemoveAll(a => a.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            // Pridaj novú chybu
            currentAlerts.Add(newAlert);

            Alerts = currentAlerts;
            UpdateValidationErrorsString();
            UpdateDisplay();
        }

        /// <summary>
        /// Odstráni validačnú chybu pre stĺpec
        /// </summary>
        public void RemoveValidationError(string columnName)
        {
            var currentAlerts = Alerts.ToList();
            currentAlerts.RemoveAll(a => a.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            Alerts = currentAlerts;
            UpdateValidationErrorsString();
            UpdateDisplay();
        }

        /// <summary>
        /// Vyčisti všetky validačné chyby
        /// </summary>
        public void ClearAllErrors()
        {
            Alerts = new List<ValidationAlert>();
            ValidationErrors = string.Empty;
            UpdateDisplay();
        }

        /// <summary>
        /// Získa chyby pre konkrétny stĺpec
        /// </summary>
        public List<ValidationAlert> GetErrorsForColumn(string columnName)
        {
            return Alerts.Where(a => a.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Aktualizuje ValidationErrors string na základe Alerts
        /// </summary>
        private void UpdateValidationErrorsString()
        {
            if (!Alerts.Any())
            {
                _validationErrors = string.Empty;
            }
            else
            {
                _validationErrors = string.Join("; ", Alerts.Select(a => $"{a.ColumnName}: {a.Message}"));
            }

            OnPropertyChanged(nameof(ValidationErrors));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Reprezentuje jeden validačný alert
    /// </summary>
    public class ValidationAlert
    {
        /// <summary>
        /// Názov stĺpca ku ktorému sa alert vzťahuje
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Správa alertu
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Závažnosť alertu
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

        /// <summary>
        /// Timestamp kedy bol alert vytvorený
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{ColumnName}: {Message}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is ValidationAlert other)
            {
                return ColumnName.Equals(other.ColumnName, StringComparison.OrdinalIgnoreCase) &&
                       Message.Equals(other.Message, StringComparison.Ordinal);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ColumnName.ToLowerInvariant(), Message);
        }
    }

    /// <summary>
    /// Závažnosť validačného alertu
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}