using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
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
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Пароль у відкритому вигляді, переданий для перевірки.
            ///
            /// (The plaintext password supplied for validation.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
