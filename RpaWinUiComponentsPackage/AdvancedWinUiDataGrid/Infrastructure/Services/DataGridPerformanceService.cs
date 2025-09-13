using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Infrastructure.Services;

/// <summary>
/// INFRASTRUCTURE: Performance monitoring and metrics service
/// CLEAN ARCHITECTURE: Infrastructure implementation of performance concerns
/// ENTERPRISE: Production-ready performance monitoring
/// </summary>
internal class DataGridPerformanceService : IDisposable
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, Stopwatch> _activeOperations;
    private readonly List<PerformanceMetric> _performanceHistory;
    private bool _disposed = false;

    public DataGridPerformanceService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activeOperations = new Dictionary<string, Stopwatch>();
        _performanceHistory = new List<PerformanceMetric>();
        
        _logger.LogDebug("DataGridPerformanceService initialized");
    }

    /// <summary>
    /// Start measuring operation performance
    /// </summary>
    public void StartOperation(string operationName)
    {
        if (_disposed) return;
        
        var stopwatch = Stopwatch.StartNew();
        _activeOperations[operationName] = stopwatch;
        
        _logger.LogTrace("Started measuring operation: {OperationName}", operationName);
    }

    /// <summary>
    /// Stop measuring operation and record metrics
    /// </summary>
    public TimeSpan StopOperation(string operationName)
    {
        if (_disposed) return TimeSpan.Zero;
        
        if (_activeOperations.TryGetValue(operationName, out var stopwatch))
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            
            var metric = new PerformanceMetric
            {
                OperationName = operationName,
                Duration = duration,
                Timestamp = DateTime.UtcNow
            };
            
            _performanceHistory.Add(metric);
            _activeOperations.Remove(operationName);
            
            _logger.LogTrace("Completed operation: {OperationName} in {Duration}ms", 
                operationName, duration.TotalMilliseconds);
            
            return duration;
        }
        
        _logger.LogWarning("Operation not found for stopping: {OperationName}", operationName);
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Get performance statistics
    /// </summary>
    public async Task<Result<GridPerformanceStatistics>> GetPerformanceStatisticsAsync()
    {
        if (_disposed) return Result<GridPerformanceStatistics>.Failure("Service disposed");
        
        try
        {
            var lastOpTime = _performanceHistory.LastOrDefault()?.Duration ?? TimeSpan.Zero;
            var avgTime = _performanceHistory.Count > 0 
                ? TimeSpan.FromTicks((long)_performanceHistory.Average(m => m.Duration.Ticks))
                : TimeSpan.Zero;
            var memUsage = GC.GetTotalMemory(false);
            var totalOps = _performanceHistory.Count;

            var performanceMetrics = PerformanceMetrics.Create(lastOpTime, memUsage, totalOps);
            
            var statistics = new GridPerformanceStatistics
            {
                ImportMetrics = performanceMetrics,
                ExportMetrics = performanceMetrics,
                SearchMetrics = performanceMetrics,
                ValidationMetrics = performanceMetrics,
                TotalMemoryUsage = memUsage,
                Uptime = TimeSpan.FromSeconds((DateTime.UtcNow - DateTime.Today).TotalSeconds)
            };
            
            return await Task.FromResult(Result<GridPerformanceStatistics>.Success(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance statistics");
            return Result<GridPerformanceStatistics>.Failure("Failed to get performance statistics", ex);
        }
    }

    /// <summary>
    /// Clear performance history
    /// </summary>
    public void ClearHistory()
    {
        if (_disposed) return;
        
        _performanceHistory.Clear();
        _logger.LogDebug("Performance history cleared");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var stopwatch in _activeOperations.Values)
            {
                stopwatch?.Stop();
            }
            
            _activeOperations.Clear();
            _performanceHistory.Clear();
            _disposed = true;
            
            _logger.LogDebug("DataGridPerformanceService disposed");
        }
    }
}

/// <summary>
/// Performance metric record
/// </summary>
internal record PerformanceMetric
{
    public string OperationName { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTime Timestamp { get; init; }
}