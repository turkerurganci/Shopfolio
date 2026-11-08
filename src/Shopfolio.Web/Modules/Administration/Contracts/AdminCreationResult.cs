using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The field a refused administrator creation names; the closed set of the command's three values (05 8.5.2). Plain constants, not a stored enum: nothing is persisted.</summary>
public static class AdminCreationFields
{
    public const string Name = "name";
    public const string Email = "email";
    public const string Password = "password";
}

/// <summary>
/// The outcome of opening an administrator account from the management command (06 8.3.1; 05 8.5.2): created, or refused with the field and the code. A refusal carries
/// no value the caller gave - neither the name, the address nor the password - so the command may write it as it is (09 13.2 item 4). A refusal writes nothing.
/// </summary>
public sealed class AdminCreationResult : IOperationResult
{
    private AdminCreationResult(Guid? adminId, string? field, ErrorCode? code)
    {
        AdminId = adminId;
        Field = field;
        Code = code;
    }

    public bool IsCreated => Code is null;

    /// <summary>The id of the new account; empty unless created.</summary>
    public Guid? AdminId { get; }

    /// <summary>The refused field; empty when created.</summary>
    public string? Field { get; }

    /// <summary>The refusal's code; empty when created.</summary>
    public ErrorCode? Code { get; }

    public bool IsRejected => !IsCreated;

    internal static AdminCreationResult Created(Guid adminId) => new(adminId, null, null);

    internal static AdminCreationResult Refused(string field, ErrorCode code) => new(null, field, code);
}
