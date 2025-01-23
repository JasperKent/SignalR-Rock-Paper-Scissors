using System.Collections.Generic;

namespace FrameworkHub.GameLogic
{

    public class GameManager
    {
        private readonly object _locker = new object();
        private readonly Dictionary<string, GameGroup> _games = new Dictionary<string, GameGroup>();
        private GameGroup _waitingGroup;

        public GameGroup Register(string name)
        {
            lock (_locker)
            { 
                if (_waitingGroup == null)
                {
                    _waitingGroup = new GameGroup();


                    _games[_waitingGroup.Name] = _waitingGroup;
                }

                _waitingGroup.AddPlayer(name);

                var retVal = _waitingGroup;

                if (_waitingGroup.Full)
                    _waitingGroup = null;

                return retVal;
            }
        }

        public Game Throw(string groupName, string player, Sign sign)
        {
            lock (_locker)
            {
                var game = _games[groupName].Game;

                game.Throw(player, sign);   

                return game;
            }
        }
    }
}
