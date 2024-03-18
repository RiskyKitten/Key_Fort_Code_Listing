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
using System.Diagnostics;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection.PortableExecutable;
using Key_Fort.View;

namespace Key_Fort
{
    //Main Class for Main window
    public partial class MainWindow : Window
    {
        //Constructor for the Main window
        public MainWindow()
        {
            InitializeComponent(); // Initializes the components of the window
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection(); // Creates a connection to the database

            login(sqlite_conn); // Calls the function to determine which user is logged in

            NavigateToPage1(); // Calls the function to switch to the default page (Logins page)
        }

        //Callback function for when the window is detected to be closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();

            // SQL statement to update the IsLoggedIn column in the Users table to 0 for the user currently logged in
            sqlite_cmd.CommandText = $"UPDATE Users SET IsLoggedIn = 0 WHERE IsLoggedIn = 1;";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }

        //Function that allows the window to be moved on the screen
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        //Function that detects when key combinations are pressed
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // If the control key and the L key are pressed, the function to switch to the Logins Page is called
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                NavigateToPage1(); // Calls the function to switch to the Logins Page
            }
            // If the control key and the P key are pressed, the function to switch to the Payments Page is called
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                NavigateToPage2(); // Calls the function to switch to the Payments Page
            }
            // If the control key and the G key are pressed, the function to switch to the Generator Page is called
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.G)
            {
                NavigateToPage3(); // Calls the function to switch to the Generator Page
            }
        }

        //Callback function for the window minimize button
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Callback function for the window close button
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();

            Application.Current.Shutdown();
        }

        //Function for deciding which user is logged in
        //SQL statement that selects the user currently logged in
        public void login(SQLiteConnection conn)
        {
            using (SQLiteCommand cmd = conn.CreateCommand()) // Creates a new command for the connection
            {
                cmd.CommandText = "SELECT Username FROM Users WHERE IsLoggedIn = 1;"; // SQL statement to select the user currently logged in

                using (SQLiteDataReader reader = cmd.ExecuteReader()) // Executes the SQL statement and reads the result
                {
                    if (reader.Read()) // If the reader has rows, the user is logged in and the label is updated
                    {
                        bool booHasRows = reader.HasRows;

                        if (booHasRows == true)
                        {
                            string CurrentTable = reader.GetString(0);
                            UserLabel.Text = CurrentTable;
                            reader.Close();
                        }
                        else if (booHasRows == false)
                        {
                            UserLabel.Text = "no user logged in"; // If no user is logged in, the label is updated to indicate this
                        }
                    }
                }
            }
        }

        //Function for switching to the Logins Page
        //Changes the background color of the button to indicate which page is currently active
        //Changes the content of the frame to the Logins Page
        private void NavigateToPage1()
        {
            MainFrame.Content = new LoginsPage();
            BtnLogins.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494d57"));
            BtnPayments.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
            BtnGenerator.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
        }

        //Function for switching to the Payments Page
        //Changes the background color of the button to indicate which page is currently active
        //Changes the content of the frame to the Payments Page
        private void NavigateToPage2()
        {
            MainFrame.Content = new PaymentsPage();
            BtnLogins.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
            BtnPayments.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494d57"));
            BtnGenerator.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
        }

        //Function for switching to the Generator Page
        //Changes the background color of the button to indicate which page is currently active
        //Changes the content of the frame to the Generator Page
        private void NavigateToPage3()
        {
            MainFrame.Content = new GeneratorPage();
            BtnLogins.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
            BtnPayments.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F343F"));
            BtnGenerator.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494d57"));
        }

        private void btnLogins_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage1(); // Calls the function to switch to the Logins Page
        }

        private void btnPayments_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage2(); // Calls the function to switch to the Payments Page
        }

        private void btnGeneratorClick(object sender, RoutedEventArgs e)
        {
            NavigateToPage3(); // Calls the function to switch to the Generator Page
        }


        private void btnlogout_Click(object sender, RoutedEventArgs e)
        {
            LoginView login = new LoginView(); // Creates a new instance of the LoginView
            login.Show(); // Shows the LoginView
            this.Close(); // Closes the current window
        }


        //Creates a connection to the database
        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            try  // Open the connection
            {
                sqlite_conn.Open();
            }
            catch (Exception)  // Catch any errors and message to user
            {
                Debug.WriteLine("Error connecting to the database");
            }
            return sqlite_conn;
        }
    }
}