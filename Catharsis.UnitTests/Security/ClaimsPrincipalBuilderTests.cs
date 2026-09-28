using Catharsis.Security;
using System.Security.Claims;

namespace Catharsis.UnitTests.Security;

///<summary>
///Unit tests for the <see cref="ClaimsPrincipalBuilder"/> class.
///</summary>
[TestClass]
public class ClaimsPrincipalBuilderTests
{
    #region WithClaim

    [TestMethod]
    public void WithClaim_NullType_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithClaim(null!, "value"));
    }

    [TestMethod]
    public void WithClaim_NullValue_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithClaim("type", null!));
    }

    [TestMethod]
    public void Build_WithClaim_IncludesClaimOnPrincipal()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithClaim("custom", "value").Build();
        Assert.AreEqual("value", principal.FindFirst("custom")?.Value);
    }

    #endregion

    #region WithName

    [TestMethod]
    public void WithName_NullName_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithName(null!));
    }

    [TestMethod]
    public void Build_WithName_SetsIdentityName()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithName("alice").Build();
        Assert.AreEqual("alice", principal.Identity!.Name);
    }

    #endregion

    #region WithRole / WithRoles

    [TestMethod]
    public void WithRole_NullRole_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithRole(null!));
    }

    [TestMethod]
    public void Build_WithRole_SatisfiesIsInRole()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithRole("admin").Build();
        Assert.IsTrue(principal.IsInRole("admin"));
    }

    [TestMethod]
    public void Build_WithRoles_SatisfiesIsInRoleForEachRole()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithRoles(["admin", "editor"]).Build();

        Assert.IsTrue(principal.IsInRole("admin"));
        Assert.IsTrue(principal.IsInRole("editor"));
    }

    [TestMethod]
    public void WithRoles_NullRoles_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithRoles(null!));
    }

    #endregion

    #region WithAuthenticationType

    [TestMethod]
    public void WithAuthenticationType_NullValue_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithAuthenticationType(null!));
    }

    [TestMethod]
    public void Build_DefaultAuthenticationType_IsAuthenticated()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithName("alice").Build();
        Assert.IsTrue(principal.Identity!.IsAuthenticated);
    }

    [TestMethod]
    public void Build_CustomAuthenticationType_IsReflectedOnIdentity()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithAuthenticationType("Bearer").Build();
        Assert.AreEqual("Bearer", principal.Identity!.AuthenticationType);
    }

    #endregion

    #region WithNameClaimType / WithRoleClaimType

    [TestMethod]
    public void WithNameClaimType_NullValue_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithNameClaimType(null!));
    }

    [TestMethod]
    public void Build_CustomNameClaimType_ResolvesIdentityName()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithNameClaimType("custom-name").WithName("alice").Build();
        Assert.AreEqual("alice", principal.Identity!.Name);
    }

    [TestMethod]
    public void WithRoleClaimType_NullValue_Throws()
    {
        ClaimsPrincipalBuilder builder = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => builder.WithRoleClaimType(null!));
    }

    [TestMethod]
    public void Build_CustomRoleClaimType_ResolvesIsInRole()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().WithRoleClaimType("custom-role").WithRole("admin").Build();
        Assert.IsTrue(principal.IsInRole("admin"));
    }

    #endregion

    #region Build

    [TestMethod]
    public void Build_NoClaimsConfigured_ReturnsUnauthenticatedEmptyPrincipal()
    {
        ClaimsPrincipal principal = new ClaimsPrincipalBuilder().Build();

        Assert.IsFalse(principal.Claims.Any());
    }

    #endregion
}
