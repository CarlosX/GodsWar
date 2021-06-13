using Framework.Constants;
using Framework.Database;
using LoginServer.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking
{
    public partial class LoginSession : IDisposable
    {
        public LoginSession(uint id, string name, LoginSocket sock)
        {
            _socket = sock;
            _accountId = id;
            _accountName = name;
            expireTime = 60000; // 1 min after socket loss, session is deleted

            m_Address = sock.GetRemoteIpAddress().Address.ToString();
            ResetTimeOutTime(false);
            //DB.Login.Execute("UPDATE account SET online = 1 WHERE id = {0};", GetAccountId());     // One-time query
        }

        public void Dispose()
        {
            // unload player if not unloaded
            /*if (_player)
                LogoutPlayer(true);*/

            // - If have unclosed socket, close it
            if (_socket != null)
            {
                _socket.CloseSocket();
                _socket = null;
            }

            // empty incoming packet queue
            LoginPacket packet;
            while (_recvQueue.TryDequeue(out packet)) ;

            //DB.Login.Execute("UPDATE account SET online = 0 WHERE id = {0};", GetAccountId());     // One-time query
        }
        public string GetPlayerInfo()
        {
            return "-";
        }
        public string GetRemoteAddress() { return m_Address; }
        public void SetInQueue(bool state) { m_inQueue = state; }
        public bool IsLogingOut() { return _logoutTime != 0 || m_playerLogout; }

        public AsyncCallbackProcessor<QueryCallback> GetQueryProcessor() { return _queryProcessor; }

        void SetLogoutStartTime(long requestTime)
        {
            _logoutTime = requestTime;
        }

        internal void KickPlayer()
        {
            throw new NotImplementedException();
        }

        public uint GetAccountId()
        {
            return _accountId;
        }

        bool ShouldLogOut(long currTime)
        {
            return (_logoutTime > 0 && currTime >= _logoutTime + 20);
        }

        void ProcessQueryCallbacks()
        {

        }

        TransactionCallback AddTransactionCallback(TransactionCallback callback)
        {
            return _transactionCallbacks.AddCallback(callback);
        }

        public void InitializeSession()
        {

        }
        void InitializeSessionCallback(SQLQueryHolder<AccountInfoQueryLoad> holder)
        {



            holder = null;
        }

        public bool Update(uint diff, LoginSessionFilter updater)
        {
            return true;
        }

        public void QueuePacket(LoginPacket packet)
        {

        }

        public bool GetPlayer()
        {
            return false;
        }

        public uint GetLatency() { return m_latency; }
        public void SetLatency(uint latency) { m_latency = latency; }
        public void ResetClientTimeDelay() { m_clientTimeDelay = 0; }
        public void ResetTimeOutTime(bool onlyActive)
        {
            if (GetPlayer())
                m_timeOutTime = GameTime.GetGameTime() + LoginConfig.GetIntValue(LoginCfg.SocketTimeoutTimeActive);
            else if (!onlyActive)
                m_timeOutTime = GameTime.GetGameTime() + LoginConfig.GetIntValue(LoginCfg.SocketTimeoutTime);
        }
        bool IsConnectionIdle()
        {
            return m_timeOutTime < GameTime.GetGameTime() && !m_inQueue;
        }

        public static implicit operator bool(LoginSession session)
        {
            return session != null;
        }

        public void SendPacket(byte[] data)
        {
            _socket.SendPacket(data);
        }

        #region Fields
        LoginSocket _socket;
        string m_Address;
        uint _accountId;
        string _accountName;
        long _logoutTime;
        bool m_inQueue;
        long m_timeOutTime;
        bool m_playerLogout;

        uint expireTime;
        bool forceExit;

        uint m_latency;
        uint m_clientTimeDelay;

        ConcurrentQueue<LoginPacket> _recvQueue = new();
        Task<SQLQueryHolder<AccountInfoQueryLoad>> _accountLoginCallback;

        AsyncCallbackProcessor<QueryCallback> _queryProcessor = new();
        AsyncCallbackProcessor<TransactionCallback> _transactionCallbacks = new();

        #endregion
    }

    struct PacketCounter
    {
        public long lastReceiveTime;
        public uint amountCounter;
    }


    class AccountInfoQueryHolder : SQLQueryHolder<AccountInfoQueryLoad>
    {
        public void Initialize(uint accountId)
        {

        }
    }

    enum AccountInfoQueryLoad
    {

    }
}
