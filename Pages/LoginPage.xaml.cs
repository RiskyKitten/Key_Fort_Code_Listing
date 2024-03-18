using Microsoft.EntityFrameworkCore.Sqlite.Internal;
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
using System.Data.SQLite;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using Microsoft.Data.Sqlite;
using System.Reflection;
using System.Xml;

namespace Key_Fort
{
    //Main class for the login page
    public partial class LoginsPage : Page
    {
        //Variable to keep track of the state of the drop down button
        public string btnToggled = "false";
        //Constructor for the login page
        public LoginsPage()
        {
            InitializeComponent(); // Initialize the page

            SQLiteConnection sqlite_conn; // Create a new connection to the database
            sqlite_conn = CreateConnection();
            
            CreateLabelsAndButtons(); // Call the CreateLabelsAndButtons function
            sqlite_conn.Close(); // Close the connection to the database
            sqlite_conn.Dispose(); // Dispose of the connection
        }

        public void RefreshPage()
        {
            CreateLabelsAndButtons();
        }

        //Method that is called when the add button is clicked
        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {  
            AddItemWindow main = new AddItemWindow();
            main.Show();
        }

        //Function to populate the table with the items from the database
        //This method is called when the page is loaded
        //Create buttons and labels iteratively
        //Every time the page is loaded, the buttons and labels are cleared and re-created
        //The buttons and labels are put into a grid, which is also created iteratively
        private void CreateLabelsAndButtons()
        {
            Grid grid = mainGrid; 

            mainGrid.Children.Clear();

            List<Tuple<string, string>> itemInfoList = GetItemInfoFromDatabase();

            //Create labels and buttons iteratively
            for (int i = 0; i < itemInfoList.Count; i++)
            {
                string itemName = itemInfoList[i].Item1;
                string collectionName = itemInfoList[i].Item2;

                // Create button for item name
                // Assigns properties to the item name button
                Button nameButton = new Button();
                nameButton.FontSize = 18;
                nameButton.FontWeight = FontWeights.Bold;
                nameButton.Foreground = Brushes.White;
                nameButton.VerticalAlignment = VerticalAlignment.Center;
                nameButton.FontFamily = new FontFamily("Montserrat");
                nameButton.Height = 30;
                nameButton.Padding = new Thickness(0, 10, 0, 0);
                nameButton.Background = Brushes.Transparent;
                nameButton.BorderThickness = new Thickness(0, 0, 0, 0);
                nameButton.Tag = itemName;
                nameButton.Content = CreateUnderlinedTextBlock(itemName);
                nameButton.HorizontalAlignment = HorizontalAlignment.Left;
                nameButton.VerticalAlignment = VerticalAlignment.Center;
                nameButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                nameButton.Click += EditButton_Click;

                // Create label for collection name
                // Assigns properties to the collection label
                Label collectionLabel = new Label();
                collectionLabel.Content = collectionName;
                collectionLabel.FontSize = 16;
                collectionLabel.FontWeight = FontWeights.Bold;
                collectionLabel.Foreground = Brushes.White;
                collectionLabel.VerticalAlignment = VerticalAlignment.Center;
                collectionLabel.FontFamily = new FontFamily("Montserrat");
                collectionLabel.Height = 30;
                collectionLabel.Padding = new Thickness(82, 10, 0, 0);

                // Create edit button
                // Assigns properties to the edit button
                Button editButton = new Button();
                editButton.Content = "Edit";
                editButton.FontSize = 14;
                editButton.Tag = itemName;
                editButton.Click += EditButton_Click;
                editButton.Margin = new Thickness(0, 10, 0, 0);
                editButton.Padding = new Thickness(0,0,5,0);
                editButton.Width = 50;
                editButton.Height = 30;
                editButton.VerticalAlignment = VerticalAlignment.Center;
                editButton.HorizontalContentAlignment = HorizontalAlignment.Center;

                // Create delete button
                // Assigns properties to the delete button
                Button deleteButton = new Button();
                deleteButton.FontSize = 14;
                deleteButton.Tag = itemName;
                deleteButton.Click += DeleteButton_Click;
                deleteButton.Margin = new Thickness(10, 10, 0, 0);
                deleteButton.Padding = new Thickness(0,0,0,0);
                deleteButton.Width = 30;
                deleteButton.Height = 30;
                deleteButton.VerticalAlignment = VerticalAlignment.Center;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Left;

                //Put trashcan icon in delete button
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/trash-can.png")); 
                icon.Width = 23;
                icon.Height = 23;

                deleteButton.Content = new StackPanel // StackPanel to hold the icon
                {
                    Orientation = Orientation.Horizontal, // Horizontal orientation
                    Children =
                    {
                        icon // Add the icon to the StackPanel
                    }
                };

                //Assign the buttons and labels to the grid rows
                mainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
                Grid.SetRow(nameButton, i);
                Grid.SetColumn(nameButton, 0);
                Grid.SetRow(collectionLabel, i);
                Grid.SetColumn(collectionLabel, 1);
                Grid.SetRow(editButton, i);
                Grid.SetColumn(editButton, 2);
                Grid.SetRow(deleteButton, i);
                Grid.SetColumn(deleteButton, 3);

                //Add buttons and labels to grid
                mainGrid.Children.Add(nameButton);
                mainGrid.Children.Add(collectionLabel);
                mainGrid.Children.Add(editButton);
                mainGrid.Children.Add(deleteButton);
            }
        }

        //Function that underlines the text in the item button
        private TextBlock CreateUnderlinedTextBlock(string text)
        {
            TextBlock textBlock = new TextBlock(); // Create a new TextBlock
            textBlock.Text = text; // Set the text of the TextBlock

            // Create a new TextDecorationCollection and add the Underline TextDecoration to it
            TextDecorationCollection textDecorations = new TextDecorationCollection();
            textDecorations.Add(TextDecorations.Underline); 
            
            // Set the TextDecorations property of the TextBlock to the TextDecorationCollection
            textBlock.TextDecorations = textDecorations; 

            return textBlock; // Return the TextBlock
        }

        //Function that is called when the edit button is clicked
        //Opens the edit item window
        //Pass current loginId to the edit item window
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender; // Get the button that was clicked
            
            // Get the loginId from the database
            string loginId = GetLoginIdFromDatabase(clickedButton.Tag.ToString());

            // Open the edit item window and pass the loginId to it
            EditLoginItemWindow editWindow = new EditLoginItemWindow(loginId);
            editWindow.Show(); // Show the edit item window

        }

        //Function that is called when the delete button is clicked
        //Opens a message box to confirm the deletion
        //If yes, call the deteleItem function
        //Else, close the message box and do nothing
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn; // Create a new connection to the database
            sqlite_conn = CreateConnection();

            Button clickedButton = (Button)sender; // Get the button that was clicked
            string loginId = GetLoginIdFromDatabase(clickedButton.Tag.ToString()); // Get the loginId from the database

            // Create a message box to confirm the deletion
            string messageBoxText = "Are you sure you want to delete this item?"; // Create a message box to confirm the deletion
            string caption = "KeyFort";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Exclamation;
            MessageBoxResult result;
            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            
            switch (result) // Switch statement to handle the result of the message box
            {
                case MessageBoxResult.Yes: 
                    deleteItem(loginId); // Call the deleteItem function
                    break;
                case MessageBoxResult.No:
                    break; // Close the messagebox and do nothing if no
            }
        }

        //Function to delete an item from the database
        //Takes the loginId as a parameter
        //Deletes the item with that loginId from the database
        private void deleteItem(string loginId)
        {
            using (SQLiteConnection conn = CreateConnection())
            {
                try
                {
                    using (SQLiteCommand sqlite_cmd = conn.CreateCommand()) // Create a new command to execute the query
                    {
                        string Createsql = "DELETE FROM Logins WHERE LoginID = @LoginId"; // Create a new query to delete the item from the database
                        sqlite_cmd.CommandText = Createsql;
                        sqlite_cmd.Parameters.AddWithValue("@LoginId", loginId); // Add the parameter to the query
                        sqlite_cmd.ExecuteNonQuery();
                    }

                    // Display a message box to inform the user that the item was deleted successfully
                    MessageBox.Show("Item deleted successfully.", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    CreateLabelsAndButtons(); // Refresh the page
                }
                catch (Exception ex) // Catch any exceptions that occur
                {
                    // Display a message box to inform the user that an error occurred while deleting the item
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        //Function to get the loginId from the database
        private string GetLoginIdFromDatabase(string itemName)
        {
            using (SQLiteConnection connection = CreateConnection()) // Create a new connection to the database
            {
                if (connection.State == ConnectionState.Open) // If the connection is open
                {
                    // Create a new query to get the loginId from the database
                    string sqlQuery = "SELECT LoginId FROM Logins WHERE ItemName = @LoginName"; 

                    using (SQLiteCommand command = new SQLiteCommand(sqlQuery, connection)) // Create a new command to execute the query
                    {
                        command.Parameters.AddWithValue("@LoginName", itemName); // Add the parameter to the query

                        object result = command.ExecuteScalar(); // Execute the query and store the result in a variable
                        if (result != null)
                        {
                            return result.ToString(); // Return the result
                        }
                    }
                }
            }

            return null; // Return null if the loginId is not found
        }

        //Function to get the item info from the database
        //Returns a list of tuples with the item name and collection name
        private List<Tuple<string, string>> GetItemInfoFromDatabase()
        {
            List<Tuple<string, string>> itemInfoList = new List<Tuple<string, string>>();

            using (SQLiteConnection connection = CreateConnection())
            {
                if (connection.State == ConnectionState.Open)
                {
                    using (SQLiteCommand command = new SQLiteCommand("SELECT L.ItemName, C.CollectionName " +
                        "FROM Logins L JOIN Collections C ON L.CollectionId = C.CollectionId " +
                        "WHERE L.userId IN (SELECT userId FROM Users WHERE IsLoggedIn = 1)", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string itemName = reader["ItemName"].ToString();
                                string collectionName = reader["CollectionName"].ToString();

                                itemInfoList.Add(new Tuple<string, string>(itemName, collectionName));
                            }
                        }
                    }
                }
            }
            return itemInfoList;
        }

        //not in documentation
        private List<Tuple<string, string>> GetOrderedItemInfoFromDatabase()
        {
            List<Tuple<string, string>> itemInfoList = new List<Tuple<string, string>>();

            using (SQLiteConnection connection = CreateConnection())
            {
                if (connection.State == ConnectionState.Open)
                {
                    using (SQLiteCommand command = new SQLiteCommand("SELECT L.ItemName, C.CollectionName FROM Logins L JOIN Collections C ON L.CollectionId = C.CollectionId WHERE L.userId IN (SELECT userId FROM Users WHERE IsLoggedIn = 1) ORDER BY CollectionName ASC", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string itemName = reader["ItemName"].ToString();
                                string collectionName = reader["CollectionName"].ToString();

                                itemInfoList.Add(new Tuple<string, string>(itemName, collectionName));
                            }
                        }
                    }
                }
            }

            return itemInfoList;
        }

        //not in documentation
        private void DropDown_Click(object sender, RoutedEventArgs e)
        {
            if (btnToggled == "false")
            {
                DropDown.Content = "▼";
                btnToggled = "true";
                CreateLabelsAndButtons();
            }
            else
            {
                DropDown.Content = "▲";
                btnToggled = "false";
                CreateLabelsAndButtons();
            }
        }
        

        //connection function
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
