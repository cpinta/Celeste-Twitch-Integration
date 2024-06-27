//using Celeste.Mod.Hyperline;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Linq;
using Celeste.Mod.Hyperline;
using Celeste.Mod.Hyperline.Triggers;

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

        public static int hairLength = 1;
        public static int hairSpeed = 1;

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

            commands.Add("color", new ChangeHairColorCommand("color"));
            commands.Add("speed", new ChangeHairSpeedCommand("speed"));
            commands.Add("length", new ChangeHairLengthCommand("length"));

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


        public static HSVColor ColorToHSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            float hue = color.GetHue();
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;

            return new HSVColor(hue, saturation, value);
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
            if(args.Length == 1)
            {
                if (int.TryParse(args[0], out int length))
                {
                    for(int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
                    {
                        Hyperline.Hyperline.Instance.UI.SetHairLength(i, length);
                        CelesteTwitchIntegrationModule.hairLength = length;
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
            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out int speed))
                {
                    for (int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
                    {
                        Hyperline.Hyperline.Instance.UI.SetHairSpeed(i, speed);
                        CelesteTwitchIntegrationModule.hairSpeed = speed;
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
            if (args.Length == 1)
            {
                try
                {
                    Color ogColor = Color.FromName(args[0]);
                    HSVColor hsvcolor = CelesteTwitchIntegrationModule.ColorToHSV(ogColor);

                    Hyperline.Hyperline.Instance.triggerManager.SetTrigger(new SolidHair(hsvcolor), 1, CelesteTwitchIntegrationModule.hairLength, CelesteTwitchIntegrationModule.hairSpeed);
                }
                catch (Exception e)
                {
                        Console.WriteLine("HAIR COLOR SWITCH FAILED WITH ERROR:\n" + e.ToString());
                }
            }
        }
    }
}