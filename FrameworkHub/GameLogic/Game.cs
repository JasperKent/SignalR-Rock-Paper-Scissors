
using System;

namespace FrameworkHub.GameLogic
{
    public class Game
    {
        public Game(Player player1, Player player2)
        {
            Player1 = player1;  
            Player2 = player2;  
        }

        public Player Player1 { get; }
        public Player Player2 { get; }

        public bool Pending => Player1.Throw == null || Player2.Throw == null;

        public string Winner
        {
            get
            {
                if (Pending)
                    throw new InvalidOperationException("Game not complete");

                switch (Signs.Beats(Player1.Throw.Value, Player2.Throw.Value))
                {
                    case true:
                        return Player1.Name;
                    case false: 
                        return Player2.Name;
                    default:
                        return null;
                }
            }
        }

        public void Reset()
        {
            if (Pending)
                throw new InvalidOperationException("Game not complete");

            switch (Signs.Beats(Player1.Throw.Value, Player2.Throw.Value))
            {
                case true: Player1.Score++; break;
                case false: Player2.Score++; break;
                default: /* Draw */ break;
            }

            Player1.Throw = null;
            Player2.Throw = null;
        }

        public string Scores => $"{Player1.Name}: {Player1.Score}. {Player2.Name}: {Player2.Score}.";

        public string WaitingFor => Player1.Throw == null ? Player1.Name : Player2.Name;

        public string Explanation
        {
            get
            {
                if (Pending)
                    throw new InvalidOperationException("Game not complete");

               
                switch (Signs.Beats(Player1.Throw.Value, Player2.Throw.Value))
                {
                    case true:
                        return $"{Player1.Throw.Value} beats {Player2.Throw.Value}";
                    case false:
                        return $"{Player2.Throw.Value} beats {Player1.Throw.Value}";
                    default:
                        return $"{Player1.Throw.Value} draws with {Player2.Throw.Value}";
                }
            }
        }

        public void Throw(string player, Sign selection)
        {
            if (player == Player1.Name)
                Player1.Throw = selection;
            else
                Player2.Throw = selection;
        }
    }
}
