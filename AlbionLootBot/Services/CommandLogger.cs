using Discord;
using Discord.Interactions;

namespace AlbionLootBot.Services;
public class CommandLogger
{
    //input literally super basic logging, so guild id, player, command, and how it resolved, if it did.


    //so this logger needs to kick to console, wrash a crtashlog, notify the user if something has failed.
    //but the main goal of this is who called what commands, and did they succeed?

    //interfecpt a slahs command?
    public static async Task LogCommandCall(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        var user = context.User;
        var commandName = info.Name;

        string consoleLine =
            result.IsSuccess ?
            $"[LOG] User {user.Username} ({user.Id}) successfully executed /{commandName}"
            : $"[LOG] User {user.Username} ({user.Id}) failed to execute /{commandName}: {result.ErrorReason}";
        Console.WriteLine(consoleLine);


        await Task.CompletedTask;
    }

    //save to db in da future
    //protected async Task LogCommandCallToDbAsync() { }


}
