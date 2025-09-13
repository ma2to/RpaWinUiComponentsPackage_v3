using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.SharedKernel.Exceptions;

/// <summary>
/// SHARED KERNEL: Base exception for domain business rule violations
/// DDD: Domain exception for business logic errors
/// </summary>
internal abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// SHARED KERNEL: Validation exception for domain validation failures
/// </summary>
internal class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// SHARED KERNEL: Business rule exception for domain rule violations
/// </summary>
internal class BusinessRuleException : DomainException
{
    public string RuleName { get; }
    
    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}