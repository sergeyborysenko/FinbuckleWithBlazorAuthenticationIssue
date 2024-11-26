using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MultiTenantBlazorDemo
{
    public static class FakeOidcServer
    {
        public static void AddFakeOidc(this IServiceCollection services)
        {
            services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryClients([
                new Client
                {
                    ClientId = "test-client",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris = { "https://localhost:7217/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:7217/signout-callback-oidc" },
                    AllowedScopes = { "openid", "profile", "tenant" },
                    RequirePkce = true,
                    RequireConsent = false
                }
            ])
            .AddInMemoryIdentityResources(
            [
                new IdentityResource("openid", "Your user identifier", ["sub"]),
                new IdentityResource("profile", "Your profile information", ["name"]),
                new IdentityResource("tenant", "Tenant information", ["tenant"])

            ])
            .AddTestUsers([
                new TestUser
                {
                    SubjectId = "1",
                    Username = "user1",
                    Password = "password1",
                    Claims =
                    [
                        new Claim("name", "User One"),
                        new Claim("tenant", "tenant1")
                    ]
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "user2",
                    Password = "password2",
                    Claims =
                    [
                        new Claim("name", "User Two"),
                        new Claim("tenant", "tenant2")
                    ]
                }
            ]);
        }

        public static void UseFakeOidc(this IApplicationBuilder app)
        {
            app.UseIdentityServer();

        }

        public static WebApplication MapAuthenticationEndpoints(this WebApplication app)
        {
            app.MapGet("Account/Login", async (HttpContext httpContext, string returnUrl) =>
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, "Test User"),
                    new("sub", "testuser")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                httpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity)).Wait();
            });

            app.MapPost("account/login", async(HttpContext httpContext, string username, string password, string returnUrl) =>
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, "Test User"),
                    new("sub", "testuser")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                httpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity)).Wait();
            });

            return app;
        }
    }
}
