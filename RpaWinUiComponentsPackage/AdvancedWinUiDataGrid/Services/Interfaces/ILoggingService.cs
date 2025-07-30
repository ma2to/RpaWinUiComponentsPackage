// Services/Interfaces/ILoggingService.cs - ✅ OPRAVENÝ accessibility
using Microsoft.Extensions.Logging;
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre logovanie (abstrakcia nad Microsoft.Extensions.Logging) - ✅ INTERNAL
    /// Poskytuje jednotné API pre logovanie s možnosťou pripojenia rôznych loggerov
    /// </summary>
    internal interface ILoggingService : IDisposable  // ✅ CHANGED: public -> internal
    {
        #region Základné logovanie

        /// <summary>
        /// Log Debug správu
        /// </summary>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Log Information správu
        /// </summary>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// Log Warning správu
        /// </summary>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Log Error správu
        /// </summary>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Log Error správu s exception
        /// </summary>
        void LogError(Exception exception, string message, params object[] args);

        /// <summary>
        /// Log Critical správu
        /// </summary>
        void LogCritical(string message, params object[] args);

        /// <summary>
        /// Log Critical správu s exception
        /// </summary>
        void LogCritical(Exception exception, string message, params object[] args);

        #endregion

        #region Štruktúrované logovanie

        /// <summary>
        /// Log správu s špecifickým log levelom a event ID
        /// </summary>
        void Log(LogLevel logLevel, EventId eventId, string message, params object[] args);

        /// <summary>
        /// Log správu s exception, log levelom a event ID
        /// </summary>
        void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args);

        #endregion

        #region DataGrid špecifické logovanie

        /// <summary>
        /// Loguje dátové operácie (load, save, export)
        /// </summary>
        void LogDataOperation(string operation, string details, bool success, TimeSpan? duration = null);

        /// <summary>
        /// Loguje validačné operácie
        /// </summary>
        void LogValidation(string cellPosition, bool validationResult, TimeSpan validationTime);

        /// <summary>
        /// Loguje performance metriky
        /// </summary>
        void LogPerformance(string operationType, PerformanceMetrics metrics);

        /// <summary>
        /// Loguje UI operácie (click, selection, navigation)
        /// </summary>
        void LogUIOperation(string uiOperation, string context, bool success);

        /// <summary>
        /// Loguje memory usage informácie
        /// </summary>
        void LogMemoryUsage(MemoryUsageInfo memoryInfo);

        #endregion

        #region Konfigurácia

        /// <summary>
        /// Minimálny log level
        /// </summary>
        LogLevel MinimumLogLevel { get; set; }

        /// <summary>
        /// Kontroluje či je povolený daný level logovania
        /// </summary>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Začne nový logging scope
        /// </summary>
        IDisposable BeginScope<TState>(TState state) where TState : notnull;

        #endregion

        #region Factory metódy

        /// <summary>
        /// Vytvorí child logger s určitou kategóriou
        /// </summary>
        ILoggingService CreateChildLogger(string categoryName);

        #endregion
    }

    #region Helper classes pre DataGrid logovanie - ✅ INTERNAL

    /// <summary>
    /// Performance metriky pre logovanie - ✅ INTERNAL
    /// </summary>
    internal class PerformanceMetrics  // ✅ CHANGED: public -> internal
    {
        public TimeSpan Duration { get; set; }
        public int ItemsProcessed { get; set; }
        public long MemoryUsedBytes { get; set; }
        public string OperationName { get; set; } = string.Empty;

        public override string ToString()
        {
            var itemsPerSecond = Duration.TotalSeconds > 0 ? ItemsProcessed / Duration.TotalSeconds : 0;
            return $"{OperationName}: {Duration.TotalMilliseconds:F2}ms, {ItemsProcessed} items ({itemsPerSecond:F1} items/s), {MemoryUsedBytes / 1024:F0} KB";
        }
    }

    /// <summary>
    /// Memory usage informácie pre logovanie - ✅ INTERNAL
    /// </summary>
    internal class MemoryUsageInfo  // ✅ CHANGED: public -> internal
    {
        public long EstimatedMemoryUsageBytes { get; set; }
        public int ObjectCount { get; set; }
        public int GC0Collections { get; set; }
        public int GC1Collections { get; set; }
        public int GC2Collections { get; set; }
        public string ComponentName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{ComponentName}: {EstimatedMemoryUsageBytes / 1024 / 1024:F1} MB, {ObjectCount} objects, GC: {GC0Collections}/{GC1Collections}/{GC2Collections}";
        }
    }

    /// <summary>
    /// Event IDs pre DataGrid logovanie - ✅ INTERNAL
    /// </summary>
    internal static class DataGridEventIds  // ✅ CHANGED: public -> internal
    {
        public static readonly EventId Initialization = new(1000, "DataGrid.Initialization");
        public static readonly EventId DataLoad = new(1001, "DataGrid.DataLoad");
        public static readonly EventId ValidationStart = new(1002, "DataGrid.ValidationStart");
        public static readonly EventId ValidationComplete = new(1003, "DataGrid.ValidationComplete");
        public static readonly EventId Export = new(1004, "DataGrid.Export");
        public static readonly EventId UIEvent = new(1005, "DataGrid.UIEvent");
        public static readonly EventId UIError = new(1006, "DataGrid.UIError");
        public static readonly EventId PerformanceWarning = new(1007, "DataGrid.PerformanceWarning");
        public static readonly EventId MemoryWarning = new(1008, "DataGrid.MemoryWarning");
        public static readonly EventId CustomValidation = new(1009, "DataGrid.CustomValidation");
        public static readonly EventId DeleteOperation = new(1010, "DataGrid.DeleteOperation");
    }

    #endregion
}