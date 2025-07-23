// Models/GridConfiguration.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Kompletná konfigurácia pre AdvancedDataGrid komponent.
    /// Obsahuje všetky nastavenia potrebné pre inicializáciu a prevádzku.
    /// INTERNAL - nie je súčasťou verejného API.
    /// </summary>
    internal class GridConfiguration : IDisposable
    {
        #region Private fields

        private bool _disposed = false;
        private readonly List<ColumnDefinition> _columns;
        private readonly List<ValidationRule> _validationRules;

        #endregion

        #region Konštruktory

        /// <summary>
        /// Vytvorí novú konfiguráciu gridu.
        /// </summary>
        public GridConfiguration(
            IEnumerable<ColumnDefinition> columns,
            IEnumerable<ValidationRule>? validationRules = null,
            ThrottlingConfig? throttlingConfig = null,
            int emptyRowsCount = 10)
        {
            _columns = new List<ColumnDefinition>(columns ?? throw new ArgumentNullException(nameof(columns)));
            _validationRules = new List<ValidationRule>(validationRules ?? Enumerable.Empty<ValidationRule>());

            ThrottlingConfig = throttlingConfig ?? ThrottlingConfig.Default;
            EmptyRowsCount = Math.Max(0, emptyRowsCount);

            InitializeAndValidate();
        }

        #endregion

        #region Základné properties

        /// <summary>
        /// Definície všetkých stĺpcov (vrátane špeciálnych).
        /// </summary>
        public IReadOnlyList<ColumnDefinition> Columns => _columns.AsReadOnly();

        /// <summary>
        /// Iba dátové stĺpce (bez špeciálnych DeleteRows a ValidAlerts).
        /// </summary>
        public IEnumerable<ColumnDefinition> DataColumns => _columns.Where(c => c.IsDataColumn);

        /// <summary>
        /// Všetky validačné pravidlá.
        /// </summary>
        public IReadOnlyList<ValidationRule> ValidationRules => _validationRules.AsReadOnly();

        /// <summary>
        /// Konfigurácia throttling-u a optimalizácií.
        /// </summary>
        public ThrottlingConfig ThrottlingConfig { get; }

        /// <summary>
        /// Počet prázdnych riadkov vytvorených pri inicializácii.
        /// </summary>
        public int EmptyRowsCount { get; }

        #endregion

        #region Špeciálne stĺpce

        /// <summary>
        /// DeleteRows stĺpec (ak existuje).
        /// </summary>
        public ColumnDefinition? DeleteColumn => _columns.FirstOrDefault(c => c.IsDeleteColumn);

        /// <summary>
        /// ValidAlerts stĺpec (vždy existuje).
        /// </summary>
        public ColumnDefinition ValidAlerts => _columns.First(c => c.IsValidationColumn);

        /// <summary>
        /// Určuje či má grid DeleteRows stĺpec.
        /// </summary>
        public bool HasDeleteColumn => DeleteColumn != null;

        /// <summary>
        /// Index DeleteRows stĺpca (ak existuje).
        /// </summary>
        public int? DeleteColumnIndex => DeleteColumn != null ? _columns.IndexOf(DeleteColumn) : null;

        /// <summary>
        /// Index ValidAlerts stĺpca.
        /// </summary>
        public int ValidAlertsColumnIndex => _columns.IndexOf(ValidAlerts);

        #endregion

        #region Validačné pravidlá - pomocné metódy

        /// <summary>
        /// Vráti validačné pravidlá pre určený stĺpec.
        /// </summary>
        public IEnumerable<ValidationRule> GetValidationRulesForColumn(string columnName)
        {
            return _validationRules.Where(r => r.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vráti validačné pravidlá pre určený index stĺpca.
        /// </summary>
        public IEnumerable<ValidationRule> GetValidationRulesForColumnIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count)
                return Enumerable.Empty<ValidationRule>();

            var columnName = _columns[columnIndex].Name;
            return GetValidationRulesForColumn(columnName);
        }

        /// <summary>
        /// Určuje či má stĺpec nejaké validačné pravidlá.
        /// </summary>
        public bool HasValidationRules(string columnName)
        {
            return _validationRules.Any(r => r.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Operácie so stĺpcami

        /// <summary>
        /// Vráti definíciu stĺpca podľa názvu.
        /// </summary>
        public ColumnDefinition? GetColumnByName(string columnName)
        {
            return _columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vráti definíciu stĺpca podľa indexu.
        /// </summary>
        public ColumnDefinition? GetColumnByIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count)
                return null;

            return _columns[columnIndex];
        }

        /// <summary>
        /// Vráti index stĺpca podľa názvu.
        /// </summary>
        public int GetColumnIndex(string columnName)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i].Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Celkový počet stĺpcov (vrátane špeciálnych).
        /// </summary>
        public int TotalColumnCount => _columns.Count;

        /// <summary>
        /// Počet dátových stĺpcov (bez špeciálnych).
        /// </summary>
        public int DataColumnCount => DataColumns.Count();

        #endregion

        #region Inicializácia a validácia

        /// <summary>
        /// Inicializuje konfiguráciu a pridá špeciálne stĺpce.
        /// </summary>
        private void InitializeAndValidate()
        {
            // Validovať vstupné stĺpce
            ValidateInputColumns();

            // Pridať ValidAlerts stĺpec ak neexistuje
            EnsureValidAlertsColumn();

            // Usporiadať stĺpce správne (DeleteRows pred ValidAlerts)
            ArrangeSpecialColumns();

            // Validovať throttling config
            ThrottlingConfig.ValidateAndFix();

            // Validovať validačné pravidlá
            ValidateValidationRules();
        }

        /// <summary>
        /// Validuje vstupné stĺpce.
        /// </summary>
        private void ValidateInputColumns()
        {
            if (_columns.Count == 0)
                throw new ArgumentException("Musí byť definovaný aspoň jeden stĺpec");

            // Kontrola duplicitných názvov
            var duplicates = _columns.GroupBy(c => c.Name.ToLowerInvariant())
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key);

            if (duplicates.Any())
                throw new ArgumentException($"Duplicitné názvy stĺpcov: {string.Join(", ", duplicates)}");

            // Validácia každého stĺpca
            foreach (var column in _columns)
            {
                if (!column.IsValid())
                {
                    var errors = column.GetValidationErrors();
                    throw new ArgumentException($"Nevalidný stĺpec '{column.Name}': {string.Join(", ", errors)}");
                }
            }
        }

        /// <summary>
        /// Zabezpečí existenciu ValidAlerts stĺpca.
        /// </summary>
        private void EnsureValidAlertsColumn()
        {
            if (!_columns.Any(c => c.IsValidationColumn))
            {
                _columns.Add(ColumnDefinition.CreateValidationColumn());
            }
        }

        /// <summary>
        /// Usporiadá špeciálne stĺpce na správne pozície.
        /// </summary>
        private void ArrangeSpecialColumns()
        {
            var dataColumns = _columns.Where(c => c.IsDataColumn).ToList();
            var deleteColumn = _columns.FirstOrDefault(c => c.IsDeleteColumn);
            var validAlertsColumn = _columns.First(c => c.IsValidationColumn);

            _columns.Clear();

            // Pridať dátové stĺpce
            _columns.AddRange(dataColumns);

            // Pridať DeleteRows stĺpec (ak existuje)
            if (deleteColumn != null)
            {
                _columns.Add(deleteColumn);
            }

            // Pridať ValidAlerts stĺpec (vždy posledný)
            _columns.Add(validAlertsColumn);
        }

        /// <summary>
        /// Validuje validačné pravidlá.
        /// </summary>
        private void ValidateValidationRules()
        {
            foreach (var rule in _validationRules)
            {
                // Skontrolovať či existuje stĺpec pre toto pravidlo
                if (!_columns.Any(c => c.Name.Equals(rule.ColumnName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Validačné pravidlo pre neexistujúci stĺpec: '{rule.ColumnName}'");
                }
            }
        }

        #endregion

        #region Utility metódy

        /// <summary>
        /// Vráti zoznam varovaní o konfigurácii.
        /// </summary>
        public List<string> GetConfigurationWarnings()
        {
            var warnings = new List<string>();

            // Throttling warnings
            warnings.AddRange(ThrottlingConfig.GetConfigurationWarnings());

            // Stĺpce warnings
            if (DataColumnCount == 0)
                warnings.Add("Žiadne dátové stĺpce definované");

            if (DataColumnCount > 50)
                warnings.Add($"Veľký počet stĺpcov ({DataColumnCount}) môže ovplyvniť výkon");

            // Validácie warnings
            var columnsWithoutValidation = DataColumns.Where(c => !HasValidationRules(c.Name)).ToList();
            if (columnsWithoutValidation.Count > 0)
                warnings.Add($"Stĺpce bez validácie: {string.Join(", ", columnsWithoutValidation.Select(c => c.Name))}");

            return warnings;
        }

        /// <summary>
        /// Vráti prehľad konfigurácie.
        /// </summary>
        public string GetConfigurationSummary()
        {
            return $"Columns: {DataColumnCount} data + {(HasDeleteColumn ? 1 : 0)} delete + 1 validation = {TotalColumnCount} total\n" +
                   $"Validation Rules: {_validationRules.Count}\n" +
                   $"Empty Rows: {EmptyRowsCount}\n" +
                   $"Throttling: {ThrottlingConfig}";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _columns.Clear();
                _validationRules.Clear();
                _disposed = true;
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"GridConfiguration: {TotalColumnCount} columns, {_validationRules.Count} validation rules";
        }

        #endregion
    }
}