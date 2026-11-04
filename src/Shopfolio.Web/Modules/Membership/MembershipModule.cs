using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>The Membership module's implementation of its open surface; internal, reached only through <see cref="IMembershipModule"/>.</summary>
internal sealed class MembershipModule : IMembershipModule
{
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddMembershipModule</c> once.</summary>
internal static class MembershipModuleRegistration
{
    public static IServiceCollection AddMembershipModule(this IServiceCollection services)
    {
        services.AddScoped<IMembershipModule, MembershipModule>();
        return services;
    }
}
