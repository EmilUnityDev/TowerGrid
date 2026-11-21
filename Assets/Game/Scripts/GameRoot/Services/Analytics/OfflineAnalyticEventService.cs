using System.Collections.Generic;

namespace Game.Scripts.GameRoot.Services.Analytics
{
    public sealed class OfflineAnalyticEventService : IAnalyticEventService
    {
        public void SendEvent(AnalyticEventType eventType) { }

        public void SendEvent(AnalyticEventType eventType, Dictionary<string, string> extraParams) { }

        public void SendEvent(string eventName) { }

        public void SendEvent(string eventName, Dictionary<string, string> extraParams) { }
    }
}
