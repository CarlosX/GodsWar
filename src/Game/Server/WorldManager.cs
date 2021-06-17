using Framework.Constants;
using Framework.Database;
using Game.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public class WorldManager : Singleton<WorldManager>
    {
        WorldManager()
        {
            foreach (LoginTimers timer in Enum.GetValues(typeof(LoginTimers)))
                m_timers[timer] = new IntervalTimer();

            _loginUpdateTime = new WorldUpdateTime();
        }

        public void SetInitialWorldSettings()
        {
            LoadConfigSettings();

            Log.outInfo(LogFilter.ServerLoading, "Initializing Opcodes...");
            PacketManager.Initialize();
        }


        public void LoadConfigSettings(bool reload = false)
        {
            WorldConfig.Load(reload);
        }

        public WorldSession FindSession(uint id)
        {
            return sessions.LookupByKey(id);
        }

        bool RemoveSession(uint id)
        {
            // Find the session, kick the user, but we can't delete session at this moment to prevent iterator invalidation
            var session = sessions.LookupByKey(id);

            /*if (session != null)
            {
                if (session.PlayerLoading())
                    return false;

                session.KickPlayer();
            }*/

            return true;
        }

        public void AddSession(WorldSession s)
        {
            addSessQueue.Enqueue(s);
        }

        public void AddInstanceSocket(WorldSocket sock, ulong connectToKey)
        {
            _linkSocketQueue.Enqueue(Tuple.Create(sock, connectToKey));
        }

        void AddSession_(WorldSession s)
        {
            Debugger.Assert(s != null);

            sessions[s.GetAccountId()] = s;
            s.InitializeSession();
        }

        public void Update(uint diff)
        {
            UpdateSessions(diff);

            ProcessQueryCallbacks();
        }

        uint GetQueuePos(WorldSession sess)
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

        void AddQueuedPlayer(WorldSession sess)
        {
            sess.SetInQueue(true);
            m_QueuedPlayer.Add(sess);

            // The 1st SMSG_AUTH_RESPONSE needs to contain other info too.
            //sess.SendAuthResponse(BattlenetRpcErrorCode.Ok, true, GetQueuePos(sess));
        }

        bool RemoveQueuedPlayer(WorldSession sess)
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
                WorldSession pop_sess = m_QueuedPlayer.First();
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
            WorldSession sess;
            while (addSessQueue.TryDequeue(out sess))
                AddSession_(sess);

            // Then send an update signal to remaining ones
            foreach (var pair in sessions)
            {
                WorldSession session = pair.Value;
                if (!session.Update(diff))    // As interval = 0
                {
                    /*if (!RemoveQueuedPlayer(session) && session != null && LoginConfig.GetIntValue(LoginCfg.IntervalDisconnectTolerance) != 0)
                        m_disconnects[session.GetAccountId()] = Time.UnixTime;*/

                    //RemoveQueuedPlayer(session);
                    sessions.TryRemove(pair.Key, out _);
                    session.Dispose();
                }
            }
        }

        void UpdateMaxSessionCounters()
        {
            m_maxActiveSessionCount = Math.Max(m_maxActiveSessionCount, (uint)(sessions.Count - m_QueuedPlayer.Count));
            m_maxQueuedSessionCount = Math.Max(m_maxQueuedSessionCount, (uint)m_QueuedPlayer.Count);
        }

        void ProcessQueryCallbacks()
        {
            _queryProcessor.ProcessReadyCallbacks();
        }

        public List<WorldSession> GetAllSessions()
        {
            return sessions.Values.ToList();
        }

        public int GetActiveAndQueuedSessionCount() { return sessions.Count; }
        public int GetActiveSessionCount() { return sessions.Count - m_QueuedPlayer.Count; }
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

        ConcurrentDictionary<uint, WorldSession> sessions = new();
        Dictionary<uint, long> m_disconnects = new();
        uint m_maxActiveSessionCount;
        uint m_maxQueuedSessionCount;
        uint m_PlayerCount;
        uint m_MaxPlayerCount;
        uint m_playerLimit;

        List<WorldSession> m_QueuedPlayer = new();
        ConcurrentQueue<WorldSession> addSessQueue = new();

        ConcurrentQueue<Tuple<WorldSocket, ulong>> _linkSocketQueue = new();

        AsyncCallbackProcessor<QueryCallback> _queryProcessor = new();

        WorldUpdateTime _loginUpdateTime;
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
