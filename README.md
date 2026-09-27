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

```yaml
server:
  port: 2333

lavalink:
  server:
    password: "ezelprotoris!"
    sources:
      youtube: false
      local: true
      spotify: true # Enable Spotify source
      bandcamp: true
      soundcloud: true
	  
  plugins:
    # Replace VERSION with the current version as shown by the Releases tab or a long commit hash for snapshots.
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.18.2"
      snapshot: false # Set to true if you want to use a snapshot version.
      
    - dependency: "com.github.topi314.lavasrc:lavasrc-plugin:4.8.3"
      repository: "https://maven.lavalink.dev/releases" # this is optional for lavalink v4.0.0-beta.5 or greater
      snapshot: false # set to true if you want to use snapshot builds (see below)
  
    - dependency: "com.github.topi314.lavasearch:lavasearch-plugin:1.0.0"
      repository: "https://maven.lavalink.dev/releases" # this is optional for lavalink v4.0.0-beta.5 or greater
      snapshot: false # set to true if you want to use snapshot builds (see below)
	  
plugins:
  youtube:
    remoteCipher:
      url: "https://cipher.kikkia.dev/"
      userAgent: "your_service_name" # Optional

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
      - TV
      - ANDROID_MUSIC
        
  lavasrc:
    sources:
      spotify: true # Enable Spotify source
      youtube: true # Enable YouTube search source (https://github.com/topi314/LavaSearch)
    
    providers: # Custom providers for track loading. This is the default
      # - "dzisrc:%ISRC%" # Deezer ISRC provider
      # - "dzsearch:%QUERY%" # Deezer search provider
      - "ytsearch:\"%ISRC%\"" # Will be ignored if track does not have an ISRC. See https://en.wikipedia.org/wiki/International_Standard_Recording_Code
      - "ytsearch:%QUERY%" # Will be used if track has no ISRC or no track could be found for the ISRC
      #  you can add multiple other fallback sources here  
    spotify:
        # clientId & clientSecret are required for using spsearch
        clientId: "CLIENT ID HERE"
        clientSecret: "CLIENT SECRET HERE"
        # spDc: "your sp dc cookie" # the sp dc cookie used for accessing the spotify lyrics api
        countryCode: "CA" # the country code you want to use for filtering the artists top tracks. See https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2
        playlistLoadLimit: 6 # The number of pages at 100 tracks each
        albumLoadLimit: 6 # The number of pages at 50 tracks each
        resolveArtistsInSearch: true # Whether to resolve artists in track search results (can be slow)
        localFiles: false # Enable local files support with Spotify playlists. Please note `uri` & `isrc` will be `null` & `identifier` will be `"local"`
        preferAnonymousToken: false # Whether to use the anonymous token for resolving tracks, artists and albums. Spotify generated playlists are always resolved with the anonymous tokens since they do not work otherwise. This requires the customTokenEndpoint to be set.
        customTokenEndpoint: "http://localhost:8080/api/token" # Optional custom endpoint for getting the anonymous token. If not set, spotify's default endpoint will be used which might not work. The response must match spotify's anonymous token response format.
```
I've also added a .ps1 file in the same folder as the .jar looking like this, so I can run it via a powershell:

java -jar Lavalink.jar

