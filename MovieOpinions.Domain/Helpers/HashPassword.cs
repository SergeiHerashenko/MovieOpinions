using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace MovieOpinions.Domain.Helpers
{
    public class HashPassword
    {
        public async Task<string> GetHashedPassword(string PasswordUser, string PasswordKey)
        {
            // Перетворюємо пароль та ключ в масив байтів
            byte[] PasswordBytes = Encoding.UTF8.GetBytes(PasswordUser + PasswordKey);
            // Обчислюємо хеш SHA-256 для об'єднаного масиву байтів паролю та ключа
            byte[] HashBytes = await Task.Run(() => new SHA256Managed().ComputeHash(PasswordBytes));
            // Перетворюємо масив байтів хешу в рядок Base64
            string HashedPassword = Convert.ToBase64String(HashBytes);
            // Повертаємо хешований пароль у вигляді рядка
            return HashedPassword;
        }
    }
}
