using System.Collections.Generic;

public interface IAnalyticsService
{
    void Initialize();
    void RecordCustomEvent(string eventName, Dictionary<string, object> parameters = null);
    void FlushData();
}