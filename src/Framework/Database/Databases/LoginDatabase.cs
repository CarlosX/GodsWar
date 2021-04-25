
namespace Framework.Database
{
    public class LoginDatabase : MySqlBase<LoginStatements>
    {
        public override void PreparedStatements()
        {
            
        }
    }

    public enum LoginStatements
    {
        SEL_REALMLIST,

        MAX_LOGINDATABASE_STATEMENTS
    }
}
