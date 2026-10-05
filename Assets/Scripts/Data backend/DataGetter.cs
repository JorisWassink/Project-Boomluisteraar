using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class DataGetter : MonoBehaviour
{
    private static readonly HttpClient HttpClient = new HttpClient();
    
    public List<DataPoint> GetData(string id, int maxPoints)
    {
        string dataUrl = $"http://nature4cloud.org:5002/nbiot_ap/TTCyber/{id}/ttcloud.txt";
        return Task.Run(() => GetDataAsync(dataUrl, maxPoints)).Result; // still blocks, but no deadlock
    }

    public bool IsDataAvailable(string id)
    {
        string dataUrl = $"http://nature4cloud.org:5002/nbiot_ap/TTCyber/{id}/ttcloud.txt";
        return Task.Run(() => IsDataAvailableAsync(dataUrl)).Result;
    }

    private static async Task<bool> IsDataAvailableAsync(string dataUrl)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(dataUrl);
        return response.IsSuccessStatusCode;
    }

    private static async Task<List<DataPoint>> GetDataAsync(string dataUrl, int maxPoints)
    {
        Debug.Log($"Getting {maxPoints} lines of data");
        string rawText = await HttpClient.GetStringAsync(dataUrl);
        return ConvertStringToDataPoints(rawText, maxPoints);
    }

    private static List<DataPoint> ConvertStringToDataPoints(string rawText, int maxPoints)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new List<DataPoint>();

        var result = new List<DataPoint>(maxPoints);
        int lineEnd = rawText.Length;
        int count = 0;

        // while (lineEnd > 0 && count < maxPoints)
        while (lineEnd > 0)
        {
            int newlineIndex = rawText.LastIndexOf('\n', lineEnd - 1);
            int lineStart = newlineIndex + 1;
            int length = lineEnd - lineStart;

            if (length > 0 && rawText[lineStart + length - 1] == '\r')
                length--;

            if (length > 0)
            {
                var line = rawText.AsSpan(lineStart, length);
                var parts = line.ToString().Split(';'); 

                result.Add(new DataPoint
                {
                    date = GetPart(parts, 0),
                    V2 = GetPart(parts, 1),
                    V3 = ParseInt(GetPart(parts, 2)),
                    V4 = GetPart(parts, 3),
                    V5 = ParseDouble(GetPart(parts, 4)),
                    V6 = ParseDouble(GetPart(parts, 5)),
                    V7 = ParseDouble(GetPart(parts, 6)),
                    V8 = ParseDouble(GetPart(parts, 7)),
                    V9 = ParseDouble(GetPart(parts, 8)),
                    V10 = ParseDouble(GetPart(parts, 9)),
                    V11 = ParseDouble(GetPart(parts, 10)),
                    V12 = ParseDouble(GetPart(parts, 11)),
                    V13 = ParseDouble(GetPart(parts, 12)),
                    V14 = ParseDouble(GetPart(parts, 13)),
                    V15 = ParseDouble(GetPart(parts, 14)),
                    V16 = ParseDouble(GetPart(parts, 15)),
                    V17 = ParseDouble(GetPart(parts, 16)),
                    V18 = ParseDouble(GetPart(parts, 17)),
                    V19 = ParseDouble(GetPart(parts, 18)),
                    V20 = ParseDouble(GetPart(parts, 19)),
                    V21 = ParseDouble(GetPart(parts, 20)),
                    V22 = ParseDouble(GetPart(parts, 21)),
                    V23 = ParseDouble(GetPart(parts, 22)),
                    V24 = ParseDouble(GetPart(parts, 23)),
                });
                count++;
            }

            lineEnd = newlineIndex; // -1 when no more newlines; loop condition catches it
        }

        result.Reverse(); // we collected newest-to-oldest, flip to chronological order
        return result;
    }
    private static string GetPart(string[] parts, int index) => index < parts.Length ? parts[index].Trim() : "";

    private static double ParseDouble(string value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0;

    private static int ParseInt(string value) =>
        int.TryParse(value, out var result) ? result : 0;
}
    
public class DataPoint
{
    public string date { get; set; } = "";
    public string V2 { get; set; } = "";
    public int V3 { get; set; }
    public string V4 { get; set; } = "";
    public double V5 { get; set; }
    public double V6 { get; set; }
    public double V7 { get; set; }
    public double V8 { get; set; }
    public double V9 { get; set; }
    public double V10 { get; set; }
    public double V11 { get; set; }
    public double V12 { get; set; }
    public double V13 { get; set; }
    public double V14 { get; set; }
    public double V15 { get; set; }
    public double V16 { get; set; }
    public double V17 { get; set; }
    public double V18 { get; set; }
    public double V19 { get; set; }
    public double V20 { get; set; }
    public double V21 { get; set; }
    public double V22 { get; set; }
    public double V23 { get; set; }
    public double V24 { get; set; }
    public string StartMeasuring { get; set; } = "";
    public string BoomNr { get; set; } = "";
    public string Soort { get; set; } = "";
    public int Plantjaar { get; set; }
    public string DataLink { get; set; } = "";
}

public class SapReadingList
{
    List<SapReading> SapReadings = new List<SapReading>();
    public SapReading averageReading;

    public SapReadingList(DataPoint string4, DataPoint string5)
    {
        if (string4.V3 != 4 && string5.V3 != 5)
            Debug.LogError("Provided Datapoint is no Sap reading");

        SapReadings.Add(new SapReading(string4.V5, string4.V6, string4.V7, string4.V8));
        SapReadings.Add(new SapReading(string4.V9, string4.V10, string4.V11, string4.V12));
        SapReadings.Add(new SapReading(string4.V13, string4.V14, string4.V15, string4.V16));
        SapReadings.Add(new SapReading(string4.V17, string4.V18, string4.V19, string4.V20));
        SapReadings.Add(new SapReading(string4.V21, string4.V22, string4.V23, string4.V24));
        
        SapReadings.Add(new SapReading(string5.V5, string5.V6, string5.V7, string5.V8));
        SapReadings.Add(new SapReading(string5.V9, string5.V10, string5.V11, string5.V12));
        SapReadings.Add(new SapReading(string5.V13, string5.V14, string5.V15, string5.V16));
        SapReadings.Add(new SapReading(string5.V17, string5.V18, string5.V19, string5.V20));
        SapReadings.Add(new SapReading(string5.V21, string5.V22, string5.V23, string5.V24));
        
        averageReading = new SapReading(
            SapReadings.First().TimeIndex,   
            SapReadings.Average(r => r.DownProbe),
            SapReadings.Average(r => r.HeatProbe),
            SapReadings.Average(r => r.UpProbe)
        );
    }
}

public class SapReading
{
    public double TimeIndex { get; set; }
    public double DownProbe  { get; set; } 
    public double HeatProbe { get; set; } 
    public double UpProbe  { get; set; } 

    public SapReading(double ti, double dp, double hp, double up)
    {
        TimeIndex = ti;
        DownProbe = dp;
        HeatProbe = hp;
        UpProbe = up;
    }
}
