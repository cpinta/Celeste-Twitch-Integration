# CelesteTwitchIntegration
A mod for Celeste that, when the game starts, starts a Twitch bot and allows chat to manipulate your game while you play

## Twitch Chat Commands
These are the commands to use in Twitch chat for this bot:

- help&ensp; Gives descriptions of the following commands
- length \<amount\>&ensp; Changes the hair length to the specified number
- speed \<amount\>&ensp; Changes the hair speed to the specified number
- color \<color name\>&ensp; Changes the hair color to the specified color. Available colors are the ones specified here: [https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.brushes?view=windowsdesktop-8.0]

## How to use the bot:
1. Make a file inside the same place your Celeste.exe is titled :
> .twitch-config.ini

2. format it the following way:
```
{
	"OAUTH": "oauth:example",
	"CHANNEL_NAME": "ludwig",
	"BOT_NAME": "twitchbotname"
}
```
Replace the example values with your appropriate values

3. When you start the game, the bot should start. Enable the Everest console log to see what the bot is outputting

## Credit
This mod utilizes Hyperline to change the hair length, speed, and color. Check it out here: https://github.com/lordseanington/Hyperline
