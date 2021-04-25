using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Database
{
    public class PreparedStatement
    {
        public string CommandText;
        public Dictionary<int, object> Parameters = new Dictionary<int, object>();

        public PreparedStatement(string commandText)
        {
            CommandText = commandText;
        }

        public void AddValue(int index, object value)
        {
            Parameters.Add(index, value);
        }

        public void Clear()
        {
            Parameters.Clear();
        }
    }

    public class PreparedStatementTask : ISqlOperation
    {
        PreparedStatement m_stmt;
        bool _needsResult;
        TaskCompletionSource<SQLResult> m_result;

        public PreparedStatementTask(PreparedStatement stmt, bool needsResult = false)
        {
            m_stmt = stmt;
            _needsResult = needsResult;
            if (needsResult)
                m_result = new TaskCompletionSource<SQLResult>();
        }

        public bool Execute<T>(MySqlBase<T> mySqlBase)
        {
            if (_needsResult)
            {
                SQLResult result = mySqlBase.Query(m_stmt);
                if (result == null)
                {
                    m_result.SetResult(new SQLResult());
                    return false;
                }

                m_result.SetResult(result);
                return true;
            }

            return mySqlBase.DirectExecute(m_stmt);
        }

        public Task<SQLResult> GetFuture() { return m_result.Task; }
    }
}
