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
using System.Numerics;

namespace Celeste.Mod.PintaTwitchIntegration
{
    public class PintaTwitchIntModule : EverestModule
    {
        public static PintaTwitchIntModule Instance { get; private set; }

        public override Type SettingsType => typeof(PintaTwitchIntModuleSettings);
        public static PintaTwitchIntModuleSettings Settings => (PintaTwitchIntModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(PintaTwitchIntModuleSession);
        public static PintaTwitchIntModuleSession Session => (PintaTwitchIntModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(PintaTwitchIntModuleSaveData);
        public static PintaTwitchIntModuleSaveData SaveData => (PintaTwitchIntModuleSaveData)Instance._SaveData;

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

        public static HairProperties hair;

        public static Dictionary<string, TwitchCommand> commands = new Dictionary<string, TwitchCommand>();

        public static int errorCount = 0;
        public static bool botRunning = false;
        public static bool restartingBot = false;

        public static int MAX_HAIR_LENGTH = 10000;

        public PintaTwitchIntModule()
        {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(PintaTwitchIntModule), LogLevel.Verbose);

#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(CelesteTwitchIntegrationModule), LogLevel.Info);
#endif
        }

        public override void Load()
        {
            typeof(CelesteTwitchIntegrationExports).ModInterop(); // TODO: delete this line if you do not need to export any functions

            Everest.Events.Level.OnLoadLevel += LevelLoaded;

            hair = new HairProperties();
            commands.Add("help", new HelpCommand("help"));
            commands.Add("color", new ChangeHairColorCommand("color"));
            commands.Add("speed", new ChangeHairSpeedCommand("speed"));
            commands.Add("length", new ChangeHairLengthCommand("length"));

            password = iniConfig.Get("OAUTH");
            botUsername = iniConfig.Get("BOT_NAME");
            twitchChannelName = iniConfig.Get("CHANNEL_NAME");

            StartBot();

            ReadTwitchChat();
        }

        public static void StartBot()
        {
            restartingBot = false;
            errorCount = 0;

            tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);

            streamReader = new StreamReader(tcpClient.GetStream());
            streamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

            streamWriter.WriteLine($"PASS {password}");
            streamWriter.WriteLine($"NICK {botUsername}");
            streamWriter.WriteLine($"JOIN #{twitchChannelName}");
            streamWriter.WriteLine($"CAP REQ :twitch.tv/commands twitch.tv/tags");
        }

        public static void GamePausedMessage(Level level, int startIndex, bool minimal, bool quickReset)
        {
            SendTwitchMessage($"the game is paused :D");
        }

        public static void SendTwitchMessage(string message)
        {
            streamWriter.WriteLine($"PRIVMSG #{twitchChannelName} :{message}");
        }
        public static void SendTwitchMessage(string messageID, string message)
        {
            streamWriter.WriteLine($"@reply-parent-msg-id={messageID} PRIVMSG #{twitchChannelName} :{message}");
        }

        public static void LevelLoaded(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (isFromLoader && playerIntro != Player.IntroTypes.Respawn)
            {
                ResetHair();
            }
        }

        public async void ReadTwitchChat()
        {
            botRunning = true;
            while (botRunning)
            {
                try
                {
                    string line = await streamReader.ReadLineAsync();
                    Console.WriteLine(line);
                    if(line.Trim() != "")
                    {
                        if (line.StartsWith("PING"))
                        {
                            streamWriter.WriteLine("PONG :tmi.twitch.tv");
                            Console.WriteLine("Responded to PING with PONG");

                        }
                        else
                        {
                            string[] split = line.Split(':');
                            string messageID = "";
                            try
                            {
                                messageID = line.Split(";id=")[1].Split(";")[0];
                            }
                            catch { }


                            if (split.Length > 2)
                            {
                                Console.WriteLine(split[2]);
                                string[] command = split[2].Split(" ");

                                if (commands.ContainsKey(command[0]))
                                {
                                    commands[command[0]].ProcessOptions(messageID, command.Skip(1).ToArray());
                                }
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR:" + e);
                    errorCount++;
                    if(errorCount > 10)
                    {
                        botRunning = false;
                        restartingBot = true;
                    }
                }
            }
            if (restartingBot)
            {
                StartBot();
            }
        }

        public static void ResetHair()
        {
            hair = new HairProperties();
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


        public virtual void ProcessOptions(string user, string[] args)
        {
            Console.WriteLine($"COMMAND {name} by {user}");
        }
    }

    public class HelpCommand : TwitchCommand
    {
        string text = "'length <number>' to change hair length.    'speed <number>' to change hair speed.    'color <color>' to change hair color";

        public HelpCommand(string name) : base(name) { }

        public override void ProcessOptions(string user, string[] args)
        {
            base.ProcessOptions(user, args);
            PintaTwitchIntModule.SendTwitchMessage(user, text);
        }
    }

    public class ChangeHairLengthCommand : TwitchCommand
    {

        public ChangeHairLengthCommand(string name) : base(name) { }

        public override void ProcessOptions(string user, string[] args)
        {
            base.ProcessOptions(user, args);
            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out int length))
                {
                    if(length <= 0 || length > PintaTwitchIntModule.MAX_HAIR_LENGTH)
                    {
                        return;
                    }
                    for(int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
                    {
                        PintaTwitchIntModule.hair.SetLength(length);
                    }
                }
            }
        }
    }

    public class ChangeHairSpeedCommand : TwitchCommand
    {
        public ChangeHairSpeedCommand(string name) : base(name) { }

        public override void ProcessOptions(string user, string[] args)
        {
            base.ProcessOptions(user, args);
            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out int speed))
                {
                    for (int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
                    {
                        PintaTwitchIntModule.hair.SetSpeed(speed);
                    }
                }
            }
        }
    }

    public class ChangeHairColorCommand : TwitchCommand
    {
        public ChangeHairColorCommand(string name) : base(name) { }

        public override void ProcessOptions(string user, string[] args)
        {
            base.ProcessOptions(user, args);
            if (args.Length == 1)
            {
                try
                {
                    Color ogColor = Color.FromName(args[0]);
                    HSVColor hsvcolor = PintaTwitchIntModule.ColorToHSV(ogColor);

                    PintaTwitchIntModule.hair.SetType(new SolidHair(hsvcolor));
                }
                catch (Exception e)
                {
                        Console.WriteLine("HAIR COLOR SWITCH FAILED WITH ERROR:\n" + e.ToString());
                }
            }
        }
    }

    public class HairProperties
    {
        public int dashCount;
        public int length;
        public int speed;
        // 0 71 67
        public HairProperties()
        {
            length = 4;
            speed = 10;

            SetHair(new SolidHair().CreateNew(0), 0);
            SetHair(new SolidHair().CreateNew(1), 1);
            SetHair(new SolidHair().CreateNew(2), 2);
        }

        public void SetType(IHairType hairType)
        {
            Hyperline.Hyperline.Instance.triggerManager.SetTrigger(hairType, 1, length, speed);
        }
        public void SetLength(int length)
        {
            this.length = length;
            for (int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
            {
                Hyperline.Hyperline.Instance.triggerManager.SetTrigger(Hyperline.Hyperline.Instance.triggerManager.GetHair(i), i, length, speed);
            }
        }
        public void SetSpeed(int speed)
        {
            this.speed = speed;
            for (int i = 0; i < Hyperline.Hyperline.MAX_DASH_COUNT; i++)
            {
                Hyperline.Hyperline.Instance.triggerManager.SetTrigger(Hyperline.Hyperline.Instance.triggerManager.GetHair(i), i, length, speed);
            }
        }

        void SetHair(IHairType hairType, int slot)
        {
            Hyperline.Hyperline.Instance.triggerManager.SetTrigger(hairType, slot, length, speed);
        }

        public IHairType GetHair(int slot)
        {
            return Hyperline.Hyperline.Instance.triggerManager.GetHair(slot);
        }
    }
}