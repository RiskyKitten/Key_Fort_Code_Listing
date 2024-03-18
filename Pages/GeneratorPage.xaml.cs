 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Key_Fort
{
    //Main class for the Generator Page
    public partial class GeneratorPage : Page
    {
        //Constructor for the Generator Page
        public GeneratorPage()
        {
            InitializeComponent();
            Password_Box.Text = GeneratePassword();
        }

        //Boolean variables that store the current state of the checkboxes
        private bool UpperCheckboxIsChecked;
        private bool LowerCheckboxIsChecked;
        private bool NumCheckboxIsChecked;
        private bool SpecialCheckboxIsChecked;

        //Dictionary that stores the current character set
        private Dictionary<string, string> CurrentCharSet = new Dictionary<string, string>();

        //Function that generates a password based on the current character set and length
        //Returns the generated password as a string to the Password_Box
        //Changes the font size dynamically based on the length of the password
        private string GeneratePassword()
        {
            CurrentCharSet = new Dictionary<string, string>();
            Password_Box.FontSize = 18;

            double len = slValue.Value;
            int length = Convert.ToInt32(len);

            if (length >= 28)
            {
                Password_Box.FontSize = 16;
            }

            string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string LowerChars = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz";
            string SpecialChars = "!@#$%^&*()_+!@#$%^&*()_+!@#$%^&*()_+!@#$%^&*()_+";
            string NumChars = "0123456789012345678901234567890123456789";

            if (UpperCheckboxIsChecked)
            {
                CurrentCharSet["UpperChars"] = UpperChars;
            }
            if (LowerCheckboxIsChecked)
            {
                CurrentCharSet["LowerChars"] = LowerChars;
            }
            if (NumCheckboxIsChecked)
            {
                CurrentCharSet["NumChars"] = NumChars;
            }
            if (SpecialCheckboxIsChecked)
            {
                CurrentCharSet["SpecialChars"] = SpecialChars;
            }

            //Creates a string that contains all the characters in the current character set
                //Creates a random object
                    //Creates a string that will store the generated password
                    //If the length is less than or equal to 0, return an empty string
                            //If the length is greater than the length of the current character set, return an empty string
                                //For loop that generates a random number between 0 and the length of the current character set
                                    //Adds the character at the index of the random number to the result string
                                        //Returns the result string
            string Temp = "";
            foreach (string key in CurrentCharSet.Keys)
            {
                Temp += CurrentCharSet[key]; // Adds the current character set to the Temp string
            }

            if (length <= 0)
            {
                return string.Empty; // Returns an empty string
            }
            if (length > Temp.Length)
            {
                return string.Empty; // Returns an empty string
            }


            string result = ""; // Stores the generated password
            Random random = new Random(); // Random object

            // Generates a random number between 0 and the length of the current character set
            for (int i = 0; i < length; i++) 
            {
                int index = random.Next(Temp.Length); // Random number
                result += Temp[index]; // Adds the character at the index of the random number to the result string
            }

            return result.ToString(); // Returns the result string

        }

        //Function that calls the GeneratePassword function and sets the Password_Box text to the generated password
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            Password_Box.Text = GeneratePassword();
        }

        //Event handler for the Copy button
        //Copies the text in the Password_Box to the clipboard
        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Password_Box.Text);
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Password_Box.Text.ToString();
            //Debug.WriteLine(text);

            int length = text.Length;
            if (length > 15 & text.Any(ch => !char.IsLetterOrDigit(ch)) & text.Any(char.IsUpper) & text.Any(char.IsLower) & text.Any(char.IsNumber))
            {
                Strength_Bar.Width = 330;
                Strength_Bar.Fill = Brushes.Green;
            }
            
            if (length <= 4)
            {
                Strength_Bar.Width = 66;
                Strength_Bar.Fill = Brushes.Red;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //Re-generates the password when the slider value is changed
        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Password_Box.Text = GeneratePassword();
        }

        //Event handler for the first checkbox (checked)
        //Sets the CheckboxIsChecked variable to true
        //Calls the GeneratePassword function
        private void UpperCheck_Checked(object sender, RoutedEventArgs e)
        {
            UpperCheckboxIsChecked = true;
            Password_Box.Text = GeneratePassword();
        }

        //Event handler for the Uppercase checkbox (unchecked)
        //Sets the CheckboxIsChecked variable to false
        //Calls the GeneratePassword function
        private void UpperCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            UpperCheckboxIsChecked = false;
            Password_Box.Text = GeneratePassword();
        }
        
        //Event handler for the Lowercase checkbox (checked)
        private void LowerCheck_Checked(object sender, RoutedEventArgs e)
        {
            LowerCheckboxIsChecked = true;
            Password_Box.Text = GeneratePassword();
        }
        //Event handler for the Lowercase checkbox (unchecked)
        private void LowerCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            LowerCheckboxIsChecked = false;
            Password_Box.Text = GeneratePassword();
        }

        //Event handler for the Number checkbox (checked)
        private void NumCheck_Checked(object sender, RoutedEventArgs e)
        {
            NumCheckboxIsChecked = true;
            Password_Box.Text = GeneratePassword();
        }
        //Event handler for the Number checkbox (unchecked)
        private void NumCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            NumCheckboxIsChecked = false;
            Password_Box.Text = GeneratePassword();
        }

        //Event handler for the Special characters checkbox (checked)
        private void SpecialCheck_Checked(object sender, RoutedEventArgs e)
        {
            SpecialCheckboxIsChecked = true;
            Password_Box.Text = GeneratePassword();
        }
        //Event handler for the Special characters checkbox (unchecked)
        private void SpecialCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            SpecialCheckboxIsChecked = false;
            Password_Box.Text = GeneratePassword();
        }

    }
}
