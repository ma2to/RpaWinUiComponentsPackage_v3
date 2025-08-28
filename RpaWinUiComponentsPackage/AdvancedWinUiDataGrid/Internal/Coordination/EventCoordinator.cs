using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Extensions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Functional;
using Microsoft.UI.Xaml;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Coordination;

/// <summary>
/// PROFESSIONAL Event Coordinator - ONLY event registration and lifecycle management
/// RESPONSIBILITY: Handle event attachment/detachment, lifecycle management (NO event processing, NO business logic)
/// SEPARATION: Pure event management - registration patterns only
/// ANTI-GOD: Single responsibility - only event coordination
/// </summary>
internal sealed class EventCoordinator : IDisposable
{
    private readonly ILogger? _logger;
    private readonly GlobalExceptionHandler _exceptionHandler;
    private bool _disposed = false;

    // EVENT TRACKING (Pure registration pattern)
    private readonly Dictionary<FrameworkElement, List<EventAttachment>> _attachedEvents = new();
    private bool _eventsAttached = false;

    // EVENT ATTACHMENT RECORD (Immutable pattern)
    private readonly record struct EventAttachment(
        string EventName,
        Delegate Handler,
        DateTime AttachedAt
    );

    public EventCoordinator(
        ILogger? logger, 
        GlobalExceptionHandler exceptionHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        
        _logger?.Info("🔌 EVENT COORDINATOR: Initialized - Pure event registration only");
    }

    /// <summary>
    /// Get event attachment status
    /// PURE COORDINATOR: Only returns registration state
    /// </summary>
    public bool EventsAttached => _eventsAttached;

    /// <summary>
    /// Get count of attached events
    /// PURE COORDINATOR: Only returns attachment statistics
    /// </summary>
    public int AttachedEventCount => _attachedEvents.Values.Sum(list => list.Count);

    /// <summary>
    /// Attach event handler to UI element
    /// PURE COORDINATOR: Only manages event registration, no processing logic
    /// </summary>
    public async Task<Result<bool>> AttachEventAsync(FrameworkElement element, string eventName, Delegate handler)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔌 EVENT ATTACH: Attaching {EventName} to element {ElementType}", eventName, element.GetType().Name);
            
            // Track attachment
            if (!_attachedEvents.ContainsKey(element))
            {
                _attachedEvents[element] = new List<EventAttachment>();
            }
            
            var attachment = new EventAttachment(eventName, handler, DateTime.UtcNow);
            _attachedEvents[element].Add(attachment);

            // Perform actual event attachment
            AttachEventByName(element, eventName, handler);
            
            _logger?.Info("✅ EVENT ATTACH: {EventName} attached successfully", eventName);
            
            await Task.CompletedTask;
            return true;
            
        }, "AttachEvent", 1, false, _logger);
    }

    /// <summary>
    /// Attach multiple events to UI element in batch
    /// PURE COORDINATOR: Batch registration pattern
    /// </summary>
    public async Task<Result<int>> AttachEventBatchAsync(FrameworkElement element, IReadOnlyList<(string EventName, Delegate Handler)> events)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔌 EVENT BATCH ATTACH: Attaching {EventCount} events to {ElementType}", events.Count, element.GetType().Name);
            
            var attachedCount = 0;
            foreach (var (eventName, handler) in events)
            {
                var result = await AttachEventAsync(element, eventName, handler);
                if (result.IsSuccess)
                {
                    attachedCount++;
                }
                else
                {
                    _logger?.Warning("⚠️ EVENT BATCH: Failed to attach {EventName}", eventName);
                }
            }
            
            _logger?.Info("✅ EVENT BATCH ATTACH: Attached {AttachedCount}/{TotalCount} events", attachedCount, events.Count);
            return attachedCount;
            
        }, "AttachEventBatch", events.Count, 0, _logger);
    }

    /// <summary>
    /// Detach event handler from UI element
    /// PURE COORDINATOR: Only manages event deregistration
    /// </summary>
    public async Task<Result<bool>> DetachEventAsync(FrameworkElement element, string eventName)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔌 EVENT DETACH: Detaching {EventName} from element {ElementType}", eventName, element.GetType().Name);
            
            if (_attachedEvents.TryGetValue(element, out var eventList))
            {
                var attachment = eventList.FirstOrDefault(e => e.EventName == eventName);
                if (attachment != default)
                {
                    // Perform actual event detachment
                    DetachEventByName(element, eventName, attachment.Handler);
                    
                    // Remove from tracking
                    eventList.Remove(attachment);
                    
                    if (!eventList.Any())
                    {
                        _attachedEvents.Remove(element);
                    }
                    
                    _logger?.Info("✅ EVENT DETACH: {EventName} detached successfully", eventName);
                    await Task.CompletedTask;
                    return true;
                }
            }
            
            _logger?.Warning("⚠️ EVENT DETACH: {EventName} not found for detachment", eventName);
            await Task.CompletedTask;
            return false;
            
        }, "DetachEvent", 1, false, _logger);
    }

    /// <summary>
    /// Detach all events from UI element
    /// PURE COORDINATOR: Bulk deregistration pattern
    /// </summary>
    public async Task<Result<int>> DetachAllEventsAsync(FrameworkElement element)
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔌 EVENT DETACH ALL: Detaching all events from {ElementType}", element.GetType().Name);
            
            var detachedCount = 0;
            if (_attachedEvents.TryGetValue(element, out var eventList))
            {
                foreach (var attachment in eventList.ToList())
                {
                    DetachEventByName(element, attachment.EventName, attachment.Handler);
                    detachedCount++;
                }
                
                _attachedEvents.Remove(element);
            }
            
            _logger?.Info("✅ EVENT DETACH ALL: Detached {DetachedCount} events", detachedCount);
            
            await Task.CompletedTask;
            return detachedCount;
            
        }, "DetachAllEvents", 1, 0, _logger);
    }

    /// <summary>
    /// Detach all events from all elements
    /// PURE COORDINATOR: Global cleanup pattern
    /// </summary>
    public async Task<Result<int>> DetachGlobalEventsAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            _logger?.Info("🔌 EVENT GLOBAL DETACH: Detaching all events from all elements");
            
            var totalDetached = 0;
            foreach (var kvp in _attachedEvents.ToList())
            {
                var element = kvp.Key;
                var eventList = kvp.Value;
                
                foreach (var attachment in eventList)
                {
                    DetachEventByName(element, attachment.EventName, attachment.Handler);
                    totalDetached++;
                }
            }
            
            _attachedEvents.Clear();
            _eventsAttached = false;
            
            _logger?.Info("✅ EVENT GLOBAL DETACH: Detached {TotalCount} events globally", totalDetached);
            
            await Task.CompletedTask;
            return totalDetached;
            
        }, "DetachGlobalEvents", _attachedEvents.Count, 0, _logger);
    }

    /// <summary>
    /// Get event attachment statistics
    /// PURE COORDINATOR: Only reports registration state
    /// </summary>
    public async Task<Result<EventStatistics>> GetEventStatisticsAsync()
    {
        return await _exceptionHandler.SafeExecuteDataAsync(async () =>
        {
            var elementCount = _attachedEvents.Count;
            var totalEvents = _attachedEvents.Values.Sum(list => list.Count);
            var allEvents = _attachedEvents.Values.SelectMany(list => list).ToList();
            var oldestAttachment = allEvents.Count > 0 
                ? allEvents.Min(e => e.AttachedAt) 
                : DateTime.UtcNow;
            
            var stats = new EventStatistics(
                ElementsWithEvents: elementCount,
                TotalAttachedEvents: totalEvents,
                OldestAttachmentTime: oldestAttachment,
                EventsAttachedFlag: _eventsAttached
            );
            
            _logger?.Info("📊 EVENT STATS: Elements: {Elements}, Events: {Events}, Oldest: {Oldest}",
                stats.ElementsWithEvents, stats.TotalAttachedEvents, stats.OldestAttachmentTime);
            
            await Task.CompletedTask;
            return stats;
            
        }, "GetEventStatistics", _attachedEvents.Count, new EventStatistics(0, 0, DateTime.UtcNow, false), _logger);
    }

    #region Private Event Attachment Methods (Pure Registration)

    private void AttachEventByName(FrameworkElement element, string eventName, Delegate handler)
    {
        switch (eventName)
        {
            case "PointerPressed":
                element.PointerPressed += (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                break;
            case "PointerMoved":
                element.PointerMoved += (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                break;
            case "PointerReleased":
                element.PointerReleased += (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                break;
            case "KeyDown":
                element.KeyDown += (Microsoft.UI.Xaml.Input.KeyEventHandler)handler;
                break;
            case "KeyUp":
                element.KeyUp += (Microsoft.UI.Xaml.Input.KeyEventHandler)handler;
                break;
            case "GotFocus":
                element.GotFocus += (RoutedEventHandler)handler;
                break;
            case "LostFocus":
                element.LostFocus += (RoutedEventHandler)handler;
                break;
            case "RightTapped":
                element.RightTapped += (Microsoft.UI.Xaml.Input.RightTappedEventHandler)handler;
                break;
            case "DoubleTapped":
                element.DoubleTapped += (Microsoft.UI.Xaml.Input.DoubleTappedEventHandler)handler;
                break;
            case "PointerEntered":
                element.PointerEntered += (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                break;
            case "PointerExited":
                element.PointerExited += (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                break;
            default:
                _logger?.Warning("⚠️ EVENT ATTACH: Unknown event name {EventName}", eventName);
                break;
        }
    }

    private void DetachEventByName(FrameworkElement element, string eventName, Delegate handler)
    {
        try
        {
            switch (eventName)
            {
                case "PointerPressed":
                    element.PointerPressed -= (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                    break;
                case "PointerMoved":
                    element.PointerMoved -= (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                    break;
                case "PointerReleased":
                    element.PointerReleased -= (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                    break;
                case "KeyDown":
                    element.KeyDown -= (Microsoft.UI.Xaml.Input.KeyEventHandler)handler;
                    break;
                case "KeyUp":
                    element.KeyUp -= (Microsoft.UI.Xaml.Input.KeyEventHandler)handler;
                    break;
                case "GotFocus":
                    element.GotFocus -= (RoutedEventHandler)handler;
                    break;
                case "LostFocus":
                    element.LostFocus -= (RoutedEventHandler)handler;
                    break;
                case "RightTapped":
                    element.RightTapped -= (Microsoft.UI.Xaml.Input.RightTappedEventHandler)handler;
                    break;
                case "DoubleTapped":
                    element.DoubleTapped -= (Microsoft.UI.Xaml.Input.DoubleTappedEventHandler)handler;
                    break;
                case "PointerEntered":
                    element.PointerEntered -= (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                    break;
                case "PointerExited":
                    element.PointerExited -= (Microsoft.UI.Xaml.Input.PointerEventHandler)handler;
                    break;
                default:
                    _logger?.Warning("⚠️ EVENT DETACH: Unknown event name {EventName}", eventName);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "🚨 EVENT DETACH ERROR: Failed to detach {EventName}", eventName);
        }
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.Info("🔄 EVENT COORDINATOR DISPOSE: Cleaning up event registrations");
            
            var detachResult = DetachGlobalEventsAsync().GetAwaiter().GetResult();
            if (detachResult.IsSuccess)
            {
                _logger?.Info("✅ EVENT COORDINATOR DISPOSE: Detached {Count} events successfully", detachResult.Value);
            }
            
            _disposed = true;
            _logger?.Info("✅ EVENT COORDINATOR DISPOSE: Disposed successfully");
        }
    }
}

/// <summary>
/// Event attachment statistics record
/// </summary>
internal record EventStatistics(
    int ElementsWithEvents,
    int TotalAttachedEvents,
    DateTime OldestAttachmentTime,
    bool EventsAttachedFlag
);