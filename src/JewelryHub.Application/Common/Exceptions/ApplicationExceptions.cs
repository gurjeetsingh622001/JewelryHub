namespace JewelryHub.Application.Common.Exceptions;

/// <summary>Thrown when a requested entity doesn't exist. API middleware maps this to 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}

/// <summary>
/// Business-rule validation failure that isn't caught by FluentValidation
/// input rules (e.g. "email already registered", "GST number already in
/// use"). Distinct from FluentValidation.ValidationException, which the
/// pipeline behavior below throws for input-shape failures. API middleware
/// maps both to 400 with a field-level error payload.
/// </summary>
public class BusinessRuleException : Exception
{
    public string? Field { get; }

    public BusinessRuleException(string message, string? field = null) : base(message)
    {
        Field = field;
    }
}

/// <summary>Authenticated, but not allowed to perform this action (e.g. a Customer calling a Seller-only endpoint). Maps to 403.</summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message = null) : base(message ?? "You do not have permission to perform this action.") { }
}

/// <summary>Credentials were present but invalid, or a token is expired/revoked. Maps to 401.</summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message) { }
}
