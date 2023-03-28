using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public partial class Form2 : Form
    {
        Regex letters = new Regex(@"^[a-zA-Z0-9]+$");   //γράμματα και αριθμοί
        List<User> list = new List<User>();
        User userBlack, userWhite = new User("", 0, 0, 0);
        Option option = new Option(1);
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionstring = "Data Source=DB1.db;Version=3;";
            SQLiteConnection conn = new SQLiteConnection(connectionstring);
            conn.Open();
            String insertQuery = "INSERT INTO PlayersAndTime (Player1, Player2, Timestamp) VALUES (@player1, @player2, @time)";
            SQLiteCommand command = new SQLiteCommand(insertQuery, conn);
            command.Parameters.AddWithValue("@player1", textBox1.Text);
            command.Parameters.AddWithValue("@player2", textBox2.Text);
            command.Parameters.AddWithValue("@time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            command.ExecuteNonQuery();
            conn.Close();
            this.Hide();
            new Form1(userBlack, userWhite, list, option.Time).ShowDialog();
            this.Close();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string connectionstring = "Data Source=DB1.db;Version=3;";
            SQLiteConnection conn = new SQLiteConnection(connectionstring);
            conn.Open();
            String selectQuery = "Select * from Users";
            SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new User(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
            conn.Close();
            comboBox1.Text = "1";
        }
        private User signupOrLogin(string textBoxText)
        {
            if (letters.IsMatch(textBoxText))
            {
                foreach (User user in list)
                    if (textBoxText == user.Name)
                        return user;    //Αν βρεθεί χρήστης με το όνομα στο textbox η συνάρτηση τότε επιστρέφει τον χρήστη
                User newuser = new User(textBoxText, 0, 0, 0);
                list.Add(newuser);
                savetoDB(newuser);
                return newuser;
            }
            else
                MessageBox.Show("Please enter a valid username.", "Error");
            return userWhite;   //Σε κάθε περίπτωση θα έχει "" για όνομα, οπότε η συνθήκη του if θα είναι ψευδής

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != textBox2.Text)
            {
                userWhite = signupOrLogin(textBox2.Text);
                if (userWhite.Name != "")
                {
                    button3.Enabled = false;
                    button1.Enabled = true;
                    textBox2.Enabled = false;
                }
            }
            else
                MessageBox.Show("You can't play with yourself!", "Error");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            userBlack = signupOrLogin(textBox1.Text);
            if (userBlack.Name != "")
            {
                button2.Enabled = false;
                button3.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = true;
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            option.Time = Convert.ToInt32(comboBox1.Text);
        }


        private void savetoDB(User user)
        {
            string connectionstring = "Data Source=DB1.db;Version=3;";
            SQLiteConnection conn = new SQLiteConnection(connectionstring);
            conn.Open();
            String insertQuery = "INSERT INTO Users (Name, GamesWon, GamesDrew, GamesLost) VALUES (@name, @victories, @draws, @defeats)";
            SQLiteCommand command = new SQLiteCommand(insertQuery, conn);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@victories", user.GamesWon);
            command.Parameters.AddWithValue("@draws", user.GamesDrew);
            command.Parameters.AddWithValue("@defeats", user.GamesLost);
            command.ExecuteNonQuery();
            conn.Close();
        }
    }

}

