using Framework.Configuration;
using Framework.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Server
{
    public class LoginConfig : ConfigMgr
    {
        public static void Load(bool reload = false)
        {
            if (reload)
                Load("LoginServer.conf");

            if (reload)
            {
                int val = GetDefaultValue("LoginPort", 5999);
                if (val != (int)Values[LoginCfg.PortLogin])
                    Log.outError(LogFilter.ServerLoading, "LoginServerPort option can't be changed at worldserver.conf reload, using current value ({0}).", Values[LoginCfg.PortLogin]);

                val = GetDefaultValue("InstanceServerPort", 6000);
                if (val != (int)Values[LoginCfg.PortInstance])
                    Log.outError(LogFilter.ServerLoading, "InstanceServerPort option can't be changed at worldserver.conf reload, using current value ({0}).", Values[LoginCfg.PortInstance]);
            }
            else
            {
                Values[LoginCfg.PortLogin] = GetDefaultValue("LoginPort", 5999);
                Values[LoginCfg.PortInstance] = GetDefaultValue("InstanceServerPort", 6000);
            }

            // Config values are in "milliseconds" but we handle SocketTimeOut only as "seconds" so divide by 1000
            Values[LoginCfg.SocketTimeoutTime] = GetDefaultValue("SocketTimeOutTime", 900000) / 1000;
            Values[LoginCfg.SocketTimeoutTimeActive] = GetDefaultValue("SocketTimeOutTimeActive", 60000) / 1000;
            Values[LoginCfg.SessionAddDelay] = GetDefaultValue("SessionAddDelay", 10000);

            Values[LoginCfg.UptimeUpdate] = GetDefaultValue("UpdateUptimeInterval", 10);
            if ((int)Values[LoginCfg.UptimeUpdate] <= 0)
            {
                Log.outError(LogFilter.ServerLoading, "UpdateUptimeInterval ({0}) must be > 0, set to default 10.", Values[LoginCfg.UptimeUpdate]);
                Values[LoginCfg.UptimeUpdate] = 10;
            }

            Values[LoginCfg.MaxResultsLookupCommands] = GetDefaultValue("Command.LookupMaxResults", 0);

            Values[LoginCfg.PacketSpoofBanduration] = GetDefaultValue("PacketSpoof.BanDuration", 86400);

            Values[LoginCfg.IpBasedActionLogging] = GetDefaultValue("Allow.IP.Based.Action.Logging", false);

            // Allow to cache data queries
            Values[LoginCfg.CacheDataQueries] = GetDefaultValue("CacheDataQueries", true);
        }

        public static uint GetUIntValue(LoginCfg confi)
        {
            return Convert.ToUInt32(Values.LookupByKey(confi));
        }

        public static int GetIntValue(LoginCfg confi)
        {
            return Convert.ToInt32(Values.LookupByKey(confi));
        }

        public static ulong GetUInt64Value(LoginCfg confi)
        {
            return Convert.ToUInt64(Values.LookupByKey(confi));
        }

        public static bool GetBoolValue(LoginCfg confi)
        {
            return Convert.ToBoolean(Values.LookupByKey(confi));
        }

        public static float GetFloatValue(LoginCfg confi)
        {
            return Convert.ToSingle(Values.LookupByKey(confi));
        }

        public static void SetValue(LoginCfg confi, object value)
        {
            Values[confi] = value;
        }

        static Dictionary<LoginCfg, object> Values = new();
    }
}
