// LoggerComponent/Configuration/LoggerConfiguration.cs - ✅ AKTUALIZOVANÁ pre null default
using System;
using System.IO;

namespace RpaWinUiComponentsPackage.Logger
{
    /// <summary>
    /// Konfigurácia pre LoggerComponent - INTERNAL
    /// ✅ AKTUALIZOVANÁ: Rozšírená o helper metódy a validation
    /// </summary>
    internal class LoggerConfiguration
    {
        /// <summary>
        /// Cesta k adresáru s log súbormi
        /// </summary>
        public string FolderPath { get; set; } = "";

        /// <summary>
        /// Základný názov súboru (napr. "app.log")
        /// </summary>
        public string BaseFileName { get; set; } = "";

        /// <summary>
        /// Maximálna veľkosť súboru v MB (0 = bez limitu)
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 0;

        /// <summary>
        /// Či je povolená rotácia súborov
        /// </summary>
        public bool EnableRotation { get; set; } = false;

        /// <summary>
        /// ✅ NOVÉ: Maximálny počet rotačných súborov (0 = neobmedzené)
        /// </summary>
        public int MaxRotationFiles { get; set; } = 0;

        /// <summary>
        /// ✅ NOVÉ: Či má automaticky vyčistiť staré súbory
        /// </summary>
        public bool AutoCleanupOldFiles { get; set; } = false;

        /// <summary>
        /// ✅ NOVÉ: Buffer size pre zápis do súboru (v bajtoch)
        /// </summary>
        public int WriteBufferSize { get; set; } = 4096;

        /// <summary>
        /// ✅ NOVÉ: Timeout pre file operations (v ms)
        /// </summary>
        public int FileOperationTimeoutMs { get; set; } = 5000;

        #region ✅ NOVÉ: Factory Methods

        /// <summary>
        /// Vytvorí konfiguráciu bez rotácie (jednoduchý súbor)
        /// </summary>
        /// <param name="folderPath">Cesta k adresáru</param>
        /// <param name="fileName">Názov súboru</param>
        /// <returns>Konfigurácia bez rotácie</returns>
        public static LoggerConfiguration WithoutRotation(string folderPath, string fileName)
        {
            return new LoggerConfiguration
            {
                FolderPath = folderPath,
                BaseFileName = fileName,
                MaxFileSizeMB = 0,
                EnableRotation = false
            };
        }

        /// <summary>
        /// Vytvorí konfiguráciu s rotáciou
        /// </summary>
        /// <param name="folderPath">Cesta k adresáru</param>
        /// <param name="fileName">Názov súboru</param>
        /// <param name="maxSizeMB">Maximálna veľkosť súboru</param>
        /// <param name="maxFiles">Maximálny počet súborov (optional)</param>
        /// <returns>Konfigurácia s rotáciou</returns>
        public static LoggerConfiguration WithRotation(string folderPath, string fileName, int maxSizeMB, int maxFiles = 0)
        {
            return new LoggerConfiguration
            {
                FolderPath = folderPath,
                BaseFileName = fileName,
                MaxFileSizeMB = maxSizeMB,
                EnableRotation = true,
                MaxRotationFiles = maxFiles,
                AutoCleanupOldFiles = maxFiles > 0
            };
        }

        /// <summary>
        /// Vytvorí default konfiguráciu pre dané parametre
        /// </summary>
        /// <param name="folderPath">Cesta k adresáru</param>
        /// <param name="fileName">Názov súboru</param>
        /// <param name="maxSizeMB">Maximálna veľkosť súboru (null = bez rotácie)</param>
        /// <returns>Konfigurácia</returns>
        public static LoggerConfiguration Create(string folderPath, string fileName, int? maxSizeMB = null)
        {
            if (maxSizeMB.HasValue && maxSizeMB.Value > 0)
            {
                return WithRotation(folderPath, fileName, maxSizeMB.Value);
            }
            else
            {
                return WithoutRotation(folderPath, fileName);
            }
        }

        #endregion

        #region ✅ NOVÉ: Validation Methods

        /// <summary>
        /// Validuje konfiguráciu a opraví chyby ak je to možné
        /// </summary>
        /// <returns>True ak je konfigurácia validná</returns>
        public bool Validate()
        {
            var isValid = true;

            // Validácia cesty
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                FolderPath = Path.GetTempPath();
                isValid = false;
            }

            // Validácia názvu súboru
            if (string.IsNullOrWhiteSpace(BaseFileName))
            {
                BaseFileName = "application.log";
                isValid = false;
            }

            // Pridaj .log extension ak chýba
            if (!Path.HasExtension(BaseFileName))
            {
                BaseFileName += ".log";
            }

            // Validácia veľkosti súboru
            if (MaxFileSizeMB < 0)
            {
                MaxFileSizeMB = 0;
                isValid = false;
            }

            // Validácia rotácie
            if (EnableRotation && MaxFileSizeMB <= 0)
            {
                EnableRotation = false;
                isValid = false;
            }

            // Validácia max files
            if (MaxRotationFiles < 0)
            {
                MaxRotationFiles = 0;
            }

            // Validácia buffer size
            if (WriteBufferSize <= 0)
            {
                WriteBufferSize = 4096;
            }

            // Validácia timeout
            if (FileOperationTimeoutMs <= 0)
            {
                FileOperationTimeoutMs = 5000;
            }

            return isValid;
        }

        /// <summary>
        /// Získa chyby validácie
        /// </summary>
        /// <returns>Zoznam chýb</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FolderPath))
                errors.Add("FolderPath je povinný");

            if (string.IsNullOrWhiteSpace(BaseFileName))
                errors.Add("BaseFileName je povinný");

            if (MaxFileSizeMB < 0)
                errors.Add("MaxFileSizeMB nemôže byť záporný");

            if (EnableRotation && MaxFileSizeMB <= 0)
                errors.Add("Pre rotáciu musí byť MaxFileSizeMB väčší ako 0");

            if (MaxRotationFiles < 0)
                errors.Add("MaxRotationFiles nemôže byť záporný");

            if (WriteBufferSize <= 0)
                errors.Add("WriteBufferSize musí byť kladný");

            if (FileOperationTimeoutMs <= 0)
                errors.Add("FileOperationTimeoutMs musí byť kladný");

            try
            {
                // Validácia cesty - pokús sa vytvoriť adresár
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Nemožno vytvoriť adresár '{FolderPath}': {ex.Message}");
            }

            return errors;
        }

        #endregion

        #region ✅ NOVÉ: Helper Methods

        /// <summary>
        /// Získa plnú cestu k base súboru
        /// </summary>
        /// <returns>Plná cesta k súboru</returns>
        public string GetFullFilePath()
        {
            return Path.Combine(FolderPath, BaseFileName);
        }

        /// <summary>
        /// Získa extension súboru
        /// </summary>
        /// <returns>Extension súboru</returns>
        public string GetFileExtension()
        {
            var ext = Path.GetExtension(BaseFileName);
            return string.IsNullOrEmpty(ext) ? ".log" : ext;
        }

        /// <summary>
        /// Získa názov súboru bez extension
        /// </summary>
        /// <returns>Názov bez extension</returns>
        public string GetFileNameWithoutExtension()
        {
            var name = Path.GetFileNameWithoutExtension(BaseFileName);
            return string.IsNullOrEmpty(name) ? "application" : name;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        /// <returns>Kópia konfigurácie</returns>
        public LoggerConfiguration Clone()
        {
            return new LoggerConfiguration
            {
                FolderPath = FolderPath,
                BaseFileName = BaseFileName,
                MaxFileSizeMB = MaxFileSizeMB,
                EnableRotation = EnableRotation,
                MaxRotationFiles = MaxRotationFiles,
                AutoCleanupOldFiles = AutoCleanupOldFiles,
                WriteBufferSize = WriteBufferSize,
                FileOperationTimeoutMs = FileOperationTimeoutMs
            };
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            var rotationInfo = EnableRotation ? $"{MaxFileSizeMB}MB rotation" : "no rotation";
            var filesInfo = MaxRotationFiles > 0 ? $", max {MaxRotationFiles} files" : "";

            return $"LoggerConfig: {GetFullFilePath()} ({rotationInfo}{filesInfo})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is LoggerConfiguration other)
            {
                return FolderPath.Equals(other.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                       BaseFileName.Equals(other.BaseFileName, StringComparison.OrdinalIgnoreCase) &&
                       MaxFileSizeMB == other.MaxFileSizeMB &&
                       EnableRotation == other.EnableRotation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                FolderPath.ToLowerInvariant(),
                BaseFileName.ToLowerInvariant(),
                MaxFileSizeMB,
                EnableRotation
            );
        }

        #endregion
    }
}