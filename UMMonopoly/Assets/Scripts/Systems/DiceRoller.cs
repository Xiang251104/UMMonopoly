namespace UMMonopoly.Systems
{
    public class DiceRoller
    {
        private readonly System.Random _rng;
        private bool _overridden;

        public int[] LastRoll { get; private set; }
        public int LastTotal { get; private set; }
        public bool LastWasDoubles { get; private set; }

        public int DiceCount { get; }
        public int Sides { get; }

        public DiceRoller(int diceCount, int sides, int seed = 0)
        {
            DiceCount = diceCount;
            Sides = sides;
            _rng = seed == 0 ? new System.Random() : new System.Random(seed);
            LastRoll = new int[diceCount];
        }

        public int Roll()
        {
            // If physical dice already set the result, use it instead of re-randomising
            if (_overridden)
            {
                _overridden = false;
                return LastTotal;
            }

            int total = 0;
            for (int i = 0; i < DiceCount; i++)
            {
                LastRoll[i] = _rng.Next(1, Sides + 1);
                total += LastRoll[i];
            }
            LastTotal = total;
            LastWasDoubles = DiceCount == 2 && LastRoll[0] == LastRoll[1];
            return total;
        }

        /// <summary>
        /// Call before RollAndMove() when physical dice provide the result.
        /// The next Roll() call will consume these values instead of randomising.
        /// </summary>
        public void OverrideResult(int die1, int die2)
        {
            LastRoll = new int[] { die1, die2 };
            LastTotal = die1 + die2;
            LastWasDoubles = die1 == die2;
            _overridden = true;
        }
    }
}
