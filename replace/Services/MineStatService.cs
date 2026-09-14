using MineStatLib;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ASimpleMinecraftUpdatesBot.Services
{
    public class MineStatService
    {
        public async Task<bool> GetAliveStatus(BotConfig config)
        {
            MineStat ms = await GetMineStat(config.MinecraftIp, config.Port);
            Console.WriteLine($"Get alive status: {ms.ServerUp}");
            return ms.ServerUp;
        }

        public async Task<MCServerStatus> GetFullStatus(BotConfig config)
        {
            Console.WriteLine($"Current Config {config.ToString}");
            MineStat ms = await GetMineStat(config.MinecraftIp, config.Port);
            if (!ms.ServerUp)
            {
                Console.WriteLine($"Full Status: MC Server Offline");
                return new MCServerStatus();
            }

            //if it doesnt fail to find the server, go on
            string players = $"{ms.CurrentPlayers}/{ms.MaximumPlayers}";
            MCServerStatus status = new MCServerStatus(players);
            Console.WriteLine($"Full Status: MC Server has {players}");
            return status;
        }
        public async Task<string[]> GetPlayerList(BotConfig config)
        {
            MineStat ms = await GetMineStat(config.MinecraftIp, config.Port);
            if (!ms.ServerUp)
            {
                Console.WriteLine($"Full Status: MC Server Offline");
                return [];
            }
            return ms.PlayerList;
        }

        public async Task<bool> GetServerUp(BotConfig config)
        {
            MineStat ms = await GetMineStat(config.MinecraftIp, config.Port);
            return ms.ServerUp;
        }

        private async Task<MineStat> GetMineStat(string ip, ushort port)
        {
            MineStat ms = await Task.Run(() => new MineStat(ip, port));
            return ms;
        }

        public async Task<bool> PingComputerAsync(string ip, int port = 25565)
        {
            Console.WriteLine($"Starting TCP ping test to {ip}:{port}");

            using var client = new TcpClient();

            var connectTask = client.ConnectAsync(ip, port);
            var timeoutTask = Task.Delay(3000);

            Console.WriteLine("Attempting connection...");

            var completed = await Task.WhenAny(connectTask, timeoutTask);

            if (completed == timeoutTask)
            {
                Console.WriteLine("Connection attempt timed out.");
                return false;
            }

            if (client.Connected)
            {
                Console.WriteLine("Connection successful!");
                return true;
            }
            else
            {
                Console.WriteLine("Connection failed.");
                return false;
            }
        }
    }

    public struct MCServerStatus
    {
        public MCServerStatus(string currentOverMaxPlayers)
        {
            IsOnline = true;
            CurrentOverMaxPlayers = currentOverMaxPlayers;
        }

        public bool IsOnline { get; set; }
        public string CurrentOverMaxPlayers { get; set; }
    }
}
