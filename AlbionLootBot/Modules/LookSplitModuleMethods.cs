using AlbionLootBot.Models;
using Discord.Interactions;

namespace AlbionLootBot.Modules;
public partial class LootSplitModule : InteractionModuleBase<SocketInteractionContext> //this is for work methods within the business logic
{
    protected async Task<Player> ConfirmUserExists(SocketInteractionContext context)
    {
        Player? player = await playerManagement.GetPlayerDBInfo(Context.User);
        if (player != null) return player;
        //if it fails, alert
        throw new UserNotFoundException("User not found in Database");
    }

}
