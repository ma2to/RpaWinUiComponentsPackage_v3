using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;

/// <summary>
/// PROFESSIONAL Configuration Coordinator - ONLY configuration management
/// RESPONSIBILITY: Handle configuration storage, validation, updates (NO UI, NO data operations, NO events)
/// SEPARATION: Pure configuration layer - immutable configuration patterns
/// ANTI-GOD: Single responsibility - only configuration coordination
/// </summary>
internal sealed class ConfigurationCoordinator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private bool _disposed = false;

    // IMMUTABLE CONFIGURATION STATE (Functional Pattern)
    private readonly record struct ConfigurationState(
        ColorConfiguration Colors,
        ValidationConfiguration Validation,
        PerformanceConfiguration Performance,
        int MinimumRows,
        bool IsInitialized
    );

    private ConfigurationState _state;

    public ConfigurationCoordinator(
        ILogger? logger, 
        GlobalExceptionHandler exceptionHandler,
        ColorConfiguration? colors = null,
        ValidationConfiguration? validation = null,
        PerformanceConfiguration? performance = null,
        int minimumRows = 5)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        
        // Initialize with default configurations
        _state = new ConfigurationState(
            Colors: colors ?? CreateDefaultColorConfiguration(),
            Validation: validation ?? CreateDefaultValidationConfiguration(), 
            Performance: performance ?? CreateDefaultPerformanceConfiguration(),
            MinimumRows: minimumRows,
            IsInitialized: true
        );
        
        _logger?.Info("⚙️ CONFIG COORDINATOR: Initialized with default configurations");
        LogCurrentConfiguration();
    }

    // Public read-only properties (Immutable exposure)
    public ColorConfiguration ColorConfiguration => _state.Colors;
    public ValidationConfiguration ValidationConfiguration => _state.Validation;
    public PerformanceConfiguration PerformanceConfiguration => _state.Performance;
    public int MinimumRows => _state.MinimumRows;
    public bool IsInitialized => _state.IsInitialized;

    /// <summary>
    /// Update color configuration immutably
    /// PURE CONFIG: Only updates configuration state, no side effects
    /// </summary>
    public async Task<Result<bool>> UpdateColorConfigurationAsync(ColorConfiguration newColors)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🎨 CONFIG UPDATE: Updating color configuration");
            
            var validationResult = await ValidateColorConfiguration(newColors);
            if (!validationResult.IsSuccess)
            {
                _logger?.Error("❌ CONFIG UPDATE: Color configuration validation failed - {Error}", validationResult.ErrorMessage);
                return false;
            }

            // Immutable update (Functional pattern)
            _state = _state with { Colors = newColors };
            
            _logger?.Info("✅ CONFIG UPDATE: Color configuration updated successfully");
            LogColorConfiguration();
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdateColorConfiguration", 1, false, _logger);
    }

    /// <summary>
    /// Update validation configuration immutably
    /// PURE CONFIG: Only updates configuration state, no side effects
    /// </summary>
    public async Task<Result<bool>> UpdateValidationConfigurationAsync(ValidationConfiguration newValidation)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔍 CONFIG UPDATE: Updating validation configuration");
            
            var validationResult = await ValidateValidationConfiguration(newValidation);
            if (!validationResult.IsSuccess)
            {
                _logger?.Error("❌ CONFIG UPDATE: Validation configuration validation failed - {Error}", validationResult.ErrorMessage);
                return false;
            }

            // Immutable update (Functional pattern)
            _state = _state with { Validation = newValidation };
            
            _logger?.Info("✅ CONFIG UPDATE: Validation configuration updated successfully");
            LogValidationConfiguration();
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdateValidationConfiguration", 1, false, _logger);
    }

    /// <summary>
    /// Update performance configuration immutably
    /// PURE CONFIG: Only updates configuration state, no side effects
    /// </summary>
    public async Task<Result<bool>> UpdatePerformanceConfigurationAsync(PerformanceConfiguration newPerformance)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("⚡ CONFIG UPDATE: Updating performance configuration");
            
            var validationResult = await ValidatePerformanceConfiguration(newPerformance);
            if (!validationResult.IsSuccess)
            {
                _logger?.Error("❌ CONFIG UPDATE: Performance configuration validation failed - {Error}", validationResult.ErrorMessage);
                return false;
            }

            // Immutable update (Functional pattern)
            _state = _state with { Performance = newPerformance };
            
            _logger?.Info("✅ CONFIG UPDATE: Performance configuration updated successfully");
            LogPerformanceConfiguration();
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdatePerformanceConfiguration", 1, false, _logger);
    }

    /// <summary>
    /// Update minimum rows setting
    /// PURE CONFIG: Only updates configuration state, no data operations
    /// </summary>
    public async Task<Result<bool>> UpdateMinimumRowsAsync(int newMinimumRows)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            if (newMinimumRows < 0)
            {
                _logger?.Error("❌ CONFIG UPDATE: Invalid minimum rows value: {MinRows} (must be >= 0)", newMinimumRows);
                return false;
            }

            _logger?.Info("📊 CONFIG UPDATE: Updating minimum rows from {OldMin} to {NewMin}", _state.MinimumRows, newMinimumRows);
            
            // Immutable update (Functional pattern)
            _state = _state with { MinimumRows = newMinimumRows };
            
            _logger?.Info("✅ CONFIG UPDATE: Minimum rows updated successfully to {MinRows}", newMinimumRows);
            
            await Task.CompletedTask;
            return true;
            
        }, "UpdateMinimumRows", 1, false, _logger);
    }

    /// <summary>
    /// Reset all configurations to defaults
    /// PURE CONFIG: Only resets configuration state
    /// </summary>
    public async Task<Result<bool>> ResetToDefaultsAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔄 CONFIG RESET: Resetting all configurations to defaults");
            
            _state = new ConfigurationState(
                Colors: CreateDefaultColorConfiguration(),
                Validation: CreateDefaultValidationConfiguration(),
                Performance: CreateDefaultPerformanceConfiguration(),
                MinimumRows: 5,
                IsInitialized: true
            );
            
            _logger?.Info("✅ CONFIG RESET: All configurations reset to defaults");
            LogCurrentConfiguration();
            
            await Task.CompletedTask;
            return true;
            
        }, "ResetToDefaults", 1, false, _logger);
    }

    /// <summary>
    /// Get current configuration snapshot (immutable)
    /// PURE CONFIG: Only returns immutable configuration state
    /// </summary>
    public ConfigurationSnapshot GetConfigurationSnapshot()
    {
        return new ConfigurationSnapshot(
            _state.Colors,
            _state.Validation,
            _state.Performance,
            _state.MinimumRows,
            DateTime.UtcNow
        );
    }

    #region Private Configuration Factories (Functional Pattern)

    private static ColorConfiguration CreateDefaultColorConfiguration() => new()
    {
        CellBackground = "#FFFFFF",
        CellForeground = "#000000", 
        CellBorder = "#E0E0E0",
        HeaderBackground = "#F0F0F0",
        HeaderForeground = "#000000",
        HeaderBorder = "#C0C0C0",
        SelectionBackground = "#0078D4",
        SelectionForeground = "#FFFFFF",
        ValidationErrorBorder = "#FF0000",
        EnableZebraStripes = false,
        AlternateRowBackground = "#F8F8F8"
    };

    private static ValidationConfiguration CreateDefaultValidationConfiguration() => new()
    {
        EnableRealtimeValidation = true,
        Rules = new Dictionary<string, Func<object, bool>>(),
        RulesWithMessages = new Dictionary<string, (Func<object, bool> Validator, string ErrorMessage)>()
    };

    private static PerformanceConfiguration CreateDefaultPerformanceConfiguration() => new()
    {
        EnableVirtualization = true,
        EnableCaching = true,
        OperationTimeout = TimeSpan.FromSeconds(30)
    };

    #endregion

    #region Private Configuration Validation

    private async Task<Result<bool>> ValidateColorConfiguration(ColorConfiguration colors)
    {
        try
        {
            // Validate color format
            var colorProperties = new[]
            {
                colors.CellBackground, colors.CellForeground, colors.CellBorder,
                colors.HeaderBackground, colors.HeaderForeground, colors.HeaderBorder,
                colors.SelectionBackground, colors.SelectionForeground, colors.ValidationErrorBorder
            };

            foreach (var color in colorProperties)
            {
                if (!IsValidHexColor(color))
                {
                    return Result<bool>.Failure($"Invalid color format: {color}");
                }
            }

            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Color configuration validation failed", ex);
        }
    }

    private async Task<Result<bool>> ValidateValidationConfiguration(ValidationConfiguration validation)
    {
        try
        {
            // Basic validation
            if (validation.Rules == null && validation.RulesWithMessages == null)
            {
                return Result<bool>.Failure("Validation configuration must have at least one rule collection");
            }

            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Validation configuration validation failed", ex);
        }
    }

    private async Task<Result<bool>> ValidatePerformanceConfiguration(PerformanceConfiguration performance)
    {
        try
        {
            if (performance.OperationTimeout <= TimeSpan.Zero)
            {
                return Result<bool>.Failure("OperationTimeout must be greater than zero");
            }

            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Performance configuration validation failed", ex);
        }
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return false;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color.Substring(1).All(c => "0123456789ABCDEFabcdef".Contains(c));
    }

    #endregion

    #region Private Logging Methods

    private void LogCurrentConfiguration()
    {
        _logger?.Info("📋 CONFIG STATE: Colors, Validation, Performance, MinRows: {MinRows}, Initialized: {Init}",
            _state.MinimumRows, _state.IsInitialized);
        
        LogColorConfiguration();
        LogValidationConfiguration();
        LogPerformanceConfiguration();
    }

    private void LogColorConfiguration()
    {
        _logger?.Info("🎨 COLOR CONFIG: Background: {BG}, Foreground: {FG}, Selection: {SEL}, Zebra: {ZEBRA}",
            _state.Colors.CellBackground, _state.Colors.CellForeground, _state.Colors.SelectionBackground, _state.Colors.EnableZebraStripes);
    }

    private void LogValidationConfiguration()
    {
        _logger?.Info("🔍 VALIDATION CONFIG: Enabled: {Enabled}, Rules: {Rules}, RulesWithMessages: {RulesWithMsg}",
            _state.Validation.EnableRealtimeValidation, 
            _state.Validation.Rules?.Count ?? 0, 
            _state.Validation.RulesWithMessages?.Count ?? 0);
    }

    private void LogPerformanceConfiguration()
    {
        _logger?.Info("⚡ PERFORMANCE CONFIG: Virtualization: {Virt}, Cache: {Cache}, Timeout: {Timeout}ms",
            _state.Performance.EnableVirtualization, _state.Performance.EnableCaching, 
            (int)_state.Performance.OperationTimeout.TotalMilliseconds);
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 CONFIG COORDINATOR DISPOSE: Cleaning up configuration state");
            _disposed = true;
            _logger?.Info("✅ CONFIG COORDINATOR DISPOSE: Disposed successfully");
        }
    }
}

/// <summary>
/// Immutable configuration snapshot for external consumers
/// </summary>
public readonly record struct ConfigurationSnapshot(
    ColorConfiguration Colors,
    ValidationConfiguration Validation,
    PerformanceConfiguration Performance,
    int MinimumRows,
    DateTime Timestamp
);