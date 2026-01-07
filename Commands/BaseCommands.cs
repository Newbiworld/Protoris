using Discord;
using Discord.Interactions;

namespace Protoris.Commands
{
    public class BaseCommands : InteractionModuleBase
    {
        protected IMessageChannel GetInteractionChannel()
        {
            return Context.Channel; // What happens if it's in dms?
        }

        protected IGuildUser? GetUser()
        {
            if (Context.User is IGuildUser) return Context.User as IGuildUser;
            return null;
        }
    }
}
