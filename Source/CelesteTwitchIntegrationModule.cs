using Celeste.Mod.Hyperline;
using MonoMod.ModInterop;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Celeste.Mod.CelesteTwitchIntegration
{
    public class CelesteTwitchIntegrationModule : EverestModule
    {
        public static CelesteTwitchIntegrationModule Instance { get; private set; }

        public override Type SettingsType => typeof(CelesteTwitchIntegrationModuleSettings);
        public static CelesteTwitchIntegrationModuleSettings Settings => (CelesteTwitchIntegrationModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(CelesteTwitchIntegrationModuleSession);
        public static CelesteTwitchIntegrationModuleSession Session => (CelesteTwitchIntegrationModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(CelesteTwitchIntegrationModuleSaveData);
        public static CelesteTwitchIntegrationModuleSaveData SaveData => (CelesteTwitchIntegrationModuleSaveData)Instance._SaveData;

        static TcpClient tcpClient = new TcpClient();
        static StreamReader streamReader;
        static StreamWriter streamWriter;

        public static string ip = "irc.chat.twitch.tv";
        public static int port = 6667;
        public static string iniPath = "./.twitch-config.ini";
        public static JSONFile iniConfig = new JSONFile(iniPath);
        public static string password;
        public static string botUsername = "pinta_bot";
        public static string twitchChannelName = "pintalive";

        public CelesteTwitchIntegrationModule()
        {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(CelesteTwitchIntegrationModule), LogLevel.Verbose);

#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(CelesteTwitchIntegrationModule), LogLevel.Info);
#endif
        }

        public override void Load()
        {

            typeof(CelesteTwitchIntegrationExports).ModInterop(); // TODO: delete this line if you do not need to export any functions
            Console.WriteLine("TEST LOG");

            password = iniConfig.Get("OAUTH");
            tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);


            streamReader = new StreamReader(tcpClient.GetStream());
            streamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

            streamWriter.WriteLine($"PASS {password}");
            streamWriter.WriteLine($"NICK {botUsername}");
            streamWriter.WriteLine($"JOIN #{twitchChannelName}");
            // TODO: apply any hooks that should always be active

            ReadTwitchChat();
        }

        public static void GamePausedMessage(Level level, int startIndex, bool minimal, bool quickReset)
        {
            SendTwitchMessage($"the game is paused :D");
        }

        public static void SendTwitchMessage(string message)
        {
            streamWriter.WriteLine($"PRIVMSG #{twitchChannelName} :{message}");
        }

        public async void ReadTwitchChat()
        {
            while (true)
            {
                //:pintalive!pintalive@pintalive.tmi.twitch.tv PRIVMSG #pintalive :rah

                string line = await streamReader.ReadLineAsync();
                string[] split = line.Split(':');
                if (split.Length > 2)
                {
                    Console.WriteLine(split[2]);

                    if (split[2].StartsWith("hair"))
                    {
                        string[] options = split[2].Split(" ");
                        if (options[1] == "yellow")
                        {
                            Hyperline.Hyperline.Instance.UI.SetHairLength(10, 10);
                            
                        }
                    }
                }
            }
        }

        public override void Unload()
        {
            // TODO: unapply any hooks applied in Load()
        }
    }
}