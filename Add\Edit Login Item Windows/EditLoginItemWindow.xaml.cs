using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data.SqlClient;
using System.Data;
using System.Reflection.Metadata;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel;

namespace Key_Fort
{
    public partial class EditLoginItemWindow : Window
    {
        //Global variable to store the loginId of the login item to be edited
        private string loginId;

        //Constructor for the EditLoginItemWindow
        public EditLoginItemWindow(string loginId)
        {
            this.loginId = loginId; // Stores the loginId
            InitializeComponent(); // Initializes the window
            SQLiteConnection sqlite_conn; // Creates a new SQLite connection
            sqlite_conn = CreateConnection();
            UpdateCombobox(sqlite_conn); // Calls the UpdateCombobox function
            PopulateTextBoxes(sqlite_conn); // Calls the PopulateTextBoxes function
        }

        //Function to update the combobox with the collections of the logged in user
        private void UpdateCombobox(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE IsLoggedIn = '1'";


            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            int intUserId = sqlite_datareader.GetInt32(0);
            sqlite_datareader.Close();


            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT CollectionName FROM Collections WHERE userID = '{intUserId}'";


            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();


            bool booHasRows1 = sqlite_datareader.HasRows;

            if (booHasRows1 == true)
            {
                do
                {
                    string strCollectionName = sqlite_datareader.GetString(0);
                    ComboBox1.Items.Add(strCollectionName);
                } while (sqlite_datareader.Read());
            }
            else
            {
                ComboBox1.Items.Add("Edit this to create new collection");
            }

            sqlite_datareader.Close();
        }

        //Function to populate the textboxes with the details of the login item to be edited
        private void PopulateTextBoxes(SQLiteConnection conn)
        {   
            SQLiteCommand sqlite_cmd; //Creates a new SQLite command
            SQLiteDataReader sqlite_datareader;

            // Gets the details of the login item to be edited
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT ItemName, Username, Password, Email FROM Logins WHERE loginId = '{loginId}'";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            string ItemName = sqlite_datareader.GetString(0); //Stores the details in variables
            string Username = sqlite_datareader.GetString(1);
            string Password = sqlite_datareader.GetString(2);
            string Email = sqlite_datareader.GetString(3);

            string key = "8m$eJXb3!8pYEsm08YHpSQhMUC1nzpxk"; // Encryption key
            string decryptedString = DecryptString(Password, key); // Decrypts the password

            ItemNameBox.Text = ItemName; //Populates the textboxes with the details
            EmailBox.Text = Email;
            Username_Box.Text = Username;
            Password_Box.Text = decryptedString;

            sqlite_datareader.Close();

            sqlite_cmd = conn.CreateCommand(); // Gets the collection name of the login item
            sqlite_cmd.CommandText = $"SELECT CollectionName FROM Collections WHERE CollectionId = " +
                $"(SELECT CollectionId FROM Logins WHERE loginId = '{loginId}')";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();
            
            string CollectionName = sqlite_datareader.GetString(0); // Stores the collection name

            ComboBox1.Text = CollectionName; // Sets the combobox to the collection name

            sqlite_datareader.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        //Event handler for the save button
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn; //Creates a new SQLite connection
            sqlite_conn = CreateConnection();
            ModifyItem(sqlite_conn); //Calls the ModifyItem function
        }

        //Function to encrypt a string
        static string EncryptString(string plainText, string key)
        {
            Debug.WriteLine("EncryptString called");
            Debug.WriteLine("plainText: " + plainText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[aesAlg.BlockSize / 8];
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        //Function to decrypt a string
        static string DecryptString(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create()) // Creates a new instance of the Aes class
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key); // Sets the key
                aesAlg.IV = new byte[aesAlg.BlockSize / 8]; // Sets the IV
                aesAlg.Padding = PaddingMode.PKCS7; // Sets the padding mode

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(); // Creates a decryptor

                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd(); // Returns the decrypted string
                }
            }
        }

        //Function to modify the login item
        //Deletes the old login item and inserts a new one with the updated details
        private void ModifyItem(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            string strEnteredName = ItemNameBox.Text; //Gets the entered details
            string strEnteredEmail = EmailBox.Text;
            string strEnteredUsername = Username_Box.Text;
            string strEnteredPassword = Password_Box.Text;
            string ComboBox = ComboBox1.Text;

            //Deletes the old login item
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"DELETE from Logins WHERE LoginID = '{loginId}'";
            sqlite_cmd.ExecuteNonQuery();

            //Gets the user ID of the logged in user
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE IsLoggedIn = '1'";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            // Stores the user ID as an integer
            int intUserId = sqlite_datareader.GetInt32(0);
            sqlite_datareader.Close();


            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{ComboBox}' " +
                $"AND userID = '{intUserId}'"; // Gets the collection ID of the selected collection
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            int CollectionID = sqlite_datareader.GetInt32(0); //Stores the collection ID
            sqlite_datareader.Close();

            //Inserts the new login item
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO Logins(LoginID, userID, ItemName, Username," +
                " Password, Email, DateCreated, CollectionID) VALUES(@LoginID, @userID, @ItemName, " +
                "@Username, @Password, @Email, @DateCreated, @CollectionID)";

            string key = "8m$eJXb3!8pYEsm08YHpSQhMUC1nzpxk"; //Encryption key
            string encryptedPassword = EncryptString(strEnteredPassword, key); //Encrypts the password

            DateTime today = DateTime.Today;

            sqlite_cmd.Parameters.AddWithValue("@LoginID", loginId); // Adds the parameters to the query
            sqlite_cmd.Parameters.AddWithValue("@userID", intUserId);
            sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
            sqlite_cmd.Parameters.AddWithValue("@Username", strEnteredUsername);
            sqlite_cmd.Parameters.AddWithValue("@Password", encryptedPassword);
            sqlite_cmd.Parameters.AddWithValue("@Email", strEnteredEmail);
            sqlite_cmd.Parameters.AddWithValue("@DateCreated", today);
            sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
            sqlite_cmd.Prepare();
            sqlite_cmd.ExecuteNonQuery();
            sqlite_datareader.Close();

            RegisterErrorLabel.Text = "Item Saved"; //Displays a message to the user
            System.Threading.Thread.Sleep(1000);
            this.Close();
        }

        private void btn_emailcopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(EmailBox.Text); //Copies the email to the clipboard
        }
        private void btn_usernamecopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Username_Box.Text); // Copies the username to the clipboard
        }
        private void btn_passwordcopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Password_Box.Text); // Copies the password to the clipboard
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Generates a random password
        //NOT IN DOC
        private string GeneratePassword()
        {
            Dictionary<string, string> CurrentCharSet = new Dictionary<string, string>();
            CurrentCharSet = new Dictionary<string, string>();

            string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string LowerChars = "abcdefghijklmnopqrstuvwxyz";
            string SpecialChars = "!@#$^&*()_+";
            string NumChars = "0123456789";

            CurrentCharSet.Add("UpperChars", "");
            CurrentCharSet.Add("LowerChars", "");
            CurrentCharSet.Add("SpecialChars", "");
            CurrentCharSet.Add("NumChars", "");

            int length = 16;

            CurrentCharSet["UpperChars"] = UpperChars;

            CurrentCharSet["LowerChars"] = LowerChars;

            CurrentCharSet["SpecialChars"] = SpecialChars;

            CurrentCharSet["NumChars"] = NumChars;

            string Temp = "";
            foreach (string key in CurrentCharSet.Keys)
            {
                Temp += CurrentCharSet[key];
            }
            Random random = new Random();

            string result = "";


            if (length <= 0)
            {
                return string.Empty;
            }
            if (length > Temp.Length)
            {
                return string.Empty;
            }

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(Temp.Length);
                result += Temp[index];
            }
            string strResult = result;

            return strResult.ToString();

        }

        //Event handler for the generate password button
        //NOT IN DOC
        private void btnGen_Click(object sender, RoutedEventArgs e)
        {
            Password_Box.Text = GeneratePassword();
        }

        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception)
            {

            }
            return sqlite_conn;
        }

    }
}
