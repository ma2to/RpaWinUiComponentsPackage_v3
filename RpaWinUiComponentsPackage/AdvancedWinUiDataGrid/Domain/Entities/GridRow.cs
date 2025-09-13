using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Primitives;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.Entities;

/// <summary>
/// DDD: Rich domain entity representing a data grid row
/// ENTERPRISE: Encapsulates row behavior and business rules
/// </summary>
internal class GridRow : Entity
{
    private readonly Dictionary<string, object?> _data;
    private readonly List<string> _validationErrors;
    private readonly List<ValidationError> _validationErrorObjects;

    public GridRow(int id) : base(id)
    {
        _data = new Dictionary<string, object?>();
        _validationErrors = new List<string>();
        _validationErrorObjects = new List<ValidationError>();
        IsSelected = false;
        IsVisible = true;
        RowIndex = -1;
    }

    public int RowIndex { get; set; }
    public int Index => RowIndex; // Compatibility alias
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; }
    public bool IsValid => !HasValidationErrors;
    public bool HasValidationErrors => _validationErrors.Count > 0 || _validationErrorObjects.Count > 0;
    
    // Backward compatibility
    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();
    
    // Enhanced validation support
    public IReadOnlyList<ValidationError> ValidationErrorObjects => _validationErrorObjects.AsReadOnly();
    public IReadOnlyDictionary<string, object?> Data => _data;
    
    public object? GetValue(string columnName)
    {
        return _data.TryGetValue(columnName, out var value) ? value : null;
    }

    public void SetValue(string columnName, object? value)
    {
        _data[columnName] = value;
        // Domain event could be raised here
    }

    public void AddValidationError(string error)
    {
        if (!_validationErrors.Contains(error))
        {
            _validationErrors.Add(error);
        }
    }
    
    public void AddValidationError(ValidationError error)
    {
        if (!_validationErrorObjects.Any(e => e.Property == error.Property && e.Message == error.Message))
        {
            _validationErrorObjects.Add(error);
        }
    }
    
    public void SetValidationErrors(IEnumerable<ValidationError> errors)
    {
        _validationErrorObjects.Clear();
        foreach (var error in errors)
        {
            _validationErrorObjects.Add(error);
        }
    }

    public void ClearValidationErrors()
    {
        _validationErrors.Clear();
        _validationErrorObjects.Clear();
    }

    public Dictionary<string, object?> GetAllData()
    {
        return new Dictionary<string, object?>(_data);
    }

    public void SetAllData(Dictionary<string, object?> data)
    {
        _data.Clear();
        foreach (var kvp in data)
        {
            _data[kvp.Key] = kvp.Value;
        }
    }
}