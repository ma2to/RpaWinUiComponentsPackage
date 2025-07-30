// LoggerComponent/Core/LoggerComponent.cs - ✅ OPRAVENÝ - default rozsekávanie NULL
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
    /// Balík je nezávislý na konkrétnom logging systéme ale poskytuje file management
    /// POŽADUJE EXTERNÝ ILOGGER - nie je možné vytvoriť bez neho
    /// ✅ OPRAVENÉ: Default rozsekávanie je NULL (žiadne rozsekávanie)
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
        private readonly int? _maxFileSizeMB; // ✅ OPRAVENÉ: nullable int
        private readonly bool _enableFileLogging;

        #region ✅ Constructor - IBA S EXTERNÝM ILOGGER

        /// <summary>
        /// Vytvorí LoggerComponent s externým ILogger + file management
        /// ✅ JEDINÝ KONŠTRUKTOR - vyžaduje externý logger
        /// ✅ OPRAVENÉ: Default rozsekávanie je NULL (žiadne rozsekávanie)
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z aplikácie (POVINNÝ)</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB (NULL = bez rotácie, inak číslo pre rotáciu)</param>
        public LoggerComponent(ILogger externalLogger, string folderPath, string fileName, int? maxFileSizeMB = null)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger), "External logger is required - LoggerComponent cannot work without it");
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _maxFileSizeMB = maxFileSizeMB.HasValue ? Math.Max(0, maxFileSizeMB.Value) : null;
            _enableFileLogging = true;

            // Vytvor folder ak neexistuje
            EnsureDirectoryExists();

            // Inicializuj file manager ak je potrebné
            LogFileManager? fileManager = null;
            if (_maxFileSizeMB.HasValue && _maxFileSizeMB.Value > 0)
            {
                // S rotáciou
                var config = new LoggerConfiguration
                {
                    FolderPath = _folderPath,
                    BaseFileName = _fileName,
                    MaxFileSizeMB = _maxFileSizeMB.Value,
                    EnableRotation = true
                };
                fileManager = new LogFileManager(config);
            }
            else
            {
                // Bez rotácie - jednoduchý súbor
                var config = new LoggerConfiguration
                {
                    FolderPath = _folderPath,
                    BaseFileName = _fileName,
                    MaxFileSizeMB = 0,
                    EnableRotation = false
                };
                fileManager = new LogFileManager(config);
            }

            _fileManager = fileManager;

            // Log inicializáciu
            _externalLogger.LogInformation("LoggerComponent initialized - File: {FilePath}, Max size: {MaxSize}, Rotation: {Rotation}",
                CurrentLogFile, _maxFileSizeMB?.ToString() ?? "UNLIMITED", IsRotationEnabled);
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
        /// ✅ OPRAVENÉ: Kontrola cez nullable int
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
        /// ✅ NOVÉ: Maximálna veľkosť súboru (nullable)
        /// </summary>
        public int? MaxFileSizeMB => _maxFileSizeMB;

        #endregion

        #region ✅ Main Logging Method

        /// <summary>
        /// Zaznamená správu do súboru + externého logger systému - ✅ HLAVNÁ METÓDA
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
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var formattedMessage = $"[{timestamp}] [{logLevel}] {message}";

                // 1. Log do súboru (ak je povolené)
                if (_enableFileLogging && _fileManager != null)
                {
                    await _fileManager.WriteLogEntryAsync(formattedMessage);
                }

                // 2. Log do externého logger systému
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
                // Ak nie je možné vytvoriť adresár, loguj cez externý logger
                _externalLogger?.LogError(ex, "Failed to create log directory: {LogDirectory}", _folderPath);
            }
        }

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
                // Fallback ak externý logger zlyhal
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

        #region ✅ Factory Methods s OPRAVENÝM default parametrom

        /// <summary>
        /// Vytvorí LoggerComponent z externého ILoggerFactory + file settings
        /// ✅ OPRAVENÉ: Default maxFileSizeMB je NULL
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB (NULL = bez rotácie)</param>
        /// <param name="categoryName">Kategória pre logger (default: "RpaDataGrid")</param>
        /// <returns>LoggerComponent s externým logger + file management</returns>
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
        /// Vytvorí LoggerComponent z typed logger + file settings
        /// ✅ OPRAVENÉ: Default maxFileSizeMB je NULL
        /// </summary>
        /// <typeparam name="T">Typ pre logger kategóriu</typeparam>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB (NULL = bez rotácie)</param>
        /// <returns>LoggerComponent s typed logger + file management</returns>
        public static LoggerComponent FromLoggerFactory<T>(
            ILoggerFactory loggerFactory,
            string folderPath,
            string fileName,
            int? maxFileSizeMB = null)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger<T>();
            return new LoggerComponent(logger, folderPath, fileName, maxFileSizeMB);
        }

        /// <summary>
        /// ✅ NOVÉ: Vytvorí LoggerComponent bez rotácie (jednoduchý súbor)
        /// </summary>
        /// <param name="externalLogger">Externý ILogger</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <returns>LoggerComponent bez rotácie</returns>
        public static LoggerComponent WithoutRotation(ILogger externalLogger, string folderPath, string fileName)
        {
            return new LoggerComponent(externalLogger, folderPath, fileName, null);
        }

        /// <summary>
        /// ✅ NOVÉ: Vytvorí LoggerComponent s rotáciou
        /// </summary>
        /// <param name="externalLogger">Externý ILogger</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxSizeMB">Maximálna veľkosť súboru pre rotáciu</param>
        /// <returns>LoggerComponent s rotáciou</returns>
        public static LoggerComponent WithRotation(ILogger externalLogger, string folderPath, string fileName, int maxSizeMB)
        {
            return new LoggerComponent(externalLogger, folderPath, fileName, maxSizeMB);
        }

        #endregion

        #region ✅ Diagnostics Methods

        /// <summary>
        /// Získa diagnostické informácie o LoggerComponent
        /// ✅ AKTUALIZOVANÉ: Zobrazuje správne info o rotácii
        /// </summary>
        /// <returns>Diagnostické info ako string</returns>
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
        /// <returns>True ak funguje</returns>
        public async Task<bool> TestLoggingAsync()
        {
            try
            {
                var testMessage = $"LoggerComponent test message at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Rotation: {IsRotationEnabled}";
                await LogAsync(testMessage, "DEBUG");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Získa informácie o rotácii
        /// </summary>
        /// <returns>Informácie o rotácii súborov</returns>
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

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _externalLogger.LogInformation("LoggerComponent disposing - Final stats: {Stats}", GetDiagnosticInfo());

            _semaphore?.Dispose();
            _fileManager?.Dispose();
            // Poznámka: _externalLogger nie je owned by LoggerComponent, takže ho nedisposujeme

            _disposed = true;
        }

        #endregion
    }
}