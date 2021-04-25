namespace Framework.Database
{
    public static class DB
    {
        public static LoginDatabase Login = new LoginDatabase();
        public static CharacterDatabase Characters = new CharacterDatabase();
        public static WorldDatabase World = new WorldDatabase();
    }
}
