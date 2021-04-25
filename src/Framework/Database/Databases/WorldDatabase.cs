 
namespace Framework.Database
{
    public class WorldDatabase : MySqlBase<WorldStatements>
    {
        public override void PreparedStatements()
        {
            
        }
    }

    public enum WorldStatements
    {
        

        MAX_WORLDDATABASE_STATEMENTS
    }
}
