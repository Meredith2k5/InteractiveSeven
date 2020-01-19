﻿using InteractiveSeven.Core;
using InteractiveSeven.Core.Diagnostics.Memory;
using InteractiveSeven.Twitch.Model;
using InteractiveSeven.Twitch.Payments;
using System.Linq;
using TwitchLib.Client.Interfaces;

namespace InteractiveSeven.Twitch.Commands
{
    public class GivePlayerGilCommand : BaseCommand
    {
        private readonly PaymentProcessor _paymentProcessor;
        private readonly ITwitchClient _twitchClient;
        private readonly IGilAccessor _gilAccessor;

        public GivePlayerGilCommand(PaymentProcessor paymentProcessor, ITwitchClient twitchClient,
            IGilAccessor gilAccessor)
            : base(x => x.GivePlayerGilCommandWords,
                x => x.EquipmentSettings.PlayerGilSettings.GiveGilEnabled)
        {
            _paymentProcessor = paymentProcessor;
            _twitchClient = twitchClient;
            _gilAccessor = gilAccessor;
        }

        public override void Execute(in CommandData commandData)
        {
            int amount = commandData.Arguments.FirstOrDefault().SafeIntParse();

            if (amount <= 0)
            {
                _twitchClient.SendMessage(commandData.Channel,
                    $"How much of your gil do you want to give to the player, {commandData.User.Username}?");
                return;
            }

            GilTransaction gilTransaction = _paymentProcessor.ProcessPayment(
                commandData, amount,
                Settings.EquipmentSettings.PlayerGilSettings.AllowModOverride);

            if (!gilTransaction.Paid)
            {
                _twitchClient.SendMessage(commandData.Channel,
                    $"You don't have {amount} gil, {commandData.User.Username}.");
                return;
            }

            var gilToAdd = (uint)(amount * Settings.EquipmentSettings.PlayerGilSettings.GiveMultiplier);
            _gilAccessor.AddGil(gilToAdd);
            _twitchClient.SendMessage(commandData.Channel,
                $"Added {gilToAdd} gil for player.");
        }
    }
}