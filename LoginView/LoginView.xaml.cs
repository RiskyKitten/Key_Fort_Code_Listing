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
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Diagnostics.Eventing.Reader;

namespace Key_Fort.View
{
    //Main Class for Login window
    public partial class LoginView : Window
    {
        //Constructor for the login window
        public LoginView()
        {
            //Initializes the window
            InitializeComponent();

            //Creates a connection to the database
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();

            //Calls the create table function
            CreateTable(sqlite_conn);

            //Calls the logout function
            Logout(sqlite_conn);

            //Closes the connection to the database
            sqlite_conn.Dispose();
        }

        //Function for enabling the window to be moved from anywhere in the window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        //Callback function for the window minimize button
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Callback function for the window close button
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Callback function for the login button
        //Calls the login function
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            Login(sqlite_conn);

        }

        //Callback function for the register button
        //Calls the register function
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            Register(sqlite_conn);
        }

        //Callback function for the register button
        //Creates a new window for registration
        private void Register(SQLiteConnection conn)
        {
            NewAccountWindow main = new NewAccountWindow();
            main.Show();
        }

        //Function for creating a hash of the password
        //Takes in the rawData entered as a password and creates a hash of it using a byte array
        static string ComputeSha256Hash(string rawData)
        {
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
                //Returns the hashed password
                return builder.ToString();
            }
        }


        //Callback function for the login button
        //Takes the input from both input boxes
        //Checks for matching username in the database, if there is a match, check for password
        //If password matches, login
        //If password doesn't match, show error label informing the user incorrect details
        //If username doesn't exist in the database, offer to create account (opens new account window)
        private void Login(SQLiteConnection conn)
        {
            //Gets input from text boxes
            string enteredUsername = UserBox.Text;
            string enteredPassword = PassBox.Password;

            //Checks if the input is empty
            if (string.IsNullOrEmpty(enteredUsername) || string.IsNullOrEmpty(enteredPassword))
            {
                ErrorLabel.Content = "Please enter a username and password"; // Error message
                return;
            }

            if (enteredUsername[0] == '\'' || enteredPassword[0] == '\'')
            {
                ErrorLabel.Content = "' is an invalid character"; // Error message
                return;
            }

            using (SQLiteCommand cmd = conn.CreateCommand()) // Creates a new command
            {
                cmd.CommandText = "SELECT userId, Password FROM Users WHERE Username = @EnteredUsername";
                cmd.Parameters.AddWithValue("@EnteredUsername", enteredUsername); // Adds the entered username to the command

                using (SQLiteDataReader reader = cmd.ExecuteReader()) // Executes the command and creates a reader
                {
                    if (reader.Read()) // If the reader has a row, the username exists
                    {
                        int userId = reader.GetInt32(0); // Gets the user ID from the reader
                        string password = reader.GetString(1); // Gets the password from the reader

                        string hashedEnteredPassword = ComputeSha256Hash(enteredPassword); // Hashes the entered password

                        if (hashedEnteredPassword == password) // If the hashed entered password matches the password in the database, login
                        {
                            using (SQLiteCommand updateCmd = conn.CreateCommand()) // Creates a new command
                            {
                                updateCmd.CommandText = "UPDATE Users SET IsLoggedIn = 1 WHERE userID = @UserId";
                                updateCmd.Parameters.AddWithValue("@UserId", userId); // Adds the user ID to the command
                                updateCmd.ExecuteNonQuery(); // Executes the command
                            }
                            GrantAccess(); // Calls the GrantAccess function
                        }
                        else
                        {
                            ErrorLabel.Content = "Password Incorrect";
                        }
                    }
                    else
                    {
                        ErrorLabel.Content = "Username or password incorrect";

                        // If the username doesn't exist, ask the user if they want to create an account
                        MessageBoxResult result = MessageBox.Show("Username not found\nDo you want to create an account?", 
                            "KeyFort", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                        // If the user clicks OK, open the new account window
                        if (result == MessageBoxResult.OK)
                        {
                            // Open the new account window
                            NewAccountWindow main = new NewAccountWindow();
                            main.Show(); // Show the window
                        }
                    }
                }
            }
        }



        //Opens the main window
        //Closes the current open window (login window)
        public void GrantAccess()
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }


        //Function for creating SQL connection to the database
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
                Debug.WriteLine("Database Connection Error");
            }
            return sqlite_conn;
        }

        //Function for creating the database tables
        static void CreateTable(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;

            // Create users table
            string create_users = "CREATE TABLE IF NOT EXISTS Users (userID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "Username VARCHAR(50)," +
                "Password VARCHAR(50)," +
                "IsLoggedIn INTEGER," +
                "DateCreated  INTEGER," +
                "CollectionID INTEGER  REFERENCES Collections(CollectionID));";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = create_users;
            sqlite_cmd.ExecuteNonQuery();

            // Create logins table
            string create_logins = "CREATE TABLE IF NOT EXISTS Logins (LoginID INTEGER PRIMARY KEY AUTOINCREMENT," +
                "userID INTEGER REFERENCES Users(userID), " +
                "ItemName VARCHAR(50), " +
                "Username VARCHAR(50), " +
                "Password VARCHAR(50)," +
                "Email VARCHAR(50), " +
                "DateCreated INTEGER," +
                "CollectionID INTEGER REFERENCES Collections(CollectionID));";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = create_logins;
            sqlite_cmd.ExecuteNonQuery();

            // Create payments table
            string create_payments = "CREATE TABLE IF NOT EXISTS Payments (PaymentID INTEGER PRIMARY KEY AUTOINCREMENT," +
                "userID INTEGER REFERENCES Users (userID)," +
                "ItemName VARCHAR (50)," +
                "CardHolderName VARCHAR (50)," +
                "CardNumber INTEGER," +
                "SecurityCode INTEGER," +
                "ExpiryDate INTEGER," +
                "CollectionID INTEGER REFERENCES Collections (CollectionID)); ";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = create_payments;
            sqlite_cmd.ExecuteNonQuery();

            // Create collections table
            string create_collections = "CREATE TABLE IF NOT EXISTS Collections (CollectionID INTEGER PRIMARY KEY AUTOINCREMENT," +
                " userID INTEGER REFERENCES Users(userID)," +
                " CollectionName VARCHAR(50)); ";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = create_collections;
            sqlite_cmd.ExecuteNonQuery();
        }

        //Logout function
        //Sets all the LoggedIn parameters in the table to be 0
        static void Logout(SQLiteConnection conn)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();

            using (SQLiteCommand cmd = sqlite_conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET IsLoggedIn = 0";
                cmd.ExecuteNonQuery();
            }

            sqlite_conn.Dispose();
        }
    }
}