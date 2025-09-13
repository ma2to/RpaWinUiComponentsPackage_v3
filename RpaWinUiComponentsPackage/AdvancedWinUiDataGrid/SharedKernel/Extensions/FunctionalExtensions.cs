using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Results;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Extensions;

/// <summary>
/// SHARED KERNEL: Functional programming extensions
/// RAILWAY ORIENTED PROGRAMMING: Extension methods for Result<T> chaining
/// </summary>
internal static class FunctionalExtensions
{
    /// <summary>
    /// FUNCTIONAL: Map extension for transforming successful results
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform)
    {
        return result.Map(transform);
    }

    /// <summary>
    /// FUNCTIONAL: Async map extension
    /// </summary>
    internal static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> transform)
    {
        return await result.MapAsync(transform);
    }

    /// <summary>
    /// FUNCTIONAL: Bind extension for monadic composition
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> transform)
    {
        return result.Bind(transform);
    }

    /// <summary>
    /// FUNCTIONAL: Async bind extension
    /// </summary>
    internal static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> transform)
    {
        return await result.BindAsync(transform);
    }

    /// <summary>
    /// FUNCTIONAL: OnSuccess side effect
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        return result.OnSuccess(action);
    }

    /// <summary>
    /// FUNCTIONAL: OnFailure side effect
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<string> action)
    {
        return result.OnFailure((error, _) => action(error));
    }

    /// <summary>
    /// FUNCTIONAL: Match pattern for handling both success and failure
    /// </summary>
    public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<string, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }
}