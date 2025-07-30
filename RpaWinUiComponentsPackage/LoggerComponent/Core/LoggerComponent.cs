// LoggerComponent/Core/LoggerComponent.cs - ✅ OPRAVENÝ - iba ILogger, bez súborového loggingu
using Microsoft.Extensions.Logging.Abstractions; // ✅ POUZE Abstractions
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger
{
    /// <summary>
    /// LoggerComponent ako bridge pre externý logging systém - ✅ PUBLIC API
    /// Balík je nezávislý na konkrétnom logging systéme - VYŽADUJE externý ILogger
    /// </summary>
    public class LoggerComponent : IDisposable
    {
        private readonly ILogger _externalLogger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;

        #region ✅ Constructors - POVINNE s ILogger

        /// <summary>
        /// Vytvorí LoggerComponent s externým ILogger systémom - JEDINÝ podporovaný spôsob
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z demo aplikácie (povinný)</param>
        public LoggerComponent(ILogger externalLogger)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger),
                "LoggerComponent vyžaduje externý ILogger. Balík je nezávislý na konkrétnom logging systéme.");
        }

        #endregion

        #region ✅ Public Properties

        /// <summary>
        /// Má externý logger (vždy true)
        /// </summary>
        public bool HasExternalLogger => true;

        /// <summary>
        /// Typ externého loggera
        /// </summary>
        public string ExternalLoggerType => _externalLogger.GetType().Name;

        #endregion

        #region ✅ Main Logging Method

        /// <summary>
        /// Zaznamená správu do externého logger systému - ✅ JEDINÁ VEREJNÁ METÓDA
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <param name="logLevel">Úroveň logovania (INFO, ERROR, DEBUG...)</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task LogAsync(string message, string logLevel = "INFO")
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoggerComponent));

            if (string.IsNullOrWhiteSpace(message))
                return;

            await _semaphore.WaitAsync();
            try
            {
                await Task.Run(() => LogToExternalLogger(message, logLevel));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        #region ✅ Private Helper Methods

        /// <summary>
        /// Loguje do externého ILogger systému
        /// </summary>
        private void LogToExternalLogger(string message, string logLevel)
        {
            try
            {
                var level = ParseLogLevel(logLevel);
                _externalLogger.Log(level, message);
            }
            catch (Exception ex)
            {
                // Fallback ak externý logger zlyhal - použij System.Diagnostics.Debug
                System.Diagnostics.Debug.WriteLine($"External logger failed: {ex.Message}. Original message: {message}");
            }
        }

        /// <summary>
        /// Parsuje string log level na Microsoft.Extensions.Logging.Abstractions.LogLevel
        /// </summary>
        private static LogLevel ParseLogLevel(string logLevel)
        {
            return logLevel.ToUpperInvariant() switch
            {
                "TRACE" => LogLevel.Trace,
                "DEBUG" => LogLevel.Debug,
                "INFO" or "INFORMATION" => LogLevel.Information,
                "WARN" or "WARNING" => LogLevel.Warning,
                "ERROR" => LogLevel.Error,
                "CRITICAL" or "FATAL" => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }

        #endregion

        #region ✅ Factory Methods - všetky vyžadujú ILogger

        /// <summary>
        /// Vytvorí LoggerComponent z externého ILoggerFactory
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <param name="categoryName">Kategória pre logger (default: "RpaDataGrid")</param>
        /// <returns>LoggerComponent s externým logger</returns>
        public static LoggerComponent FromLoggerFactory(ILoggerFactory loggerFactory, string categoryName = "RpaDataGrid")
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger(categoryName);
            return new LoggerComponent(logger);
        }

        /// <summary>
        /// Vytvorí LoggerComponent z typed logger
        /// </summary>
        /// <typeparam name="T">Typ pre logger kategóriu</typeparam>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <returns>LoggerComponent s typed logger</returns>
        public static LoggerComponent FromLoggerFactory<T>(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger<T>();
            return new LoggerComponent(logger);
        }

        #endregion

        #region ✅ Diagnostics Methods

        /// <summary>
        /// Získa diagnostické informácie o LoggerComponent
        /// </summary>
        /// <returns>Diagnostické info ako string</returns>
        public string GetDiagnosticInfo()
        {
            return $"LoggerComponent: External Logger Type = {ExternalLoggerType}";
        }

        /// <summary>
        /// Testuje funkčnosť loggera
        /// </summary>
        /// <returns>True ak funguje</returns>
        public async Task<bool> TestLoggingAsync()
        {
            try
            {
                var testMessage = $"LoggerComponent test message at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
                await LogAsync(testMessage, "DEBUG");
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ✅ IDisposable Implementation

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _semaphore?.Dispose();
            // Poznámka: _externalLogger nie je owned by LoggerComponent, takže ho nedisposujeme

            _disposed = true;
        }

        #endregion
    }
}