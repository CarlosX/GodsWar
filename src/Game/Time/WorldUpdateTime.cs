using Framework;
using Game.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class WorldUpdateTime : UpdateTime
    {
        uint _recordUpdateTimeInverval;
        uint _recordUpdateTimeMin;
        uint _lastRecordTime;

        public void LoadFromConfig()
        {
            _recordUpdateTimeInverval = WorldConfig.GetDefaultValue("RecordUpdateTimeDiffInterval", 60000u);
            _recordUpdateTimeMin = WorldConfig.GetDefaultValue("MinRecordUpdateTimeDiff", 100u);
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
