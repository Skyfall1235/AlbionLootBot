using AlbionLootBot.Database;
using AlbionLootBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


Console.WriteLine($"this Discord bot is now setting up...");
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
});

builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    return new InteractionService(client);
});


// Login and Start
Env.TraversePath().Load("lootbot.env");
//if (File.Exists("lootbot.env")) Env.Load();


//ADD THE NORMAL SERVICES
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "lootbot.db";
builder.Services.AddDbContextFactory<LootBotDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddSingleton<JsonService>();
builder.Services.AddSingleton<IConfigService, ConfigService>();
builder.Services.AddSingleton<AdminService>();
builder.Services.AddSingleton<LootSplitService>();



Console.WriteLine($"Services added, now building app...");
using IHost host = builder.Build();


using (var scope = host.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LootBotDbContext>>();
    using var context = factory.CreateDbContext();
    context.Database.Migrate();
    Console.WriteLine("✅ Database schema verified and updated successfully!");
}

var client = host.Services.GetRequiredService<DiscordSocketClient>();
InteractionService? interactionService = host.Services.GetRequiredService<InteractionService>();

client.Ready += async () =>
{
    try
    {
        await interactionService.AddModulesAsync(typeof(Program).Assembly, host.Services);
        await Task.Delay(3000);
        var guilds = client.Guilds;
        if (guilds != null)
        {
            foreach (SocketGuild guild in guilds)
            {
                await interactionService.RegisterCommandsToGuildAsync(guild.Id);
                Console.WriteLine($"✅ Registered {interactionService.Modules.Count} modules to {guild.Name}");
                Console.WriteLine($"✅ Connected to {guild.Name} ({guild.Id}) and registered commands!");
            }
        }
        else
        {
            Console.WriteLine("⚠️ The bot isn't in any servers yet! Invite it to your server first.");
        }
        Console.WriteLine($"✅ Connected all guilds! Count: {client.Guilds.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"READY HANDLER FAILED: {ex}");
    }

};

client.JoinedGuild += async (SocketGuild guild) =>
{
    try
    {
        // The modules are already loaded via Ready, so we just map them to the new guild ID immediately
        await interactionService.RegisterCommandsToGuildAsync(guild.Id);
        Console.WriteLine($"🎉 Joined new server and registered commands to: {guild.Name} ({guild.Id})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Failed to register commands on join for {guild.Name}: {ex.Message}");
    }
};

client.SlashCommandExecuted += async (interaction) =>
{
    var ctx = new SocketInteractionContext(client, interaction);

    var user = ctx.User;
    var commandName = interaction.Data.Name;

    await interactionService.ExecuteCommandAsync(ctx, host.Services);
};

interactionService.SlashCommandExecuted += CommandLogger.LogCommandCall;

string apiKey = Environment.GetEnvironmentVariable("DiscordToken") ?? throw new Exception("Discord token Environment variable not found");
Console.WriteLine($"Discord token found, Bot will now begin!");

client.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };
interactionService.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };

await client.LoginAsync(TokenType.Bot, apiKey);
await client.StartAsync();

await host.RunAsync();

//make the inputs for silver to take Ks instead of full ,000
//if a person adds to a lootsplit, add that user to the split
//add value should be simplified
//change from dashs to camel case for commands
//help command required inmprovemenrt
//add name to coinbags so we can track people silver***
//calculate and close should be seperate (and calcandsClose)
//add remove omney to cut shares early
//cut in people for later lootsaplits so they only get loot for what they joined in later or whatever

