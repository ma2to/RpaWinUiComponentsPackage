// LoggerComponent/Core/LoggerComponent.cs - ✅ OPRAVENÝ ako bridge pre externý logging systém
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions; // ✅ Iba Abstractions
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger
{
    /// <summary>
    /// LoggerComponent ako bridge medzi externým logging systémom a DataGrid komponentom - ✅ PUBLIC API
    /// Podporuje buď súborové logovanie alebo externý ILogger
    /// </summary>
    public class LoggerComponent : IDisposable
    {
        private readonly LoggerConfiguration _config;
        private readonly LogFileManager? _fileManager;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger? _externalLogger;
        private readonly LoggerMode _mode;
        private bool _disposed = false;

        /// <summary>
        /// Mód logovania
        /// </summary>
        public enum LoggerMode
        {
            /// <summary>
            /// Logovanie do súborov s rotáciou
            /// </summary>
            FileLogging,

            /// <summary>
            /// Použitie externého ILogger systému
            /// </summary>
            ExternalLogger,

            /// <summary>
            /// Hybrid - súbory + externý logger
            /// </summary>
            Hybrid
        }

        #region ✅ Constructors

        /// <summary>
        /// Vytvorí LoggerComponent pre súborové logovanie s rotáciou (pôvodný spôsob)
        /// </summary>
        /// <param name="folderPath">Cesta k priečinku pre log súbory</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Maximálna veľkosť súboru v MB (0 = bez rotácie)</param>
        public LoggerComponent(string folderPath, string fileName, int maxFileSizeMB = 0)
        {
            // Validácia vstupných parametrov
            ValidateInputs(folderPath, fileName, maxFileSizeMB);

            _mode = LoggerMode.FileLogging;
            _config = new LoggerConfiguration
            {
                FolderPath = folderPath,
                BaseFileName = EnsureLogExtension(fileName),
                MaxFileSizeMB = maxFileSizeMB,
                EnableRotation = maxFileSizeMB > 0
            };

            _fileManager = new LogFileManager(_config);
            _externalLogger = null;
        }

        /// <summary>
        /// ✅ NOVÝ: Vytvorí LoggerComponent s externým ILogger systémom
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z aplikácie</param>
        public LoggerComponent(ILogger externalLogger)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger));
            _mode = LoggerMode.ExternalLogger;
            _config = new LoggerConfiguration(); // Prázdna config
            _fileManager = null;
        }

        /// <summary>
        /// ✅ NOVÝ: Vytvorí LoggerComponent s hybrid módom (súbory + externý logger)
        /// </summary>
        /// <param name="externalLogger">Externý ILogger z aplikácie</param>
        /// <param name="folderPath">Cesta k priečinku pre log súbory</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Maximálna veľkosť súboru v MB</param>
        public LoggerComponent(ILogger externalLogger, string folderPath, string fileName, int maxFileSizeMB = 0)
        {
            _externalLogger = externalLogger ?? throw new ArgumentNullException(nameof(externalLogger));

            // Validácia súborových parametrov
            ValidateInputs(folderPath, fileName, maxFileSizeMB);

            _mode = LoggerMode.Hybrid;
            _config = new LoggerConfiguration
            {
                FolderPath = folderPath,
                BaseFileName = EnsureLogExtension(fileName),
                MaxFileSizeMB = maxFileSizeMB,
                EnableRotation = maxFileSizeMB > 0
            };

            _fileManager = new LogFileManager(_config);
        }

        #endregion

        #region ✅ Public Properties

        /// <summary>
        /// Mód logovania
        /// </summary>
        public LoggerMode Mode => _mode;

        /// <summary>
        /// Aktuálny log súbor (iba pre File a Hybrid módy)
        /// </summary>
        public string CurrentLogFile => _fileManager?.CurrentLogFile ?? "";

        /// <summary>
        /// Veľkosť aktuálneho log súboru v MB (iba pre File a Hybrid módy)
        /// </summary>
        public double CurrentFileSizeMB => _fileManager?.GetCurrentFileSizeMB() ?? 0;

        /// <summary>
        /// Počet log súborov v rotácii (iba pre File a Hybrid módy)
        /// </summary>
        public int RotationFileCount => _fileManager?.RotationFileCount ?? 0;

        /// <summary>
        /// Či je rotácia povolená (iba pre File a Hybrid módy)
        /// </summary>
        public bool IsRotationEnabled => _config?.EnableRotation ?? false;

        /// <summary>
        /// Má externý logger
        /// </summary>
        public bool HasExternalLogger => _externalLogger != null;

        /// <summary>
        /// Má súborové logovanie
        /// </summary>
        public bool HasFileLogging => _fileManager != null;

        #endregion

        #region ✅ Main Logging Method

        /// <summary>
        /// Zaznamená správu do log súboru a/alebo externého logger systému - ✅ JEDINÁ VEREJNÁ METÓDA
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
                var logEntry = CreateLogEntry(message, logLevel);

                // Parallel logging pre lepší výkon
                var tasks = new List<Task>();

                // ✅ Log do súboru (ak je dostupný)
                if (_fileManager != null)
                {
                    tasks.Add(_fileManager.WriteLogEntryAsync(logEntry));
                }

                // ✅ Log do externého logger systému (ak je dostupný)
                if (_externalLogger != null)
                {
                    tasks.Add(Task.Run(() => LogToExternalLogger(message, logLevel)));
                }

                // Vykonaj všetky logging operácie paralelne
                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                }
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
            if (_externalLogger == null) return;

            try
            {
                var level = ParseLogLevel(logLevel);
                _externalLogger.Log(level, message);
            }
            catch (Exception ex)
            {
                // Fallback ak externý logger zlyhal
                System.Diagnostics.Debug.WriteLine($"External logger failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Parsuje string log level na Microsoft.Extensions.Logging.LogLevel
        /// </summary>
        private static Microsoft.Extensions.Logging.LogLevel ParseLogLevel(string logLevel)
        {
            return logLevel.ToUpperInvariant() switch
            {
                "TRACE" => Microsoft.Extensions.Logging.LogLevel.Trace,
                "DEBUG" => Microsoft.Extensions.Logging.LogLevel.Debug,
                "INFO" or "INFORMATION" => Microsoft.Extensions.Logging.LogLevel.Information,
                "WARN" or "WARNING" => Microsoft.Extensions.Logging.LogLevel.Warning,
                "ERROR" => Microsoft.Extensions.Logging.LogLevel.Error,
                "CRITICAL" or "FATAL" => Microsoft.Extensions.Logging.LogLevel.Critical,
                _ => Microsoft.Extensions.Logging.LogLevel.Information
            };
        }

        /// <summary>
        /// Validuje vstupné parametre konštruktora
        /// </summary>
        private static void ValidateInputs(string folderPath, string fileName, int maxFileSizeMB)
        {
            // Validácia folder path
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path nemôže byť prázdny", nameof(folderPath));

            if (!Path.IsPathRooted(folderPath))
                throw new ArgumentException("Folder path musí byť absolútna cesta", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Priečinok '{folderPath}' neexistuje");

            // Validácia file name
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name nemôže byť prázdny", nameof(fileName));

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("File name obsahuje nepovolené znaky", nameof(fileName));

            // Validácia max file size
            if (maxFileSizeMB < 0)
                throw new ArgumentException("Max file size nemôže byť záporný", nameof(maxFileSizeMB));
        }

        /// <summary>
        /// Zabezpečí že súbor má .log extension
        /// </summary>
        private static string EnsureLogExtension(string fileName)
        {
            return Path.HasExtension(fileName) && Path.GetExtension(fileName).Equals(".log", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : Path.ChangeExtension(fileName, ".log");
        }

        /// <summary>
        /// Vytvorí formátovaný log entry
        /// </summary>
        private string CreateLogEntry(string message, string logLevel)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"[{timestamp}] [{logLevel.ToUpper()}] {message}";
        }

        #endregion

        #region ✅ Factory Methods

        /// <summary>
        /// ✅ NOVÝ: Vytvorí LoggerComponent z externého ILoggerFactory
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory z aplikácie</param>
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
        /// ✅ NOVÝ: Vytvorí LoggerComponent s hybrid módom z ILoggerFactory
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory z aplikácie</param>
        /// <param name="folderPath">Cesta k priečinku pre log súbory</param>
        /// <param name="fileName">Názov log súboru</param>
        /// <param name="maxFileSizeMB">Maximálna veľkosť súboru v MB</param>
        /// <param name="categoryName">Kategória pre logger (default: "RpaDataGrid")</param>
        /// <returns>LoggerComponent s hybrid módom</returns>
        public static LoggerComponent FromLoggerFactoryWithFiles(
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

        #endregion

        #region ✅ Diagnostics Methods

        /// <summary>
        /// Získa diagnostické informácie o LoggerComponent
        /// </summary>
        /// <returns>Diagnostické info ako string</returns>
        public string GetDiagnosticInfo()
        {
            var info = $"LoggerComponent Mode: {_mode}";

            if (HasExternalLogger)
                info += ", External: Yes";

            if (HasFileLogging)
                info += $", File: {CurrentLogFile} ({CurrentFileSizeMB:F2}MB)";

            if (IsRotationEnabled)
                info += $", Rotation: {RotationFileCount} files";

            return info;
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
            _fileManager?.Dispose();
            // Poznámka: _externalLogger nie je owned by LoggerComponent, takže ho nedisposujeme

            _disposed = true;
        }

        #endregion
    }
}