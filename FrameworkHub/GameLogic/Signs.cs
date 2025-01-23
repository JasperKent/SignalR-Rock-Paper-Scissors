namespace FrameworkHub.GameLogic
{
    public enum Sign { Rock, Paper, Scissors }

    public static class Signs
    {
        static readonly private bool?[,] _beats;

        public static bool? Beats(Sign s1, Sign s2) => _beats[(int)s1, (int)s2];

        public static int Count => (int)Sign.Scissors + 1;

        static Signs()
        {
                                  //             Rock,  Paper, Scissors        
            _beats = new bool?[,] /*Rock*/    {{ null,  false, true},  
                                  /*Paper*/    { true,  null,  false}, 
                                  /*Scissors*/ { false, true,  null}};
        }
    }
}
