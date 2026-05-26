using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace IndustrialRevolution.SellPrisoners
{
    public class SellPrisoners : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption("village", "sell_all_prisoners", "{=IR_SELL_ALL_PRISONERS}Sell all prisoners",
                new GameMenuOption.OnConditionDelegate(this.sell_prisoners_condition),
                new GameMenuOption.OnConsequenceDelegate(this.sell_all_prisoners_consequence), false, -1, false, null);
            starter.AddGameMenuOption("village", "sell_some_prisoners", "{=IR_SELL_SOME_PRISONERS}Sell some prisoners",
                new GameMenuOption.OnConditionDelegate(this.sell_prisoners_condition),
                new GameMenuOption.OnConsequenceDelegate(this.sell_some_prisoners_consequence), false, -1, false, null);

            starter.AddGameMenuOption("castle", "sell_all_prisoners", "{=IR_SELL_ALL_PRISONERS}Sell all prisoners",
                new GameMenuOption.OnConditionDelegate(this.sell_prisoners_condition),
                new GameMenuOption.OnConsequenceDelegate(this.sell_all_prisoners_consequence), false, -1, false, null);
            starter.AddGameMenuOption("castle", "sell_some_prisoners", "{=IR_SELL_SOME_PRISONERS}Sell some prisoners",
                new GameMenuOption.OnConditionDelegate(this.sell_prisoners_condition),
                new GameMenuOption.OnConsequenceDelegate(this.sell_some_prisoners_consequence), false, -1, false, null);
        }

        private bool sell_prisoners_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            if (MobileParty.MainParty.PrisonRoster.TotalManCount > 0) return true;
            args.IsEnabled = false;
            return true;
        }

        private void sell_all_prisoners_consequence(MenuCallbackArgs args)
        {
            int totalGold = 0;
            int totalCount = 0;
            List<CharacterObject> nonHeroesToSell = new List<CharacterObject>();
            List<Hero> heroesToRelease = new List<Hero>();

            foreach (TroopRosterElement element in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
            {
                totalGold += this.GetPrisonerPrice(element.Character) * element.Number;
                totalCount += element.Number;
                if (element.Character.IsHero)
                    heroesToRelease.Add(element.Character.HeroObject);
                else
                    nonHeroesToSell.Add(element.Character);
            }

            if (totalCount > 0)
            {
                Hero.MainHero.ChangeHeroGold(totalGold);

                foreach (CharacterObject character in nonHeroesToSell)
                {
                    int count = MobileParty.MainParty.PrisonRoster.GetTroopCount(character);
                    MobileParty.MainParty.PrisonRoster.RemoveTroop(character, count);
                }

                foreach (Hero hero in heroesToRelease)
                    EndCaptivityAction.ApplyByRansom(hero, Hero.MainHero);

                var msg = new TextObject("{=IR_PRISONERS_SOLD_MSG}You sold {COUNT} prisoners for {GOLD} gold.");
                msg.SetTextVariable("COUNT", totalCount);
                msg.SetTextVariable("GOLD", totalGold);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString()));
            }

            Campaign.Current.CurrentMenuContext.Refresh();
        }

        private void sell_some_prisoners_consequence(MenuCallbackArgs args)
        {
            PartyScreenHelper.OpenScreenWithCondition(
                new IsTroopTransferableDelegate(this.IsPrisonerTransferable),
                new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
                new PartyPresentationDoneButtonDelegate(this.DoneClicked),
                null,
                PartyScreenLogic.TransferState.Transferable,
                PartyScreenLogic.TransferState.NotTransferable,
                new TextObject("{=IR_SELL_PRISONERS_SCREEN}Sell Prisoners"),
                100000,
                false,
                true,
                PartyScreenHelper.PartyScreenMode.TroopsManage,
                null,
                null
            );
        }

        private bool IsPrisonerTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
        {
            return type == PartyScreenLogic.TroopType.Prisoner;
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            int totalGold = 0;
            foreach (TroopRosterElement element in leftPrisonRoster.GetTroopRoster())
                totalGold += this.GetPrisonerPrice(element.Character) * element.Number;
            var hint = new TextObject("{=IR_SELL_PRICE_HINT}Total: {GOLD} gold");
            hint.SetTextVariable("GOLD", totalGold);
            return new Tuple<bool, TextObject>(true, hint);
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            int totalGold = 0;
            int totalCount = 0;

            foreach (TroopRosterElement element in leftPrisonRoster.GetTroopRoster())
            {
                totalGold += this.GetPrisonerPrice(element.Character) * element.Number;
                totalCount += element.Number;
            }

            if (totalGold > 0)
            {
                Hero.MainHero.ChangeHeroGold(totalGold);
                var msg = new TextObject("{=IR_PRISONERS_SOLD_MSG}You sold {COUNT} prisoners for {GOLD} gold.");
                msg.SetTextVariable("COUNT", totalCount);
                msg.SetTextVariable("GOLD", totalGold);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString()));
            }

            // Ensure hero prisoners moved to the sell side have their captivity properly ended
            foreach (TroopRosterElement element in leftPrisonRoster.GetTroopRoster())
            {
                if (element.Character.IsHero && element.Character.HeroObject.IsPrisoner)
                    EndCaptivityAction.ApplyByRansom(element.Character.HeroObject, Hero.MainHero);
            }

            return true;
        }

        private int GetPrisonerPrice(CharacterObject character)
        {
            return Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(character, Hero.MainHero);
        }
    }
}
