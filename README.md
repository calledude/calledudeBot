# calledudeBot

A personal bot I wrote because there weren't any other options out there that let you interact with viewers easily if you only have one monitor. This bot will help mitigate the ever so frustating problem of having to alt-tab constantly.

## Required:
##### The bot will prompt you if any of these are missing.
* **[obs-websocket plugin](https://github.com/Palakis/obs-websocket/releases)**
* **BotNick** - The name of the bot you created on Twitch.tv
* **ChannelName** - Your twitch-name with a # in front of it basically
* **OsuNick** - Your osu!-nickname
* **AnnounceChannelID** - The channelID in which you want the bot to announce when your stream goes live.
* **DiscordToken** - The token for the discord bot you created.
* **TwitchIRC** - The Twitch IRC token
* **osuIRC** - The osu!-IRC token
* **osuAPI** - The osu!-API token
* **StreamerID** - Your userID on discord.

## Functionality:
* twitch -> osu! relaying of messages
* osu! -> twitch relay
* Announcing when stream goes live on discord
* Various commands 
  * !np/!song/!playing
  * !uptime
  * !help/!cmds/!commands
* Ability to add/delete custom commands with !addcmd and !delcmd
* Requesting osu! beatmaps in twitch chat (relayed to osu!)
* osu! rank updates displayed in twitch chat
* Automation of initial setup
* Verification of credentials

## Planned:
* Disconnection watcher
* Spam protection
* Wiki - To explain how to use the bot and how to set it up.
* Encryption of API keys
* Hopefully get Spotify integration going again

## Development information:
This project uses a modified version of `obs-websocket-dotnet` and my fork of it can be found [here](https://github.com/calledude/obs-websocket-dotnet). The .dll is however included for your convenience.

#### Disclaimer:

Now, I realize next to no-one will ever read this, but I want it to be said either way.

I created it firstly because I only have one monitor and tabbing out to read chat and whatnot became a hassle. With that in mind, I had some prior programming experience and wanted to see if I could do it, as a learning experience of sorts.
At first it was a very simple program and I felt like I knew what I was doing. This changed when I wanted to start experimenting, test my limits and knowledge. 
At this point I'm basically just throwing things around, so if you're trying to understand the codebase, expect to get angry since I'm re-structuring pretty much all the time. It's because I'm not the best programmer that exist in this world, I simply don't have all the knowledge and this is my way to improve.
Why did you even read this?
