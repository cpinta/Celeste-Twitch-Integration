//using Celeste.Mod.Hyperline;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;

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
        public static string botUsername;
        public static string twitchChannelName;

        public static Dictionary<string, TwitchCommand> commands = new Dictionary<string, TwitchCommand>();

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

            password = iniConfig.Get("OAUTH");
            botUsername = iniConfig.Get("BOT_NAME");
            twitchChannelName = iniConfig.Get("CHANNEL_NAME");

            commands.Add("haircolor", new ChangeHairColorCommand("haircolor"));
            commands.Add("hairspeed", new ChangeHairSpeedCommand("hairspeed"));
            commands.Add("hairlength", new ChangeHairLengthCommand("hairlength"));

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
                    string[] command = split[2].Split(" ");

                    if (commands.ContainsKey(command[0]))
                    {
                        commands[command[0]].ProcessOptions(command.Skip(1).ToArray());
                    }
                }
            }
        }



        public override void Unload()
        {
            // TODO: unapply any hooks applied in Load()
        }
    }

    public abstract class TwitchCommand
    {
        public string name;

        public TwitchCommand(string name)
        {
            this.name = name;
        }


        public abstract void ProcessOptions(string[] args);
    }

    public class ChangeHairLengthCommand : TwitchCommand
    {

        public ChangeHairLengthCommand(string name) : base(name) { }

        public override void ProcessOptions(string[] args)
        {
            if(args.Length == 2)
            {
                if (int.TryParse(args[0], out int slot))
                {
                    if (int.TryParse(args[1], out int length))
                    {
                        Hyperline.Hyperline.Instance.UI.SetHairLength(slot, length);
                    }
                }
            }
        }
    }

    public class ChangeHairSpeedCommand : TwitchCommand
    {

        public ChangeHairSpeedCommand(string name) : base(name) { }

        public override void ProcessOptions(string[] args)
        {
            if (args.Length == 2)
            {
                if (int.TryParse(args[0], out int slot))
                {
                    if (int.TryParse(args[1], out int speed))
                    {
                        Hyperline.Hyperline.Instance.UI.SetHairSpeed(slot, speed);
                    }
                }
            }
        }
    }

    public class ChangeHairColorCommand : TwitchCommand
    {

        public ChangeHairColorCommand(string name) : base(name) { }

        public override void ProcessOptions(string[] args)
        {
            if (args.Length == 2)
            {
                if (int.TryParse(args[0], out int slot))
                {
                    System.Drawing.Color ogColor = System.Drawing.Color.FromName(args[1]);
                    Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(ogColor.R, ogColor.G, ogColor.B);
                    Hyperline.Hyperline.Instance.lastColor = color;
                }
            }
        }
    }
}