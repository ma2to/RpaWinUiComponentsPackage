// LoggerComponent.cs - ✅ NOVÝ komponent pre logovanie v balíku
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.LoggerComponent
{
    /// <summary>
    /// LoggerComponent pre logovanie do súborov s automatickou rotáciou - ✅ PUBLIC API
    /// </summary>
    public class LoggerComponent : IDisposable
    {
        private readonly LoggerConfiguration _config;
        private readonly LogFileManager _fileManager;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;

        /// <summary>
        /// Vytvorí novú inštanciu LoggerComponent
        /// </summary>
        /// <param name="folderPath">Cesta k priečinku pre log súbory</param>
        /// <param name="fileName">Názov log súboru (bez/s .log extension)</param>
        /// <param name="maxFileSizeMB">Maximálna veľkosť súboru v MB (0 = bez rotácie)</param>
        /// <exception cref="ArgumentException">Neplatná cesta alebo názov súboru</exception>
        /// <exception cref="DirectoryNotFoundException">Priečinok neexistuje</exception>
        public LoggerComponent(string folderPath, string fileName, int maxFileSizeMB = 0)
        {
            // Validácia vstupných parametrov
            ValidateInputs(folderPath, fileName, maxFileSizeMB);

            _config = new LoggerConfiguration
            {
                FolderPath = folderPath,
                BaseFileName = EnsureLogExtension(fileName),
                MaxFileSizeMB = maxFileSizeMB,
                EnableRotation = maxFileSizeMB > 0
            };

            _fileManager = new LogFileManager(_config);
        }

        /// <summary>
        /// Zaznamená správu do log súboru - ✅ JEDINÁ VEREJNÁ METÓDA
        /// </summary>
        /// <param name="message">Správa na zaznamenanie</param>
        /// <param name="logLevel">Úroveň logovania (INFO, ERROR, DEBUG...)</param>
        /// <returns>Task pre async operáciu</returns>
        /// <exception cref="ObjectDisposedException">Logger je disposed</exception>
        /// <exception cref="IOException">Chyba pri zápise do súboru</exception>
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
                await _fileManager.WriteLogEntryAsync(logEntry);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _semaphore?.Dispose();
            _fileManager?.Dispose();
            _disposed = true;
        }

        #region Private Helper Methods

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

        #region Public Properties (pre diagnostiku)

        /// <summary>
        /// Aktuálny log súbor
        /// </summary>
        public string CurrentLogFile => _fileManager?.CurrentLogFile ?? "";

        /// <summary>
        /// Veľkosť aktuálneho log súboru v MB
        /// </summary>
        public double CurrentFileSizeMB => _fileManager?.GetCurrentFileSizeMB() ?? 0;

        /// <summary>
        /// Počet log súborov v rotácii
        /// </summary>
        public int RotationFileCount => _fileManager?.RotationFileCount ?? 0;

        /// <summary>
        /// Či je rotácia povolená
        /// </summary>
        public bool IsRotationEnabled => _config?.EnableRotation ?? false;

        #endregion
    }

    #region Internal Support Classes

    /// <summary>
    /// Konfigurácia pre logger - INTERNAL
    /// </summary>
    

    

    #endregion
}