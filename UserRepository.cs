using System;
using System.Data.SqlClient;

namespace DiarySec.DiarySec.DataAccess
{
    public class Users
    {
        private SqlConnection connection;
        private string connectionString = @"Server=DESKTOP-35T25HC\SQLEXPRESS; Database=DiaryWork; Trusted_Connection=SSPI;";

        public Users()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        public bool UserInfo(string username, string password)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                int count = (int)command.ExecuteScalar();

                return count > 0;
            }
        }

        public bool AddUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Geçersiz kullanıcı adı veya şifre.");
                return false;
            }

            string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                int count = command.ExecuteNonQuery();

                return count > 0;
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }
    }
}
