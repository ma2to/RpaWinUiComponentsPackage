// Services/LoggingServiceAdapter.cs
using Microsoft.Extensions.Logging;
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Adapter pre Microsoft.Extensions.Logging.ILogger implementujúci naše ILoggingService.
    /// Poskytuje bridge medzi našou abstrakciou a štandardným .NET logging systémom.
    /// </summary>
    internal class LoggingServiceAdapter : ILoggingService
    {
        #region Private fields

        private readonly ILogger? _logger;
        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Vytvorí nový adapter pre ILogger.
        /// </summary>
        /// <param name="logger">Microsoft.Extensions.Logging.ILogger inštancia (môže byť null)</param>
        public LoggingServiceAdapter(ILogger? logger)
        {
            _logger = logger;
            MinimumLogLevel = LogLevel.Information;
        }

        #endregion

        #region ILoggingService - Základné logovanie

        public void LogDebug(string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug(message, args);
            }
        }

        public void LogInformation(string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation(message, args);
            }
        }

        public void LogWarning(string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Warning) == true)
            {
                _logger.LogWarning(message, args);
            }
        }

        public void LogError(string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Error) == true)
            {
                _logger.LogError(message, args);
            }
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Error) == true)
            {
                _logger.LogError(exception, message, args);
            }
        }

        public void LogCritical(string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Critical) == true)
            {
                _logger.LogCritical(message, args);
            }
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            if (_logger?.IsEnabled(LogLevel.Critical) == true)
            {
                _logger.LogCritical(exception, message, args);
            }
        }

        #endregion

        #region ILoggingService - Štruktúrované logovanie

        public void Log(LogLevel logLevel, EventId eventId, string message, params object[] args)
        {
            if (_logger?.IsEnabled(logLevel) == true)
            {
                _logger.Log(logLevel, eventId, message, args);
            }
        }

        public void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args)
        {
            if (_logger?.IsEnabled(logLevel) == true)
            {
                _logger.Log(logLevel, eventId, exception, message, args);
            }
        }

        #endregion

        #region ILoggingService - DataGrid špecifické logovanie

        public void LogDataOperation(string operation, string details, bool success, TimeSpan? duration = null)
        {
            if (!IsEnabled(LogLevel.Information)) return;

            var durationText = duration.HasValue ? $" ({duration.Value.TotalMilliseconds:F2}ms)" : "";
            var status = success ? "SUCCESS" : "FAILED";

            Log(LogLevel.Information, DataGridEventIds.DataLoad,
                "DataGrid operation {Operation}: {Status} - {Details}{Duration}",
                operation, status, details, durationText);
        }

        public void LogValidation(string cellPosition, bool validationResult, TimeSpan validationTime)
        {
            if (!IsEnabled(LogLevel.Debug)) return;

            var status = validationResult ? "VALID" : "INVALID";

            Log(LogLevel.Debug, DataGridEventIds.ValidationComplete,
                "Cell {CellPosition} validation: {Status} ({Duration:F2}ms)",
                cellPosition, status, validationTime.TotalMilliseconds);
        }

        public void LogPerformance(string operationType, PerformanceMetrics metrics)
        {
            if (!IsEnabled(LogLevel.Information)) return;

            if (metrics.Duration.TotalMilliseconds > 1000) // Slow operation warning
            {
                Log(LogLevel.Warning, DataGridEventIds.PerformanceWarning,
                    "Slow {OperationType} operation: {Metrics}",
                    operationType, metrics.ToString());
            }
            else
            {
                Log(LogLevel.Information, DataGridEventIds.DataLoad,
                    "{OperationType} performance: {Metrics}",
                    operationType, metrics.ToString());
            }
        }

        public void LogUIOperation(string uiOperation, string context, bool success)
        {
            if (!IsEnabled(LogLevel.Debug)) return;

            var eventId = success ? DataGridEventIds.UIEvent : DataGridEventIds.UIError;
            var logLevel = success ? LogLevel.Debug : LogLevel.Warning;
            var status = success ? "SUCCESS" : "FAILED";

            Log(logLevel, eventId,
                "UI operation {UIOperation} in {Context}: {Status}",
                uiOperation, context, status);
        }

        public void LogMemoryUsage(MemoryUsageInfo memoryInfo)
        {
            if (!IsEnabled(LogLevel.Information)) return;

            if (memoryInfo.EstimatedMemoryUsageBytes > 50 * 1024 * 1024) // 50MB warning
            {
                Log(LogLevel.Warning, DataGridEventIds.MemoryWarning,
                    "High memory usage detected: {MemoryInfo}",
                    memoryInfo.ToString());
            }
            else
            {
                Log(LogLevel.Information, DataGridEventIds.DataLoad,
                    "Memory usage: {MemoryInfo}",
                    memoryInfo.ToString());
            }
        }

        #endregion

        #region ILoggingService - Konfigurácia

        public LogLevel MinimumLogLevel { get; set; }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel < MinimumLogLevel) return false;
            return _logger?.IsEnabled(logLevel) == true;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger?.BeginScope(state) ?? NullScope.Instance;
        }

        #endregion

        #region ILoggingService - Factory metódy

        public ILoggingService CreateChildLogger(string categoryName)
        {
            // V reálnej implementácii by sme vytvorili nový ILogger s category name
            // Zatiaľ vraciame self (jednoduchšia implementácia)
            return this;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            // ILogger sa normálne nepoužíva dispose (je singleton)
            // Ale pre istotu si to označíme
            _disposed = true;
        }

        #endregion

        #region Helper classes

        /// <summary>
        /// Null implementation IDisposable pre prípady keď nemáme skutočný scope.
        /// </summary>
        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            private NullScope() { }
            public void Dispose() { }
        }

        #endregion
    }
}