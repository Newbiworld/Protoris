using Discord;
using Protoris.Data;

namespace Protoris.Service
{
    public static class DiscordComponentHelper
    {
        public static ButtonBuilder CreateButton(EmoteWithFallBack emoteWithFallBack, string id, ButtonStyle style)
        {
            return emoteWithFallBack.Emote == null
                ? CreateButton(emoteWithFallBack.FallBackDescription, id, style)
                : CreateButton(emoteWithFallBack.Emote, id, style);
        }

        private static ButtonBuilder CreateButton(Emote emote, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithEmote(emote);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }

        public static ButtonBuilder CreateButton(string label, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithLabel(label);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }
    }
}
