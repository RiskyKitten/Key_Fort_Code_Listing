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
using System.Text.RegularExpressions;

namespace Key_Fort
{
    public partial class EditPaymentItemWindow : Window
    {
        private string paymentId;
        public EditPaymentItemWindow(string paymentId)
        {
            this.paymentId = paymentId;
            InitializeComponent();
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            UpdateCombobox(sqlite_conn);
            PopulateTextBoxes(sqlite_conn);
        }

        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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


        private void PopulateTextBoxes(SQLiteConnection conn)
        {
            string strEnteredName = ItemNameBox.Text; // Get the text from each textbox
            string strEnteredCardName = CardNameBox.Text;
            string strEnteredCardNum = CardNumberBox.Text;
            string strEnteredCode = SecurityCodeBox.Text;
            string strExpiryBox1 = ExpiryBox1.Text;
            string strExpiryBox2 = ExpiryBox2.Text;
            string strCombobox = ComboBox1.Text;

            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            sqlite_cmd = conn.CreateCommand(); // Get the item details
            sqlite_cmd.CommandText = $"SELECT ItemName, CardHolderName, CardNumber, SecurityCode, " +
                $"ExpiryDate FROM Payments WHERE paymentId = '{paymentId}'";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            string ItemName = sqlite_datareader.GetString(0);
            string CardHolderName = sqlite_datareader.GetString(1);
            string CardNumber = sqlite_datareader[2].ToString();
            string SecurityDate = sqlite_datareader[3].ToString();
            int ExpiryDateInt = sqlite_datareader.GetInt32(4);

            string ExpiryDateStr = ExpiryDateInt.ToString(); // Cast the expiry date to a string

            ItemNameBox.Text = ItemName;
            CardNameBox.Text = CardHolderName;
            CardNumberBox.Text = CardNumber;
            SecurityCodeBox.Text = SecurityDate;
            ExpiryBox1.Text = ExpiryDateStr.Substring(0, 2);
            ExpiryBox2.Text = ExpiryDateStr.Substring(2, 2);

            sqlite_datareader.Close();

            sqlite_cmd = conn.CreateCommand(); // Get the collection name
            sqlite_cmd.CommandText = $"SELECT CollectionName FROM Collections WHERE CollectionId = " +
                $"(SELECT CollectionId FROM Payments WHERE paymentId = '{paymentId}')";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            string CollectionName = sqlite_datareader.GetString(0);

            ComboBox1.Text = CollectionName; // Set the combobox to the collection name

            sqlite_datareader.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            ModifyItem(sqlite_conn);
        }

        private void ModifyItem(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            string strEnteredName = ItemNameBox.Text; // Get the text from each textbox
            string strEnteredCardName = CardNameBox.Text;
            string strEnteredCardNum = CardNumberBox.Text;
            string strEnteredCode = SecurityCodeBox.Text;
            string strExpiryBox1 = ExpiryBox1.Text;
            string strExpiryBox2 = ExpiryBox2.Text;
            string strCombobox = ComboBox1.Text;

            string ExpiryBox = strExpiryBox1 + strExpiryBox2; // Concatenate the expiry date

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"DELETE from Payments WHERE PaymentID = '{paymentId}'"; // Delete the old item
            sqlite_cmd.ExecuteNonQuery();

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT userId FROM Users WHERE IsLoggedIn = '1'"; // Get the user ID

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            sqlite_datareader.Read();

            int intUserId = sqlite_datareader.GetInt32(0);
            sqlite_datareader.Close();

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"SELECT CollectionID FROM Collections WHERE CollectionName = '{strCombobox}' AND userID = '{intUserId}'";
            sqlite_datareader = sqlite_cmd.ExecuteReader(); // Get the collection ID
            sqlite_datareader.Read();

            int CollectionID = sqlite_datareader.GetInt32(0);
            sqlite_datareader.Close();

            sqlite_cmd = conn.CreateCommand(); // Insert the new item
            sqlite_cmd.CommandText = "INSERT INTO Payments(userID, ItemName, CardHolderName, CardNumber, " +
                "SecurityCode, ExpiryDate, CollectionID) VALUES(@userID, @ItemName, @CardName," +
                " @CardNum, @EnteredCode, @Expiry, @CollectionID)";

            sqlite_cmd.Parameters.AddWithValue("@userID", intUserId); // Add the parameters
            sqlite_cmd.Parameters.AddWithValue("@ItemName", strEnteredName);
            sqlite_cmd.Parameters.AddWithValue("@CardName", strEnteredCardName);
            sqlite_cmd.Parameters.AddWithValue("@CardNum", strEnteredCardNum);
            sqlite_cmd.Parameters.AddWithValue("@EnteredCode", strEnteredCode);
            sqlite_cmd.Parameters.AddWithValue("@Expiry", ExpiryBox);
            sqlite_cmd.Parameters.AddWithValue("@CollectionID", CollectionID);
            sqlite_cmd.Prepare();
            sqlite_cmd.ExecuteNonQuery();

            RegisterErrorLabel.Text = "Item Saved";// Display a message to the user
            System.Threading.Thread.Sleep(1000);
            this.Close();
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

        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True;Compress = True; ");
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
