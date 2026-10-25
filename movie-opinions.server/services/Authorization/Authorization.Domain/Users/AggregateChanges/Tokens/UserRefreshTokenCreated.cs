using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    /// <summary>
    /// Фіксує новостворений refresh-токен користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records a newly created user refresh token
    /// as an aggregate change to be persisted.)
    /// </summary>
    public sealed class UserRefreshTokenCreated : AggregateChange
    {
        public UserRefreshToken UserRefreshToken { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження нового refresh-токена.
        ///
        /// (Creates an aggregate change for persisting a new refresh token.)
        /// </summary>
        /// <param name="userRefreshToken">
        /// Створений refresh-токен користувача.
        ///
        /// (The newly created user refresh token.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення токена.
        ///
        /// (The token creation time.)
        /// </param>
        public UserRefreshTokenCreated(
            UserRefreshToken userRefreshToken,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRefreshToken = userRefreshToken;
        }
    }
}
