using Framework.Constants;
using Framework.Constants.Network;
using Framework.Database;
using Game.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Networking
{
    public partial class WorldSession : IDisposable
    {
        public WorldSession(uint id, string name, WorldSocket sock)
        {
            _loginSocket = sock;
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
            if (_loginSocket != null)
            {
                _loginSocket.CloseSocket();
                _loginSocket = null;
            }

            // empty incoming packet queue
            WorldPacket packet;
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

        public bool Update(uint diff)
        {
            WorldPacket firstDelayedPacket = null;
            uint processedPackets = 0;
            long currentTime = Time.UnixTime;

            WorldPacket packet;
            while (_loginSocket != null && !_recvQueue.IsEmpty && (_recvQueue.TryPeek(out packet) && packet != firstDelayedPacket) && _recvQueue.TryDequeue(out packet))
            {
                try
                {

                    var handler = PacketManager.GetHandler((ClientOpcodes)packet.GetOpcode());
                    switch (handler.sessionStatus)
                    {
                        case SessionStatus.Loggedin:
                            handler.Invoke(this, packet);
                            break;
                        default:
                            handler.Invoke(this, packet);
                            break;
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                processedPackets++;

                if (processedPackets > 100)
                    break;
            }

            //ProcessQueryCallbacks();

            return true;
        }

        public void QueuePacket(WorldPacket packet)
        {
            _recvQueue.Enqueue(packet);
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
                m_timeOutTime = GameTime.GetGameTime() + WorldConfig.GetIntValue(WorldCfg.SocketTimeoutTimeActive);
            else if (!onlyActive)
                m_timeOutTime = GameTime.GetGameTime() + WorldConfig.GetIntValue(WorldCfg.SocketTimeoutTime);
        }
        bool IsConnectionIdle()
        {
            return m_timeOutTime < GameTime.GetGameTime() && !m_inQueue;
        }

        public static implicit operator bool(WorldSession session)
        {
            return session != null;
        }

        private void SendPacket(ServerPacket data)
        {
            _loginSocket.SendPacket(data);
        }

        public void SendPacket(byte[] data)
        {
            _loginSocket.SendPacket(data);
        }

        #region Fields
        WorldSocket _loginSocket;
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

        ConcurrentQueue<WorldPacket> _recvQueue = new();
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
