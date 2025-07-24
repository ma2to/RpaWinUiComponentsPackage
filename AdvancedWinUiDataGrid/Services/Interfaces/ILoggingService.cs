// Services/Interfaces/ILoggingService.cs
using Microsoft.Extensions.Logging;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre logovanie (abstrakcia nad Microsoft.Extensions.Logging)
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Log Debug správu
        /// </summary>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Log Info správu
        /// </summary>
        void LogInfo(string message, params object[] args);

        /// <summary>
        /// Log Warning správu
        /// </summary>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Log Error správu
        /// </summary>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Log Error správu s exception
        /// </summary>
        void LogError(System.Exception exception, string message, params object[] args);

        /// <summary>
        /// Nastaví minimálny level logovania
        /// </summary>
        void SetMinimumLevel(LogLevel level);

        /// <summary>
        /// Kontroluje či je povolený daný level logovania
        /// </summary>
        bool IsEnabled(LogLevel level);
    }
}