using Authorization.Domain.Common.ValueObjects;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Errors;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects
{
    /// <summary>
    /// Криптографічно випадковий непрозорий токен,
    /// який ідентифікує потік підтвердження відкладеної дії користувача.
    ///
    /// (Cryptographically random opaque token that identifies
    /// a pending user-action confirmation flow.)
    /// </summary>
    public sealed class ConfirmationFlowToken : FlowToken
    {
        private ConfirmationFlowToken(string value)
            : base(value) { }

        #region Creation
        /// <summary>
        /// Генерує новий токен підтвердження відкладеної дії.
        ///
        /// (Generates a new pending-action confirmation token.)
        /// </summary>
        internal static ConfirmationFlowToken Create()
        {
            return new ConfirmationFlowToken(GenerateValue());
        }
        #endregion

        #region Parse
        /// <summary>
        /// Перевіряє та перетворює отримане значення
        /// на ConfirmationFlowToken.
        ///
        /// (Validates and parses the provided value
        /// into a ConfirmationFlowToken.)
        /// </summary>
        /// <param name="rawConfirmationFlowToken">
        /// Необроблене закодоване значення токена.
        /// </param>
        /// <returns>
        /// Валідний токен або очікувану помилку довжини, формату чи порожнього значення.
        /// </returns>
        public static Result<ConfirmationFlowToken> Parse(string? rawConfirmationFlowToken)
        {
            if (string.IsNullOrWhiteSpace(rawConfirmationFlowToken))
            {
                return Result<ConfirmationFlowToken>.Failure(
                    ConfirmationFlowTokenErrors.Empty<ConfirmationFlowToken>()
                );
            }

            if (!HasExpectedLength(rawConfirmationFlowToken))
            {
                return Result<ConfirmationFlowToken>.Failure(
                    ConfirmationFlowTokenErrors.InvalidLength<ConfirmationFlowToken>(
                        rawConfirmationFlowToken.Length,
                        ExpectedEncodedLength
                    )
                );
            }

            if (!HasValidFormat(rawConfirmationFlowToken))
            {
                return Result<ConfirmationFlowToken>.Failure(
                    ConfirmationFlowTokenErrors.InvalidFormat<ConfirmationFlowToken>()
                );
            }

            return Result<ConfirmationFlowToken>.Success(
                new ConfirmationFlowToken(rawConfirmationFlowToken)
            );
        }
        #endregion
    }
}
