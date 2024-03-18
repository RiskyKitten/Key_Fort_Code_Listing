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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Data;
using Key_Fort;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Security.RightsManagement;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Reflection.PortableExecutable;

namespace Key_Fort
{
    //Main class for the New Account window
    public partial class NewAccountWindow : Window
    {
        //Constructor for the New Account window
        public NewAccountWindow()
        {
            InitializeComponent();
            RegisterErrorLabel.Content = "";
        }

        //Callback function for the Register Button
        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn; // Create a new SQLite connection
            sqlite_conn = CreateConnection();

            Register(sqlite_conn); // Call the Register function
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        //Function that is called when the register button is clicked
        //Takes text from texboxes and returns to string
        //Checks for invalid characters/empty fields
        //Checks if username is already in use
        //Checks if passwords match
        //Checks if password is long enough
        //If all checks pass, create a new user in the database
        private void Register(SQLiteConnection conn)
        {
            string strEnteredUsername = RegisterUserBox.Text;
            string strEnteredPassword = RegisterPassBox.Password;
            string strEnteredSecondPassword = ReEnterPassBox.Password;


            using (SQLiteCommand sqlite_cmd = conn.CreateCommand())
            {
                // Check if the username or password is empty
                if (string.IsNullOrEmpty(strEnteredUsername) || string.IsNullOrEmpty(strEnteredPassword) || string.IsNullOrEmpty(strEnteredSecondPassword))
                {
                    RegisterErrorLabel.Content = "Please enter a username and password";
                    return;
                }

                // Check if the username or password contains an apostrophe
                if (strEnteredUsername[0] == '\'' || strEnteredPassword[0] == '\'' || strEnteredSecondPassword[0] == '\'')
                {
                    RegisterErrorLabel.Content = "' Is an invalid character";
                }
                else
                {
                    // Check if the username is already in use
                    sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE Username = @username";
                    sqlite_cmd.Parameters.AddWithValue("@username", strEnteredUsername);

                    // Execute the query and check if the username is already in use
                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        if (sqlite_datareader.Read())
                        {
                            // Notify the user that the username is in use
                            RegisterErrorLabel.Content = "That username is in use";
                            return;
                        }
                    }

                    using (SQLiteCommand insert_cmd = conn.CreateCommand())
                    {
                        // Insert the new user into the database
                        insert_cmd.CommandText = "INSERT INTO Users(Username, Password, IsLoggedIn, DateCreated) " +
                            "VALUES(@username, @password, @IsLoggedIn, @DateCreated)";
                        insert_cmd.Parameters.AddWithValue("@username", strEnteredUsername);

                        if (strEnteredPassword == strEnteredSecondPassword) // Check if the passwords match
                        {
                            if (strEnteredPassword.Length > 7) // Check if the password is long enough
                            {
                                string hashedPassword = ComputeSha256Hash(strEnteredPassword); // Hash the password
                                DateTime today = DateTime.Today; // Get the current date

                                // Add the new user to the database with the parameters
                                insert_cmd.Parameters.AddWithValue("@password", hashedPassword);
                                insert_cmd.Parameters.AddWithValue("@IsLoggedIn", 0);
                                insert_cmd.Parameters.AddWithValue("@DateCreated", today);
                                insert_cmd.Prepare();
                                insert_cmd.ExecuteNonQuery();

                                RegisterErrorLabel.Content = "New user created"; // Notify the user that the new user was created
                                RegisterUserBox.Text = ""; // Clear the textboxes
                                RegisterPassBox.Password = "";
                                ReEnterPassBox.Password = "";

                                this.Close(); // Close the window

                                MessageBox.Show("New user created", "KeyFort", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            else
                            {
                                // Password is too short
                                RegisterErrorLabel.Content = "Password must be 8 characters or more";
                            }
                        }
                        else
                        {
                            // Passwords don't match
                            RegisterErrorLabel.Content = "Passwords don't match";
                        }
                    }
                }
            }
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
