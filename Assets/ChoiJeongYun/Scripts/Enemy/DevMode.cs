namespace ChoiJeongYun.Scripts.Enemy
{
    public static class DevMode
    {
        public static bool OneHitKillMonsters { get; private set; }
        public static bool CowardMode { get; private set; }

        public static void Enable()
        {
            OneHitKillMonsters = true;
        }

        public static void EnableCowardMode()
        {
            CowardMode = true;
        }
    }
}
