using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

/// <summary>
/// FUNCTIONAL: Railway-Oriented Programming for enterprise error handling
/// SENIOR DESIGN: Monadic pattern for composable, predictable error handling
/// THREAD-SAFE: Immutable value type for concurrent scenarios
/// 
/// BENEFITS:
/// - Eliminates exception-based error handling in business logic
/// - Enables functional composition and chaining
/// - Provides explicit success/failure states
/// - Thread-safe by design (readonly struct)
/// - Performance optimized (stack-allocated value type)
/// </summary>
public readonly struct Result<T>
{
    #region FUNCTIONAL: Immutable State
    
    private readonly T? _value;
    private readonly string? _error;
    private readonly Exception? _exception;
    private readonly bool _isSuccess;
    private readonly IReadOnlyList<ValidationError>? _validationErrors;
    
    #endregion
    
    #region FUNCTIONAL: Factory Methods
    
    /// <summary>
    /// Create successful result with value
    /// </summary>
    public static Result<T> Success(T value) => 
        new(value, null, null, true, null);
    
    /// <summary>
    /// Create failure result with error message
    /// </summary>
    public static Result<T> Failure(string error) => 
        new(default, error, null, false, null);
    
    /// <summary>
    /// Create failure result with exception
    /// </summary>
    public static Result<T> Failure(Exception exception) => 
        new(default, exception.Message, exception, false, null);
    
    /// <summary>
    /// Create failure result with error message and exception
    /// </summary>
    public static Result<T> Failure(string error, Exception exception) => 
        new(default, error, exception, false, null);
    
    /// <summary>
    /// Create failure result with validation errors
    /// </summary>
    public static Result<T> Failure(string error, IReadOnlyList<ValidationError> validationErrors) => 
        new(default, error, null, false, validationErrors);
    
    #endregion
    
    #region ENTERPRISE: Constructor
    
    private Result(T? value, string? error, Exception? exception, bool isSuccess, 
        IReadOnlyList<ValidationError>? validationErrors)
    {
        _value = value;
        _error = error;
        _exception = exception;
        _isSuccess = isSuccess;
        _validationErrors = validationErrors;
    }
    
    #endregion
    
    #region FUNCTIONAL: Properties
    
    /// <summary>Is operation successful</summary>
    public bool IsSuccess => _isSuccess;
    
    /// <summary>Is operation failed</summary>
    public bool IsFailure => !_isSuccess;
    
    /// <summary>Result value (only valid when IsSuccess)</summary>
    public T Value => _isSuccess ? _value! : 
        throw new InvalidOperationException($"Cannot access value of failed result: {_error}");
    
    /// <summary>Error message (only valid when IsFailure)</summary>
    public string Error => _error ?? "Unknown error";
    
    /// <summary>Exception (may be null even when IsFailure)</summary>
    public Exception? Exception => _exception;
    
    /// <summary>Validation errors collection</summary>
    public IReadOnlyList<ValidationError>? ValidationErrors => _validationErrors;
    
    #endregion
    
    #region FUNCTIONAL: Monadic Operations
    
    /// <summary>
    /// FUNCTIONAL: Map operation - transform success value
    /// COMPOSITION: Enables functional chaining
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> transform)
    {
        if (_isSuccess)
        {
            try
            {
                var result = transform(_value!);
                return Result<TOut>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure(ex);
            }
        }
        
        return Result<TOut>.Failure(_error!, _validationErrors);
    }
    
    /// <summary>
    /// FUNCTIONAL: Async map operation
    /// </summary>
    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> transform)
    {
        if (_isSuccess)
        {
            try
            {
                var result = await transform(_value!);
                return Result<TOut>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure(ex);
            }
        }
        
        return Result<TOut>.Failure(_error!, _validationErrors);
    }
    
    /// <summary>
    /// FUNCTIONAL: Bind operation - monadic composition
    /// RAILWAY: Enables railway-oriented programming
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> transform)
    {
        if (_isSuccess)
        {
            try
            {
                return transform(_value!);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure(ex);
            }
        }
        
        return Result<TOut>.Failure(_error!, _validationErrors);
    }
    
    /// <summary>
    /// FUNCTIONAL: Async bind operation
    /// </summary>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> transform)
    {
        if (_isSuccess)
        {
            try
            {
                return await transform(_value!);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure(ex);
            }
        }
        
        return Result<TOut>.Failure(_error!, _validationErrors);
    }
    
    /// <summary>
    /// FUNCTIONAL: Side effect on success
    /// ENTERPRISE: Logging, metrics, notifications
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (_isSuccess)
        {
            try
            {
                action(_value!);
            }
            catch
            {
                // Side effects should not break the pipeline
            }
        }
        return this;
    }
    
    /// <summary>
    /// FUNCTIONAL: Side effect on failure
    /// ENTERPRISE: Error logging, alerting
    /// </summary>
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (_isSuccess) return this;
        
        try
        {
            action(_error!, _exception);
        }
        catch
        {
            // Side effects should not break the pipeline
        }
        return this;
    }
    
    /// <summary>
    /// ENTERPRISE: Add validation errors to existing result
    /// </summary>
    public Result<T> WithValidationErrors(IReadOnlyList<ValidationError>? errors)
    {
        if (errors == null || !errors.Any()) return this;
        
        var combinedErrors = _validationErrors?.Concat(errors).ToList() ?? errors.ToList();
        return new Result<T>(_value, _error, _exception, _isSuccess, combinedErrors.AsReadOnly());
    }
    
    /// <summary>
    /// FUNCTIONAL: Provide fallback value on failure
    /// </summary>
    public T ValueOrDefault(T defaultValue) => _isSuccess ? _value! : defaultValue;
    
    /// <summary>
    /// FUNCTIONAL: Provide fallback value from factory on failure
    /// </summary>
    public T ValueOrDefault(Func<T> factory) => _isSuccess ? _value! : factory();
    
    #endregion
    
    #region ENTERPRISE: Combinators
    
    /// <summary>
    /// FUNCTIONAL: Combine two results - both must succeed
    /// ENTERPRISE: Parallel operation results
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> first, Result<T2> second)
    {
        if (first.IsSuccess && second.IsSuccess)
        {
            return Result<(T1, T2)>.Success((first.Value, second.Value));
        }
        
        var errors = new List<string>();
        var validationErrors = new List<ValidationError>();
        Exception? combinedException = null;
        
        if (first.IsFailure)
        {
            errors.Add(first.Error);
            if (first.ValidationErrors != null)
                validationErrors.AddRange(first.ValidationErrors);
            combinedException = first.Exception;
        }
        
        if (second.IsFailure)
        {
            errors.Add(second.Error);
            if (second.ValidationErrors != null)
                validationErrors.AddRange(second.ValidationErrors);
            combinedException = second.Exception ?? combinedException;
        }
        
        var combinedError = string.Join("; ", errors);
        var result = new Result<(T1, T2)>(default, combinedError, combinedException, false, 
            validationErrors.AsReadOnly());
        
        return result;
    }
    
    /// <summary>
    /// FUNCTIONAL: Execute operation safely with Result wrapper
    /// </summary>
    public static Result<T> Try(Func<T> operation)
    {
        try
        {
            var result = operation();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }
    
    /// <summary>
    /// FUNCTIONAL: Execute async operation safely with Result wrapper
    /// </summary>
    public static async Task<Result<T>> TryAsync(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }
    
    #endregion
    
    #region ENTERPRISE: Operators
    
    /// <summary>
    /// Implicit conversion from value to successful result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
    
    /// <summary>
    /// Implicit conversion from exception to failed result
    /// </summary>
    public static implicit operator Result<T>(Exception exception) => Failure(exception);
    
    #endregion
    
    #region ENTERPRISE: String Representation
    
    public override string ToString()
    {
        if (_isSuccess)
        {
            return $"Success: {_value}";
        }
        
        return $"Failure: {_error}";
    }
    
    #endregion
}

/// <summary>
/// ENTERPRISE: Validation error for business rule violations
/// DDD: Value object representing validation failure
/// </summary>
public record ValidationError(
    string Property,
    string Message,
    object? AttemptedValue = null,
    string? ErrorCode = null)
{
    // Compatibility properties for DataGrid usage
    public string ColumnName => Property;
    public string ErrorMessage => Message;
    public int RowIndex { get; init; } = -1;
    public string? ValidationRule { get; init; }
    public ValidationLevel Level { get; init; } = ValidationLevel.Error;
    
    public static ValidationError Create(string property, string message) =>
        new(property, message);
    
    public static ValidationError Create(string property, string message, object? attemptedValue) =>
        new(property, message, attemptedValue);
        
    public static ValidationError CreateForGrid(string columnName, string errorMessage, int rowIndex = -1, string? rule = null, ValidationLevel level = ValidationLevel.Error) =>
        new(columnName, errorMessage)
        {
            RowIndex = rowIndex,
            ValidationRule = rule,
            Level = level
        };
}

/// <summary>
/// ENTERPRISE: Collection of validation errors
/// FUNCTIONAL: Immutable collection for validation results
/// </summary>
public class ValidationResult
{
    public bool IsValid => !ValidationErrors.Any();
    public bool IsFailure => ValidationErrors.Any();
    public IReadOnlyList<ValidationError> ValidationErrors { get; }
    
    public ValidationResult(IReadOnlyList<ValidationError>? errors = null)
    {
        ValidationErrors = errors ?? Array.Empty<ValidationError>();
    }
    
    public static ValidationResult Valid() => new();
    public static ValidationResult Invalid(params ValidationError[] errors) => new(errors);
    public static ValidationResult Invalid(IEnumerable<ValidationError> errors) => new(errors.ToList().AsReadOnly());
}