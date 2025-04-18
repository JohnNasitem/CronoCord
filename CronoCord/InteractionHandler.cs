﻿//***********************************************************************************
//Program: InteractionHandler.cs
//Description: Interaction Handler from the discord.net repo
//Src: https://github.com/discord-net/Discord.Net/blob/dev/samples/InteractionFramework/InteractionHandler.cs
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using CronoCord.Interactions.Modals;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using CronoCord.Classes;
using CronoCord.Interactions.MessageComponents;

namespace CronoCord
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
        {
            _client = client;
            _handler = handler;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Process when the client is ready, so we can register our commands.
            _client.Ready += ReadyAsync;
            // Process modal submissions
            _client.ModalSubmitted += ModalSubmitted;
            // Process select selections
            _client.SelectMenuExecuted += SelectExecuted;
            _client.ButtonExecuted += ButtonExecute;
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            // Also process the result of the command execution.
            _handler.InteractionExecuted += HandleInteractionExecute;
        }

        private async Task ButtonExecute(SocketMessageComponent arg)
        {
            switch (arg.Data.CustomId.Split(':')[0])
            {
                case "edit-schedule-button": 
                    await EditScheduleMessageComponent.ButtonPressed(arg);
                    break;
            }
        }

        private async Task SelectExecuted(SocketMessageComponent arg)
        {
            switch (arg.Data.CustomId)
            {
                case "edit-schedule-menu":
                    string[] selectData = string.Join("", arg.Data.Values).Split(',');
                    Availability availability = new Availability(arg.User.Id, long.Parse(selectData[0]), long.Parse(selectData[1]), (Availability.Recurring)Enum.Parse(typeof(Availability.Recurring), selectData[2]));
                    await arg.RespondWithModalAsync(new EditAvailabilityModal(availability).Build());
                    break;
            }
        }

        private async Task ModalSubmitted(SocketModal modal)
        {
            switch (modal.Data.CustomId.Split(':')[0])
            {
                case "create_availability":
                    await CreateAvailabilityModal.ModelSubmit(modal);
                    break;
                case "edit_availability":
                    await EditAvailabilityModal.ModelSubmit(modal);
                    break;
            }
        }

        private async Task ReadyAsync()
        {
            // Register the commands globally.
            // alternatively you can use _handler.RegisterCommandsGloballyAsync() to register commands to a specific guild.
            try
            {
                await _handler.RegisterCommandsGloballyAsync();
            }
            catch (Exception ex)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Error registering commands globally: {ex.Message}", UtilityMethods.LogLevel.Error);
                UtilityMethods.PrettyConsoleWriteLine(ex.StackTrace, UtilityMethods.LogLevel.Trace);
            }

            UtilityMethods.PrettyConsoleWriteLine($"{_client.CurrentUser} is connected!", UtilityMethods.LogLevel.SystemMessage);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
                var context = new SocketInteractionContext(_client, interaction);

                // Execute the incoming command.
                var result = await _handler.ExecuteCommandAsync(context, _services);

                // Due to async nature of InteractionFramework, the result here may always be success.
                // That's why we also need to handle the InteractionExecuted event.
                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            // Handle the case where the precondition for the command is not met
                            // This can be due to user permissions, missing arguments, or other conditions
                            await interaction.RespondAsync("You do not meet the preconditions to run this command.");
                            break;
                        case InteractionCommandError.UnknownCommand:
                            // Happens when the uses submits a modal, code this runs fine but just putting this here so error doesnt get logged
                            break;
                        default:
                            UtilityMethods.PrettyConsoleWriteLine($"Unknown problem occured in during an interaction create: Error: {result.Error} - {result.ErrorReason}.", UtilityMethods.LogLevel.Error);
                            break;
                    }
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // Handle the case where the precondition for the command is not met
                        // This can be due to user permissions, missing arguments, or other conditions
                        context.Interaction.RespondAsync("You do not meet the preconditions to execute this command.");
                        break;
                    case InteractionCommandError.Exception:
                        UtilityMethods.PrettyConsoleWriteLine($"Exception occured in {commandInfo.Name}. Exception: {result.ErrorReason}", UtilityMethods.LogLevel.Error);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // Happens when the uses submits a modal, code this runs fine but just putting this here so error doesnt get logged
                        break;
                    default:
                        UtilityMethods.PrettyConsoleWriteLine($"Unknown problem occured in during an interaction execute: Error: {result.Error} - {result.ErrorReason}.", UtilityMethods.LogLevel.Error);
                        break;
                }

            return Task.CompletedTask;
        }
    }
}
