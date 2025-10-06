namespace AspNetIdentity.Model
{
    public class OidcSettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string TenantId { get; set; }
        public required string CallbackPath { get; set; }
    }
}
