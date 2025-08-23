namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Domain.Common;

/// <summary>
/// Railway-oriented programming Result type
/// FUNCTIONAL: Immutable result with success/failure semantics
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage, null);
    public static Result<T> Failure(Exception exception) => new(false, default, exception.Message, exception);
    public static Result<T> Failure(string errorMessage, Exception exception) => new(false, default, errorMessage, exception);
    
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Functional mapping - transform success value while preserving failure state
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess && Value != null 
            ? Result<TOut>.Success(mapper(Value)) 
            : Result<TOut>.Failure(ErrorMessage ?? "Mapping failed", Exception);
    }
    
    /// <summary>
    /// Functional binding - chain operations that can fail
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        return IsSuccess && Value != null 
            ? binder(Value) 
            : Result<TOut>.Failure(ErrorMessage ?? "Binding failed", Exception);
    }
    
    /// <summary>
    /// Match pattern - handle success and failure cases
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, Exception?, TOut> onFailure)
    {
        return IsSuccess && Value != null 
            ? onSuccess(Value) 
            : onFailure(ErrorMessage ?? "Unknown error", Exception);
    }
}

/// <summary>
/// Result without value - for operations that just succeed or fail
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private Result(bool isSuccess, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage, null);
    public static Result Failure(Exception exception) => new(false, exception.Message, exception);
    public static Result Failure(string errorMessage, Exception exception) => new(false, errorMessage, exception);
    
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Convert to Result<T>
    /// </summary>
    public Result<T> ToResult<T>(T value)
    {
        return IsSuccess 
            ? Result<T>.Success(value) 
            : Result<T>.Failure(ErrorMessage ?? "Conversion failed", Exception);
    }
}