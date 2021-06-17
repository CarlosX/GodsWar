using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Constants
{
    public enum LoginCfg
    {
        EnableSinfoLogin,
        MaxResultsLookupCommands,
        CacheDataQueries,
        IpBasedActionLogging,
        PacketSpoofBanduration,
        PacketSpoofBanmode,
        UptimeUpdate,
        SocketTimeoutTime,
        SocketTimeoutTimeActive,
        SessionAddDelay,
        PortLogin,
        PortInstance,
        IntervalDisconnectTolerance,
        IntervalSave
    }
    public enum WorldCfg
    {
        EnableSinfoLogin,
        MaxResultsLookupCommands,
        CacheDataQueries,
        IpBasedActionLogging,
        PacketSpoofBanduration,
        PacketSpoofBanmode,
        UptimeUpdate,
        SocketTimeoutTime,
        SocketTimeoutTimeActive,
        SessionAddDelay,
        PortWorld,
        PortInstance,
        IntervalDisconnectTolerance,
        IntervalSave
    }

    public enum ComparisionType
    {
        EQ = 0,
        High,
        Low,
        HighEQ,
        LowEQ,
        Max
    }
}
