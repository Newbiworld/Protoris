using Discord;

namespace Protoris.Extensions
{
    public static class IGuildUserExtension
    {
        public static string GetNicknameOrUsername(this IGuildUser user)
        {
            return user?.Nickname ?? user?.Username ?? string.Empty;
        }
    }
}
