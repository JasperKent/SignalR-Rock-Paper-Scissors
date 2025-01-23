using System;

namespace FrameworkHub.GameLogic
{
    public class GameGroup
    {
        private Player _player1;
        private Player _player2;
        private Game _game;

        public Game Game => _game ?? throw new InvalidOperationException("Game not created");
        public string Name { get; } = Guid.NewGuid().ToString();
        public bool Full => _game != null;

        public void AddPlayer (string name)
        {
            if (_player1 == null)
                _player1 = new Player(name);
            else
            {
                _player2 = new Player(name);

                _game = new Game(_player1, _player2);
            }
        }
    }
}
