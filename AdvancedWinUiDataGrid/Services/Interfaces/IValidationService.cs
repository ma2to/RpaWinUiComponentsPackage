// Services/Interfaces/IValidationService.cs
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre validačné služby DataGrid
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Inicializuje validačnú službu s konfiguráciou
        /// </summary>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Validuje hodnotu bunky podľa pravidiel
        /// </summary>
        Task<List<string>> ValidateCellAsync(string columnName, object? value);

        /// <summary>
        /// Validuje celý riadok
        /// </summary>
        Task<List<string>> ValidateRowAsync(Dictionary<string, object?> rowData);

        /// <summary>
        /// Validuje všetky riadky
        /// </summary>
        Task<bool> ValidateAllRowsAsync();

        /// <summary>
        /// Pridá nové validačné pravidlo
        /// </summary>
        Task AddValidationRuleAsync(ValidationRule rule);

        /// <summary>
        /// Odstráni validačné pravidlo
        /// </summary>
        Task RemoveValidationRuleAsync(string columnName, ValidationType type);

        /// <summary>
        /// Získa všetky validačné pravidlá pre stĺpec
        /// </summary>
        List<ValidationRule> GetValidationRules(string columnName);

        /// <summary>
        /// Vyčisti všetky validačné chyby
        /// </summary>
        Task ClearAllValidationErrorsAsync();
    }
}