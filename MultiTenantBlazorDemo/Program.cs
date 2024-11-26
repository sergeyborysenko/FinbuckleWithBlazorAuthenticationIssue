using Duende.IdentityServer.Extensions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MultiTenantBlazorDemo;
using MultiTenantBlazorDemo.Components;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddFakeOidc();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie("Cookies")
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "__invalid";
    options.ClientId = "__invalid";
    options.ClientSecret = "__invalid";
    options.RequireHttpsMetadata = false;
});

builder.Services.AddMultiTenant<DemoTenantInfo>()
    .WithRouteStrategy(tenantParam: "tenant")
    .WithDelegateStrategy((context) =>
    {
        var httpContext = (context as HttpContext)!;
        var referrer = httpContext.Request.Headers["Referer"].ToString();
        var tenant = referrer.Split('/').Skip(3).FirstOrDefault();
        if (tenant.IsNullOrEmpty() && httpContext.Request.Path.Equals("/_blazor") && httpContext.Request.Method.Equals("CONNECT", StringComparison.OrdinalIgnoreCase))
        {
           httpContext.Items.Add("__tenant____bypass_validate_principal__", true);
        }
        return Task.FromResult(tenant);
    })
    .WithConfigurationStore()
    .WithPerTenantAuthentication();

builder.Services.ConfigurePerTenant<OpenIdConnectOptions, DemoTenantInfo>(OpenIdConnectDefaults.AuthenticationScheme, (options, tenantInfo) =>
{
    options.Authority = tenantInfo.OpenIdConnectAuthority;
    options.ClientId = tenantInfo.OpenIdConnectClientId;

    options.ClientSecret = tenantInfo.OpenIdConnectClientSecret;
    options.RequireHttpsMetadata = false;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.ResponseMode = OpenIdConnectResponseMode.Query;
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("offline_access");
    options.MapInboundClaims = false;
    options.GetClaimsFromUserInfoEndpoint = true;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();


app.UseMultiTenant();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthenticationEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
