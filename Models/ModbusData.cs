namespace ModbusRtuWebApi.Models;

public class ModbusData
{
    public float Voltage { get; set; }
    public float Temperature { get; set; }
    public DateTime Timestamp { get; set; }
}
