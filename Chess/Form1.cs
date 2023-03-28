using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public partial class Form1 : Form
    {
        // Το κάθε picturebox έχει διαφορετικό όνομα και από το ψηφίο μπορώ να κατάλαβω αν το αντίστοιχο πιόνι είναι μαύρο ή άσπρο: 0<=15 μαύρο, 16<=31 άσπρο
        private Point point;
        bool move;
        int[] values = new int[8]; //πίνακας με καθορισμένες τιμές (θέσεις x,y)
        double add = 87.5;  //double για μεγαλύτερη ακρίβεια
        int start = 10;     //Ξεκινάει από το 10 (Χ ή Υ) του panel
        int tempX = 0;      //Για την "ελεύθερη" κίνηση
        int tempY = 0;      //>>
        int newLocationX = 700; //Για τα
        int newLocationY = 12;  //πιόνια που "τρώγονται"
        int minutesWhite; //Για τον
        int secondsWhite = 1;   //χρόνο
        int minutesBlack; //που απομένει
        int secondsBlack = 1;   //στον κάθε χρήστη
        bool turn; //true όταν είναι η σειρά του "μαύρου"
        int[,] previousXAndY = new int[32,2];   //Περιέχει τις αρχικές συντεταγμένες του κάθε pictureBox (μέσω της Form1_Load) και ανανεώνεται όταν αλλάζει θέση (μέσω της pictureBox_MouseUp)
        string nameBlack, nameWhite;
        List<User> users = new List<User>();
        User blackUser, whiteUser = new User("", 0, 0, 0);

        public Form1(User userBlack, User userWhite, List<User> list, int mins)
        {
            InitializeComponent();
            minutesBlack = mins;
            minutesWhite = mins;
            nameBlack = userBlack.Name;
            nameWhite = userWhite.Name;
            label4.Text = nameBlack + "      Time left: " + minutesBlack + ":00";
            label2.Text = "Victories: " + userBlack.GamesWon.ToString();
            label3.Text = "Draws: " + userBlack.GamesDrew.ToString();
            label6.Text = "Defeats: " + userBlack.GamesLost.ToString();
            label5.Text = nameWhite + "      Time left: " + minutesWhite + ":00";
            label7.Text = "Victories: " + userWhite.GamesWon.ToString();
            label8.Text = "Draws: " + userWhite.GamesDrew.ToString();
            label9.Text = "Defeats: " + userWhite.GamesLost.ToString();
            users = list;
            blackUser = userBlack;
            whiteUser = userWhite;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i <= 7; i++)
                values[i] = Convert.ToInt32(start + i * add);
            label1.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            foreach (var pB in panel1.Controls.OfType<PictureBox>())    //αρχικές συντεταγμένες
            {
                previousXAndY[Convert.ToInt32(pB.Name.Substring(10)), 0] = pB.Location.X;
                previousXAndY[Convert.ToInt32(pB.Name.Substring(10)), 1] = pB.Location.Y;
            }
        }
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            point = e.Location;
            move = true;
        }
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            tempX = pictureBox.Left + e.X - point.X;
            tempY = pictureBox.Top + e.Y - point.Y;
            if (move)
                pictureBox.Location = new Point(tempX, tempY);    //ελεύθερη κίνηση
        }
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            move = false;
            int x = previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 0];
            int y = previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 1];
            pictureBox.Location = new Point(finalPosition(tempX), finalPosition(tempY));
            kill(pictureBox);
            previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 0] = pictureBox.Location.X;
            previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 1] = pictureBox.Location.Y;
            if (x != previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 0] || y != previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 1])   //Ελέγχω αν το πιόνι άλλαξε θέση (άσπρο πάνω σε άσπρο ή μαύρο πάνω σε μαύρο) 
            {
                if ((Convert.ToInt32(pictureBox.Name.Substring(10)) <= 15 && Convert.ToInt32(pictureBox.Name.Substring(10)) >= 8 && pictureBox.Location.Y != 622) || Convert.ToInt32(pictureBox.Name.Substring(10)) < 8 || (Convert.ToInt32(pictureBox.Name.Substring(10)) <= 15 && pictureBox.Tag.ToString() == "new"))    //μαύρο και δεν είναι στρατιώτης στην τελευταία σειρά(για promote) ή μαύρο (όχι στρατιώτης) ή promoted μαύρο
                    turn = false;    //σειρά του "άσπρου"
                else if ((Convert.ToInt32(pictureBox.Name.Substring(10)) > 15 && Convert.ToInt32(pictureBox.Name.Substring(10)) <= 23 && pictureBox.Location.Y != 10) || Convert.ToInt32(pictureBox.Name.Substring(10)) > 23 || pictureBox.Tag.ToString() == "new"|| (Convert.ToInt32(pictureBox.Name.Substring(10)) > 15 && pictureBox.Tag.ToString() == "new"))   //άσπρο και δεν είναι στρατιώτης στην τελευταία σειρά(για promote) ή άσπρο (όχι στρατιώτης) ή promoted άσπρο
                    turn = true;    //σειρά του "μαύρου"
            }
        }
        
        private int finalPosition(int temp)
        {
            int final = values[0];
            for (int i = 1; i <= 7; i++)    //βρίσκω αναμέσα σε ποιες τιμές (του πίνακα values) είναι το X ή το Υ (ανάλογα την παράμετρο) του picturebox
            {
                if (temp >= values[i - 1] && temp <= values[i])
                {
                    if ((temp - values[i - 1] <= values[i] - temp))
                        final = values[i - 1];
                    else
                    {
                        final = values[i];
                    }
                }
                else if (temp > 622)   //Έξω από το panel
                {
                    final = 622;
                }
            }
            return final;

        }
        private void kill(PictureBox pictureBox)
        {
            foreach (var pB in panel1.Controls.OfType<PictureBox>())
            {
                //Ελέγχω σε κάθε μπλοκ τα locations και αν είναι μαύρο ή άσπρο πιόνι (ώστε να μην μπορεί ένα πιόνι να "τρώει" άλλο πιόνι ίδιου χρώματος)
                if (pB.Location == pictureBox.Location && pB != pictureBox && ((Convert.ToInt32(pictureBox.Name.Substring(10)) > 15 && Convert.ToInt32(pB.Name.Substring(10)) <= 15) || Convert.ToInt32(pB.Name.Substring(10)) > 15 && Convert.ToInt32(pictureBox.Name.Substring(10)) <= 15))
                {
                    pB.Width = 35;
                    pB.Height = 35;
                    pB.Location = new Point(newLocationX, newLocationY);
                    if (newLocationY <= 620)
                        newLocationY += 40;
                    else
                    {
                        newLocationX += 35;
                        newLocationY = 12;
                    }
                    pB.Enabled = false;
                    if (Convert.ToInt32(pB.Name.Substring(10)) == 5)    //μαύρος βασιλιάς
                        gameOver("white");
                    else if (Convert.ToInt32(pB.Name.Substring(10)) == 27)  //άσπρος βασιλιάς
                        gameOver("black");
                }
                else if (pB.Location == pictureBox.Location && pB != pictureBox && ((Convert.ToInt32(pictureBox.Name.Substring(10)) > 15 && Convert.ToInt32(pB.Name.Substring(10)) > 15) || Convert.ToInt32(pB.Name.Substring(10)) <= 15 && Convert.ToInt32(pictureBox.Name.Substring(10)) <= 15))  //άσπρο-άσπρο, μαύρο-μαύρο
                    pictureBox.Location = new Point(previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 0], previousXAndY[Convert.ToInt32(pictureBox.Name.Substring(10)), 1]);
            }
        }
        private void timer1_Tick(object sender, EventArgs e) 
        {
            if (turn)
                secondsBlack--;
            else
                secondsWhite--;
            if (secondsBlack == 0)
            {
                if (minutesBlack == 0)
                {
                    timer1.Enabled = false;
                    MessageBox.Show(nameBlack + " ran out of time!", "Game over!");
                    gameOver("white");
                    return;
                }
                else
                    secondsBlack = 60;
                minutesBlack--;
            }
            else if (secondsWhite == 0)
            {
                if (minutesWhite == 0)
                {
                    timer1.Enabled = false;
                    MessageBox.Show(nameWhite + " ran out of time!","Game over!");
                    gameOver("black");
                    return;
                }
                else
                    secondsWhite = 60;
                minutesWhite--;
                
            }
            if (secondsBlack <= 10)
                label4.Text = nameBlack + "      Time left: " + minutesBlack + ":0" + (secondsBlack - 1).ToString();
            else
                label4.Text = nameBlack + "      Time left: " + minutesBlack + ":" + (secondsBlack - 1).ToString();
            if (secondsWhite <= 10)
                label5.Text = nameWhite + "      Time left: " + minutesWhite + ":0" + (secondsWhite - 1).ToString();
            else
                label5.Text = nameWhite + "      Time left: " + minutesWhite + ":" + (secondsWhite - 1).ToString();

        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            button1.Enabled = false;
            button1.Visible = false;
            foreach (PictureBox pictureBox in panel1.Controls.OfType<PictureBox>())
                pictureBox.Enabled = true;
            button2.Enabled = true;
        }

       
        private void promote(object sender, String piece)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ContextMenuStrip owner = (ContextMenuStrip)item.Owner;
            Control usedControl = owner.SourceControl;
            PictureBox pictureBox = (PictureBox)usedControl;
            string color;
            if (Convert.ToInt32(pictureBox.Name.Substring(10)) > 15)
                color = "white";
            else
                color = "black";
            if ((pictureBox.Location.Y == 10 && color == "white") || (pictureBox.Location.Y == 622 && color == "black"))
            {
                var newPictureBox = new PictureBox     //Δημιουργώ καινούριο picturebox, το οποίο δεν μπορεί να ξαναγίνει promote (αφού δεν θα έχει την ιδιότητα του contextMenuStrip
                {
                    BackColor = pictureBox.BackColor,
                    ImageLocation = "images/" + color + piece + ".png", //Το κατάλληλο path, ανάλογα με την επιλογή του χρήστη(piece)
                    SizeMode = pictureBox.SizeMode,
                    Name = pictureBox.Name,
                    Size = pictureBox.Size,
                    Location = pictureBox.Location,
                    Tag = "new"     //Χρησιμεύει για την συνάρτηση pictureBox_MouseUp, ώστε να παίρνει την κατάλληλη τιμή η μεταβλητή turn και να παίξει ο επόμενος
                };
                newPictureBox.MouseDown += pictureBox_MouseDown;
                newPictureBox.MouseUp += pictureBox_MouseUp;
                newPictureBox.MouseMove += pictureBox_MouseMove;
                panel1.Controls.Add(newPictureBox);
                pictureBox.Dispose();
                if (Convert.ToInt32(newPictureBox.Name.Substring(10)) > 15)
                    turn = true;
                else
                    turn = false;
            }
            else
                MessageBox.Show("You must be in the final row in order to promote your pawn.", "Error");
        }
        private void queenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            promote(sender, "queen");
        }

        private void rookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            promote(sender, "rook");
        }

        private void knightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            promote(sender, "knight");
        }


        private void button3_Click(object sender, EventArgs e)
        {
            gameOver("draw");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder("");
            users = users.OrderByDescending(x => x.GamesWon).ToList();  //Ταξινόμηση της λίστας με βάση τις νίκες
            foreach (User user in users)
                stringBuilder.Append(user.Name + ": " + user.GamesWon + " Victory/ies" + Environment.NewLine);
            MessageBox.Show(stringBuilder.ToString(),"Leaderboard");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder("");
            users = users.OrderByDescending(x => x.calculateWinRate()).ToList();    //Ταξινόμηση της λίστας με βάση το ποσοστό νικών (νίκες/πόσα παιχνίδια έχει παίξει)
            foreach (User user in users)
            {   //try catch γιατί μπορεί η τιμή να έχει πολλά δεκαδικά ψηφία, αλλά μπορεί να είναι και 100 
                try
                {
                    stringBuilder.Append(user.Name + ": " + user.calculateWinRate().ToString().Substring(0,5) + "% Winrate" + Environment.NewLine);
                }
                catch
                {
                    stringBuilder.Append(user.Name + ": " + user.calculateWinRate() + "% Winrate" + Environment.NewLine);
                }
            }
            MessageBox.Show(stringBuilder.ToString(), "Leaderboard");
        }

        private void bishopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            promote(sender, "bishop");
        }

        private void gameOver(string winner)
        {
            panel1.Enabled = false;
            button1.Dispose();
            button2.Dispose();
            timer1.Enabled = false;
            string connectionstring = "Data Source=DB1.db;Version=3;";
            SQLiteConnection conn = new SQLiteConnection(connectionstring);
            conn.Open();
            string updateQuery="";
            switch (winner)
            {
                case "white":
                    updateQuery = "UPDATE Users SET GamesWon = " + (++whiteUser.GamesWon) + " WHERE Name = '" + whiteUser.Name + "';" +
                        "UPDATE Users SET GamesLost = " + (++blackUser.GamesLost) + " WHERE Name = '" + blackUser.Name + "';";
                    MessageBox.Show("Congratulations " + whiteUser.Name + "!", "Winner");
                    break;
                case "draw":
                    updateQuery = "UPDATE Users SET GamesDrew = " + (++whiteUser.GamesDrew) + " WHERE Name = '" + whiteUser.Name + "';" +
                        "UPDATE Users SET GamesDrew = " + (++blackUser.GamesDrew) + " WHERE Name = '" + blackUser.Name + "';";
                    MessageBox.Show("Game ended. It's a draw.", "Draw");
                    break;
                case "black":
                    updateQuery = "UPDATE Users SET GamesWon = " + (++blackUser.GamesWon) + " WHERE Name = '" + blackUser.Name + "';" +
                        "UPDATE Users SET GamesLost = " + (++whiteUser.GamesLost) + " WHERE Name = '" + whiteUser.Name + "';";
                    MessageBox.Show("Congratulations " + blackUser.Name + "!", "Winner");
                    break;
            }
            label2.Text = "Victories: " + blackUser.GamesWon.ToString();
            label3.Text = "Draws: " + blackUser.GamesDrew.ToString();
            label6.Text = "Defeats: " + blackUser.GamesLost.ToString();
            label7.Text = "Victories: " + whiteUser.GamesWon.ToString();
            label8.Text = "Draws: " + whiteUser.GamesDrew.ToString();
            label9.Text = "Defeats: " + whiteUser.GamesLost.ToString();
            SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
