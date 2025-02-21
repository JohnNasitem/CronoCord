//***********************************************************************************
//Program: DatabaseModule.cs
//Description: Holds database access commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Threading.Tasks;
using Discord.Interactions;
using CronoCord.Utilities;
namespace CronoCord.Modules
{
    public class DatabaseModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseModule"/>
        /// </summary>
        public DatabaseModule() { }



        /// <summary>
        /// Resets the selected database table
        /// </summary>
        /// <returns>Task</returns>
        [RequireOwner]
        [SlashCommand("reset-database", "Reset a database table")]
        public async Task ResetDatabaseCommand(DatabaseManagement.DatabaseTables table)
        {
            bool success = false;
            await Task.Run(() => success = DatabaseManagement.ResetTable(table));
            if (success)
                await RespondAsync($"Successfully reset database");
            else
                await RespondAsync($"Error, Couldn't reset database");
        }
    }
}
