using Authorization.Application.Common.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.DTOs.Communication
{
    public class VerificationCommand
    {
        public UserId UserId { get; }

        public MessageActions MessageActions { get; }

        public string Code { get; }

        private VerificationCommand(UserId userId, MessageActions messageActions, string code)
        {
            UserId = userId;
            MessageActions = messageActions;
            Code = code;
        }

        public static VerificationCommand Create(UserId userId, MessageActions messageActions, string code)
            => new(userId, messageActions, code);
    }
}
