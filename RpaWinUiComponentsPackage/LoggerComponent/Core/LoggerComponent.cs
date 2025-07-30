// LoggerComponent/Core/LoggerComponent.cs - ✅ KOMPLETNE OPRAVENÝ - iba s externým ILogger
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
        private readonly int _maxFileSizeMB;
        private readonly bool _enableFileLogging;

        #region ✅ Constructor - IBA S EXTERNÝM ILOGGER

        /// <summary>
        /// Vytvorí LoggerComponent s externým ILogger + file management
        /// ✅ JEDINÝ KONŠTRUKTOR - vyžaduje externý logger
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z aplikácie (POVINNÝ)</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB (0 = bez rotácie)</param>
        public LoggerComponent(ILogger externalLogger, string folderPath, string fileName, int maxFileSizeMB = 10)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger), "External logger is required - LoggerComponent cannot work without it");
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _maxFileSizeMB = Math.Max(0, maxFileSizeMB);
            _enableFileLogging = true;

            // Vytvor folder ak neexistuje
            EnsureDirectoryExists();

            // Inicializuj file manager
            var config = new LoggerConfiguration
            {
                FolderPath = _folderPath,
                BaseFileName = _fileName,
                MaxFileSizeMB = _maxFileSizeMB,
                EnableRotation = _maxFileSizeMB > 0
            };

            _fileManager = new LogFileManager(config);

            // Log inicializáciu
            _externalLogger.LogInformation("LoggerComponent initialized with file logging: {FilePath}, Max size: {MaxSize}MB, Rotation: {Rotation}",
                CurrentLogFile, _maxFileSizeMB, IsRotationEnabled);
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
        public bool IsRotationEnabled => _maxFileSizeMB > 0;

        /// <summary>
        /// Externý logger (pre AdvancedDataGrid použitie)
        /// </summary>
        public ILogger ExternalLogger => _externalLogger;

        /// <summary>
        /// Typ externého loggera
        /// </summary>
        public string ExternalLoggerType => _externalLogger.GetType().Name;

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

        #region ✅ Factory Methods

        /// <summary>
        /// Vytvorí LoggerComponent z externého ILoggerFactory + file settings
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB</param>
        /// <param name="categoryName">Kategória pre logger (default: "RpaDataGrid")</param>
        /// <returns>LoggerComponent s externým logger + file management</returns>
        public static LoggerComponent FromLoggerFactory(
            ILoggerFactory loggerFactory,
            string folderPath,
            string fileName,
            int maxFileSizeMB = 10,
            string categoryName = "RpaDataGrid")
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger(categoryName);
            return new LoggerComponent(logger, folderPath, fileName, maxFileSizeMB);
        }

        /// <summary>
        /// Vytvorí LoggerComponent z typed logger + file settings
        /// </summary>
        /// <typeparam name="T">Typ pre logger kategóriu</typeparam>
        /// <param name="loggerFactory">ILoggerFactory z demo aplikácie</param>
        /// <param name="folderPath">Cesta k log súborom</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Max veľkosť súboru v MB</param>
        /// <returns>LoggerComponent s typed logger + file management</returns>
        public static LoggerComponent FromLoggerFactory<T>(
            ILoggerFactory loggerFactory,
            string folderPath,
            string fileName,
            int maxFileSizeMB = 10)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger<T>();
            return new LoggerComponent(logger, folderPath, fileName, maxFileSizeMB);
        }

        #endregion

        #region ✅ Diagnostics Methods

        /// <summary>
        /// Získa diagnostické informácie o LoggerComponent
        /// </summary>
        /// <returns>Diagnostické info ako string</returns>
        public string GetDiagnosticInfo()
        {
            return $"LoggerComponent: External Logger = {ExternalLoggerType}, " +
                   $"File = {CurrentLogFile}, Size = {CurrentFileSizeMB:F2}MB, " +
                   $"Rotation = {IsRotationEnabled}, Files = {RotationFileCount}";
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

            _externalLogger.LogInformation("LoggerComponent disposing...");

            _semaphore?.Dispose();
            _fileManager?.Dispose();
            // Poznámka: _externalLogger nie je owned by LoggerComponent, takže ho nedisposujeme

            _disposed = true;
        }

        #endregion
    }

    #region ✅ Internal supporting classes - NEMENNÉ

    /// <summary>
    /// Konfigurácia pre LoggerComponent file management - INTERNAL
    /// </summary>
    internal class LoggerConfiguration
    {
        public string FolderPath { get; set; } = "";
        public string BaseFileName { get; set; } = "";
        public int MaxFileSizeMB { get; set; }
        public bool EnableRotation { get; set; }
    }

    /// <summary>
    /// Správca log súborov s rotáciou - INTERNAL
    /// </summary>
    internal class LogFileManager : IDisposable
    {
        private readonly LoggerConfiguration _config;
        private string _currentLogFile;
        private int _currentRotationNumber = 0;
        private bool _disposed = false;

        public LogFileManager(LoggerConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _currentLogFile = DetermineCurrentLogFile();
        }

        public string CurrentLogFile => _currentLogFile;
        public int RotationFileCount => _currentRotationNumber;

        /// <summary>
        /// Zapíše log entry do súboru
        /// </summary>
        public async Task WriteLogEntryAsync(string logEntry)
        {
            if (_disposed) return;

            // Skontroluj či treba rotovať súbor
            if (_config.EnableRotation && ShouldRotateFile())
            {
                RotateToNextFile();
            }

            // Zapíš do súboru
            await File.AppendAllTextAsync(_currentLogFile, logEntry + Environment.NewLine);
        }

        /// <summary>
        /// Získa veľkosť aktuálneho súboru v MB
        /// </summary>
        public double GetCurrentFileSizeMB()
        {
            try
            {
                if (!File.Exists(_currentLogFile))
                    return 0;

                var fileInfo = new FileInfo(_currentLogFile);
                return Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2);
            }
            catch
            {
                return 0;
            }
        }

        private string DetermineCurrentLogFile()
        {
            if (!_config.EnableRotation)
            {
                return Path.Combine(_config.FolderPath, _config.BaseFileName);
            }

            var baseNameWithoutExtension = Path.GetFileNameWithoutExtension(_config.BaseFileName);
            var extension = Path.GetExtension(_config.BaseFileName);

            var pattern = $"{baseNameWithoutExtension}_*.log";
            var existingFiles = Directory.GetFiles(_config.FolderPath, pattern);

            var maxNumber = 0;
            foreach (var file in existingFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var numberPart = fileName.Substring(baseNameWithoutExtension.Length + 1);

                if (int.TryParse(numberPart, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            if (maxNumber == 0)
            {
                _currentRotationNumber = 1;
                return Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_1{extension}");
            }

            _currentRotationNumber = maxNumber;
            var lastFile = Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{maxNumber}{extension}");

            if (File.Exists(lastFile))
            {
                var fileInfo = new FileInfo(lastFile);
                var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

                if (fileSizeMB >= _config.MaxFileSizeMB)
                {
                    _currentRotationNumber++;
                    return Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{_currentRotationNumber}{extension}");
                }
            }

            return lastFile;
        }

        private bool ShouldRotateFile()
        {
            if (!File.Exists(_currentLogFile))
                return false;

            var fileInfo = new FileInfo(_currentLogFile);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

            return fileSizeMB >= _config.MaxFileSizeMB;
        }

        private void RotateToNextFile()
        {
            _currentRotationNumber++;

            var baseNameWithoutExtension = Path.GetFileNameWithoutExtension(_config.BaseFileName);
            var extension = Path.GetExtension(_config.BaseFileName);

            _currentLogFile = Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{_currentRotationNumber}{extension}");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }

    #endregion
}