using Framework.Constants;
using Framework.Database;
using LoginServer.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Server
{
    public class LoginManager : Singleton<LoginManager>
    {
        LoginManager()
        {
            foreach (LoginTimers timer in Enum.GetValues(typeof(LoginTimers)))
                m_timers[timer] = new IntervalTimer();

            _loginUpdateTime = new LoginUpdateTime();
        }

        public void SetInitialLoginSettings()
        {
            LoadConfigSettings();
        }


        public void LoadConfigSettings(bool reload = false)
        {
            LoginConfig.Load(reload);
        }

        public LoginSession FindSession(uint id)
        {
            return m_sessions.LookupByKey(id);
        }

        bool RemoveSession(uint id)
        {
            // Find the session, kick the user, but we can't delete session at this moment to prevent iterator invalidation
            var session = m_sessions.LookupByKey(id);

            /*if (session != null)
            {
                if (session.PlayerLoading())
                    return false;

                session.KickPlayer();
            }*/

            return true;
        }

        public void AddSession(LoginSession s)
        {
            addSessQueue.Enqueue(s);
        }

        public void AddInstanceSocket(LoginSocket sock, ulong connectToKey)
        {
            _linkSocketQueue.Enqueue(Tuple.Create(sock, connectToKey));
        }

        void AddSession_(LoginSession s)
        {
            Debugger.Assert(s != null);

            //NOTE - Still there is race condition in WorldSession* being used in the Sockets

            // kick already loaded player with same account (if any) and remove session
            // if player is in loading and want to load again, return
            if (!RemoveSession(s.GetAccountId()))
            {
                s.KickPlayer();
                return;
            }

            // decrease session counts only at not reconnection case
            bool decrease_session = true;

            // if session already exist, prepare to it deleting at next world update
            // NOTE - KickPlayer() should be called on "old" in RemoveSession()
            {
                var old = m_sessions.LookupByKey(s.GetAccountId());
                if (old != null)
                {
                    // prevent decrease sessions count if session queued
                    if (RemoveQueuedPlayer(old))
                        decrease_session = false;
                }
            }

            m_sessions[s.GetAccountId()] = s;

            int Sessions = GetActiveAndQueuedSessionCount();
            uint pLimit = GetPlayerAmountLimit();
            int QueueSize = GetQueuedSessionCount(); //number of players in the queue

            //so we don't count the user trying to
            //login as a session and queue the socket that we are using
            if (decrease_session)
                --Sessions;

            s.InitializeSession();

            UpdateMaxSessionCounters();

            // Updates the population
            if (pLimit > 0)
            {
                float popu = GetActiveSessionCount();              // updated number of users on the server
                popu /= pLimit;
                popu *= 2;
                Log.outInfo(LogFilter.Server, "Server Population ({0}).", popu);
            }
        }

        public void Update(uint diff)
        {
            UpdateSessions(diff);

            ProcessQueryCallbacks();
        }

        void ProcessLinkInstanceSocket(Tuple<LoginSocket, ulong> linkInfo)
        {
            if (!linkInfo.Item1.IsOpen())
                return;
        }

        uint GetQueuePos(LoginSession sess)
        {
            uint position = 1;

            foreach (var iter in m_QueuedPlayer)
            {
                if (iter != sess)
                    ++position;
                else
                    return position;
            }
            return 0;
        }

        void AddQueuedPlayer(LoginSession sess)
        {
            sess.SetInQueue(true);
            m_QueuedPlayer.Add(sess);

            // The 1st SMSG_AUTH_RESPONSE needs to contain other info too.
            //sess.SendAuthResponse(BattlenetRpcErrorCode.Ok, true, GetQueuePos(sess));
        }

        bool RemoveQueuedPlayer(LoginSession sess)
        {
            // sessions count including queued to remove (if removed_session set)
            int sessions = GetActiveSessionCount();

            uint position = 1;

            // search to remove and count skipped positions
            bool found = false;

            foreach (var iter in m_QueuedPlayer)
            {
                if (iter != sess)
                    ++position;
                else
                {
                    sess.SetInQueue(false);
                    sess.ResetTimeOutTime(false);
                    m_QueuedPlayer.Remove(iter);
                    found = true;                                   // removing queued session
                    break;
                }
            }

            // iter point to next socked after removed or end()
            // position store position of removed socket and then new position next socket after removed

            // if session not queued then we need decrease sessions count
            if (!found && sessions != 0)
                --sessions;

            // accept first in queue
            if ((m_playerLimit == 0 || sessions < m_playerLimit) && !m_QueuedPlayer.Empty())
            {
                LoginSession pop_sess = m_QueuedPlayer.First();
                pop_sess.InitializeSession();

                m_QueuedPlayer.RemoveAt(0);

                // update iter to point first queued socket or end() if queue is empty now
                position = 1;
            }

            // update position from iter to end()
            // iter point to first not updated socket, position store new position
            /*foreach (var iter in m_QueuedPlayer)
            {
                iter.SendAuthWaitQue(++position);
            }*/

            return found;
        }

        public void UpdateSessions(uint diff)
        {
            // Add new sessions
            LoginSession sess;
            while (addSessQueue.TryDequeue(out sess))
                AddSession_(sess);

            // Then send an update signal to remaining ones
            foreach (var pair in m_sessions)
            {
                LoginSession session = pair.Value;
                LoginSessionFilter updater = new(session);
                if (!session.Update(diff, updater))    // As interval = 0
                {
                    if (!RemoveQueuedPlayer(session) && session != null && LoginConfig.GetIntValue(LoginCfg.IntervalDisconnectTolerance) != 0)
                        m_disconnects[session.GetAccountId()] = Time.UnixTime;

                    RemoveQueuedPlayer(session);
                    m_sessions.TryRemove(pair.Key, out _);
                    session.Dispose();
                }
            }
        }

        void UpdateMaxSessionCounters()
        {
            m_maxActiveSessionCount = Math.Max(m_maxActiveSessionCount, (uint)(m_sessions.Count - m_QueuedPlayer.Count));
            m_maxQueuedSessionCount = Math.Max(m_maxQueuedSessionCount, (uint)m_QueuedPlayer.Count);
        }

        void ProcessQueryCallbacks()
        {
            _queryProcessor.ProcessReadyCallbacks();
        }

        public List<LoginSession> GetAllSessions()
        {
            return m_sessions.Values.ToList();
        }

        public int GetActiveAndQueuedSessionCount() { return m_sessions.Count; }
        public int GetActiveSessionCount() { return m_sessions.Count - m_QueuedPlayer.Count; }
        public int GetQueuedSessionCount() { return m_QueuedPlayer.Count; }
        // Get the maximum number of parallel sessions on the server since last reboot
        public uint GetMaxQueuedSessionCount() { return m_maxQueuedSessionCount; }
        public uint GetMaxActiveSessionCount() { return m_maxActiveSessionCount; }

        public uint GetPlayerCount() { return m_PlayerCount; }
        public uint GetMaxPlayerCount() { return m_MaxPlayerCount; }
        public void SetPlayerAmountLimit(uint limit) { m_playerLimit = limit; }
        public uint GetPlayerAmountLimit() { return m_playerLimit; }

        public void IncreasePlayerCount()
        {
            m_PlayerCount++;
            m_MaxPlayerCount = Math.Max(m_MaxPlayerCount, m_PlayerCount);
        }
        public void DecreasePlayerCount() { m_PlayerCount--; }

        public bool LoadListRealm()
        {

            return true;
        }

        public bool IsClosed()
        {
            return m_isClosed;
        }

        #region Fields
        uint m_ShutdownTimer;
        public bool IsStopped;

        bool m_isClosed;
        Dictionary<LoginTimers, IntervalTimer> m_timers = new();

        ConcurrentDictionary<uint, LoginSession> m_sessions = new();
        Dictionary<uint, long> m_disconnects = new();
        uint m_maxActiveSessionCount;
        uint m_maxQueuedSessionCount;
        uint m_PlayerCount;
        uint m_MaxPlayerCount;
        uint m_playerLimit;

        List<LoginSession> m_QueuedPlayer = new();
        ConcurrentQueue<LoginSession> addSessQueue = new();

        ConcurrentQueue<Tuple<LoginSocket, ulong>> _linkSocketQueue = new();

        AsyncCallbackProcessor<QueryCallback> _queryProcessor = new();

        LoginUpdateTime _loginUpdateTime;
        #endregion
    }

    public enum LoginTimers
    {
        UpTime,
        CleanDB,
        PingDB,
        Max
    }
}
