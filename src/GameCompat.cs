using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Localization;
#if !GAME_V1212
using Helpers;
#endif

namespace IndustrialRevolution
{
    // Party screen modes Industrial Revolution opens; mapped to the game's enum for each supported version.
    internal enum IRScreenMode
    {
        TroopsManage,
        Ransom
    }

    // Game API differences between Bannerlord 1.2.12 (GAME_V1212, built by src-v1212) and current versions.
    internal static class IRCompat
    {
        public static TextObject EmptyText
        {
            get
            {
#if GAME_V1212
                return TextObject.Empty;
#else
                return TextObject.GetEmpty();
#endif
            }
        }

        public static float PartyStrength(PartyBase party)
        {
#if GAME_V1212
            return party.TotalStrength;
#else
            return party.EstimatedStrength;
#endif
        }

        // 1.2.12 has no BuildingHelper.CheckIfBuildingIsComplete; this mirrors the later game's implementation.
        public static void CheckIfBuildingIsComplete(Building building)
        {
#if GAME_V1212
            if (building.GetConstructionCost() <= building.BuildingProgress)
            {
                if (building.CurrentLevel < 3)
                {
                    building.LevelUp();
                }
                if (building.CurrentLevel == 3)
                {
                    building.BuildingProgress = building.GetConstructionCost();
                }
                building.Town.BuildingsInProgress.Dequeue();
            }
#else
            BuildingHelper.CheckIfBuildingIsComplete(building);
#endif
        }

#if GAME_V1212
        public static PartyScreenMode ToGame(IRScreenMode mode)
        {
            return mode == IRScreenMode.Ransom ? PartyScreenMode.Ransom : PartyScreenMode.TroopsManage;
        }

        // In 1.2.12 the screen mode and donating flag live on PartyScreenManager (private setters) rather than on
        // PartyState. Set them the way the game's own Open* methods do; if the fields ever differ the screen still
        // opens, just in whatever mode was last used.
        public static void SetPartyScreenManagerState(IRScreenMode mode, bool isDonating)
        {
            var manager = PartyScreenManager.Instance;
            if (manager == null)
            {
                return;
            }
            const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            try
            {
                typeof(PartyScreenManager).GetField("_currentMode", Flags)?.SetValue(manager, ToGame(mode));
                typeof(PartyScreenManager).GetField("<IsDonating>k__BackingField", Flags)?.SetValue(manager, isDonating);
            }
            catch (System.Exception)
            {
            }
        }
#else
        public static PartyScreenHelper.PartyScreenMode ToGame(IRScreenMode mode)
        {
            return mode == IRScreenMode.Ransom ? PartyScreenHelper.PartyScreenMode.Ransom : PartyScreenHelper.PartyScreenMode.TroopsManage;
        }
#endif

        public static void OpenScreenWithCondition(IsTroopTransferableDelegate isTroopTransferable,
            PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyPresentationDoneButtonDelegate onDoneClicked,
            PartyPresentationCancelButtonDelegate onCancelClicked, PartyScreenLogic.TransferState memberTransferState,
            PartyScreenLogic.TransferState prisonerTransferState, TextObject leftPartyName, int limit, bool showProgressBar,
            bool isDonating, IRScreenMode screenMode, TroopRoster memberRosterLeft, TroopRoster prisonerRosterLeft)
        {
#if GAME_V1212
            PartyScreenManager.OpenScreenWithCondition(
#else
            PartyScreenHelper.OpenScreenWithCondition(
#endif
                isTroopTransferable, doneButtonCondition, onDoneClicked, onCancelClicked, memberTransferState,
                prisonerTransferState, leftPartyName, limit, showProgressBar, isDonating, ToGame(screenMode),
                memberRosterLeft, prisonerRosterLeft);
        }
    }
}
