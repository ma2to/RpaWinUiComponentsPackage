// Services/Interfaces/ILoggingService.cs
using Microsoft.Extensions.Logging;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Abstrakcia pre logovanie v DataGrid komponente.
    /// Podporuje Microsoft.Extensions.Logging štandard.
    /// </summary>
    internal interface ILoggingService : IDisposable
    {
        #region Základné logovanie

        /// <summary>
        /// Loguje debug správu.
        /// </summary>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Loguje informačnú správu.
        /// </summary>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// Loguje varovanie.
        /// </summary>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Loguje chybu.
        /// </summary>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Loguje chybu s exception.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogError(Exception exception, string message, params object[] args);

        /// <summary>
        /// Loguje kritickú chybu.
        /// </summary>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogCritical(string message, params object[] args);

        /// <summary>
        /// Loguje kritickú chybu s exception.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre pre formátovanie</param>
        void LogCritical(Exception exception, string message, params object[] args);

        #endregion

        #region Štruktúrované logovanie

        /// <summary>
        /// Loguje správu s určeným log level.
        /// </summary>
        /// <param name="logLevel">Úroveň logu</param>
        /// <param name="eventId">ID eventu</param>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre</param>
        void Log(LogLevel logLevel, EventId eventId, string message, params object[] args);

        /// <summary>
        /// Loguje správu s exception a určeným log level.
        /// </summary>
        /// <param name="logLevel">Úroveň logu</param>
        /// <param name="eventId">ID eventu</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Správa</param>
        /// <param name="args">Parametre</param>
        void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args);

        #endregion

        #region DataGrid špecifické logovanie

        /// <summary>
        /// Loguje operáciu s dátami.
        /// </summary>
        /// <param name="operation">Typ operácie</param>
        /// <param name="details">Detaily operácie</param>
        /// <param name="success">Či bola operácia úspešná</param>
        /// <param name="duration">Trvanie operácie</param>
        void LogDataOperation(string operation, string details, bool success, TimeSpan? duration = null);

        /// <summary>
        /// Loguje validačnú operáciu.
        /// </summary>
        /// <param name="cellPosition">Pozícia bunky</param>
        /// <param name="validationResult">Výsledok validácie</param>
        /// <param name="validationTime">Čas validácie</param>
        void LogValidation(string cellPosition, bool validationResult, TimeSpan validationTime);

        /// <summary>
        /// Loguje performance metriky.
        /// </summary>
        /// <param name="operationType">Typ operácie</param>
        /// <param name="metrics">Metriky</param>
        void LogPerformance(string operationType, PerformanceMetrics metrics);

        /// <summary>
        /// Loguje UI operáciu.
        /// </summary>
        /// <param name="uiOperation">Typ UI operácie</param>
        /// <param name="context">Kontext operácie</param>
        /// <param name="success">Úspešnosť</param>
        void LogUIOperation(string uiOperation, string context, bool success);

        /// <summary>
        /// Loguje memory usage.
        /// </summary>
        /// <param name="memoryInfo">Informácie o pamäti</param>
        void LogMemoryUsage(MemoryUsageInfo memoryInfo);

        #endregion

        #region Konfigurácia

        /// <summary>
        /// Aktuálna minimálna úroveň logovania.
        /// </summary>
        LogLevel MinimumLogLevel { get; set; }

        /// <summary>
        /// Určuje či je určená úroveň logovania povolená.
        /// </summary>
        /// <param name="logLevel">Úroveň logovania</param>
        /// <returns>True ak je povolená</returns>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Začne scope pre kontextové logovanie.
        /// </summary>
        /// <typeparam name="TState">Typ stavu</typeparam>
        /// <param name="state">Stav pre scope</param>
        /// <returns>IDisposable scope</returns>
        IDisposable BeginScope<TState>(TState state) where TState : notnull;

        #endregion

        #region Factory metódy

        /// <summary>
        /// Vytvorí child logger s dodatočným kontextom.
        /// </summary>
        /// <param name="categoryName">Názov kategórie</param>
        /// <returns>Child logger</returns>
        ILoggingService CreateChildLogger(string categoryName);

        #endregion
    }

    #region Helper classes

    /// <summary>
    /// Performance metriky pre logovanie.
    /// </summary>
    internal class PerformanceMetrics
    {
        public TimeSpan Duration { get; set; }
        public long MemoryUsedBytes { get; set; }
        public int ItemsProcessed { get; set; }
        public double ItemsPerSecond => Duration.TotalSeconds > 0 ? ItemsProcessed / Duration.TotalSeconds : 0;
        public string? AdditionalInfo { get; set; }

        public override string ToString()
        {
            return $"Duration: {Duration.TotalMilliseconds:F2}ms, Items: {ItemsProcessed}, Rate: {ItemsPerSecond:F1}/s, Memory: {MemoryUsedBytes / 1024:F1}KB";
        }
    }

    #endregion

    #region Event IDs pre štruktúrované logovanie

    /// <summary>
    /// Predefinované Event ID pre DataGrid operácie.
    /// </summary>
    internal static class DataGridEventIds
    {
        // Inicializácia
        public static readonly EventId Initialization = new(1001, "DataGrid.Initialization");
        public static readonly EventId Configuration = new(1002, "DataGrid.Configuration");

        // Dátové operácie
        public static readonly EventId DataLoad = new(2001, "DataGrid.DataLoad");
        public static readonly EventId DataSave = new(2002, "DataGrid.DataSave");
        public static readonly EventId DataClear = new(2003, "DataGrid.DataClear");
        public static readonly EventId DataExport = new(2004, "DataGrid.DataExport");

        // Validácia
        public static readonly EventId ValidationStart = new(3001, "DataGrid.ValidationStart");
        public static readonly EventId ValidationComplete = new(3002, "DataGrid.ValidationComplete");
        public static readonly EventId ValidationError = new(3003, "DataGrid.ValidationError");

        // UI operácie
        public static readonly EventId UIRender = new(4001, "DataGrid.UIRender");
        public static readonly EventId UIEvent = new(4002, "DataGrid.UIEvent");
        public static readonly EventId UIError = new(4003, "DataGrid.UIError");

        // Copy/Paste operácie
        public static readonly EventId CopyOperation = new(5001, "DataGrid.CopyOperation");
        public static readonly EventId PasteOperation = new(5002, "DataGrid.PasteOperation");
        public static readonly EventId CutOperation = new(5003, "DataGrid.CutOperation");

        // Performance
        public static readonly EventId PerformanceWarning = new(6001, "DataGrid.PerformanceWarning");
        public static readonly EventId MemoryWarning = new(6002, "DataGrid.MemoryWarning");
        public static readonly EventId ThrottlingActivated = new(6003, "DataGrid.ThrottlingActivated");

        // Chyby
        public static readonly EventId UnexpectedError = new(9001, "DataGrid.UnexpectedError");
        public static readonly EventId ConfigurationError = new(9002, "DataGrid.ConfigurationError");
        public static readonly EventId DataError = new(9003, "DataGrid.DataError");
    }

    #endregion
}