using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DataGetter : MonoBehaviour
{
    static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler { UseProxy = false });
    
    private static readonly Dictionary<StaticVariables.DataType, HashSet<int>> IgnoredTypes = new()
    {
        [StaticVariables.DataType.CarbonSensor] = new() { 3, 6, 7 },
        [StaticVariables.DataType.CyberSensor]  = new() { 3 },
        [StaticVariables.DataType.SoilSensor]   = new() { 4, 5, 6, 7, 8 },
    };
    
    private readonly string latestUrl = $"https://boomluisteraareu-default-rtdb.europe-west1.firebasedatabase.app/feed/latest";

    //Public Functions
    public List<DataPoint> GetData(string id, int maxPoints, StaticVariables.DataType dataType)
    {
        DateTime startTime = DateTime.Now;
        
        List<DataPoint> points = new();
        
        //first get the keys
        string countUrl = $"{latestUrl}/count/{id}.json";
        int dataAmount = GetDataAmount(countUrl);
        
        int newest = dataAmount + 1;
        int oldest = Math.Max(0, dataAmount - maxPoints);
        
        string url = $"{latestUrl}/data/{id}.json" +
                     $"?orderBy=%22%24key%22&startAt=%22{oldest}%22&endAt=%22{newest}%22";
        try
        {
            string json = HttpClient.GetStringAsync(url).Result;
            points = ParseData(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        DateTime endTime = DateTime.Now;
        Debug.Log($"Data collection complete.\n" +
                  $"Total time: {(endTime - startTime).TotalMilliseconds} ms");
        return new List<DataPoint>(points);
    }
    

    private int GetDataAmount(string url)
    {
        string json = HttpClient.GetStringAsync(url).Result;
        if (json == "null") return 0;

        return int.Parse(json);
    }

    //Helpers
    private List<DataPoint> ParseData(string json)
    {
        List<DataPoint> points = new ();
        if (string.IsNullOrEmpty(json) || json == "null") return points;

        var token = JToken.Parse(json);

        if (token is JArray arr)              
        {
            foreach (var item in arr)
                if (item is JObject o) points.Add(ParsePoint(o));
        }
        else if (token is JObject obj)
        {
            if (obj["dataType"] != null)                
                points.Add(ParsePoint(obj));
            else                                       
                foreach (var prop in obj.Properties())
                    if (prop.Value is JObject o) points.Add(ParsePoint(o));
        }

        return points;
    }

    
    private DataPoint ParsePoint(JObject obj)
    {
        return (string)obj["dataType"] switch
        {
            "SapFlow"       => obj.ToObject<SapFlowDataPoint>(),
            "Spectrometer"  => obj.ToObject<SpectrometerDataPoint>(),
            "Spectrometer2" => obj.ToObject<Spectrometer2DataPoint>(),
            "General"          => obj.ToObject<GeneralDataPoint>(),
            "Soil"          => obj.ToObject<SoilDataPoint>(),
            _               => obj.ToObject<DataPoint>(),
        };
    }
    
}

public class DataPoint
{
    public string time { get; set; } = "";
    public string sensorId { get; set; } = "";
    public int stringType { get; set; }
    public string timeStamp { get; set; } = "";
}

public class SapFlowDataPoint : DataPoint
{
    public List<SapReading> SapFlowScans { get; set; } =  new ();
}

public class GeneralDataPoint : DataPoint
{
    public Vec position { get; set; } = new Vec();
    public Vec positionSTD { get; set; } = new Vec();
    public float battery { get; set; }
    public float heaterProbePowerInput { get; set; }
    public float airTemperature { get; set; }
    public float airHumidity { get; set; }
    public float growth { get; set; }
    
    public override string ToString()
    {
        return $"position: {position}\n" +
               $"positionSTD: {positionSTD}\n" +
               $"battery: {battery}\n" +
               $"heaterProbePowerInput: {heaterProbePowerInput}\n" +
               $"airTemperature: {airTemperature}\n" +
               $"airHumidity: {airHumidity}\n" +
               $"growth: {growth}";
    }
}

public class SoilDataPoint : DataPoint
{
    public Vec acceleration { get; set; } = new Vec();
    public Vec accelerationSTD { get; set; } = new Vec();
    public float batteryVoltage { get; set; }    // V
    public float airTemperature { get; set; }    // °C
    public float airHumidity { get; set; }       // %
    public float soilHumidity { get; set; }      // mV (raw)
    public float soilTemperature { get; set; }   // Ohm (raw)

    public override string ToString()
    {
        return $"Data Type: Soil\n" +
               $"acceleration: {acceleration}\n" +
               $"acceleration_std: {accelerationSTD}\n" +
               $"Battery Voltage: {batteryVoltage} V\n" +
               $"Air Temperature: {airTemperature} °C\n" +
               $"Air Relative Humidity: {airHumidity} %\n" +
               $"Soil Humidity 1: {soilHumidity} mV (raw)\n" +
               $"Soil Temperature: {soilTemperature} Ohm (raw)";
    }
}

public class SpectrometerDataPoint : DataPoint
{
    public float gain { get; set; }
    public Dictionary<string, float> channels { get; set; } = new Dictionary<string, float>();

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Data Type: Spectrometer");
        sb.AppendLine($"Gain: {gain}");
        foreach (var kv in channels)
            sb.AppendLine($"\t{kv.Key.Replace("nm", "")}: {kv.Value}");
        return sb.ToString().TrimEnd();
    }
}

public class Spectrometer2DataPoint : SpectrometerDataPoint
{
    public float clearNoCalib { get; set; }
    public float nitNoCalib { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Data Type: Spectrometer (2)");
        sb.AppendLine($"Gain: {gain}");
        foreach (var kv in channels)
            sb.AppendLine($"\t{kv.Key.Replace("nm", "")}: {kv.Value}");
        sb.AppendLine($"CLEAR No Calib: {clearNoCalib}");
        sb.Append($"NIT No Calib: {nitNoCalib}");
        return sb.ToString();
    }
}


public class Vec
{
    public float x, y, z;
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

public class SapReading
{
    [JsonProperty("scanNumber")]      public int scanNumber { get; set; }
    [JsonProperty("timeStamp")]       public double timeIndex { get; set; }
    [JsonProperty("downStreamProbe")] public double downProbe { get; set; }
    [JsonProperty("heater")]          public double heatProbe { get; set; }
    [JsonProperty("upStreamProbe")]   public double upProbe { get; set; }
}