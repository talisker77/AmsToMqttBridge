using KaifaHanReader.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaifaHanReaderService.Services
{
    public class ReaderService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<ReaderService> _logger;
        private Timer? _timer = null;
        private readonly SerialPort vPort;

        public ReaderService(ILogger<ReaderService> logger)
        {
            _logger = logger;
            vPort = new("/dev/ttyUSB0", 2400, Parity.Even, 8, StopBits.One);
            _logger.LogDebug("vPort created");
        }

        private void VPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _logger.LogDebug("Setting up data reciever");
            var vPort = sender as SerialPort;
            byte[] vBuffer = new byte[1024];
            var bufferList = new List<byte>();
            int vBytesRead = vPort.Read(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i < vBytesRead; i++)
            {
                bufferList.Add(vBuffer[i]);

                // If we're catching a '7E' and it's not the beginning, it must be the end
                if (bufferList.Count > 1 && vBuffer[i] == 0x7e)
                {
                    var data = WriteAndEmptyBuffer(bufferList);
                    _logger.LogInformation("Data: {@data}", data);
                }
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reader Service Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(1)
                , TimeSpan.Zero);

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                var count = Interlocked.Increment(ref executionCount);

                vPort.DataReceived += VPort_DataReceived;
                vPort.Open();
                _logger.LogInformation("Reader Service is working. Count: {Count}", count);

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    _logger.LogDebug("Pulse...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure");
                _ = _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.Zero);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reader Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private static ReaderData? WriteAndEmptyBuffer(List<byte> gBuffer)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Received {gBuffer.Count} (0x{gBuffer.Count:X2}) bytes]");
            var reader = new KaifaHanReader.Readers.KaifaHanReader(gBuffer.ToArray());
            if (reader.IsValid())
            {
                Console.WriteLine("Received bytes is valid");
            }
            else
            {
                Console.WriteLine("Received bytes not valid");
                return null;
            }
            var data = reader.Anaylyze();
            data.ToConsole();
            gBuffer.Clear();
            return data;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
