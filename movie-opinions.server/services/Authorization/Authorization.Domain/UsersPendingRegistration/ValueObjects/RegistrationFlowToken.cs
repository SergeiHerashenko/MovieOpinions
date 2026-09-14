using Authorization.Domain.Common.ValueObjects;
using Authorization.Domain.Results;
using Authorization.Domain.UsersPendingRegistration.Errors;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    /// <summary>
    /// Криптографічно випадковий непрозорий ідентифікатор
    /// реєстраційного потоку.
    ///
    /// (Cryptographically random opaque identifier
    /// of a registration flow.)
    /// </summary>
    public sealed class RegistrationFlowToken : FlowToken
    {
        private RegistrationFlowToken(string value)
            : base(value) { }

        #region Creation
        /// <summary>
        /// Генерує новий криптографічно випадковий токен
        /// реєстраційного потоку.
        ///
        /// (Generates a new cryptographically random registration-flow token.)
        /// </summary>
        internal static RegistrationFlowToken Create()
        {
            return new RegistrationFlowToken(GenerateValue());
        }
        #endregion

        #region Parse
        /// <summary>
        /// Перевіряє та перетворює отримане значення
        /// на RegistrationFlowToken.
        ///
        /// (Validates and parses the provided value
        /// into a RegistrationFlowToken.)
        /// </summary>
        /// <param name="rawRegistrationFlowToken">
        /// Необроблене закодоване значення токена.
        /// </param>
        /// <returns>
        /// Валідний токен або очікувану помилку довжини, формату чи порожнього значення.
        /// </returns>
        public static Result<RegistrationFlowToken> Parse(string? rawRegistrationFlowToken)
        {
            if (string.IsNullOrWhiteSpace(rawRegistrationFlowToken))
            {
                return Result<RegistrationFlowToken>.Failure(
                    RegistrationFlowTokenErrors.Empty<RegistrationFlowToken>()
                );
            }
            
            if (!HasExpectedLength(rawRegistrationFlowToken))
            {
                return Result<RegistrationFlowToken>.Failure(
                    RegistrationFlowTokenErrors.InvalidLength<RegistrationFlowToken>(
                        rawRegistrationFlowToken.Length,
                        ExpectedEncodedLength
                    )
                );
            }

            if (!HasValidFormat(rawRegistrationFlowToken))
            {
                return Result<RegistrationFlowToken>.Failure(
                    RegistrationFlowTokenErrors.InvalidFormat<RegistrationFlowToken>()
                );
            }

            return Result<RegistrationFlowToken>.Success(
                new RegistrationFlowToken(rawRegistrationFlowToken)
            );
        }
        #endregion
    }
}
