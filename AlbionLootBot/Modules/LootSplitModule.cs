using AlbionLootBot.Services;
using Discord.Interactions;

namespace AlbionLootBot.Modules
{
    public class LootSplitModule(InteractionService interactionService, AdminService adminService, LootSplitService splitService) : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _interactionService = interactionService;
        private readonly AdminService adminService = adminService;
        private readonly LootSplitService splitService = splitService;
        //add member
        [SlashCommand("AddSplitParticipant", "Adda a member to a given lootsplit")]
        public async Task AddMemberToSplit()
        {

        }
        //+ remove member from split,
        [SlashCommand("AddSplitParticipant", "")]
        public async Task AddSplitParticipant()
        {

        }
        //make split,
        [SlashCommand("CreateSplit", "")]
        public async Task CreateSplit()
        {

        }
        //delete split,
        [SlashCommand("DeleteSplit", "")]
        public async Task DeleteSplit()
        {

        }
        //add to split total w/ items,
        [SlashCommand("AddItemValue", "")]
        public async Task AddItemValueToSplit()
        {

        }
        //add to split total with silver bags.
        [SlashCommand("AddSilverValue", "")]
        public async Task AddSilverValueToSplit()
        {

        }
        //calculate split,
        [SlashCommand("CalculateSplit", "")]
        public async Task CalculateSplit()
        {

        }



    }
}