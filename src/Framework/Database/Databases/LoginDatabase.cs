
namespace Framework.Database
{
    public class LoginDatabase : MySqlBase<LoginStatements>
    {
        public override void PreparedStatements()
        {
            PrepareStatement(LoginStatements.SEL_ACCOUNT_INFO_BY_USERNAME, "SELECT id, username, password FROM users WHERE username = ?");
        }
    }

    public enum LoginStatements
    {
        SEL_REALMLIST,
        SEL_ACCOUNT_INFO_BY_USERNAME,

        MAX_LOGINDATABASE_STATEMENTS
    }
}
