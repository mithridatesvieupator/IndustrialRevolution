using Helpers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace IndustrialRevolution.GiveTroops
{
    internal class GiveTroopsBehavior : CampaignBehaviorBase
    {
        public int previous_party_count = 0;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddPlayerLine("give_troops_start", "hero_main_options", "give_troops_yes", "{=GTNMOD01T}I have recruited and trained fine warriors. I could gift some to you, to defend this settlement and protect your interests. What say you?", new ConversationSentence.OnConditionDelegate(this.ConditionToAskAboutGivingTroops), null, 40, null, null);
            campaignGameStarter.AddDialogLine("give_troops_step1", "give_troops_yes", "give_troops_transition", "{=GTNMOD02T}Of course, that could help us a lot here.", null, null, 100, null);
            campaignGameStarter.AddDialogLine("give_troops_step2", "give_troops_transition", "give_troops_end", "{=GTNMOD03T}This text should be under the party screen.", null, new ConversationSentence.OnConsequenceDelegate(this.OpenTroopTransfer), 100, null);
            campaignGameStarter.AddDialogLine("give_troops_step3", "give_troops_end", "hero_main_options", "{=GTNMOD04T}Thank you.", new ConversationSentence.OnConditionDelegate(this.ConditionToSayThankYou), null, 100, null);
            campaignGameStarter.AddDialogLine("give_troops_step4", "give_troops_end", "hero_main_options", "{=GTNMOD04T}Very well.", null, null, 100, null);
        }

        private bool ConditionToAskAboutGivingTroops()
        {
            if (Settlement.CurrentSettlement == null || Hero.OneToOneConversationHero == null || !Hero.OneToOneConversationHero.IsNotable) return false;

            foreach (TroopRosterElement troopRosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
            {
                if (this.IsTroopCharacterTransferable(troopRosterElement.Character)) return true;
            }
            return false;
        }

        // Compare head counts, not the number of distinct troop types (gifting part of a stack changes only the former).
        private bool ConditionToSayThankYou() => MobileParty.MainParty.MemberRoster.TotalManCount != this.previous_party_count;

        private void OpenTroopTransfer()
        {
            this.previous_party_count = MobileParty.MainParty.MemberRoster.TotalManCount;
            IRCompat.OpenScreenWithCondition(new IsTroopTransferableDelegate(this.IsTroopTransferable), new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition), new PartyPresentationDoneButtonDelegate(this.DoneClicked), new PartyPresentationCancelButtonDelegate(this.CancelClicked), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, Hero.OneToOneConversationHero.Name, 0, false, false, IRScreenMode.TroopsManage, null, null);
        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
        {
            return this.IsTroopCharacterTransferable(character);
        }

        private bool IsTroopCharacterTransferable(CharacterObject character)
        {
            return !character.IsHero;
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            return new Tuple<bool, TextObject>(true, IRCompat.EmptyText);
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
            foreach (TroopRosterElement troopRosterElement in leftMemberRoster.GetTroopRoster())
            {
                if (!troopRosterElement.Character.IsHero)
                {
                    troopRoster.Add(troopRosterElement);
                }
            }

            Hero notableHero = Hero.OneToOneConversationHero;
            if (troopRoster.Count > 0)
            {
                if (notableHero.HomeSettlement.MilitiaPartyComponent == null)
                {
                    // Unique id: a fixed id could clash with a militia party created (and destroyed) earlier.
                    string militiaId = "militias_of_" + notableHero.HomeSettlement.StringId + "_ir_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    MilitiaPartyComponent.CreateMilitiaParty(militiaId, notableHero.HomeSettlement);
                }
                notableHero.HomeSettlement.MilitiaPartyComponent.Party.AddMembers(troopRoster);
            }

            int rewardScore = this.Calculate_RewardValue(leftMemberRoster);
            if (rewardScore > 0)
            {
                notableHero.AddPower((float)rewardScore);

                int relationGain = rewardScore;
                float renownGain = (float)rewardScore / 10f;

                int relationBefore = notableHero.GetRelation(Hero.MainHero);
                ChangeRelationAction.ApplyPlayerRelation(notableHero, relationGain, true, true);
                int actualRelationGain = notableHero.GetRelation(Hero.MainHero) - relationBefore;

                Hero.MainHero.Clan.AddRenown(renownGain);

                var giveMsg = new TextObject("{=IR_GIVE_TROOPS_MSG}{NAME} gained {POWER} power, relation increased by {RELATION}, your clan gained {RENOWN} renown, and the donated troops joined the militia.");
                giveMsg.SetTextVariable("NAME", notableHero.Name);
                giveMsg.SetTextVariable("POWER", rewardScore);
                giveMsg.SetTextVariable("RELATION", actualRelationGain);
                giveMsg.SetTextVariable("RENOWN", renownGain.ToString("F1"));
                InformationManager.DisplayMessage(new InformationMessage(giveMsg.ToString()));
            }

            if (Campaign.Current.ConversationManager.IsConversationInProgress)
                Campaign.Current.ConversationManager.ContinueConversation();

            return true;
        }

        private int Calculate_RewardValue(TroopRoster leftMemberRoster)
        {
            int score = 0;
            foreach (TroopRosterElement troopRosterElement in leftMemberRoster.GetTroopRoster())
            {
                score += (troopRosterElement.Character.Tier > 0 ? troopRosterElement.Character.Tier : 1) * troopRosterElement.Number;
            }
            return score;
        }

        private void CancelClicked()
        {
            if (Campaign.Current.ConversationManager.IsConversationInProgress)
                Campaign.Current.ConversationManager.ContinueConversation();
        }
    }
}