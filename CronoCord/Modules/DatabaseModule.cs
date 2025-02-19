//***********************************************************************************
//Program: DatabaseModule.cs
//Description: Holds database access commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using CronoCord.Utilities;
using System.Runtime.InteropServices;

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
        public async Task ResetDatabase(DatabaseManagement.DatabaseTables table)
        {
            if (DatabaseManagement.ResetTable(table))
            {
                await RespondAsync($"Successfully reset database");
            }
            else
            {
                await RespondAsync($"Error, Couldn't reset database");
            }
        }
    }
}
