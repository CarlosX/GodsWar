using Framework;
using LoginServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class LoginUpdateTime : UpdateTime
    {
        uint _recordUpdateTimeInverval;
        uint _recordUpdateTimeMin;
        uint _lastRecordTime;

        public void LoadFromConfig()
        {
            _recordUpdateTimeInverval = LoginConfig.GetDefaultValue("RecordUpdateTimeDiffInterval", 60000u);
            _recordUpdateTimeMin = LoginConfig.GetDefaultValue("MinRecordUpdateTimeDiff", 100u);
        }

        public void SetRecordUpdateTimeInterval(uint t)
        {
            _recordUpdateTimeInverval = t;
        }

        public void RecordUpdateTime(uint gameTimeMs, uint diff, uint sessionCount)
        {
            if (_recordUpdateTimeInverval > 0 && diff > _recordUpdateTimeMin)
            {
                if (Time.GetMSTimeDiff(_lastRecordTime, gameTimeMs) > _recordUpdateTimeInverval)
                {
                    Log.outDebug(LogFilter.Misc, $"Update time diff: {GetAverageUpdateTime()}. Players online: {sessionCount}.");
                    _lastRecordTime = gameTimeMs;
                }
            }
        }

        public void RecordUpdateTimeDuration(string text)
        {
            RecordUpdateTimeDuration(text, _recordUpdateTimeMin);
        }
    }
}
