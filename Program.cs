using System;
using System.Data.SqlClient;
using System.Security;
using DiarySec.DiarySec.DataAccess;

namespace DiaryWork
{

    internal class Program
    {
        private static SqlConnection connection;

        private static void Main(string[] args)
        {
            connection = new SqlConnection(@"Server=DESKTOP-35T25HC\SQLEXPRESS; Database=DiaryWork; Trusted_Connection=SSPI;");
            connection.Open();

            Users users = new Users();

            while (true)
            {
                Console.WriteLine("Kullanıcı Girişi Yapmak İçin 1' e Basınız");
                Console.WriteLine("Yeni Kullanıcı Eklemek İçin 2' e Basınız");
                Console.WriteLine("Çıkış Yapmak için 3' e Basınız");

                int secim = Convert.ToInt32(Console.ReadLine());

                switch (secim)
                {
                    case 1:
                        if (UserLogin(users))
                        {
                            MainMenu(users);
                        }
                        break;
                    case 2:
                        AddUser(users);
                        break;
                    case 3:
                        users.CloseConnection();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Geçersiz seçenek. Tekrar deneyin.");
                        break;
                }
            }
        }

        public static void AddUser(Users users)
        {
            Console.WriteLine("Yeni Kullanıcı Adı: ");
            string newUsername = Console.ReadLine();

            Console.WriteLine("Yeni Şifre: ");
            SecureString newPasswordSecure = GetSecureInput();
            string newPassword = ConvertSecureStringToString(newPasswordSecure);

            if (users.AddUser(newUsername, newPassword))
            {
                Console.WriteLine("Yeni kullanıcı başarıyla eklendi.");
            }
            else
            {
                Console.WriteLine("Kullanıcı eklenemedi.");
            }
        }


        public static bool UserLogin(Users users)
        {
            Console.WriteLine("Kullanıcı Adınızı Giriniz");
            string username = Console.ReadLine();

            Console.WriteLine("Şifrenizi Giriniz");
            SecureString securePassword = GetSecureInput();
            string password = ConvertSecureStringToString(securePassword);

            return users.UserInfo(username, password);
        }

        public static void MainMenu(Users users)
        {
            while (true)
            {
                Console.WriteLine("***Günlük İşlemleri***");
                Console.WriteLine("Günlük Girişi için 1'e Basınız");
                Console.WriteLine("Günlük Silme işlemi İçin 2'e Basınız");
                Console.WriteLine("Günlük Seçme İşlemi ve Düzenleme İşlemi için 3'e Basınız");
                Console.WriteLine("Ana Menüye Dönmek İçin 4'e Basınız");

                int secim = Convert.ToInt32(Console.ReadLine());

                switch (secim)
                {
                    case 1:
                        DiaryEntries();
                        break;
                    case 2:
                        DiaryDelete();
                        break;
                    case 3:
                        DiarySelect();
                        break;
                    case 4:
                        return;
                    default:
                        Console.WriteLine("Geçersiz seçenek. Tekrar deneyin.");
                        break;
                }
                Console.ReadLine();
                Console.Clear();
            }
        }

        public static void DiaryEntries()
        {
            DateTime currentDate = DateTime.Now;
            string DiaryDate = currentDate.ToString("dd.MM.yyyy");

            using (SqlCommand checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = "SELECT COUNT(*) FROM DiaryEntries WHERE DiaryDate = @DiaryDate";
                checkCommand.Parameters.AddWithValue("@DiaryDate", DiaryDate);
                int existingCount = (int)checkCommand.ExecuteScalar();

                if (existingCount > 0)
                {
                    Console.WriteLine("Bir tarihe tek bir günlük yazabilirsiniz.");
                    return;
                }
            }

            Console.WriteLine("Günlüğünüzü Yazınız");
            string DiaryContent = Console.ReadLine();

            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = "INSERT INTO DiaryEntries(DiaryDate,DiaryContent) VALUES (@DiaryDate, @DiaryContent)";
                sqlCommand.Parameters.AddWithValue("@DiaryDate", DiaryDate);
                sqlCommand.Parameters.AddWithValue("@DiaryContent", DiaryContent);

                int count = sqlCommand.ExecuteNonQuery();

                if (count > 0)
                    Console.WriteLine("Günlük Girişi Başarılı");
                else
                    Console.WriteLine("Günlük Girişi Yapılamadı");
            }
        }


        public static void DiaryDelete()
        {
            Console.WriteLine("Silmek İstediğiniz Günlüğün Tarihini Giriniz");
            string diarydelete = Console.ReadLine();
            using (SqlCommand sqlcommand = connection.CreateCommand())
            {
                sqlcommand.CommandText = "DELETE FROM DiaryEntries WHERE DiaryDate = @DiaryDate";
                sqlcommand.Parameters.AddWithValue("@DiaryDate", diarydelete);
                int count = sqlcommand.ExecuteNonQuery();
                if (count > 0)
                    Console.WriteLine("Silme İşlemi Başarılı");
                else Console.WriteLine("Silme işlemi Yapılamadı");
            }
        }
        public static void DiarySelect()
        {
            Console.WriteLine("Başlangıç Tarihini Giriniz");
            string startDate = Console.ReadLine();
            Console.WriteLine("Bitiş Tarihini Giriniz");
            string endDate = Console.ReadLine();

            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = "SELECT DiaryDate, DiaryContent FROM DiaryEntries WHERE DiaryDate BETWEEN @StartDate AND @EndDate";
                sqlCommand.Parameters.AddWithValue("@StartDate", startDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", endDate);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"Tarih: {reader["DiaryDate"]}, İçerik: {reader["DiaryContent"]}");
                }

                reader.Close();
            }

            Console.WriteLine("Düzenlemek istediğiniz tarihi giriniz");
            string editDate = Console.ReadLine();

            Console.WriteLine($"Günlük içeriğini düzenlemek için yeni içeriği giriniz (Tarih: {editDate})");
            string newDiaryContent = Console.ReadLine();

            using (SqlCommand sqlCommand = connection.CreateCommand())
            {
                sqlCommand.CommandText = "UPDATE DiaryEntries SET DiaryContent = @NewDiaryContent WHERE DiaryDate = @EditDate";
                sqlCommand.Parameters.AddWithValue("@NewDiaryContent", newDiaryContent);
                sqlCommand.Parameters.AddWithValue("@EditDate", editDate);

                int count = sqlCommand.ExecuteNonQuery();

                if (count > 0)
                    Console.WriteLine("Günlük Düzenleme Başarılı");
                else
                    Console.WriteLine("Günlük Düzenleme Yapılamadı");
            }

            Console.ReadKey();
        }


        private static SecureString GetSecureInput()
        {
            SecureString secureString = new SecureString();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Enter)
                {
                    secureString.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            secureString.MakeReadOnly();
            return secureString;
        }

        private static string ConvertSecureStringToString(SecureString secureString)
        {
            IntPtr unmanagedPassword = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
            string password = System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedPassword);
            System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedPassword);

            return password;
        }
    }
}



