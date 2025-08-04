// Models/NetworkIOOptimizationConfiguration.cs - ✅ NOVÉ: Network/IO optimization configuration
using System;
using System.IO.Compression;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre Network/IO optimization
    /// </summary>
    public class NetworkIOOptimizationConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť Network/IO optimizácie
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Povoliť streaming operations
        /// </summary>
        public bool EnableStreaming { get; set; } = true;

        /// <summary>
        /// Povoliť compression
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Typ kompresie
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

        /// <summary>
        /// Povoliť progressive file loading
        /// </summary>
        public bool EnableProgressiveLoading { get; set; } = true;

        /// <summary>
        /// Veľkosť buffer pre streaming (bytes)
        /// </summary>
        public int StreamingBufferSize { get; set; } = 8192; // 8KB

        /// <summary>
        /// Veľkosť chunk pre progressive loading (bytes)
        /// </summary>
        public int ProgressiveChunkSize { get; set; } = 65536; // 64KB

        /// <summary>
        /// Max počet concurrent streams
        /// </summary>
        public int MaxConcurrentStreams { get; set; } = 4;

        /// <summary>
        /// Timeout pre network operations (ms)
        /// </summary>
        public int NetworkTimeoutMs { get; set; } = 30000; // 30 sekúnd

        /// <summary>
        /// Timeout pre file operations (ms)
        /// </summary>
        public int FileTimeoutMs { get; set; } = 60000; // 60 sekúnd

        /// <summary>
        /// Min veľkosť súboru pre compression (bytes)
        /// </summary>
        public long MinCompressionFileSize { get; set; } = 1024; // 1KB

        /// <summary>
        /// Max veľkosť súboru pre progressive loading (bytes)
        /// </summary>
        public long MaxProgressiveFileSize { get; set; } = 104857600; // 100MB

        /// <summary>
        /// Povoliť retry mechanizmus
        /// </summary>
        public bool EnableRetry { get; set; } = true;

        /// <summary>
        /// Max počet retry pokusov
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay medzi retry pokusmi (ms)
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Povoliť performance monitoring
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = true;

        /// <summary>
        /// Cache veľkosť pre metadata (items)
        /// </summary>
        public int MetadataCacheSize { get; set; } = 1000;

        #endregion

        #region Default Configurations

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static NetworkIOOptimizationConfiguration Default => new NetworkIOOptimizationConfiguration();

        /// <summary>
        /// High performance konfigurácia
        /// </summary>
        public static NetworkIOOptimizationConfiguration HighPerformance => new NetworkIOOptimizationConfiguration
        {
            StreamingBufferSize = 32768, // 32KB
            ProgressiveChunkSize = 131072, // 128KB
            MaxConcurrentStreams = 8,
            CompressionLevel = CompressionLevel.Fastest,
            NetworkTimeoutMs = 60000, // 60 sekúnd
            FileTimeoutMs = 120000, // 120 sekúnd
            EnablePerformanceMonitoring = true
        };

        /// <summary>
        /// Low bandwidth konfigurácia
        /// </summary>
        public static NetworkIOOptimizationConfiguration LowBandwidth => new NetworkIOOptimizationConfiguration
        {
            StreamingBufferSize = 4096, // 4KB
            ProgressiveChunkSize = 32768, // 32KB
            MaxConcurrentStreams = 2,
            CompressionLevel = CompressionLevel.Optimal,
            EnableCompression = true,
            NetworkTimeoutMs = 90000, // 90 sekúnd
            MaxRetryAttempts = 5,
            RetryDelayMs = 2000
        };

        /// <summary>
        /// Disabled konfigurácia
        /// </summary>
        public static NetworkIOOptimizationConfiguration Disabled => new NetworkIOOptimizationConfiguration
        {
            IsEnabled = false,
            EnableStreaming = false,
            EnableCompression = false,
            EnableProgressiveLoading = false,
            EnableRetry = false,
            EnablePerformanceMonitoring = false
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (StreamingBufferSize <= 0)
                throw new ArgumentException("StreamingBufferSize must be greater than 0");

            if (ProgressiveChunkSize <= 0)
                throw new ArgumentException("ProgressiveChunkSize must be greater than 0");

            if (MaxConcurrentStreams <= 0)
                throw new ArgumentException("MaxConcurrentStreams must be greater than 0");

            if (NetworkTimeoutMs <= 0)
                throw new ArgumentException("NetworkTimeoutMs must be greater than 0");

            if (FileTimeoutMs <= 0)
                throw new ArgumentException("FileTimeoutMs must be greater than 0");

            if (MaxRetryAttempts < 0)
                throw new ArgumentException("MaxRetryAttempts cannot be negative");

            if (RetryDelayMs < 0)
                throw new ArgumentException("RetryDelayMs cannot be negative");

            if (MetadataCacheSize < 0)
                throw new ArgumentException("MetadataCacheSize cannot be negative");

            if (MinCompressionFileSize < 0)
                throw new ArgumentException("MinCompressionFileSize cannot be negative");

            if (MaxProgressiveFileSize <= 0)
                throw new ArgumentException("MaxProgressiveFileSize must be greater than 0");
        }

        /// <summary>
        /// Klonuje konfiguráciu
        /// </summary>
        public NetworkIOOptimizationConfiguration Clone()
        {
            return new NetworkIOOptimizationConfiguration
            {
                IsEnabled = IsEnabled,
                EnableStreaming = EnableStreaming,
                EnableCompression = EnableCompression,
                CompressionLevel = CompressionLevel,
                EnableProgressiveLoading = EnableProgressiveLoading,
                StreamingBufferSize = StreamingBufferSize,
                ProgressiveChunkSize = ProgressiveChunkSize,
                MaxConcurrentStreams = MaxConcurrentStreams,
                NetworkTimeoutMs = NetworkTimeoutMs,
                FileTimeoutMs = FileTimeoutMs,
                MinCompressionFileSize = MinCompressionFileSize,
                MaxProgressiveFileSize = MaxProgressiveFileSize,
                EnableRetry = EnableRetry,
                MaxRetryAttempts = MaxRetryAttempts,
                RetryDelayMs = RetryDelayMs,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring,
                MetadataCacheSize = MetadataCacheSize
            };
        }

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public override string ToString()
        {
            return $"NetworkIOOptimizationConfiguration[Enabled: {IsEnabled}, " +
                   $"Streaming: {EnableStreaming}, Compression: {EnableCompression}, " +
                   $"Progressive: {EnableProgressiveLoading}, BufferSize: {StreamingBufferSize}, " +
                   $"ChunkSize: {ProgressiveChunkSize}, MaxStreams: {MaxConcurrentStreams}]";
        }

        #endregion
    }

    #region Enumerations

    /// <summary>
    /// Typ network operácie
    /// </summary>
    public enum NetworkOperationType
    {
        Download,
        Upload,
        Stream,
        Metadata
    }

    /// <summary>
    /// Stav network operácie
    /// </summary>
    public enum NetworkOperationStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled,
        Retrying
    }

    #endregion
}