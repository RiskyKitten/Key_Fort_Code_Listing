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
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;

namespace Key_Fort
{
    /// <summary>
    /// Interaction logic for AddItemWindow.xaml
    /// </summary>
    public partial class AddItemWindow : Window
    {
        //Main class for the AddItemWindow
        public AddItemWindow()
        {
            InitializeComponent(); // Initialize the window

            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection(); // Create a new SQLite connection
            UpdateCombobox(sqlite_conn); // Call the UpdateCombobox function

            sqlite_conn.Dispose(); // Dispose of the connection
        }

        //Function to update the combobox
        //Takes the list of collections from the collections table with the current user id
        //Adds the collection names to the combobox
        private void UpdateCombobox(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd; // Create a new SQLite command
            SQLiteDataReader sqlite_datareader;

            sqlite_cmd = conn.CreateCommand(); // Prepare the query to get the collection names
            sqlite_cmd.CommandText = $"SELECT CollectionName FROM Collections WHERE userID = " +
                $"(SELECT userId FROM Users WHERE IsLoggedIn = '1')";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            bool booHasRows1 = sqlite_datareader.HasRows; // Check if the query returned any rows

            if (booHasRows1 == true) // If the query returned rows...
            {
                do
                {
                    string strCollectionName = sqlite_datareader.GetString(0); // Get the collection name
                    ComboBox1.Items.Add(strCollectionName); // Add the collection name to the combobox
                } while (sqlite_datareader.Read());
            }
            else
            {
                ComboBox1.Items.Add("Edit this to create new collection"); // Add a default item to the combobox
            }

            sqlite_datareader.Close();
        }

        //Function to handle dragging the window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        //Event handler for closing the window
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the window
        }

        //not in doc
        private void EmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //Event handler for the save button
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn; // Create a new SQLite connection
            sqlite_conn = CreateConnection();
            NewItem(sqlite_conn); // Call the NewItem function
        }

        //Encryption function
        static string EncryptString(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create()) // Create a new instance of the Aes class
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key); // Set the key
                aesAlg.IV = new byte[aesAlg.BlockSize / 8]; // Set the IV
                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(); // Create an encryptor

                using (MemoryStream msEncrypt = new MemoryStream()) // Create a new memory stream
                {
                    // Create a CryptoStream using the memory stream and the encryptor
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText); // Write the plain text to the stream
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray()); // Return the encrypted text
                }
            }
        }

        //Called when the save button is clicked
        //Validates the user input
        //Adds the new information into the database
        private void NewItem(SQLiteConnection conn)
        {
            // Get the user input
            string strEnteredName = ItemNameBox.Text;
            string strEnteredEmail = EmailBox.Text;
            string strEnteredUsername = Username_Box.Text;
            string strEnteredPassword = Password_Box.Text;
            string ComboBox = ComboBox1.Text;

            string key = "8m$eJXb3!8pYEsm08YHpSQhMUC1nzpxk"; // Encryption key
            string encryptedPassword = EncryptString(strEnteredPassword, key); // Encrypt the password

            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            string empty = ""; // Create an empty string

            // Check if the fields are empty
            if (strEnteredName == empty || strEnteredEmail == empty || strEnteredUsername == empty || strEnteredPassword == empty || ComboBox == empty)
            {
                RegisterErrorLabel.Text = ("Please complete fields");

            }
            // If the fields are not empty...
            else if (strEnteredName != empty || strEnteredEmail != empty || strEnteredUsername != empty || strEnteredPassword != empty || ComboBox != empty)
            {

                if (strEnteredUsername[0] == '\'' || strEnteredPassword[0] == '\'' || strEnteredName[0] == '\'' || strEnteredEmail[0] == '\'')
                {
                    RegisterErrorLabel.Text = ("' Is an invalid character"); // Display a message to the user
                }
                else
                {
                    sqlite_cmd = conn.CreateCommand();
                    // Get the user id
                    sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE IsLoggedIn = '1'";

                    sqlite_datareader = sqlite_cmd.ExecuteReader();
                    sqlite_datareader.Read();

                    int intUserId = sqlite_datareader.GetInt32(0);
                    sqlite_datareader.Close();

                    sqlite_cmd = conn.CreateCommand();
                    // Check if the collection exists
                    sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{ComboBox}'";

                    sqlite_datareader = sqlite_cmd.ExecuteReader();
                    sqlite_datareader.Read();

                    bool booHasRows2 = sqlite_datareader.HasRows;

                    if (booHasRows2 == true)
                    {
                        sqlite_datareader.Close();
                        sqlite_cmd = conn.CreateCommand();
                        // Get the collection id
                        sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{ComboBox}'";

                        sqlite_datareader = sqlite_cmd.ExecuteReader();
                        sqlite_datareader.Read();

                        // Check if the collection exists
                        bool booHasRows1 = sqlite_datareader.HasRows;

                        if (booHasRows1 == true)
                        {
                            int CollectionID = sqlite_datareader.GetInt32(0); // Stores the CollectionID as an integer
                            sqlite_datareader.Close();
                            sqlite_cmd = conn.CreateCommand(); // Prepare the query to insert the new item into the database
                            sqlite_cmd.CommandText = "INSERT INTO Logins(userID, ItemName, Username, Password," +
                                " Email, DateCreated, CollectionID) " +
                                "VALUES(@userID, @ItemName, @Username, @Password, @Email, @DateCreated, @CollectionID)";

                            DateTime today = DateTime.Today;

                            // Add the parameters to the query
                            sqlite_cmd.Parameters.AddWithValue("@userID", intUserId);
                            sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
                            sqlite_cmd.Parameters.AddWithValue("@Username", strEnteredUsername);
                            sqlite_cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                            sqlite_cmd.Parameters.AddWithValue("@Email", strEnteredEmail);
                            sqlite_cmd.Parameters.AddWithValue("@DateCreated", today);
                            sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
                            sqlite_cmd.Prepare();
                            sqlite_cmd.ExecuteNonQuery();

                            RegisterErrorLabel.Text = ("New item created"); // Display a message to the user

                            ItemNameBox.Text = (""); // Clear the text boxes
                            EmailBox.Text = ("");
                            Username_Box.Text = ("");
                            Password_Box.Text = ("");

                            sqlite_datareader.Close();
                            this.Close();
                        }
                        else
                        {
                            RegisterErrorLabel.Text = ("error");
                        }
                    }

                    else
                    {
                        sqlite_cmd = conn.CreateCommand(); // Create a new collection
                        sqlite_cmd.CommandText = "INSERT INTO Collections(userID, CollectionName) VALUES(@userID, @CollectionName)";

                        sqlite_cmd.Parameters.AddWithValue("@userID", intUserId); // Add the user id to the collection
                        sqlite_cmd.Parameters.AddWithValue("@CollectionName", ComboBox); // Add the collection name to the collection

                        sqlite_cmd.Prepare();
                        sqlite_cmd.ExecuteNonQuery(); // Execute the command
                        sqlite_datareader.Close();

                        sqlite_cmd = conn.CreateCommand(); // Get the collection id
                        sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{ComboBox}'";

                        sqlite_datareader = sqlite_cmd.ExecuteReader();
                        sqlite_datareader.Read();

                        int CollectionID = sqlite_datareader.GetInt32(0); // Stores the CollectionID as an integer 
                        sqlite_datareader.Close();

                        sqlite_cmd = conn.CreateCommand(); // Insert the new item into the database
                        sqlite_cmd.CommandText = "INSERT INTO Logins(userID, ItemName, Username, Password," +
                            " Email, DateCreated, CollectionID) " +
                            "VALUES(@userID, @ItemName, @Username, @Password, @Email, @DateCreated, @CollectionID)";

                        DateTime today = DateTime.Today;

                        // Add the user id, item name, username, password, email, date created and collection id to the database
                        sqlite_cmd.Parameters.AddWithValue("@userID", intUserId);
                        sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
                        sqlite_cmd.Parameters.AddWithValue("@Username", strEnteredUsername);
                        sqlite_cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                        sqlite_cmd.Parameters.AddWithValue("@Email", strEnteredEmail);
                        sqlite_cmd.Parameters.AddWithValue("@DateCreated", today);
                        sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
                        sqlite_cmd.Prepare();
                        sqlite_cmd.ExecuteNonQuery();

                        RegisterErrorLabel.Text = ("New user created"); // Display a message to the user

                        ItemNameBox.Text = (""); // Clear the text boxes
                        EmailBox.Text = ("");
                        Username_Box.Text = ("");
                        Password_Box.Text = ("");
                        sqlite_datareader.Close();
                        this.Close();
                    }
                }
            }
        }


        //Function to generate a password
        //NOT IN DOC =========================================
        private string GeneratePassword()
        {
            Dictionary<string, string> CurrentCharSet = new Dictionary<string, string>();
            CurrentCharSet = new Dictionary<string, string>();

            string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string LowerChars = "abcdefghijklmnopqrstuvwxyz";
            string SpecialChars = "!@#$%^&*()_+";
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

        //Combobox event handler
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionsLabel.Text = (""); // Clear the label
        }

        //DB connection function
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

       

        //Event handler for the generate password button
        //NOT IN DOC (YET)
        private void btnGen_Click(object sender, RoutedEventArgs e)
        {
            Password_Box.Text = GeneratePassword();
        }
    }
}