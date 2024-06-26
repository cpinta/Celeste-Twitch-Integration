using MonoMod.ModInterop;
using System;

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


        public static string ip = "irc.chat.twitch.tv";
        public static int port = 6667;
        public static string password = "oauth:";
        public static string botUsername = "pinta_bot";

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

            // TODO: apply any hooks that should always be active
        }

        public override void Unload()
        {
            // TODO: unapply any hooks applied in Load()
        }
    }
}