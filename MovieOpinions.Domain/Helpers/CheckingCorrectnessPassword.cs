using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.Domain.Helpers
{
    public class CheckingCorrectnessPassword
    {
        public async Task<bool> VerifyPassword(string EnteredPassword, string PasswordKey, string StoredHash)
        {
            // Шифруємо введений пароль з використанням ключа (солі)
            string EnteredHash = await new HashPassword().GetHashedPassword(EnteredPassword, PasswordKey);
            // Порівнюємо отриманий хеш зі збереженим хешем
            return StoredHash.Equals(EnteredHash);
        }
    }
}
