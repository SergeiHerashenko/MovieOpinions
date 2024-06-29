using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Connect_Database
{
    public class ConnectMovieOpinions
    {
        // IP-адреса сервера бази даних /The IP address of the database server/
        public string Host { get; private set; } = "";

        // Ім'я користувача бази даних /Database username/
        public string User { get; private set; } = "posgres";

        // Пароль для підключення до бази даних /Password to connect to the database/
        public string Password { get; private set; } = "";

        // Порт підключення до бази даних /Database connection port/
        public string Port { get; private set; } = "";
        // Назва бази даних /The name of the database/
        public string DataBaseName { get; private set; } = "";

        // Рядок підключення до бази /Connection string to the database/
        public string ConnectMovieOpinionsDataBase()
        {
            string connString =
            string.Format(
                "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                Host,
                User,
                DataBaseName,
                Port,
                Password);

            return connString;
        }
    }
}
