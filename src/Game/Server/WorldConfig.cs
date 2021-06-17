using Framework.Configuration;
using Framework.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public class WorldConfig : ConfigMgr
    {
        public static void Load(bool reload = false)
        {
            if (reload)
                Load("WorldServer.conf");

            if (reload)
            {
                int val = GetDefaultValue("WorldPort", 7000);
                if (val != (int)Values[WorldCfg.PortWorld])
                    Log.outError(LogFilter.ServerLoading, "WorldServerPort option can't be changed at worldserver.conf reload, using current value ({0}).", Values[WorldCfg.PortWorld]);

                val = GetDefaultValue("InstanceServerPort", 7001);
                if (val != (int)Values[WorldCfg.PortInstance])
                    Log.outError(LogFilter.ServerLoading, "InstanceServerPort option can't be changed at worldserver.conf reload, using current value ({0}).", Values[WorldCfg.PortInstance]);
            }
            else
            {
                Values[WorldCfg.PortWorld] = GetDefaultValue("WorldPort", 7000);
                Values[WorldCfg.PortInstance] = GetDefaultValue("InstanceServerPort", 7001);
            }

            // Config values are in "milliseconds" but we handle SocketTimeOut only as "seconds" so divide by 1000
            Values[WorldCfg.SocketTimeoutTime] = GetDefaultValue("SocketTimeOutTime", 900000) / 1000;
            Values[WorldCfg.SocketTimeoutTimeActive] = GetDefaultValue("SocketTimeOutTimeActive", 60000) / 1000;
            Values[WorldCfg.SessionAddDelay] = GetDefaultValue("SessionAddDelay", 10000);

            Values[WorldCfg.UptimeUpdate] = GetDefaultValue("UpdateUptimeInterval", 10);
            if ((int)Values[WorldCfg.UptimeUpdate] <= 0)
            {
                Log.outError(LogFilter.ServerLoading, "UpdateUptimeInterval ({0}) must be > 0, set to default 10.", Values[WorldCfg.UptimeUpdate]);
                Values[WorldCfg.UptimeUpdate] = 10;
            }

            Values[WorldCfg.MaxResultsLookupCommands] = GetDefaultValue("Command.LookupMaxResults", 0);

            Values[WorldCfg.PacketSpoofBanduration] = GetDefaultValue("PacketSpoof.BanDuration", 86400);

            Values[WorldCfg.IpBasedActionLogging] = GetDefaultValue("Allow.IP.Based.Action.Logging", false);

            // Allow to cache data queries
            Values[WorldCfg.CacheDataQueries] = GetDefaultValue("CacheDataQueries", true);
        }

        public static uint GetUIntValue(WorldCfg confi)
        {
            return Convert.ToUInt32(Values.LookupByKey(confi));
        }

        public static int GetIntValue(WorldCfg confi)
        {
            return Convert.ToInt32(Values.LookupByKey(confi));
        }

        public static ulong GetUInt64Value(WorldCfg confi)
        {
            return Convert.ToUInt64(Values.LookupByKey(confi));
        }

        public static bool GetBoolValue(WorldCfg confi)
        {
            return Convert.ToBoolean(Values.LookupByKey(confi));
        }

        public static float GetFloatValue(WorldCfg confi)
        {
            return Convert.ToSingle(Values.LookupByKey(confi));
        }

        public static void SetValue(WorldCfg confi, object value)
        {
            Values[confi] = value;
        }

        static Dictionary<WorldCfg, object> Values = new();
    }
}
