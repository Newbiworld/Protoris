using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoris.Service
{
    public static class DiscordComponentHelper
    {
        public static ButtonBuilder CreateButton(Emote emote, string id, ButtonStyle style)
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
