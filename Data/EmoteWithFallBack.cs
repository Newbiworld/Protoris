using Discord;

namespace Protoris.Data
{
    public class EmoteWithFallBack
    {
        public EmoteWithFallBack(string description)
        {
            FallBackDescription = description;
        }

        public override string ToString()
        {
            if (Emote != null) return Emote.ToString();
            return FallBackDescription;
        }

        public Emote? Emote { get; set; }
        public string FallBackDescription { get; set; }
    }
}
