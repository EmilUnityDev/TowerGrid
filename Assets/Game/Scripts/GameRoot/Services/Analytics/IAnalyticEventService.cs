using System.Collections.Generic;

namespace Game.Scripts.GameRoot.Services.Analytics
{
    public interface IAnalyticEventService
    {
        void SendEvent(AnalyticEventType eventType);
        void SendEvent(AnalyticEventType eventType,Dictionary<string, string> extraParams);
        void SendEvent(string eventName);
        void SendEvent(string eventName,Dictionary<string, string> extraParams);
    }
}