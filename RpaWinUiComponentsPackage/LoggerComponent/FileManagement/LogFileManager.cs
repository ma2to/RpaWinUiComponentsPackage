// LoggerComponent/FileManagement/LogFileManager.cs - ✅ OPRAVENÝ namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger  // ✅ CHANGED: LoggerComponent -> Logger
{
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

        /// <summary>
        /// Určí aktuálny log súbor (existujúci alebo nový)
        /// </summary>
        private string DetermineCurrentLogFile()
        {
            if (!_config.EnableRotation)
            {
                // Bez rotácie - vždy rovnaký súbor
                return Path.Combine(_config.FolderPath, _config.BaseFileName);
            }

            // S rotáciou - nájdi najvyšší číslovany súbor
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
        /// </summary>
        private bool ShouldRotateFile()
        {
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

            _currentLogFile = Path.Combine(_config.FolderPath, $"{baseNameWithoutExtension}_{_currentRotationNumber}{extension}");
        }

        public void Dispose()
        {
            if (_disposed) return;
            // Žiadne resources na dispose v tomto prípade
            _disposed = true;
        }
    }
}