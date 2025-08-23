namespace RpaWinUiComponentsPackage.Core.Functional;

/// <summary>
/// FUNCTIONAL: Result type for composable error handling
/// Implements monadic operations for clean functional composition
/// Used throughout the hybrid functional-OOP architecture
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly string? _errorMessage;
    private readonly Exception? _exception;
    private readonly bool _isSuccess;

    #region Constructors

    private Result(T? value, string? errorMessage, Exception? exception, bool isSuccess)
    {
        _value = value;
        _errorMessage = errorMessage;
        _exception = exception;
        _isSuccess = isSuccess;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Create successful result
    /// </summary>
    public static Result<T> Success(T value) => new(value, null, null, true);

    /// <summary>
    /// Create failure result with message
    /// </summary>
    public static Result<T> Failure(string errorMessage) => new(default, errorMessage, null, false);

    /// <summary>
    /// Create failure result with message and exception
    /// </summary>
    public static Result<T> Failure(string errorMessage, Exception exception) => 
        new(default, errorMessage, exception, false);

    /// <summary>
    /// Create failure result from exception
    /// </summary>
    public static Result<T> Failure(Exception exception) => 
        new(default, exception.Message, exception, false);

    #endregion

    #region Properties

    /// <summary>
    /// Is result successful
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Is result failure
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Result value (only valid if IsSuccess)
    /// </summary>
    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Cannot access value of failed result");

    /// <summary>
    /// Error message (only valid if IsFailure)
    /// </summary>
    public string ErrorMessage => _errorMessage ?? "Unknown error";

    /// <summary>
    /// Exception (may be null even if IsFailure)
    /// </summary>
    public Exception? Exception => _exception;

    #endregion

    #region Monadic Operations

    /// <summary>
    /// FUNCTIONAL: Monadic bind operation for composable error handling
    /// Allows chaining operations that may fail
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        if (_isSuccess)
        {
            try
            {
                return func(_value!);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure($"Bind operation failed: {ex.Message}", ex);
            }
        }
        
        return Result<TOut>.Failure(_errorMessage!, _exception);
    }

    /// <summary>
    /// FUNCTIONAL: Async monadic bind operation
    /// </summary>
    public async Task<Result<TOut>> Bind<TOut>(Func<T, Task<Result<TOut>>> func)
    {
        if (_isSuccess)
        {
            try
            {
                return await func(_value!);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure($"Async bind operation failed: {ex.Message}", ex);
            }
        }
        
        return Result<TOut>.Failure(_errorMessage!, _exception);
    }

    /// <summary>
    /// FUNCTIONAL: Map operation for transforming successful values
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        if (_isSuccess)
        {
            try
            {
                var result = func(_value!);
                return Result<TOut>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure($"Map operation failed: {ex.Message}", ex);
            }
        }
        
        return Result<TOut>.Failure(_errorMessage!, _exception);
    }

    /// <summary>
    /// FUNCTIONAL: Async map operation
    /// </summary>
    public async Task<Result<TOut>> Map<TOut>(Func<T, Task<TOut>> func)
    {
        if (_isSuccess)
        {
            try
            {
                var result = await func(_value!);
                return Result<TOut>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failure($"Async map operation failed: {ex.Message}", ex);
            }
        }
        
        return Result<TOut>.Failure(_errorMessage!, _exception);
    }

    /// <summary>
    /// FUNCTIONAL: Execute side effect if successful, return original result
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (_isSuccess)
        {
            try
            {
                action(_value!);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"Tap operation failed: {ex.Message}", ex);
            }
        }
        
        return this;
    }

    /// <summary>
    /// FUNCTIONAL: Async side effect
    /// </summary>
    public async Task<Result<T>> Tap(Func<T, Task> action)
    {
        if (_isSuccess)
        {
            try
            {
                await action(_value!);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"Async tap operation failed: {ex.Message}", ex);
            }
        }
        
        return this;
    }

    /// <summary>
    /// FUNCTIONAL: Provide default value if result is failure
    /// </summary>
    public T ValueOr(T defaultValue) => _isSuccess ? _value! : defaultValue;

    /// <summary>
    /// FUNCTIONAL: Provide default value from function if result is failure
    /// </summary>
    public T ValueOr(Func<T> defaultProvider) => _isSuccess ? _value! : defaultProvider();

    /// <summary>
    /// FUNCTIONAL: Execute action on failure, return original result
    /// </summary>
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (_isSuccess)
            return this;
            
        try
        {
            action(_errorMessage!, _exception);
        }
        catch
        {
            // Ignore exceptions in error handling
        }
        
        return this;
    }

    /// <summary>
    /// FUNCTIONAL: Execute action on success, return original result
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
                // Don't let side effects break the result chain
            }
        }
        
        return this;
    }

    #endregion

    #region Combinators

    /// <summary>
    /// FUNCTIONAL: Combine two results, both must succeed
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> result1, Result<T2> result2)
    {
        if (result1.IsSuccess && result2.IsSuccess)
        {
            return Result<(T1, T2)>.Success((result1.Value, result2.Value));
        }

        var errors = new List<string>();
        var exceptions = new List<Exception>();

        if (result1.IsFailure)
        {
            errors.Add(result1.ErrorMessage);
            if (result1.Exception != null)
                exceptions.Add(result1.Exception);
        }

        if (result2.IsFailure)
        {
            errors.Add(result2.ErrorMessage);
            if (result2.Exception != null)
                exceptions.Add(result2.Exception);
        }

        var combinedException = exceptions.Any() ? new AggregateException(exceptions) : null;
        return Result<(T1, T2)>.Failure(string.Join("; ", errors), combinedException);
    }

    /// <summary>
    /// FUNCTIONAL: Combine three results
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(Result<T1> result1, Result<T2> result2, Result<T3> result3)
    {
        return Combine(result1, result2)
            .Bind(tuple => result3.Map(val3 => (tuple.Item1, tuple.Item2, val3)));
    }

    /// <summary>
    /// FUNCTIONAL: Try operation and wrap in Result
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
    /// FUNCTIONAL: Async try operation
    /// </summary>
    public static async Task<Result<T>> Try(Func<Task<T>> operation)
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

    #region Collection Operations

    /// <summary>
    /// FUNCTIONAL: Traverse collection of Results, fail fast on first error
    /// </summary>
    public static Result<IReadOnlyList<T>> Traverse(IEnumerable<Result<T>> results)
    {
        var values = new List<T>();
        
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return Result<IReadOnlyList<T>>.Failure(result.ErrorMessage, result.Exception);
            }
            values.Add(result.Value);
        }
        
        return Result<IReadOnlyList<T>>.Success(values.AsReadOnly());
    }

    /// <summary>
    /// FUNCTIONAL: Sequence operations, collecting all errors
    /// </summary>
    public static Result<IReadOnlyList<T>> Sequence(IEnumerable<Result<T>> results)
    {
        var values = new List<T>();
        var errors = new List<string>();
        var exceptions = new List<Exception>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                values.Add(result.Value);
            }
            else
            {
                errors.Add(result.ErrorMessage);
                if (result.Exception != null)
                    exceptions.Add(result.Exception);
            }
        }

        if (errors.Any())
        {
            var combinedException = exceptions.Any() ? new AggregateException(exceptions) : null;
            return Result<IReadOnlyList<T>>.Failure(string.Join("; ", errors), combinedException);
        }

        return Result<IReadOnlyList<T>>.Success(values.AsReadOnly());
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Convert to nullable value
    /// </summary>
    public T? ToNullable() => _isSuccess ? _value : default;

    /// <summary>
    /// Convert to tuple
    /// </summary>
    public (bool Success, T? Value, string? Error) ToTuple() => (_isSuccess, _value, _errorMessage);

    /// <summary>
    /// Convert to option type
    /// </summary>
    public Option<T> ToOption() => _isSuccess ? Option<T>.Some(_value!) : Option<T>.None();

    #endregion

    #region Operators

    /// <summary>
    /// Implicit conversion from value to successful result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from exception to failed result
    /// </summary>
    public static implicit operator Result<T>(Exception exception) => Failure(exception);

    #endregion

    #region ToString

    public override string ToString()
    {
        if (_isSuccess)
        {
            return $"Success: {_value}";
        }
        
        return $"Failure: {_errorMessage}";
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for Result type
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Convert Task<T> to Task<Result<T>>
    /// </summary>
    public static async Task<Result<T>> ToResult<T>(this Task<T> task)
    {
        try
        {
            var result = await task;
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    /// Apply function to result value if successful
    /// </summary>
    public static async Task<Result<TOut>> Apply<T, TOut>(this Result<T> result, Func<T, Task<TOut>> func)
    {
        return await result.Map(func);
    }

    /// <summary>
    /// Filter result based on predicate
    /// </summary>
    public static Result<T> Where<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage = "Predicate failed")
    {
        if (result.IsFailure)
            return result;

        if (predicate(result.Value))
            return result;

        return Result<T>.Failure(errorMessage);
    }

    /// <summary>
    /// Ensure result meets condition
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
    {
        return result.Where(predicate, errorMessage);
    }

    /// <summary>
    /// Flatten nested results
    /// </summary>
    public static Result<T> Flatten<T>(this Result<Result<T>> nestedResult)
    {
        if (nestedResult.IsFailure)
            return Result<T>.Failure(nestedResult.ErrorMessage, nestedResult.Exception);

        return nestedResult.Value;
    }

    /// <summary>
    /// Extension method for Task<Result<T>> Bind operation
    /// </summary>
    public static async Task<Result<TOut>> Bind<T, TOut>(this Task<Result<T>> task, Func<T, Task<Result<TOut>>> func)
    {
        var result = await task;
        return await result.Bind(func);
    }

    /// <summary>
    /// Extension method for Task<Result<T>> synchronous Bind operation
    /// </summary>
    public static async Task<Result<TOut>> Bind<T, TOut>(this Task<Result<T>> task, Func<T, Result<TOut>> func)
    {
        var result = await task;
        return result.Bind(func);
    }
}

#endregion

#region Option Type (for completeness)

/// <summary>
/// FUNCTIONAL: Option type for representing optional values
/// Complements Result<T> for functional programming patterns
/// </summary>
public readonly struct Option<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Option(T? value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    public static Option<T> Some(T value) => new(value, true);
    public static Option<T> None() => new(default, false);

    public bool HasValue => _hasValue;
    public T Value => _hasValue ? _value! : throw new InvalidOperationException("Option has no value");

    public T ValueOr(T defaultValue) => _hasValue ? _value! : defaultValue;
    public T ValueOr(Func<T> defaultProvider) => _hasValue ? _value! : defaultProvider();

    public Option<TOut> Map<TOut>(Func<T, TOut> func) =>
        _hasValue ? Option<TOut>.Some(func(_value!)) : Option<TOut>.None();

    public Option<TOut> Bind<TOut>(Func<T, Option<TOut>> func) =>
        _hasValue ? func(_value!) : Option<TOut>.None();

    public Result<T> ToResult(string errorMessage = "Option has no value") =>
        _hasValue ? Result<T>.Success(_value!) : Result<T>.Failure(errorMessage);

    public static implicit operator Option<T>(T value) => Some(value);

    public override string ToString() => _hasValue ? $"Some({_value})" : "None";
}

#endregion