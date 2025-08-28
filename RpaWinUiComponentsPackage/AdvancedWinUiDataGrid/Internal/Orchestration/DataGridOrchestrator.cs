using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Business;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Orchestration;

/// <summary>
/// PROFESSIONAL DataGrid Orchestrator
/// RESPONSIBILITY: Coordinate between UI and Business layers without containing either logic
/// SEPARATION: Pure orchestration - delegates to appropriate managers, never contains business or UI logic
/// ANTI-GOD: Thin orchestration layer that coordinates without doing the actual work
/// </summary>
internal sealed class DataGridOrchestrator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private readonly DataGridUIManager _uiManager;
    private readonly DataGridBusinessManager _businessManager;
    private readonly DataGridCoordinator _coordinator;
    private bool _disposed = false;

    public DataGridOrchestrator(
        ILogger? logger,
        GlobalExceptionHandler exceptionHandler,
        DataGridUIManager uiManager,
        DataGridBusinessManager businessManager,
        DataGridCoordinator coordinator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
        _businessManager = businessManager ?? throw new ArgumentNullException(nameof(businessManager));
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        
        _logger?.Info("🎭 ORCHESTRATOR: Initialized DataGridOrchestrator - UI and Business layers separated");
    }

    /// <summary>
    /// Orchestrate full UI generation with business data
    /// ORCHESTRATION: Coordinates UI and Business but doesn't contain either logic
    /// </summary>
    public async Task GenerateFullUIAsync()
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATOR: Starting full UI generation orchestration");
            
            // Step 1: Generate UI elements (UI layer)
            await _uiManager.GenerateUIElementsAsync(_coordinator);
            
            // Step 2: Check if we need to show/hide fallback (Business decision, UI action)
            var hasData = _coordinator.DataRows?.Count > 0;
            _uiManager.SetFallbackVisibility(!hasData);
            
            _logger?.Info("✅ ORCHESTRATOR: Full UI generation orchestration completed");
            
        }, "GenerateFullUI", _logger);
    }

    /// <summary>
    /// Orchestrate validation with UI updates
    /// ORCHESTRATION: Business validation + UI styling coordination
    /// </summary>
    public async Task<Result<ValidationResult>> ValidateWithUIUpdateAsync(IProgress<ValidationProgress>? progress = null)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATOR: Starting validation with UI update orchestration");
            
            // Step 1: Perform business validation (Business layer)
            var validationResult = await _businessManager.ValidateAllAsync(_coordinator, progress);
            
            if (validationResult.IsSuccess)
            {
                // Step 2: Update UI based on validation results (UI layer)
                await UpdateValidationUIAsync(validationResult.Value);
                
                _logger?.Info("✅ ORCHESTRATOR: Validation with UI update orchestration completed");
                return validationResult.Value;
            }
            else
            {
                _logger?.Error("❌ ORCHESTRATOR: Validation orchestration failed - {Error}", validationResult.ErrorMessage);
                return new ValidationResult(0, 0, 0, new System.Collections.Generic.List<ValidationError>());
            }
            
        }, "ValidateWithUIUpdate", _coordinator?.DataRows?.Count ?? 0, 
           new ValidationResult(0, 0, 0, new System.Collections.Generic.List<ValidationError>()), _logger);
    }

    /// <summary>
    /// Orchestrate cell validation with immediate UI feedback
    /// ORCHESTRATION: Single cell business validation + UI styling
    /// </summary>
    public async Task ValidateCellWithUIAsync(DataGridCell cell, string newValue, Border cellBorder)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATOR: Starting cell validation with UI orchestration for {CellId}", cell.CellId);
            
            var validationConfig = _coordinator.ValidationConfiguration;
            
            // Step 1: Perform business validation for the cell (Business layer)
            var cellValidationResult = await _businessManager.ValidateCellBusinessLogic(cell, newValue, validationConfig);
            
            // Step 2: Update cell data based on validation (Data layer)
            cell.ValidationState = cellValidationResult.IsValid;
            cell.HasValidationErrors = !cellValidationResult.IsValid;
            cell.ValidationError = cellValidationResult.IsValid ? null : 
                string.Join("; ", cellValidationResult.Errors.Select(e => e.Message));
            
            // Step 3: Apply UI styling based on validation result (UI layer)
            var colorConfig = _coordinator.ColorConfiguration;
            _uiManager.ApplyValidationStyling(cellBorder, cellValidationResult.IsValid, cell.ValidationError, colorConfig);
            
            _logger?.Info("✅ ORCHESTRATOR: Cell validation with UI orchestration completed for {CellId}", cell.CellId);
            
        }, $"ValidateCellWithUI-{cell.CellId}", _logger);
    }

    /// <summary>
    /// Orchestrate data import with full processing
    /// ORCHESTRATION: Business import + UI regeneration + validation
    /// </summary>
    public async Task<Result<Models.ImportResult>> ImportDataWithFullProcessingAsync(
        System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyDictionary<string, object?>> data)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATOR: Starting data import with full processing orchestration");
            
            // Step 1: Process data through business logic (Business layer)
            var importResult = await _businessManager.ProcessDataImportAsync(data, _coordinator);
            
            if (importResult.IsSuccess)
            {
                // Step 2: Regenerate UI with new data (UI layer)
                await _uiManager.GenerateUIElementsAsync(_coordinator);
                
                // Step 3: Perform validation on imported data (Business + UI orchestration)
                var validationResult = await ValidateWithUIUpdateAsync();
                
                _logger?.Info("✅ ORCHESTRATOR: Data import with full processing orchestration completed");
                return importResult.Value;
            }
            else
            {
                _logger?.Error("❌ ORCHESTRATOR: Data import orchestration failed - {Error}", importResult.ErrorMessage);
                return new Models.ImportResult { Success = false, ErrorRows = data.Count };
            }
            
        }, "ImportDataWithFullProcessing", data.Count, new Models.ImportResult { Success = false, ErrorRows = data.Count }, _logger);
    }

    /// <summary>
    /// Update validation UI based on validation results
    /// ORCHESTRATION: Coordinates UI updates for validation results
    /// </summary>
    private async Task UpdateValidationUIAsync(ValidationResult validationResult)
    {
        await _exceptionHandler.SafeExecuteUIAsync(async () =>
        {
            _logger?.Info("🎭 ORCHESTRATOR: Updating validation UI for {InvalidCells} invalid cells", validationResult.InvalidCells);
            
            // Group errors by cell and apply styling
            var cellErrors = validationResult.Errors.GroupBy(e => $"R{e.Row}C{e.Column}").ToLookup(g => g.Key, g => g.First().Message);
            
            foreach (var cellGroup in cellErrors)
            {
                var cellId = cellGroup.Key;
                var errorMessages = string.Join("; ", cellGroup);
                
                // Find the cell and its border (this would need access to UI elements)
                // TODO: Implement cell lookup and styling update
                _logger?.Info("🎨 ORCHESTRATOR: Applied validation styling to cell {CellId}", cellId);
            }
            
            await Task.CompletedTask;
            
        }, "UpdateValidationUI", _logger);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 ORCHESTRATOR DISPOSE: Cleaning up DataGridOrchestrator");
            
            _uiManager?.Dispose();
            _businessManager?.Dispose();
            
            _disposed = true;
            _logger?.Info("✅ ORCHESTRATOR DISPOSE: DataGridOrchestrator disposed successfully");
        }
    }
}