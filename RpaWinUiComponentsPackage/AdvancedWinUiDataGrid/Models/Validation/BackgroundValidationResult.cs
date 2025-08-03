// Models/Validation/BackgroundValidationResult.cs - Result of background validation
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// Výsledok background validácie
    /// </summary>
    public class BackgroundValidationResult
    {
        /// <summary>
        /// Či je validácia úspešná
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Chybová správa ak validácia zlyhala
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Varovná správa (validation je úspešná ale má upozornenie)
        /// </summary>
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// Dodatočné informácie z validácie
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();

        /// <summary>
        /// Čas dokončenia validácie
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Trvanie validácie v milisekundách
        /// </summary>
        public double DurationMs { get; set; }

        /// <summary>
        /// Vytvorí úspešný výsledok
        /// </summary>
        public static BackgroundValidationResult Success(string? warningMessage = null)
        {
            return new BackgroundValidationResult
            {
                IsValid = true,
                WarningMessage = warningMessage ?? string.Empty
            };
        }

        /// <summary>
        /// Vytvorí neúspešný výsledok
        /// </summary>
        public static BackgroundValidationResult Error(string errorMessage)
        {
            return new BackgroundValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Vytvorí výsledok s upozornením
        /// </summary>
        public static BackgroundValidationResult Warning(string warningMessage)
        {
            return new BackgroundValidationResult
            {
                IsValid = true,
                WarningMessage = warningMessage
            };
        }

        /// <summary>
        /// Vytvorí výsledok s dodatočnými dátami
        /// </summary>
        public static BackgroundValidationResult WithData(bool isValid, string message, Dictionary<string, object> data)
        {
            var result = isValid ? Success() : Error(message);
            result.AdditionalData = data;
            return result;
        }
    }
}