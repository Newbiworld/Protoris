# Protoris Music Bot

This is my personal music bot, **Protoris**, made public with his code!

You can invite him to your server using this link: https://discord.com/oauth2/authorize?client_id=1038864337386868776&permissions=0&integration_type=0&scope=bot+applications.commands

## How to host it?

If you don't want to have my bot, but to host it, here's how to do it!

Prerequisite:
- Visual studio 2026: https://visualstudio.microsoft.com/insiders/?rwnlp=fr
- LavaLink https://github.com/lavalink-devs/Lavalink (You can find video on how to install it)
- Java 17+ : https://www.azul.com/downloads/?package=jdk#zulu

Once both are installed and you've pulled this repos, you'll need to go to Program.cs and change the config to point it to your lavalink application
After that, you'll need to copy sample.settings.json and rename it to local.settings.json. Once it's done, you'll need to fill in your config

FilePath: The place where the bot will log errors and save files
BotTestingGround: The server Id of where you can do some testing with your bot
BotToken: The token of your bot
IsBetaTesting: If you want to add new features to your bot, turn it on so the commands won't go in every channel
HasEmotes:  Keep it at false if you don't want emotes. The bot answers with emotes often, but since it depends on your bot, you'll need to implement them all. But that's long and blergh, so set it to false so it won't use them


For the lavalink, here's how my application.yml looks like, it'll allow you to play youtube links easily:

server:
  port: 2333

lavalink:
  server:
    password: "ezelprotoris!"
    sources:
      youtube: false
      local: true
  plugins:
    # Replace VERSION with the current version as shown by the Releases tab or a long commit hash for snapshots.
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.16.0"
      snapshot: false # Set to true if you want to use a snapshot version.
plugins:
  youtube:
      enabled: true # Whether this source can be used.
      allowSearch: true # Whether "ytsearch:" and "ytmsearch:" can be used.
      allowDirectVideoIds: true # Whether just video IDs can match. If false, only complete URLs will be loaded.
      allowDirectPlaylistIds: true # Whether just playlist IDs can match. If false, only complete URLs will be loaded.
      # The clients to use for track loading. See below for a list of valid clients.
      # Clients are queried in the order they are given (so the first client is queried first and so on...)
      clients:
        - MUSIC
        - ANDROID_VR
        - WEB
        - WEBEMBEDDED 

I've also added a .ps1 file in the same folder as the .jar looking like this, so I can run it via a powershell:

java -jar Lavalink.jar

