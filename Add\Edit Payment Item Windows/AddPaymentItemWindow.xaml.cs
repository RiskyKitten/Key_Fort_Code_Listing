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
    public partial class AddPaymentItemWindow : Window
    {
        //Constructor for the AddPaymentItemWindow
        public AddPaymentItemWindow()
        {
            InitializeComponent(); // Initialises the window

            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection(); // Creates a connection to the database
            UpdateCombobox(sqlite_conn); // Runs the UpdateCombobox function

            sqlite_conn.Dispose(); // Disposes of the connection
        }

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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionsLabel.Text = ("");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            NewItem(sqlite_conn);
        }

        static string EncryptString(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[aesAlg.BlockSize / 8];

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

        //Called when the save button is clicked
        //Validates the user input
        //Adds the new information into the database
        private void NewItem(SQLiteConnection conn)
        {
            string strEnteredName = ItemNameBox.Text;
            string strEnteredCardName = CardNameBox.Text;
            string strEnteredCardNum = CardNumberBox.Text;
            string strEnteredCode = SecurityCodeBox.Text;
            string strExpiryBox1 = ExpiryBox1.Text;
            string strExpiryBox2 = ExpiryBox2.Text;
            string strCombobox = ComboBox1.Text;

            //Combines the two expiry box strings into one
            string ExpiryBox = strExpiryBox1 + strExpiryBox2;

            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            string key = "8m$eJXb3!8pYEsm08YHpSQhMUC1nzpxk"; // Key for encryption

            //Encrypts the card number, security code and expiry date
            string encryptedCardNum = EncryptString(strEnteredCardNum, key);
            string encryptedCode = EncryptString(strEnteredCode, key); 
            string encryptedExpiry = EncryptString(ExpiryBox, key); 

            string empty = "";

            // Checks if any of the fields are empty
            if (strEnteredName == empty || strEnteredCardName == empty || strEnteredCardNum == empty || strEnteredCode == empty || strExpiryBox1 == empty || strExpiryBox2 == empty || strCombobox == empty)
            {
                RegisterErrorLabel.Text = ("Please complete fields");

            }
            else if (strEnteredName != empty || strEnteredCardName != empty || strEnteredCardNum != empty || strEnteredCode != empty || strExpiryBox1 != empty || strExpiryBox2 != empty || strCombobox != empty)
            {
                // Checks if any of the fields contain an apostrophe
                if (strEnteredName[0] == '\'' || strEnteredCardName[0] == '\'' || strEnteredCardNum[0] == '\'' || strEnteredCode[0] == '\'' || strExpiryBox1[0] == '\'' || strExpiryBox2[0] == '\'' || strCombobox[0] == '\'')
                {
                    RegisterErrorLabel.Text = ("' Is an invalid character");
                }
                else
                {   
                    sqlite_cmd = conn.CreateCommand();
                    sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE IsLoggedIn = '1'";

                    sqlite_datareader = sqlite_cmd.ExecuteReader();
                    sqlite_datareader.Read();

                    int intUserId = sqlite_datareader.GetInt32(0);
                    sqlite_datareader.Close();



                    sqlite_cmd = conn.CreateCommand();
                    sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{strCombobox}'";

                    sqlite_datareader = sqlite_cmd.ExecuteReader();
                    sqlite_datareader.Read();


                    bool booHasRows2 = sqlite_datareader.HasRows;

                    if (booHasRows2 == true)
                    {
                        sqlite_datareader.Close();
                        sqlite_cmd = conn.CreateCommand();
                        sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{strCombobox}'";

                        sqlite_datareader = sqlite_cmd.ExecuteReader();
                        sqlite_datareader.Read();

                        bool booHasRows1 = sqlite_datareader.HasRows;

                        if (booHasRows1 == true)
                        {
                            int CollectionID = sqlite_datareader.GetInt32(0); // Gets the CollectionID of the selected collection
                            sqlite_datareader.Close();
                            sqlite_cmd = conn.CreateCommand(); // Inserts the new item into the database
                            sqlite_cmd.CommandText = "INSERT INTO Payments(userID, ItemName, CardHolderName, CardNumber, SecurityCode," +
                                " ExpiryDate, CollectionID) VALUES(@userID, @ItemName, @CardName, @CardNum, @EnteredCode, @Expiry, @CollectionID)";

                            sqlite_cmd.Parameters.AddWithValue("@userID", intUserId); // Adds parameters to the query
                            sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
                            sqlite_cmd.Parameters.AddWithValue("@CardName", strEnteredCardName);
                            sqlite_cmd.Parameters.AddWithValue("@CardNum", encryptedCardNum);
                            sqlite_cmd.Parameters.AddWithValue("@EnteredCode", encryptedCode);
                            sqlite_cmd.Parameters.AddWithValue("@Expiry", encryptedExpiry);
                            sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
                            sqlite_cmd.Prepare();
                            sqlite_cmd.ExecuteNonQuery();

                            RegisterErrorLabel.Text = ("New item created"); // Displays a message to the user

                            ItemNameBox.Text = (""); // Clears the textboxes
                            CardNameBox.Text = ("");
                            CardNumberBox.Text = ("");
                            SecurityCodeBox.Text = ("");
                            ExpiryBox1.Text = ("");
                            ExpiryBox2.Text = ("");
                            ComboBox1.Text = ("");

                            sqlite_datareader.Close();
                            this.Close();
                        }
                        else
                        {
                            RegisterErrorLabel.Text = ("error"); // Displays an error message
                        }
                    }

                    else
                    {
                        sqlite_cmd = conn.CreateCommand();
                        sqlite_cmd.CommandText = "INSERT INTO Collections(userID, CollectionName) VALUES(@userID, @CollectionName)";

                        sqlite_cmd.Parameters.AddWithValue("@userID", intUserId);
                        sqlite_cmd.Parameters.AddWithValue("@CollectionName", strCombobox);

                        sqlite_cmd.Prepare();
                        sqlite_cmd.ExecuteNonQuery();
                        sqlite_datareader.Close();

                        sqlite_cmd = conn.CreateCommand();
                        sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{strCombobox}'";

                        sqlite_datareader = sqlite_cmd.ExecuteReader();
                        sqlite_datareader.Read();

                        int CollectionID = sqlite_datareader.GetInt32(0);
                        sqlite_datareader.Close();
                        sqlite_cmd = conn.CreateCommand();
                        sqlite_cmd.CommandText = "INSERT INTO Payments(userID, ItemName, CardHolderName, CardNumber, SecurityCode, ExpiryDate, CollectionID) VALUES(@userID, @ItemName, @CardName, @CardNum, @EnteredCode, @Expiry, @CollectionID)";


                        sqlite_cmd.Parameters.AddWithValue("@userID", intUserId);
                        sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
                        sqlite_cmd.Parameters.AddWithValue("@CardName", strEnteredCardName);
                        sqlite_cmd.Parameters.AddWithValue("@CardNum", strEnteredCardNum);
                        sqlite_cmd.Parameters.AddWithValue("@EnteredCode", strEnteredCode);
                        sqlite_cmd.Parameters.AddWithValue("@Expiry", strExpiryBox1);
                        sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
                        sqlite_cmd.Prepare();
                        sqlite_cmd.ExecuteNonQuery();


                        RegisterErrorLabel.Text = ("New item created");

                        ItemNameBox.Text = ("");
                        CardNameBox.Text = ("");
                        CardNumberBox.Text = ("");
                        SecurityCodeBox.Text = ("");
                        ExpiryBox1.Text = ("");
                        ExpiryBox2.Text = ("");
                        ComboBox1.Text = ("");

                        sqlite_datareader.Close();
                        this.Close();
                    }


                }



            }
        }

        //Function to only allow numbers to be entered into the CardNumberBox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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