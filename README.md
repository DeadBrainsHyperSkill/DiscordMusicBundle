# DiscordMusicBundle

gRPC based music bots balancer over IPC socket.

*Initialize bots until you run out of RAM.*

![Instances](https://cdn.discordapp.com/attachments/997659771475869696/997676934785941544/Instances.png)

## Features

- 7 Commands
  - `/play`
  - `/skip`
  - `/stop`
  - `/queue`
  - `/pause`
  - `/resume`
  - `/seek`

- Youtube Autocomplete 

![Youtube Autocomplete](https://cdn.discordapp.com/attachments/997659771475869696/997676956592132166/Youtube_Autocomplete.gif)

- Auto Disconnect
 
  - *Bot leaves the voice channel if there are no users or the queue is empty.*

## Prerequisites

- [.NET Runtime 6 & ASP.NET Core Runtime 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Lavalink](https://github.com/freyacodes/Lavalink)
 
## Setup

1. [Configure and run Lavalink server](https://github.com/freyacodes/Lavalink#server-configuration)
...
