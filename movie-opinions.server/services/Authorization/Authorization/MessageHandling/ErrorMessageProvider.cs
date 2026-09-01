using Authorization.MessageHandling.Localization;

namespace Authorization.MessageHandling
{
    public sealed class ErrorMessageProvider : IErrorMessageProvider
    {
        private readonly ILogger<ErrorMessageProvider> _logger;

        private static readonly IReadOnlyDictionary<string, string>
           _fallbackMessages = new Dictionary<string, string>
           {
               [SupportedCultures.Ukrainian] = "Не вдалося виконати запит.",
               [SupportedCultures.English] = "The request could not be completed."
           };

        public ErrorMessageProvider(
            ILogger<ErrorMessageProvider> logger)
        {
            _logger = logger;
        }

        public string GetErrorMessage(string errorCode, string culture)
        {
            var normalizedCulture = NormalizeCulture(culture);

            if (string.IsNullOrWhiteSpace(errorCode))
            {
                _logger.LogError(
                    "An empty error code was provided for localized message resolution!"
                );

                return GetFallbackMessage(normalizedCulture);
            }

            if (!ErrorMessageCatalog.Messages.TryGetValue(
                    errorCode,
                    out var translations))
            {
                _logger.LogError(
                    "Localized message mapping was not found for error code {ErrorCode}.",
                    errorCode
                );

                return GetFallbackMessage(normalizedCulture);
            }

            if(translations.TryGetValue(
                normalizedCulture,
                out var message))
            {
                return message;
            }

            if(normalizedCulture != SupportedCultures.Default)
            {
                _logger.LogWarning(
                    "Localized message for error code {ErrorCode} and culture {Culture} was not found. Default culture {DefaultCulture} will be used.",
                    errorCode,
                    normalizedCulture,
                    SupportedCultures.Default
                );

                if (translations.TryGetValue(
                    SupportedCultures.Default,
                    out var defaultMessage))
                {
                    return defaultMessage;
                }
            }

            _logger.LogError(
                "Default localized message was not found for error code {ErrorCode}.",
                errorCode
            );

            return GetFallbackMessage(normalizedCulture);
        }

        private static string NormalizeCulture(string culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
                return SupportedCultures.Default;

            var normalizedCulture = culture
                .Trim()
                .ToLowerInvariant();

            var language = normalizedCulture
                .Split('-', '_')[0];

            return SupportedCultures.All.Contains(language)
                ? language
                : SupportedCultures.Default;
        }

        private static string GetFallbackMessage(string culture)
        {
            if (_fallbackMessages.TryGetValue(
                    culture,
                    out var message))
            {
                return message;
            }

            return _fallbackMessages[SupportedCultures.Default];
        }
    }
}
