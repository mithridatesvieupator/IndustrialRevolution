using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace IndustrialRevolution.Philanthropist
{
    public class ThePhilanthropistCampaignBehavior : CampaignBehaviorBase
    {
        private Settings _settings => Settings.Instance;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.OnHourlyTickSettlementEvent));
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption("town", "settlement_donation", "Donate to townsfolk", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);
            starter.AddGameMenuOption("village", "settlement_donation", "Donate to villagers", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);
            starter.AddGameMenuOption("castle", "settlement_donation", "Donate to castle", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);

            starter.AddGameMenuOption("village_looted", "rebuild_village", "Help rebuild {VILLAGE_NAME}", new GameMenuOption.OnConditionDelegate(this.rebuild_village_on_condition), new GameMenuOption.OnConsequenceDelegate(this.rebuild_village_on_consequence), false, -1, false, null);
            starter.AddWaitGameMenu("rebuild_village", "You are helping to rebuild the village.", new OnInitDelegate(this.rebuild_village_on_init), new OnConditionDelegate(this.back_on_condition), new OnConsequenceDelegate(this.wait_menu_rebuild_village_on_consequence), new OnTickDelegate(this.wait_menu_rebuild_village_on_tick), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption, GameMenu.MenuOverlayType.None, 0f, GameMenu.MenuFlags.None, null);
            starter.AddGameMenuOption("rebuild_village", "rebuild_village_end", "End Rebuilding", new GameMenuOption.OnConditionDelegate(this.leave_on_condition), new GameMenuOption.OnConsequenceDelegate(this.wait_menu_end_rebuilding_on_consequence), true, -1, false, null);

            // Financial Support Dialogue
            starter.AddPlayerLine("indrev_financial_support", "hero_main_options", "indrev_financial_response", "{=!}I am in need of funds. Given our good standing, could you provide some financial backing?", new ConversationSentence.OnConditionDelegate(this.ConditionToAskForSupport), null, 40, null, null);
            starter.AddDialogLine("indrev_financial_agree", "indrev_financial_response", "hero_main_options", "{=!}Of course. You've been a good friend to us. Here is what we can spare.", null, new ConversationSentence.OnConsequenceDelegate(this.ConsequenceForSupportAgree), 100, null);
        }

        // --- FINANCIAL SUPPORT LOGIC ---

        private bool ConditionToAskForSupport()
        {
            if (Hero.OneToOneConversationHero == null || !Hero.OneToOneConversationHero.IsNotable) return false;
            return Hero.MainHero.GetRelation(Hero.OneToOneConversationHero) >= 20;
        }

        private void ConsequenceForSupportAgree()
        {
            Hero notable = Hero.OneToOneConversationHero;
            int relationCost = 20;
            int goldReward = relationCost * this._settings.FinancialSupportGoldPerRelation;

            ChangeRelationAction.ApplyPlayerRelation(notable, -relationCost, true, true);
            Hero.MainHero.ChangeHeroGold(goldReward);

            InformationManager.DisplayMessage(new InformationMessage($"You received {goldReward} gold. Your relation with {notable.Name} decreased by {relationCost}."));
        }

        // --- DONATION LOGIC ---

        private bool settlement_donation_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Bribe;
            return true;
        }

        private void settlement_donation_on_consequence(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            string donationText = "How much gold would you like to donate to the people?\n(You can type 'max' to donate the maximum possible amount based on your purse and the settlement limit).";

            bool canDonate = false;
            if (settlement.IsTown && settlement.Town.Prosperity < this._settings.DonateTownProsperityMax) canDonate = true;
            else if (settlement.IsCastle && settlement.Town.Prosperity < this._settings.DonateCastleProsperityMax) canDonate = true;
            else if (settlement.IsVillage && settlement.Village.Hearth < this._settings.DonateVillageProsperityMax) canDonate = true;

            if (canDonate)
            {
                TextInquiryData data = new TextInquiryData("Donation", donationText, true, true, "Donate", "Cancel", new Action<string>(this.OnDonateToSettlement), null, false, new Func<string, Tuple<bool, string>>(this.IsDonationTextValid), "", "");
                InformationManager.ShowTextInquiry(data, false, false);
            }
            else
            {
                InformationManager.ShowInquiry(new InquiryData("Thank you", "The settlement has already reached its maximum capacity for your donations.", true, false, "Leave", string.Empty, null, null, "", 0f, null, null, null), false, false);
            }
        }

        private void OnDonateToSettlement(string text)
        {
            Settlement settlement = Settlement.CurrentSettlement;

            float currentVal = settlement.IsVillage ? settlement.Village.Hearth : settlement.Town.Prosperity;
            float maxVal = settlement.IsTown ? this._settings.DonateTownProsperityMax : (settlement.IsCastle ? this._settings.DonateCastleProsperityMax : this._settings.DonateVillageProsperityMax);

            float prosperityNeeded = maxVal - currentVal;
            int maxGoldAllowed = (int)(prosperityNeeded * this._settings.GoldToProsperityRatio);
            int maxGoldPossible = Math.Min(Hero.MainHero.Gold, maxGoldAllowed);

            int donationAmount = 0;
            if (text.Trim().ToLower() == "max") donationAmount = maxGoldPossible;
            else if (int.TryParse(text, out int parsedAmount)) donationAmount = parsedAmount;

            if (donationAmount <= 0) return;

            float increaseAmount = (float)donationAmount / (float)this._settings.GoldToProsperityRatio;

            this.IncreaseSettlementProsperityOrHearth(settlement, increaseAmount);
            Hero.MainHero.ChangeHeroGold(-donationAmount);

            string statName = settlement.IsVillage ? "hearths" : "prosperity";
            InformationManager.DisplayMessage(new InformationMessage($"You donated {donationAmount} gold and increased {settlement.Name}'s {statName} by {increaseAmount:F1}."));

            int relationBoost = (int)(increaseAmount / (float)this._settings.ProsperityToRelationsRatio);

            if (relationBoost > 0 && settlement.Notables != null && settlement.Notables.Count > 0)
            {
                foreach (Hero notable in settlement.Notables)
                {
                    ChangeRelationAction.ApplyPlayerRelation(notable, relationBoost, true, true);
                }
            }

            Campaign.Current.CurrentMenuContext.Refresh();
        }

        private Tuple<bool, string> IsDonationTextValid(string text)
        {
            if (string.IsNullOrEmpty(text)) return new Tuple<bool, string>(false, "Invalid amount.");
            if (text.Trim().ToLower() == "max") return new Tuple<bool, string>(true, string.Empty);
            if (!int.TryParse(text, out int donationAmount)) return new Tuple<bool, string>(false, "Please enter a valid number or 'max'.");
            if (donationAmount <= 0) return new Tuple<bool, string>(false, "Donation must be greater than zero.");
            if (donationAmount > Hero.MainHero.Gold) return new Tuple<bool, string>(false, "You do not have enough gold.");

            Settlement settlement = Settlement.CurrentSettlement;
            float currentVal = settlement.IsVillage ? settlement.Village.Hearth : settlement.Town.Prosperity;
            float maxVal = settlement.IsTown ? this._settings.DonateTownProsperityMax : (settlement.IsCastle ? this._settings.DonateCastleProsperityMax : this._settings.DonateVillageProsperityMax);

            float prosperityNeeded = maxVal - currentVal;
            int maxGoldAllowed = (int)(prosperityNeeded * this._settings.GoldToProsperityRatio);

            if (donationAmount > maxGoldAllowed) return new Tuple<bool, string>(false, $"You can only donate up to {maxGoldAllowed} gold to reach the cap.");

            return new Tuple<bool, string>(true, string.Empty);
        }

        private void IncreaseSettlementProsperityOrHearth(Settlement settlement, float prosperityIncreaseAmount)
        {
            if (settlement.IsTown) settlement.Town.Prosperity = Math.Min(this._settings.DonateTownProsperityMax, settlement.Town.Prosperity + prosperityIncreaseAmount);
            else if (settlement.IsCastle) settlement.Town.Prosperity = Math.Min(this._settings.DonateCastleProsperityMax, settlement.Town.Prosperity + prosperityIncreaseAmount);
            else if (settlement.IsVillage) settlement.Village.Hearth = Math.Min(this._settings.DonateVillageProsperityMax, settlement.Village.Hearth + prosperityIncreaseAmount);
        }

        // --- REBUILD LOGIC ---

        private void OnHourlyTickSettlementEvent(Settlement settlement)
        {
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (settlement == currentSettlement && settlement.IsRaided)
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(0.02f + MobileParty.MainParty.Party.EstimatedStrength / 6000f, false, null);
                IncreaseSettlementHealthAction.Apply(currentSettlement, explainedNumber.ResultNumber);
            }
        }

        private bool rebuild_village_on_condition(MenuCallbackArgs args)
        {
            MBTextManager.SetTextVariable("VILLAGE_NAME", Settlement.CurrentSettlement.Name, false);
            args.optionLeaveType = GameMenuOption.LeaveType.Craft;
            return true;
        }

        private void rebuild_village_on_consequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu("rebuild_village");
        }

        private void rebuild_village_on_init(MenuCallbackArgs args)
        {
            args.MenuContext.SetBackgroundMeshName(Settlement.CurrentSettlement.SettlementComponent.WaitMeshName);
        }

        private bool back_on_condition(MenuCallbackArgs args)
        {
            return true;
        }

        private void wait_menu_rebuild_village_on_tick(MenuCallbackArgs args, CampaignTime dt)
        {
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(Settlement.CurrentSettlement.SettlementHitPoints);
        }

        // TRIGGERED AUTOMATICALLY WHEN 100% REBUILT
        private void wait_menu_rebuild_village_on_consequence(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement != null && settlement.Notables != null && settlement.SettlementHitPoints >= 1f)
            {
                foreach (Hero notable in settlement.Notables)
                {
                    ChangeRelationAction.ApplyPlayerRelation(notable, 50, true, true);
                }
                InformationManager.DisplayMessage(new InformationMessage($"You helped rebuild {settlement.Name}. Relations with local notables increased by +50."));
            }
            GameMenu.SwitchToMenu("village");
        }

        private bool leave_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        // TRIGGERED ONLY IF QUIT EARLY (MANUALLY CLICKED)
        private void wait_menu_end_rebuilding_on_consequence(MenuCallbackArgs args)
        {
            // Player quit early, return to looted village screen with no reward.
            GameMenu.SwitchToMenu("village_looted");
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}