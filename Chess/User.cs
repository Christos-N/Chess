using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class User
    {
        public string Name { get; set; }
        public double GamesWon { get; set; }
        public double GamesDrew { get; set; }
        public double GamesLost { get; set; }

        public User(string name, double gamesWon, double gamesDrew, double gamesLost)
        {
            Name = name;
            GamesWon = gamesWon;
            GamesDrew = gamesDrew;
            GamesLost = gamesLost;
        }
        public double calculateWinRate()
        {
            double sum = GamesWon+GamesDrew+GamesLost;
            if (sum == 0)
                return 0;   //Δεν έχει παίξει κανένα παιχνίδι ο χρήστης
            else
                return (GamesWon / sum * 100);
        }
    }
}
