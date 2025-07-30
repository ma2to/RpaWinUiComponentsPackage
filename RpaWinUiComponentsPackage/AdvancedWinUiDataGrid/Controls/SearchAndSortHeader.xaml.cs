// Controls/SearchAndSortHeader.xaml.cs - ✅ OPRAVENÉ XLS0414: PUBLIC pre XAML parser
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Header komponent s Search/Sort funkciou - ✅ PUBLIC (kvôli XAML parser, ale NIE oficiálne API)
    /// </summary>
    public sealed partial class SearchAndSortHeader : UserControl, INotifyPropertyChanged
    {
        #region Private Fields

        private string _headerTitle = string.Empty;
        private string _columnName = string.Empty;
        private SortDirection _currentSortDirection = SortDirection.None;
        private string _searchText = string.Empty;

        #endregion

        #region Constructor

        public SearchAndSortHeader()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Názov header-u (zobrazovaný text)
        /// </summary>
        public string HeaderTitle
        {
            get => _headerTitle;
            set => SetProperty(ref _headerTitle, value);
        }

        /// <summary>
        /// Názov stĺpca (pre search/sort operácie)
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set => SetProperty(ref _columnName, value);
        }

        /// <summary>
        /// Aktuálny sort direction
        /// </summary>
        public SortDirection CurrentSortDirection
        {
            get => _currentSortDirection;
            set
            {
                if (SetProperty(ref _currentSortDirection, value))
                {
                    UpdateSortIndicator();
                }
            }
        }

        /// <summary>
        /// Aktuálny search text
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Event pre sort request (klik na header)
        /// </summary>
        public event EventHandler<SortRequestEventArgs>? SortRequested;

        /// <summary>
        /// Event pre search request (zmena search textu)
        /// </summary>
        public event EventHandler<SearchRequestEventArgs>? SearchRequested;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Klik na header - toggle sort
        /// </summary>
        private void OnHeaderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Header klik pre stĺpec: {ColumnName}");

                var newDirection = CurrentSortDirection switch
                {
                    SortDirection.None => SortDirection.Ascending,
                    SortDirection.Ascending => SortDirection.Descending,
                    SortDirection.Descending => SortDirection.None,
                    _ => SortDirection.None
                };

                CurrentSortDirection = newDirection;

                SortRequested?.Invoke(this, new SortRequestEventArgs(ColumnName, newDirection));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri header click: {ex.Message}");
            }
        }

        /// <summary>
        /// Zmena search textu
        /// </summary>
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    SearchText = textBox.Text;
                    System.Diagnostics.Debug.WriteLine($"🔍 Search text zmena pre {ColumnName}: '{SearchText}'");

                    SearchRequested?.Invoke(this, new SearchRequestEventArgs(ColumnName, SearchText));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri search text change: {ex.Message}");
            }
        }

        /// <summary>
        /// Klávesy v search boxe
        /// </summary>
        private void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Enter:
                        // Force trigger search
                        if (sender is TextBox textBox)
                        {
                            SearchRequested?.Invoke(this, new SearchRequestEventArgs(ColumnName, textBox.Text));
                        }
                        e.Handled = true;
                        break;

                    case Windows.System.VirtualKey.Escape:
                        // Clear search
                        if (sender is TextBox textBox2)
                        {
                            textBox2.Text = "";
                            SearchRequested?.Invoke(this, new SearchRequestEventArgs(ColumnName, ""));
                        }
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri search key down: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Aktualizuje sort indikátor (▲▼)
        /// </summary>
        private void UpdateSortIndicator()
        {
            if (SortIndicator != null)
            {
                var indicatorText = CurrentSortDirection switch
                {
                    SortDirection.Ascending => "▲",
                    SortDirection.Descending => "▼",
                    _ => ""
                };

                SortIndicator.Text = indicatorText;
                SortIndicator.Visibility = string.IsNullOrEmpty(indicatorText) ? Visibility.Collapsed : Visibility.Visible;

                System.Diagnostics.Debug.WriteLine($"🔄 Sort indikátor aktualizovaný pre {ColumnName}: '{indicatorText}'");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Vyčistí search text
        /// </summary>
        public void ClearSearch()
        {
            if (SearchTextBox != null)
            {
                SearchTextBox.Text = "";
                SearchText = "";
            }
        }

        /// <summary>
        /// Nastaví search text programmatically
        /// </summary>
        public void SetSearchText(string text)
        {
            if (SearchTextBox != null)
            {
                SearchTextBox.Text = text ?? "";
                SearchText = text ?? "";
            }
        }

        /// <summary>
        /// Reset sort direction
        /// </summary>
        public void ResetSort()
        {
            CurrentSortDirection = SortDirection.None;
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

    #region Event Args Classes - ✅ PUBLIC kvôli XAML

    /// <summary>
    /// Event args pre sort request - ✅ PUBLIC (kvôli XAML parser)
    /// </summary>
    public class SortRequestEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public SortDirection Direction { get; }

        public SortRequestEventArgs(string columnName, SortDirection direction)
        {
            ColumnName = columnName;
            Direction = direction;
        }
    }

    /// <summary>
    /// Event args pre search request - ✅ PUBLIC (kvôli XAML parser)
    /// </summary>
    public class SearchRequestEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public string SearchText { get; }

        public SearchRequestEventArgs(string columnName, string searchText)
        {
            ColumnName = columnName;
            SearchText = searchText ?? "";
        }
    }

    #endregion
}