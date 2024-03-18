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
    public partial class PaymentsPage : Page
    {
        public PaymentsPage()
        {
            InitializeComponent();
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            CreateLabelsAndButtons();
            sqlite_conn.Close();
            sqlite_conn.Dispose();
            Debug.WriteLine("PaymentsPage closed");
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AddPaymentItemWindow main = new AddPaymentItemWindow();
            main.Show();
        }

        private void CreateLabelsAndButtons()
        {
            Grid grid = mainGrid;

            mainGrid.Children.Clear();

            Debug.WriteLine("CreateLabelsAndButtons() called");

            List<Tuple<string, string>> itemInfoList = GetItemInfoFromDatabase();

            for (int i = 0; i < itemInfoList.Count; i++)
            {
                string itemName = itemInfoList[i].Item1;
                string collectionName = itemInfoList[i].Item2;

                // Create label for item name
                Label nameLabel = new Label();
                nameLabel.Content = itemName;
                nameLabel.FontSize = 18;
                nameLabel.FontWeight = FontWeights.Bold;
                nameLabel.Foreground = Brushes.White;
                nameLabel.VerticalAlignment = VerticalAlignment.Center;
                nameLabel.FontFamily = new FontFamily("Montserrat");
                nameLabel.Height = 30;
                nameLabel.Padding = new Thickness(10, 10, 0, 0);

                // Create label for collection name
                Label collectionLabel = new Label();
                collectionLabel.Content = collectionName;
                collectionLabel.FontSize = 16;
                collectionLabel.FontWeight = FontWeights.Bold;
                collectionLabel.Foreground = Brushes.White;
                collectionLabel.VerticalAlignment = VerticalAlignment.Center;
                collectionLabel.FontFamily = new FontFamily("Montserrat");
                collectionLabel.Height = 30;
                collectionLabel.Padding = new Thickness(94, 10, 0, 0);

                // Create edit button
                Button editButton = new Button();
                editButton.Content = "Edit";
                editButton.FontSize = 14;
                editButton.Tag = itemName;
                editButton.Click += EditButton_Click;
                editButton.Margin = new Thickness(120, 10, 0, 0);
                editButton.Width = 50;
                editButton.Height = 30;
                editButton.VerticalAlignment = VerticalAlignment.Center;

                // Create delete button
                Button deleteButton = new Button();
                deleteButton.Content = "Delete";
                deleteButton.FontSize = 14;
                deleteButton.Tag = itemName;
                deleteButton.Click += DeleteButton_Click;
                deleteButton.Margin = new Thickness(0, 10, 0, 0);
                deleteButton.Width = 50;
                deleteButton.Height = 30;
                deleteButton.VerticalAlignment = VerticalAlignment.Center;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Left;

                mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                Grid.SetRow(nameLabel, i);
                Grid.SetColumn(nameLabel, 0);
                Grid.SetRow(collectionLabel, i);
                Grid.SetColumn(collectionLabel, 1);
                Grid.SetRow(editButton, i);
                Grid.SetColumn(editButton, 2);
                Grid.SetRow(deleteButton, i);
                Grid.SetColumn(deleteButton, 3);

                mainGrid.Children.Add(nameLabel);
                mainGrid.Children.Add(collectionLabel);
                mainGrid.Children.Add(editButton);
                mainGrid.Children.Add(deleteButton);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string paymentId = GetPaymentIdFromDatabase(clickedButton.Tag.ToString());

            Debug.WriteLine(clickedButton.Tag.ToString());
            Debug.WriteLine(paymentId);

            EditPaymentItemWindow editWindow = new EditPaymentItemWindow(paymentId);
            editWindow.Show();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();

            Button clickedButton = (Button)sender;
            string paymentId = GetPaymentIdFromDatabase(clickedButton.Tag.ToString());

            string messageBoxText = "Are you sure you want to delete this item?";
            string caption = "KeyFort";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Exclamation;
            MessageBoxResult result;
            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    deleteItem(paymentId);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void deleteItem(string paymentId)
        {
            using (SQLiteConnection conn = CreateConnection()) // Create a new connection to the database
            {
                try // Try to delete the item from the database
                {
                    using (SQLiteCommand sqlite_cmd = conn.CreateCommand())
                    {
                        string Createsql = "DELETE FROM Payments WHERE PaymentId = @PaymentId"; // SQL query to delete the item from the database
                        sqlite_cmd.CommandText = Createsql;
                        sqlite_cmd.Parameters.AddWithValue("@PaymentId", paymentId); // Add the payment ID to the query
                        sqlite_cmd.ExecuteNonQuery();
                    }
                    // If the item was deleted successfully, show a message box to the user
                    MessageBox.Show("Item deleted successfully.", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    CreateLabelsAndButtons();
                }
                catch (Exception ex) // If an error occurs, show a message box to the user
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private string GetPaymentIdFromDatabase(string itemName)
        {
            using (SQLiteConnection connection = CreateConnection()) // Create a new connection to the database
            {
                if (connection.State == ConnectionState.Open) // If the connection is open, execute the query
                {
                    // SQL query to get the payment ID from the database
                    string sqlQuery = "SELECT PaymentID FROM Payments WHERE ItemName = @ItemName";

                    using (SQLiteCommand command = new SQLiteCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ItemName", itemName);

                        object result = command.ExecuteScalar(); // Execute the query and store the result in a variable
                        if (result != null)
                        {
                            return result.ToString(); // Return the result
                        }
                    }
                }
            }

            return null; // If the query fails, return null
        }


        private List<Tuple<string, string>> GetItemInfoFromDatabase()
        {
            List<Tuple<string, string>> itemInfoList = new List<Tuple<string, string>>();

            using (SQLiteConnection connection = CreateConnection())
            {
                if (connection.State == ConnectionState.Open)
                {
                    using (SQLiteCommand command = new SQLiteCommand("SELECT L.ItemName, C.CollectionName FROM Payments L JOIN Collections C ON L.CollectionId = C.CollectionId WHERE L.userId IN (SELECT userId FROM Users WHERE IsLoggedIn = 1)", connection))
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

        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            
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
