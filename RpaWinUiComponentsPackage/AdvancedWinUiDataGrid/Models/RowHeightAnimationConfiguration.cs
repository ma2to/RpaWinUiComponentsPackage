// Models/RowHeightAnimationConfiguration.cs - ✅ NOVÉ: Row Height Animation Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre row height animations - smooth transitions pri rozšírení riadkov
    /// </summary>
    internal class RowHeightAnimationConfiguration
    {
        #region Properties

        /// <summary>
        /// Enable/disable row height animations
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Animation duration in milliseconds
        /// </summary>
        public int AnimationDurationMs { get; set; } = 300;

        /// <summary>
        /// Animation easing function
        /// </summary>
        public RowHeightEasingFunction EasingFunction { get; set; } = RowHeightEasingFunction.EaseInOut;

        /// <summary>
        /// Minimum animation threshold - only animate if height change is greater than this
        /// </summary>
        public double MinAnimationThreshold { get; set; } = 10.0;

        /// <summary>
        /// Maximum animation threshold - don't animate if height change is greater than this
        /// </summary>
        public double MaxAnimationThreshold { get; set; } = 500.0;

        /// <summary>
        /// Enable auto-sizing based on content
        /// </summary>
        public bool EnableAutoSizing { get; set; } = true;

        /// <summary>
        /// Auto-sizing debounce time in milliseconds
        /// </summary>
        public int AutoSizingDebounceMs { get; set; } = 200;

        /// <summary>
        /// Enable smooth transitions for text wrapping
        /// </summary>
        public bool EnableTextWrappingTransitions { get; set; } = true;

        /// <summary>
        /// Maximum concurrent animations
        /// </summary>
        public int MaxConcurrentAnimations { get; set; } = 10;

        /// <summary>
        /// Performance monitoring for animations
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = false;

        #endregion

        #region Predefined Configurations

        /// <summary>
        /// Default row height animation configuration
        /// </summary>
        public static RowHeightAnimationConfiguration Default => new()
        {
            IsEnabled = true,
            AnimationDurationMs = 300,
            EasingFunction = RowHeightEasingFunction.EaseInOut,
            MinAnimationThreshold = 10.0,
            MaxAnimationThreshold = 500.0,
            EnableAutoSizing = true,
            AutoSizingDebounceMs = 200,
            EnableTextWrappingTransitions = true,
            MaxConcurrentAnimations = 10,
            EnablePerformanceMonitoring = false
        };

        /// <summary>
        /// Fast animation configuration
        /// </summary>
        public static RowHeightAnimationConfiguration Fast => new()
        {
            IsEnabled = true,
            AnimationDurationMs = 150,
            EasingFunction = RowHeightEasingFunction.EaseOut,
            MinAnimationThreshold = 5.0,
            MaxAnimationThreshold = 300.0,
            EnableAutoSizing = true,
            AutoSizingDebounceMs = 100,
            EnableTextWrappingTransitions = true,
            MaxConcurrentAnimations = 15,
            EnablePerformanceMonitoring = false
        };

        /// <summary>
        /// Smooth animation configuration
        /// </summary>
        public static RowHeightAnimationConfiguration Smooth => new()
        {
            IsEnabled = true,
            AnimationDurationMs = 500,
            EasingFunction = RowHeightEasingFunction.EaseInOut,
            MinAnimationThreshold = 15.0,
            MaxAnimationThreshold = 600.0,
            EnableAutoSizing = true,
            AutoSizingDebounceMs = 300,
            EnableTextWrappingTransitions = true,
            MaxConcurrentAnimations = 8,
            EnablePerformanceMonitoring = true
        };

        /// <summary>
        /// Performance-optimized configuration
        /// </summary>
        public static RowHeightAnimationConfiguration Performance => new()
        {
            IsEnabled = true,
            AnimationDurationMs = 200,
            EasingFunction = RowHeightEasingFunction.Linear, // Fastest
            MinAnimationThreshold = 20.0,
            MaxAnimationThreshold = 200.0,
            EnableAutoSizing = false, // Disable for max performance
            AutoSizingDebounceMs = 50,
            EnableTextWrappingTransitions = false,
            MaxConcurrentAnimations = 5,
            EnablePerformanceMonitoring = false
        };

        /// <summary>
        /// Disabled animation configuration
        /// </summary>
        public static RowHeightAnimationConfiguration Disabled => new()
        {
            IsEnabled = false,
            AnimationDurationMs = 0,
            EasingFunction = RowHeightEasingFunction.Linear,
            MinAnimationThreshold = 0,
            MaxAnimationThreshold = 0,
            EnableAutoSizing = false,
            AutoSizingDebounceMs = 0,
            EnableTextWrappingTransitions = false,
            MaxConcurrentAnimations = 0,
            EnablePerformanceMonitoring = false
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validate configuration
        /// </summary>
        public void Validate()
        {
            if (AnimationDurationMs < 0)
                throw new ArgumentException("AnimationDurationMs cannot be negative");
            
            if (MinAnimationThreshold < 0)
                throw new ArgumentException("MinAnimationThreshold cannot be negative");
            
            if (MaxAnimationThreshold < MinAnimationThreshold)
                throw new ArgumentException("MaxAnimationThreshold must be greater than MinAnimationThreshold");
            
            if (AutoSizingDebounceMs < 0)
                throw new ArgumentException("AutoSizingDebounceMs cannot be negative");
            
            if (MaxConcurrentAnimations < 0)
                throw new ArgumentException("MaxConcurrentAnimations cannot be negative");
        }

        /// <summary>
        /// Clone configuration
        /// </summary>
        public RowHeightAnimationConfiguration Clone()
        {
            return new RowHeightAnimationConfiguration
            {
                IsEnabled = IsEnabled,
                AnimationDurationMs = AnimationDurationMs,
                EasingFunction = EasingFunction,
                MinAnimationThreshold = MinAnimationThreshold,
                MaxAnimationThreshold = MaxAnimationThreshold,
                EnableAutoSizing = EnableAutoSizing,
                AutoSizingDebounceMs = AutoSizingDebounceMs,
                EnableTextWrappingTransitions = EnableTextWrappingTransitions,
                MaxConcurrentAnimations = MaxConcurrentAnimations,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring
            };
        }

        #endregion
    }

    /// <summary>
    /// Row height animation easing functions
    /// </summary>
    public enum RowHeightEasingFunction
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        BounceIn,
        BounceOut,
        ElasticIn,
        ElasticOut
    }
}