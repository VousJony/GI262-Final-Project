using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class UGSAnalyticsTracker : MonoBehaviour, IAnalyticsService
{
    private async void Start()
    {
        Initialize();
    }

    public async void Initialize()
    {
        try
        {
            // ทำการ Initialize Unity Services เพื่อเตรียมพร้อมส่งข้อมูล
            await UnityServices.InitializeAsync();

            // เริ่มต้นการเก็บข้อมูล
            AnalyticsService.Instance.StartDataCollection();

            Debug.Log("UGS Analytics Initialized Successfully.");
        }
        catch (ServicesInitializationException e)
        {
            Debug.LogError($"Failed to initialize UGS: {e.Message}");
        }
    }

    public void RecordCustomEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        CustomEvent customEvent = new CustomEvent(eventName);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                // แยกประเภทข้อมูลเพื่อ Add เข้า Custom Event อย่างถูกต้อง
                if (param.Value is string s) customEvent.Add(param.Key, s);
                else if (param.Value is int i) customEvent.Add(param.Key, i);
                else if (param.Value is float f) customEvent.Add(param.Key, f);
                else if (param.Value is bool b) customEvent.Add(param.Key, b);
            }
        }

        AnalyticsService.Instance.RecordEvent(customEvent);
        Debug.Log($"[Analytics] Recorded Event: {eventName}");
    }

    public void FlushData()
    {
        // บังคับส่งข้อมูลที่ค้างอยู่ใน Cache ขึ้น Server ทันที
        AnalyticsService.Instance.Flush();
    }
}