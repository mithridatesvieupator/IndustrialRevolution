using Helpers;
using IndustrialRevolution.Philanthropist;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace IndustrialRevolution.SellTroops
{
    public class SellTroops : CampaignBehaviorBase
    {
        // Tracks the last total shown via DisplayMessage so we only fire a new
        // notification when the amount actually changes (one message per drag).
        private int _lastNotifiedGold = -1;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption("town", "sell_troops", "{=IR_SELL_TROOPS}Sell troops", new GameMenuOption.OnConditionDelegate(this.sell_soldiers_condition), new GameMenuOption.OnConsequenceDelegate(this.sell_soldiers_consequence), false, -1, false, null);
            starter.AddGameMenuOption("village", "sell_troops", "{=IR_SELL_TROOPS}Sell troops", new GameMenuOption.OnConditionDelegate(this.sell_soldiers_condition), new GameMenuOption.OnConsequenceDelegate(this.sell_soldiers_consequence), false, -1, false, null);
            starter.AddGameMenuOption("castle", "sell_troops", "{=IR_SELL_TROOPS}Sell troops", new GameMenuOption.OnConditionDelegate(this.sell_soldiers_condition), new GameMenuOption.OnConsequenceDelegate(this.sell_soldiers_consequence), false, -1, false, null);
        }

        private bool sell_soldiers_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return true;
        }

        private void sell_soldiers_consequence(MenuCallbackArgs args)
        {
            _lastNotifiedGold = -1;
            this.OpenSoldierSellingScreen();
        }

        private void OpenSoldierSellingScreen()
        {
            // TransferableWithTrade on the member section activates the native
            // centre-bottom gold ticker.  Non-null memberRosterLeft is required
            // to properly initialise the left-side member slot.
            // Note: the ticker will show the game's own troop trade value which
            // may differ from our custom formula; the Done button hint and the
            // post-sale notification both reflect the actual amount paid.
            PartyScreenHelper.OpenScreenWithCondition(
                new IsTroopTransferableDelegate(this.IsTroopTransferable),
                new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
                new PartyPresentationDoneButtonDelegate(this.DoneClicked),
                null,
                PartyScreenLogic.TransferState.TransferableWithTrade,  // members: trade mode → gold ticker
                PartyScreenLogic.TransferState.NotTransferable,        // prisoners: locked
                new TextObject("{=IR_SELL_TROOPS_SCREEN}Sell Troops"),
                100000,
                false,
                false,
                PartyScreenHelper.PartyScreenMode.TroopsManage,
                TroopRoster.CreateDummyTroopRoster(),   // memberRosterLeft — non-null initialises the section
                null
            );
        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
        {
            return !character.IsHero;
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            int totalBaseValue = 0;
            foreach (TroopRosterElement element in leftMemberRoster.GetTroopRoster())
                totalBaseValue += this.GetSoldierPrice(element.Character) * element.Number;
            float finalGoldF = (float)totalBaseValue * Settings.Instance.SoldierSellingPriceMultiplier;
            int finalGold = (int)Math.Max(0f, finalGoldF);

            // Show a notification in the message log whenever the running total
            // changes — one message per drag so the player can see the live value
            // in the same area they're watching, without spamming the log.
            if (finalGold != _lastNotifiedGold)
            {
                _lastNotifiedGold = finalGold;
                var live = new TextObject("{=IR_SELL_PRICE_HINT}Total: {GOLD} gold");
                live.SetTextVariable("GOLD", finalGold);
                InformationManager.DisplayMessage(new InformationMessage(live.ToString()));
            }

            var hint = new TextObject("{=IR_SELL_PRICE_HINT}Total: {GOLD} gold");
            hint.SetTextVariable("GOLD", finalGold);
            return new Tuple<bool, TextObject>(true, hint);
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            int totalBaseValue = 0;

            foreach (TroopRosterElement element in leftMemberRoster.GetTroopRoster())
            {
                totalBaseValue += this.GetSoldierPrice(element.Character) * element.Number;
            }

            if (totalBaseValue > 0)
            {
                float finalGoldF = (float)totalBaseValue * Settings.Instance.SoldierSellingPriceMultiplier;
                int finalGold = (int)Math.Max(1f, finalGoldF);

                Hero.MainHero.ChangeHeroGold(finalGold);
                var msg = new TextObject("{=IR_SELL_TROOPS_MSG}You have sold your troops for {GOLD} gold.");
                msg.SetTextVariable("GOLD", finalGold);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString()));
            }

            return true;
        }

        private int GetSoldierPrice(CharacterObject character)
        {
            // Base price calculation: Level 20 troop = 400 base units = 400 gold at default slider.
            return Math.Max(10, character.Level * 20);
        }
    }
}