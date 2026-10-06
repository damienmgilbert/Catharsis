using System.Security.Claims;

namespace Catharsis.Security;

///<summary>
///A fluent builder assembling a <see cref="ClaimsPrincipal"/> from a single <see cref="ClaimsIdentity"/>, handy for
///tests and local-auth scenarios that need a principal without standing up a full authentication pipeline.
///</summary>
public sealed class ClaimsPrincipalBuilder
{
    #region Fields
    readonly List<Claim> _claims = [];
    string _authenticationType = "Custom";
    string _nameClaimType = ClaimTypes.Name;
    string _roleClaimType = ClaimTypes.Role;
    #endregion

    #region Public methods
    ///<summary>
    ///Sets the authentication type recorded on the built identity, which controls whether
    ///<see cref="ClaimsIdentity.IsAuthenticated"/> is <c>true</c>.
    ///</summary>
    ///<param name="authenticationType">The authentication type. Defaults to <c>"Custom"</c> if never set.</param>
    ///<exception cref="ArgumentException"><paramref name="authenticationType"/> is <c>null</c>, empty, or whitespace.</exception>
    public ClaimsPrincipalBuilder WithAuthenticationType(string authenticationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationType);
        _authenticationType = authenticationType;
        return this;
    }

    ///<summary>
    ///Adds an arbitrary claim.
    ///</summary>
    ///<param name="type">The claim type.</param>
    ///<param name="value">The claim value.</param>
    ///<exception cref="ArgumentException"><paramref name="type"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public ClaimsPrincipalBuilder WithClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(value);

        _claims.Add(new Claim(type, value));
        return this;
    }

    ///<summary>
    ///Adds a name claim, using whichever claim type was configured via <see cref="WithNameClaimType"/> (defaults to
    ///<see cref="ClaimTypes.Name"/>).
    ///</summary>
    ///<param name="name">The name.</param>
    ///<exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty, or whitespace.</exception>
    public ClaimsPrincipalBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithClaim(_nameClaimType, name);
    }

    ///<summary>
    ///Sets the claim type that <see cref="ClaimsIdentity.Name"/> is resolved from.
    ///</summary>
    ///<param name="nameClaimType">The name claim type. Defaults to <see cref="ClaimTypes.Name"/> if never set.</param>
    ///<exception cref="ArgumentException"><paramref name="nameClaimType"/> is <c>null</c>, empty, or whitespace.</exception>
    public ClaimsPrincipalBuilder WithNameClaimType(string nameClaimType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameClaimType);
        _nameClaimType = nameClaimType;
        return this;
    }

    ///<summary>
    ///Adds a role claim, using whichever claim type was configured via <see cref="WithRoleClaimType"/> (defaults to
    ///<see cref="ClaimTypes.Role"/>).
    ///</summary>
    ///<param name="role">The role.</param>
    ///<exception cref="ArgumentException"><paramref name="role"/> is <c>null</c>, empty, or whitespace.</exception>
    public ClaimsPrincipalBuilder WithRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return WithClaim(_roleClaimType, role);
    }

    ///<summary>
    ///Sets the claim type that <see cref="ClaimsPrincipal.IsInRole"/> checks against.
    ///</summary>
    ///<param name="roleClaimType">The role claim type. Defaults to <see cref="ClaimTypes.Role"/> if never set.</param>
    ///<exception cref="ArgumentException"><paramref name="roleClaimType"/> is <c>null</c>, empty, or whitespace.</exception>
    public ClaimsPrincipalBuilder WithRoleClaimType(string roleClaimType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleClaimType);
        _roleClaimType = roleClaimType;
        return this;
    }

    ///<summary>
    ///Adds a role claim for each role in <paramref name="roles"/>.
    ///</summary>
    ///<param name="roles">The roles to add.</param>
    ///<exception cref="ArgumentNullException"><paramref name="roles"/> is <c>null</c>.</exception>
    public ClaimsPrincipalBuilder WithRoles(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);

        foreach (string role in roles)
        {
            WithRole(role);
        }

        return this;
    }

    ///<summary>
    ///Builds the <see cref="ClaimsPrincipal"/> from every claim and setting configured so far.
    ///</summary>
    public ClaimsPrincipal Build()
    {
        ClaimsIdentity identity = new(_claims, _authenticationType, _nameClaimType, _roleClaimType);
        return new ClaimsPrincipal(identity);
    }
    #endregion
}
