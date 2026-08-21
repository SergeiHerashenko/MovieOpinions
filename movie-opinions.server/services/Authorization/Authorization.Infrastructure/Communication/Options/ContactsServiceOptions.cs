namespace Authorization.Infrastructure.Communication.Options
{
    public class ContactsServiceOptions
    {
        public const string SectionName = "Contacts";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;

        public string GetActiveEndpoint {  get; set; } = string.Empty;
    }
}
