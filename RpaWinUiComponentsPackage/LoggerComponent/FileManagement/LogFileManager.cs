// LoggerComponent/FileManagement/LogFileManager.cs - ✅ AKTUALIZOVANÝ pre null default
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger
{
    /// <summary>
    /// Správca log súborov s rotáciou - INTERNAL
    /// ✅ AKTUALIZOVANÝ: Podporuje režim bez rotácie (MaxFileSizeMB = 0)
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
        /// ✅ AKTUALIZOVANÉ: Kontroluje rotáciu len ak je povolená
        /// </summary>
        public async Task WriteLogEntryAsync(string logEntry)
        {
            if (_disposed) return;

            // Skontroluj či treba rotovať súbor (len ak je rotácia povolená)
            if (_config.EnableRotation && _config.MaxFileSizeMB > 0 && ShouldRotateFile())
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

        /// <summary>
        /// Určí aktuálny log súbor (existujúci alebo nový)
        /// ✅ AKTUALIZOVANÉ: Rozlišuje medzi režimom s rotáciou a bez rotácie
        /// </summary>
        private string DetermineCurrentLogFile()
        {
            if (!_config.EnableRotation || _config.MaxFileSizeMB <= 0)
            {
                // Bez rotácie - vždy rovnaký súbor
                return Path.Combine(_config.FolderPath, _config.BaseFileName);
            }

            // S rotáciou - nájdi najvyšší číslovany súbor
            var baseNameWithoutExtension = Path.GetFileNameWithoutExtension(_config.BaseFileName);
            var extension = Path.GetExtension(_config.BaseFileName);

            // Ak extension je prázdne, pridaj .log
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".log";
            }

            var pattern = $"{baseNameWithoutExtension}_*.log";

            // ✅ OPRAVENÉ: Zabezpeč že folder existuje pred hľadaním súborov
            if (!Directory.Exists(_config.FolderPath))
            {
                Directory.CreateDirectory(_config.FolderPath);
                // Ak folder neexistoval, začni s _1
                _currentRotationNumber = 1;
                return Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_1{extension}");
            }

            var existingFiles = Directory.GetFiles(_config.FolderPath, pattern);

            var maxNumber = 0;
            foreach (var file in existingFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var underscoreIndex = fileName.LastIndexOf('_');

                if (underscoreIndex >= 0 && underscoreIndex < fileName.Length - 1)
                {
                    var numberPart = fileName.Substring(underscoreIndex + 1);
                    if (int.TryParse(numberPart, out var number) && number > maxNumber)
                    {
                        maxNumber = number;
                    }
                }
            }

            // Ak niet súborov, začni s _1
            if (maxNumber == 0)
            {
                _currentRotationNumber = 1;
                return Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_1{extension}");
            }

            // Skontroluj či posledný súbor neprekročil limit
            _currentRotationNumber = maxNumber;
            var lastFile = Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{maxNumber}{extension}");

            if (File.Exists(lastFile))
            {
                var fileInfo = new FileInfo(lastFile);
                var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

                if (fileSizeMB >= _config.MaxFileSizeMB)
                {
                    // Vytvor nový súbor
                    _currentRotationNumber++;
                    return Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{_currentRotationNumber}{extension}");
                }
            }

            return lastFile;
        }

        /// <summary>
        /// Skontroluje či treba rotovať súbor
        /// ✅ AKTUALIZOVANÉ: Vracia false ak rotácia nie je povolená
        /// </summary>
        private bool ShouldRotateFile()
        {
            if (!_config.EnableRotation || _config.MaxFileSizeMB <= 0)
                return false;

            if (!File.Exists(_currentLogFile))
                return false;

            var fileInfo = new FileInfo(_currentLogFile);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

            return fileSizeMB >= _config.MaxFileSizeMB;
        }

        /// <summary>
        /// Rotuje na ďalší súbor v sekvencii
        /// </summary>
        private void RotateToNextFile()
        {
            _currentRotationNumber++;

            var baseNameWithoutExtension = Path.GetFileNameWithoutExtension(_config.BaseFileName);
            var extension = Path.GetExtension(_config.BaseFileName);

            // Ak extension je prázdne, pridaj .log
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".log";
            }

            _currentLogFile = Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{_currentRotationNumber}{extension}");
        }

        /// <summary>
        /// ✅ NOVÉ: Získa zoznam všetkých log súborov (pre rotáciu)
        /// </summary>
        public List<string> GetAllLogFiles()
        {
            try
            {
                if (!_config.EnableRotation)
                {
                    return File.Exists(_currentLogFile) ? new List<string> { _currentLogFile } : new List<string>();
                }

                var baseNameWithoutExtension = Path.GetFileNameWithoutExtension(_config.BaseFileName);
                var pattern = $"{baseNameWithoutExtension}_*.log";

                if (!Directory.Exists(_config.FolderPath))
                    return new List<string>();

                return Directory.GetFiles(_config.FolderPath, pattern)
                    .OrderBy(f => f)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Získa celkovú veľkosť všetkých log súborov
        /// </summary>
        public double GetTotalLogFilesSizeMB()
        {
            try
            {
                var allFiles = GetAllLogFiles();
                long totalBytes = 0;

                foreach (var file in allFiles)
                {
                    if (File.Exists(file))
                    {
                        var fileInfo = new FileInfo(file);
                        totalBytes += fileInfo.Length;
                    }
                }

                return Math.Round(totalBytes / (1024.0 * 1024.0), 2);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Vyčistí staré log súbory (ponechá posledných N súborov)
        /// </summary>
        public async Task CleanupOldLogFilesAsync(int keepLastFiles = 10)
        {
            try
            {
                if (!_config.EnableRotation) return;

                var allFiles = GetAllLogFiles();
                if (allFiles.Count <= keepLastFiles) return;

                // Zostav zoznam súborov na zmazanie (všetky okrem posledných N)
                var filesToDelete = allFiles.Take(allFiles.Count - keepLastFiles).ToList();

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignoruj chyby pri mazaní jednotlivých súborov
                    }
                }

                await Task.CompletedTask;
            }
            catch
            {
                // Ignoruj chyby v cleanup
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            // Žiadne resources na dispose v tomto prípade
            _disposed = true;
        }
    }
}