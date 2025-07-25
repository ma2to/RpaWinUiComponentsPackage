// Services/Interfaces/IValidationService.cs - ✅ OPRAVENÝ - len IValidationService
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre validačné služby DataGrid - ✅ INTERNAL
    /// </summary>
    internal interface IValidationService
    {
        /// <summary>
        /// Inicializuje validačnú službu s konfiguráciou
        /// </summary>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Validuje hodnotu bunky pre konkrétny stĺpec
        /// </summary>
        Task<List<string>> ValidateCellAsync(string columnName, object? value);

        /// <summary>
        /// Validuje celý riadok dát
        /// </summary>
        Task<List<string>> ValidateRowAsync(Dictionary<string, object?> rowData);

        /// <summary>
        /// Validuje všetky riadky v gridu
        /// </summary>
        Task<bool> ValidateAllRowsAsync();

        /// <summary>
        /// Pridá nové validačné pravidlo
        /// </summary>
        Task AddValidationRuleAsync(ValidationRule rule);

        /// <summary>
        /// Odstráni validačné pravidlo pre stĺpec
        /// </summary>
        Task RemoveValidationRuleAsync(string columnName, ValidationType type);

        /// <summary>
        /// Získa validačné pravidlá pre stĺpec
        /// </summary>
        List<ValidationRule> GetValidationRules(string columnName);

        /// <summary>
        /// Vyčisti všetky validačné chyby
        /// </summary>
        Task ClearAllValidationErrorsAsync();

        /// <summary>
        /// Vlastnosť - všetky validačné chyby
        /// </summary>
        IReadOnlyDictionary<string, List<string>> ValidationErrors { get; }

        /// <summary>
        /// Kontroluje či má stĺpec validačné chyby
        /// </summary>
        bool HasValidationErrors(string columnName);

        /// <summary>
        /// Získa validačné chyby pre stĺpec
        /// </summary>
        List<string> GetValidationErrors(string columnName);

        /// <summary>
        /// Kontroluje či sú nejaké validačné chyby
        /// </summary>
        bool HasAnyValidationErrors { get; }

        /// <summary>
        /// Celkový počet validačných chýb
        /// </summary>
        int TotalValidationErrorCount { get; }
    }
}