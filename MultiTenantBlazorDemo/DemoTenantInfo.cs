using Finbuckle.MultiTenant.Abstractions;

namespace MultiTenantBlazorDemo
{
    public class DemoTenantInfo : ITenantInfo
    {
        public string? Id { get; set; }
        public string? Identifier { get; set; }
        public string? Name { get; set; }
        public string? BaseAddress { get; set; }
        public string? ChallengeScheme { get; set; }
        public string? OpenIdConnectAuthority { get; set; }
        public string? OpenIdConnectClientId { get; set; }
        public string? OpenIdConnectClientSecret { get; set; }

        public string Authority => new Uri(new Uri(BaseAddress ?? string.Empty), Identifier ?? string.Empty).AbsoluteUri;
    }
}
