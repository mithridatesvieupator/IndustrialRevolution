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
            starter.AddGameMenuOption("town", "settlement_donation", "{=IR_DONATE_TOWN}Donate to townsfolk", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);
            starter.AddGameMenuOption("village", "settlement_donation", "{=IR_DONATE_VILLAGE}Donate to villagers", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);
            starter.AddGameMenuOption("castle", "settlement_donation", "{=IR_DONATE_CASTLE}Donate to castle", new GameMenuOption.OnConditionDelegate(this.settlement_donation_on_condition), new GameMenuOption.OnConsequenceDelegate(this.settlement_donation_on_consequence), false, -1, false, null);

            starter.AddGameMenuOption("village_looted", "rebuild_village", "{=IR_REBUILD_VILLAGE_OPTION}Help rebuild {VILLAGE_NAME}", new GameMenuOption.OnConditionDelegate(this.rebuild_village_on_condition), new GameMenuOption.OnConsequenceDelegate(this.rebuild_village_on_consequence), false, -1, false, null);
            starter.AddWaitGameMenu("rebuild_village", "{=IR_REBUILD_WAIT_TEXT}You are helping to rebuild the village.", new OnInitDelegate(this.rebuild_village_on_init), new OnConditionDelegate(this.back_on_condition), new OnConsequenceDelegate(this.wait_menu_rebuild_village_on_consequence), new OnTickDelegate(this.wait_menu_rebuild_village_on_tick), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption, GameMenu.MenuOverlayType.None, 0f, GameMenu.MenuFlags.None, null);
            starter.AddGameMenuOption("rebuild_village", "rebuild_village_end", "{=IR_END_REBUILDING}End Rebuilding", new GameMenuOption.OnConditionDelegate(this.leave_on_condition), new GameMenuOption.OnConsequenceDelegate(this.wait_menu_end_rebuilding_on_consequence), true, -1, false, null);

            // Financial Support Dialogue
            starter.AddPlayerLine("indrev_financial_support", "hero_main_options", "indrev_financial_response", "{=IR_FINANCIAL_ASK}I am in need of funds. Given our good standing, could you provide some financial backing?", new ConversationSentence.OnConditionDelegate(this.ConditionToAskForSupport), null, 40, null, null);
            starter.AddDialogLine("indrev_financial_agree", "indrev_financial_response", "hero_main_options", "{=IR_FINANCIAL_AGREE}Of course. You've been a good friend to us. Here is what we can spare.", null, new ConversationSentence.OnConsequenceDelegate(this.ConsequenceForSupportAgree), 100, null);
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

            var financialMsg = new TextObject("{=IR_FINANCIAL_MSG}You received {GOLD} gold. Your relation with {NAME} decreased by {COST}.");
            financialMsg.SetTextVariable("GOLD", goldReward);
            financialMsg.SetTextVariable("NAME", notable.Name);
            financialMsg.SetTextVariable("COST", relationCost);
            InformationManager.DisplayMessage(new InformationMessage(financialMsg.ToString()));
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
            string donationText = new TextObject("{=IR_DONATION_PROMPT}How much gold would you like to donate to the people?\n(You can type 'max' to donate the maximum possible amount based on your purse and the settlement limit).").ToString();

            bool canDonate = false;
            if (settlement.IsTown && settlement.Town.Prosperity < this._settings.DonateTownProsperityMax) canDonate = true;
            else if (settlement.IsCastle && settlement.Town.Prosperity < this._settings.DonateCastleProsperityMax) canDonate = true;
            else if (settlement.IsVillage && settlement.Village.Hearth < this._settings.DonateVillageProsperityMax) canDonate = true;

            if (canDonate)
            {
                TextInquiryData data = new TextInquiryData(
                    new TextObject("{=IR_DONATION_TITLE}Donation").ToString(),
                    donationText, true, true,
                    new TextObject("{=IR_DONATE_BTN}Donate").ToString(),
                    new TextObject("{=IR_CANCEL_BTN}Cancel").ToString(),
                    new Action<string>(this.OnDonateToSettlement), null, false, new Func<string, Tuple<bool, string>>(this.IsDonationTextValid), "", "");
                InformationManager.ShowTextInquiry(data, false, false);
            }
            else
            {
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=IR_DONATE_CAP_TITLE}Thank you").ToString(),
                    new TextObject("{=IR_DONATE_CAP_TEXT}The settlement has already reached its maximum capacity for your donations.").ToString(),
                    true, false,
                    new TextObject("{=IR_LEAVE_BTN}Leave").ToString(),
                    string.Empty, null, null, "", 0f, null, null, null), false, false);
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

            var donateMsg = settlement.IsVillage
                ? new TextObject("{=IR_DONATE_VILLAGE_MSG}You donated {GOLD} gold and increased {NAME}'s hearths by {AMOUNT}.")
                : new TextObject("{=IR_DONATE_TOWN_MSG}You donated {GOLD} gold and increased {NAME}'s prosperity by {AMOUNT}.");
            donateMsg.SetTextVariable("GOLD", donationAmount);
            donateMsg.SetTextVariable("NAME", settlement.Name);
            donateMsg.SetTextVariable("AMOUNT", new TextObject(increaseAmount.ToString("F1")));
            InformationManager.DisplayMessage(new InformationMessage(donateMsg.ToString()));

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
                var rebuildMsg = new TextObject("{=IR_REBUILD_MSG}You helped rebuild {NAME}. Relations with local notables increased by +50.");
                rebuildMsg.SetTextVariable("NAME", settlement.Name);
                InformationManager.DisplayMessage(new InformationMessage(rebuildMsg.ToString()));
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