namespace FrameworkHub.GameLogic
{
    public class Player
    {
        public string Name { get; }
        public Sign? Throw { get; set; }
        public int Score { get; set; }

        public Player(string name) => Name = name;
    }
}
