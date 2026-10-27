using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceChecker : MonoBehaviour
{
    private const string Url = "http://nature4cloud.org:5002/nbiot_ap/TTCyber/9B261005/ttcloud.txt";
    private bool logged = false;

    public async void LogDeviceData()
    {
        if (logged) return;
        
        try
        {
            string resultMessage = "";

            float timeSum = 0;
            int tests = 3;
            for (int i = 0; i < tests; i++)
            {
                float time = await PingServer(Url);
                if (time < 0) return;
                timeSum += await PingServer(Url);
            }
            float averageTime = timeSum / tests;
        
            resultMessage += $"Device Scan Results:\n";
            resultMessage += $"Device Type: {SystemInfo.deviceModel}\n";
            resultMessage += $"Device Name: {SystemInfo.deviceName}\n";
            resultMessage += $"processor: {SystemInfo.processorModel}\n";
            resultMessage += $"Average ping server time over {tests} attempts: {averageTime} ms\n";
            Debug.Log(resultMessage);
            logged = true;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    private static async Task<float> PingServer(string url)
    {
        using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var response = await client.SendAsync(
                new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, url),
                System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
            sw.Stop();
            return (float)sw.ElapsedMilliseconds;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return -1;
        }
    }
}