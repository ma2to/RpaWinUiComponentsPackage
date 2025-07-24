// Controls/SpecialColumns/DeleteRowColumn.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Špeciálny stĺpec pre mazanie riadkov
    /// </summary>
    public sealed partial class DeleteRowColumn : UserControl
    {
        #region Constructor

        public DeleteRowColumn()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Index riadku ktorý tento stĺpec reprezentuje
        /// </summary>
        public int RowIndex { get; set; } = -1;

        /// <summary>
        /// Dáta riadku ktoré majú byť zmazané
        /// </summary>
        public object? RowData { get; set; }

        /// <summary>
        /// Či je button povolený
        /// </summary>
        public bool IsDeleteEnabled
        {
            get => DeleteButton?.IsEnabled ?? false;
            set
            {
                if (DeleteButton != null)
                    DeleteButton.IsEnabled = value;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event ktorý sa spustí keď sa klikne na delete button
        /// </summary>
        public event EventHandler<DeleteRowEventArgs>? DeleteRowRequested;

        #endregion

        #region Event Handlers

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Spusti event pre parent komponent
                DeleteRowRequested?.Invoke(this, new DeleteRowEventArgs(RowIndex, RowData));
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Chyba pri mazaní riadku: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Nastaví row data pre tento delete button
        /// </summary>
        /// <param name="rowIndex">Index riadku</param>
        /// <param name="rowData">Dáta riadku</param>
        public void SetRowData(int rowIndex, object? rowData)
        {
            RowIndex = rowIndex;
            RowData = rowData;

            // Povoliť/zakázať button na základe toho či riadok má dáta
            IsDeleteEnabled = rowData != null && !IsRowEmpty(rowData);
        }

        /// <summary>
        /// Kontroluje či je riadok prázdny
        /// </summary>
        private bool IsRowEmpty(object? rowData)
        {
            if (rowData == null) return true;

            // Ak je to Dictionary, skontroluj či sú všetky hodnoty prázdne
            if (rowData is System.Collections.Generic.Dictionary<string, object?> dict)
            {
                foreach (var kvp in dict)
                {
                    // Ignoruj špeciálne stĺpce
                    if (kvp.Key == "DeleteRows" || kvp.Key == "ValidAlerts")
                        continue;

                    // Ak má nejakú hodnotu, riadok nie je prázdny
                    if (kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                        return false;
                }
                return true;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// Event args pre DeleteRowRequested event
    /// </summary>
    public class DeleteRowEventArgs : EventArgs
    {
        /// <summary>
        /// Index riadku ktorý má byť zmazaný
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// Dáta riadku ktoré majú byť zmazané
        /// </summary>
        public object? RowData { get; }

        public DeleteRowEventArgs(int rowIndex, object? rowData)
        {
            RowIndex = rowIndex;
            RowData = rowData;
        }
    }
}