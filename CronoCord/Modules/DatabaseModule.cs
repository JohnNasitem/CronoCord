//***********************************************************************************
//Program: DatabaseModule.cs
//Description: Holds database access commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



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
            try
            {
                bool success = false;
                await Task.Run(() => success = DatabaseManagement.ResetTable(table));
                if (success)
                    await RespondAsync($"Successfully reset database", ephemeral: true);
                else
                    await RespondAsync($"Error, Couldn't reset database", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in ResetDatabaseCommand(). Exception: {ex}", UtilityMethods.LogLevel.Error);
            }
        }



        /// <summary>
        /// Resets the selected database table
        /// </summary>
        /// <returns>Task</returns>
        [RequireOwner]
        [SlashCommand("clean-up-database", "Removes old entries in the specified table")]
        public async Task CleanUpDatabaseCommand(DatabaseManagement.DatabaseTables table)
        {
            try
            {
                List<int> deletedEntries = new List<int>();
                long sundayUnix = UtilityMethods.GetSundayUnixTimeStamp(true);

                // Clean up availabilites table
                if (table == DatabaseManagement.DatabaseTables.Availabilities || table == DatabaseManagement.DatabaseTables.All)
                    await Task.Run(() => deletedEntries.Add(DatabaseManagement.CleanUpAvailabilities(sundayUnix)));

                string message = "";

                if (deletedEntries.Count == 0)
                    message = "Specified table doesnt seem to have any clean up implemented";
                else if (deletedEntries.Any(i => i == -1))
                    message = "Error, One of the specified databases couldn't be cleaned up";
                else
                    message = $"Successfully cleaned up database(s) - Removed entries: {deletedEntries.Sum()}";

                await RespondAsync(message, ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in CleanUpDatabaseCommand(). Exception: {ex}", UtilityMethods.LogLevel.Error);
            }
        }
    }
}
