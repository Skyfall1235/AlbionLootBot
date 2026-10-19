using Discord;
using Discord.Interactions;

namespace AlbionLootBot.Models;
public class DiscordResponse
{
    // store the response buildiong stuff in this class
    //turn a calc entity into usable stuff rfor the person
    public static async Task SendCalculationResultAsync(SocketInteractionContext context, CalculationEntity calc, string sessionName)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"✅ Split Completed: {sessionName}")
            .WithDescription("The split session has been successfully finalized and calculated.")
            .WithColor(Color.Green)
            .AddField("📊 Financial Summary",
                $"• **Gross Total:** {calc.ItemTotal:N0} Silver\n" +
                $"• **Tax ({calc.TaxRatePercent}%):** -{calc.TotalTaxAmount:N0} Silver\n" +
                $"• **Net Pool:** {calc.NetTotalAfterTax:N0} Silver",
                inline: false)
            .AddField("👥 Payout Distribution",
                $"• **Recipients:** {calc.RecipientCount}\n" +
                $"• **Per Person Net Share:** **{calc.PerPersonNetShare:N0}** Silver\n" +
                (calc.Remainder > 0 ? $"• **Leftover Remainder:** {calc.Remainder:N2} Silver" : ""),
                inline: false)
            .WithFooter($"Calculation ID: {calc.Id}")
            .WithCurrentTimestamp()
            .Build();

        // Send as a followup since the command was likely deferred
        await context.Interaction.FollowupAsync(embed: embed);
    }
    //display a text yes or no box
}
