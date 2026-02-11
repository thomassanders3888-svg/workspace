using System;
using System.Threading;
using System.Threading.Tasks;

namespace TerraForgeServer.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           TERRAFORGE SERVER v0.1.0                     ║");
            Console.WriteLine("║     Unity Voxel MMO Server - .NET 8                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                var server = new GameServer();
                await server.InitializeAsync();
                await server.StartAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL] Server crashed: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }
}
