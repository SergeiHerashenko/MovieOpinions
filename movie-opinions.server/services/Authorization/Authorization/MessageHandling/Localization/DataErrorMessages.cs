using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal class DataErrorMessages
    {
        private static readonly IReadOnlyDictionary<
            string,
            string> GenericServerError =
            new Dictionary<string, string>
            {
                [SupportedCultures.Ukrainian] = "Помилка сервера!",
                [SupportedCultures.English] = "Server error!"
            };

        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.Data.EmptyValue] = GenericServerError,
                [DomainErrorCodes.Data.InvalidFormat] = GenericServerError,
                [DomainErrorCodes.Data.UnsupportedType] = GenericServerError,
                [DomainErrorCodes.Data.OutOfRange] = GenericServerError,
            };
    }
}
