// LoggerComponent/Core/LoggerComponent.cs - ✅ AKTUALIZOVANÝ - priame metódy pre log levely
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions; // ✅ POUZE Abstractions
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger
{
    /// <summary>
    /// LoggerComponent - wrapper pre externý ILogger + file management - ✅ PUBLIC API
    /// Poskytuje priame metódy pre Info, Debug, Warning, Error
    /// ✅ AKTUALIZOVANÉ: Priame metódy namiesto LogAsync(message, level)
    /// </summary>
    public class LoggerComponent : IDisposable
    {
        private readonly ILogger _externalLogger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly LogFileManager? _fileManager;
        private bool _disposed = false;

        // File management properties
        private readonly string _folderPath;
        private readonly string _fileName;
        private readonly int? _maxFileSizeMB;
        private readonly bool _enableFileLogging;

        #region ✅ Constructor

        /// <summary>
        /// Vytvorí LoggerComponent s externým ILogger + file management
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z aplikácie (POVINNÝ)</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB (NULL = bez rotácie)</param>
        public LoggerComponent(ILogger externalLogger, string folderPath, string fileName, int? maxFileSizeMB = null)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger), "External logger is required");
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _maxFileSizeMB = maxFileSizeMB.HasValue ? Math.Max(0, maxFileSizeMB.Value) : null;
            _enableFileLogging = true;

            // Vytvor folder ak neexistuje
            EnsureDirectoryExists();

            // ✅ OPRAVENÉ: Inicializuj file manager priamo v konštruktore (readonly field)
            LoggerConfiguration config;
            if (_maxFileSizeMB.HasValue && _maxFileSizeMB.Value > 0)
            {
                // S rotáciou
                config = new LoggerConfiguration
                {
                    FolderPath = _folderPath,
                    BaseFileName = _fileName,
                    MaxFileSizeMB = _maxFileSizeMB.Value,
                    EnableRotation = true
                };
            }
            else
            {
                // Bez rotácie
                config = new LoggerConfiguration
                {
                    FolderPath = _folderPath,
                    BaseFileName = _fileName,
                    MaxFileSizeMB = 0,
                    EnableRotation = false
                };
            }

            _fileManager = new LogFileManager(config);

            // Log inicializáciu
            _externalLogger.LogInformation("LoggerComponent initialized - File: {FilePath}, Max size: {MaxSize}, Rotation: {Rotation}",
                CurrentLogFile, _maxFileSizeMB?.ToString() ?? "UNLIMITED", IsRotationEnabled);
        }

        #endregion

        #region ✅ NOVÉ: Priame metódy pre log levely

        /// <summary>
        /// Zaznamená INFO správu
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task Info(string message)
        {
            await LogInternalAsync(message, "INFO", LogLevel.Information);
        }

        /// <summary>
        /// Zaznamená DEBUG správu
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task Debug(string message)
        {
            await LogInternalAsync(message, "DEBUG", LogLevel.Debug);
        }

        /// <summary>
        /// Zaznamená WARNING správu
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task Warning(string message)
        {
            await LogInternalAsync(message, "WARNING", LogLevel.Warning);
        }

        /// <summary>
        /// Zaznamená ERROR správu
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task Error(string message)
        {
            await LogInternalAsync(message, "ERROR", LogLevel.Error);
        }

        /// <summary>
        /// ✅ NOVÉ: Zaznamená ERROR správu s exception
        /// </summary>
        /// <param name="exception">Exception na zaznamenanie</param>
        /// <param name="message">Dodatočná správa (optional)</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task Error(Exception exception, string? message = null)
        {
            var fullMessage = string.IsNullOrWhiteSpace(message)
                ? $"Exception: {exception.Message}"
                : $"{message} | Exception: {exception.Message}";

            await LogInternalAsync(fullMessage, "ERROR", LogLevel.Error);
        }

        /// <summary>
        /// ✅ ZACHOVANÁ: Pôvodná LogAsync metóda pre backward compatibility
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <param name="logLevel">Úroveň logovania (INFO, ERROR, DEBUG, WARNING)</param>
        /// <returns>Task pre async operáciu</returns>
        public async Task LogAsync(string message, string logLevel = "INFO")
        {
            var level = ParseLogLevel(logLevel);
            await LogInternalAsync(message, logLevel.ToUpperInvariant(), level);
        }

        #endregion

        #region ✅ PRIVATE: Hlavná logging implementácia

        /// <summary>
        /// Interná metóda pre logovanie s automatickým timestamp a formátovaním
        /// </summary>
        private async Task LogInternalAsync(string message, string levelText, LogLevel logLevel)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LoggerComponent));

            if (string.IsNullOrWhiteSpace(message))
                return;

            await _semaphore.WaitAsync();
            try
            {
                // ✅ Automatický timestamp a formát: [timestamp] [LEVEL] message
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var formattedMessage = $"[{timestamp}] [{levelText}] {message}";

                // 1. Log do súboru (ak je povolené)
                if (_enableFileLogging && _fileManager != null)
                {
                    await _fileManager.WriteLogEntryAsync(formattedMessage);
                }

                // 2. Log do externého logger systému
                await Task.Run(() => _externalLogger.Log(logLevel, message));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        #region ✅ Public Properties

        /// <summary>
        /// Cesta k aktuálnemu log súboru
        /// </summary>
        public string CurrentLogFile => _fileManager?.CurrentLogFile ?? Path.Combine(_folderPath, _fileName);

        /// <summary>
        /// Veľkosť aktuálneho log súboru v MB
        /// </summary>
        public double CurrentFileSizeMB => _fileManager?.GetCurrentFileSizeMB() ?? 0;

        /// <summary>
        /// Počet rotačných súborov
        /// </summary>
        public int RotationFileCount => _fileManager?.RotationFileCount ?? 0;

        /// <summary>
        /// Či je rotácia súborov povolená
        /// </summary>
        public bool IsRotationEnabled => _maxFileSizeMB.HasValue && _maxFileSizeMB.Value > 0;

        /// <summary>
        /// Externý logger (pre AdvancedDataGrid použitie)
        /// </summary>
        public ILogger ExternalLogger => _externalLogger;

        /// <summary>
        /// Typ externého loggera
        /// </summary>
        public string ExternalLoggerType => _externalLogger.GetType().Name;

        /// <summary>
        /// Maximálna veľkosť súboru (nullable)
        /// </summary>
        public int? MaxFileSizeMB => _maxFileSizeMB;

        #endregion

        #region ✅ Private Helper Methods

        /// <summary>
        /// Zabezpečí existenciu adresára
        /// </summary>
        private void EnsureDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_folderPath))
                {
                    Directory.CreateDirectory(_folderPath);
                }
            }
            catch (Exception ex)
            {
                _externalLogger?.LogError(ex, "Failed to create log directory: {LogDirectory}", _folderPath);
            }
        }



        /// <summary>
        /// Parsuje string log level na LogLevel enum
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

        #region ✅ Factory Methods

        /// <summary>
        /// Vytvorí LoggerComponent z externého ILoggerFactory + file settings
        /// </summary>
        public static LoggerComponent FromLoggerFactory(
            ILoggerFactory loggerFactory,
            string folderPath,
            string fileName,
            int? maxFileSizeMB = null,
            string categoryName = "RpaDataGrid")
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger(categoryName);
            return new LoggerComponent(logger, folderPath, fileName, maxFileSizeMB);
        }

        /// <summary>
        /// Vytvorí LoggerComponent bez rotácie (jednoduchý súbor)
        /// </summary>
        public static LoggerComponent WithoutRotation(ILogger externalLogger, string folderPath, string fileName)
        {
            return new LoggerComponent(externalLogger, folderPath, fileName, null);
        }

        /// <summary>
        /// Vytvorí LoggerComponent s rotáciou
        /// </summary>
        public static LoggerComponent WithRotation(ILogger externalLogger, string folderPath, string fileName, int maxSizeMB)
        {
            return new LoggerComponent(externalLogger, folderPath, fileName, maxSizeMB);
        }

        #endregion

        #region ✅ Diagnostics Methods

        /// <summary>
        /// Získa diagnostické informácie o LoggerComponent
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var rotationInfo = IsRotationEnabled ? $"{_maxFileSizeMB}MB rotation" : "NO rotation";
            var filesInfo = IsRotationEnabled ? $"{RotationFileCount} files" : "single file";

            return $"LoggerComponent: External Logger = {ExternalLoggerType}, " +
                   $"File = {CurrentLogFile}, Size = {CurrentFileSizeMB:F2}MB, " +
                   $"Mode = {rotationInfo}, Files = {filesInfo}";
        }

        /// <summary>
        /// Testuje funkčnosť loggera
        /// </summary>
        public async Task<bool> TestLoggingAsync()
        {
            try
            {
                await Debug($"LoggerComponent test message at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Rotation: {IsRotationEnabled}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Získa informácie o rotácii
        /// </summary>
        public string GetRotationInfo()
        {
            if (!IsRotationEnabled)
                return "Rotation: DISABLED - Single log file mode";

            return $"Rotation: ENABLED - Max {_maxFileSizeMB}MB per file, " +
                   $"Current: {CurrentFileSizeMB:F2}MB, " +
                   $"Files created: {RotationFileCount}";
        }

        #endregion

        #region ✅ IDisposable Implementation

        public void Dispose()
        {
            if (_disposed) return;

            _externalLogger.LogInformation("LoggerComponent disposing - Final stats: {Stats}", GetDiagnosticInfo());

            _semaphore?.Dispose();
            _fileManager?.Dispose();

            _disposed = true;
        }

        #endregion
    }
}