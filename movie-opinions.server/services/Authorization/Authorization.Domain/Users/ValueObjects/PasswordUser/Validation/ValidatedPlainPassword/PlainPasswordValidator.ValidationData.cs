using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Містить пароль у відкритому вигляді та контекст операції,
        /// необхідні правилам валідації PlainPassword.
        ///
        /// (Contains the plaintext password and operation context
        /// required by the PlainPassword validation rules.)
        /// </summary>
        private sealed class PlainPasswordValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
