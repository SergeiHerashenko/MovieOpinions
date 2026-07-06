namespace Authorization.Infrastructure.Integration.Options
{
    public class ContactsServiceOptions
    {
        public const string SectionName = "Contacts";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
