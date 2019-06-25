﻿using InteractiveSeven.Core.Data;
using InteractiveSeven.Core.Data.Items;
using InteractiveSeven.Core.Memory;
using InteractiveSeven.Core.Model;
using InteractiveSeven.Core.Models;
using InteractiveSeven.Twitch.Commands;
using InteractiveSeven.Twitch.Model;
using Moq;
using System.Linq;
using TwitchLib.Client.Interfaces;
using Xunit;

namespace UnitTests.Twitch.Commands
{
    public class WeaponCommandShould
    {
        [Fact]
        public void ChangeCharacterWeapon_GivenValidCallAndEnoughGil()
        {
            var (characterName, weaponNumber) = (CharNames.Cloud.DefaultName, 1);
            var (commandData, gilBank, eqAccessor, itemAccessor, chat) = SetUpTest(1000, characterName, weaponNumber.ToString());
            var weaponCommand = new WeaponCommand(eqAccessor.Object, itemAccessor.Object, null, gilBank, chat.Object, new EquipmentData<Weapons>());

            weaponCommand.Execute(commandData);

            eqAccessor.Verify(x => x.SetCharacterEquipment(CharNames.Cloud, It.IsAny<byte>(), m => m.Weapon.Address), Times.Once);
        }

        [Fact]
        public void ReportError_GivenInvalidCommandArgs()
        {
            var (commandData, gilBank, eqAccessor, itemAccessor, chat) = SetUpTest(1000, "cloud");
            var weaponCommand = new WeaponCommand(eqAccessor.Object, itemAccessor.Object, null, gilBank, chat.Object, new EquipmentData<Weapons>());

            weaponCommand.Execute(commandData);

            chat.Verify(x => x.SendMessage(commandData.Channel, It.IsAny<string>(), false), Times.Once);
            eqAccessor.Verify(x => x.SetCharacterEquipment(It.IsAny<CharNames>(), It.IsAny<byte>(), m => m.Weapon.Address), Times.Never);
        }

        [Fact]
        public void ReportError_GivenInsufficientGil()
        {
            var (commandData, gilBank, eqAccessor, itemAccessor, chat) = SetUpTest(0, "cloud", "1");
            var weaponCommand = new WeaponCommand(eqAccessor.Object, itemAccessor.Object, null, gilBank, chat.Object, new EquipmentData<Weapons>());

            weaponCommand.Execute(commandData);

            chat.Verify(x => x.SendMessage(commandData.Channel, It.IsAny<string>(), false), Times.Once);
            eqAccessor.Verify(x => x.SetCharacterEquipment(It.IsAny<CharNames>(), It.IsAny<byte>(), m => m.Weapon.Address), Times.Never);
        }

        private (CommandData data, GilBank gilBank,
            Mock<IEquipmentAccessor> eqAccessor, Mock<IInventoryAccessor> itemAccessor,
            Mock<ITwitchClient> chat)
            SetUpTest(int gil, params string[] args)
        {
            var eqAccessor = new Mock<IEquipmentAccessor>();
            var itemAccessor = new Mock<IInventoryAccessor>();
            var chat = new Mock<ITwitchClient>();
            var gilBank = new GilBank();
            var chatUser = new ChatUser { Username = "Fred" };
            gilBank.Deposit(chatUser, gil);
            var commandData = new CommandData
            {
                User = chatUser,
                Arguments = args.ToList()
            };
            return (commandData, gilBank, eqAccessor, itemAccessor, chat);
        }
    }
}