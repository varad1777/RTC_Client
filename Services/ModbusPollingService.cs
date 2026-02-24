using System.IO.Ports;
using NModbus;
using NModbus.Serial;
using ModbusRtuWebApi.Models;

namespace ModbusRtuWebApi.Services;

public class ModbusPollingService : BackgroundService
{
    public static ModbusData LiveData { get; private set; } = new();

    // const string SERIAL_PORT = "COM4";
    const string SERIAL_PORT = "/dev/serial0";
    const int BAUD = 9600;
    const byte SLAVE_ID = 1;

    const ushort REG_VOLTAGE = 0;
    const ushort REG_TEMPERATURE = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var port = new SerialPort(SERIAL_PORT, BAUD, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                port.Open();
                Console.WriteLine($"[INFO] Connected to {SERIAL_PORT}");

                var factory = new ModbusFactory();

                var adapter = new SerialPortAdapter(port);
                using var master = factory.CreateRtuMaster(adapter);

                master.Transport.Retries = 2;
                master.Transport.WaitToRetryMilliseconds = 200;

                while (!stoppingToken.IsCancellationRequested)
                {
                    ushort[] v = master.ReadHoldingRegisters(SLAVE_ID, REG_VOLTAGE, 2);
                    ushort[] t = master.ReadHoldingRegisters(SLAVE_ID, REG_TEMPERATURE, 2);

                    float voltage = RegsToFloat(v[0], v[1]);
                    float temperature = RegsToFloat(t[0], t[1]);

                    LiveData = new ModbusData
                    {
                        Voltage = voltage,
                        Temperature = temperature,
                        Timestamp = DateTime.Now
                    };

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"[{LiveData.Timestamp:HH:mm:ss}] ");
                    Console.ResetColor();
                    Console.WriteLine($"Voltage={voltage:F2}V  Temp={temperature:F2}°C");

                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex} — reconnecting...");
                Console.ResetColor();

                await Task.Delay(3000, stoppingToken);
            }
        }
    }

    static float RegsToFloat(ushort hi, ushort lo)
    {
        uint raw = ((uint)hi << 16) | lo;
        return BitConverter.ToSingle(BitConverter.GetBytes(raw), 0);
    }
}
