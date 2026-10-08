using System;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace IndustrialRevolution.Construction
{
    public class ConstructionBoostBehavior : CampaignBehaviorBase
    {
        // The reserve boost is applied by the game itself: the base construction power adds GetBoostAmount and the
        // daily building tick deducts GetBoostCost (both overridden below). This behaviour used to add the bonus to
        // building progress a second time and charge the reserve extra; it now does nothing, and is kept registered
        // only so existing saves that contain it load unchanged.
        public override void RegisterEvents() { }

        public override void SyncData(IDataStore store) { }
    }

    public class ConstructionBoostModel : DefaultBuildingConstructionModel
    {
        private int _cachedCost;
        private int _cachedBonus;

        private void CalculateCostAndBonus(Town town)
        {
            int currentReserve = town.BoostBuildingProcess;
            if (currentReserve <= 0 || town.Settlement == null)
            {
                // Never report a 0 boost cost: BuildingHelper.GetDaysToComplete divides the reserve by it
                // (int division), so an empty reserve threw DivideByZeroException in the building project screen.
                this._cachedBonus = 0;
                this._cachedCost = town.IsCastle ? 250 : 500;
            }
            else
            {
                bool isCastle = town.Settlement.IsCastle;

                float fraction = isCastle ? 0.025f : 0.05f;
                int minBurn = isCastle ? 250 : 500;

                int burn = (int)Math.Floor((double)((float)currentReserve * fraction));
                if (burn < minBurn) burn = minBurn;
                if (burn > currentReserve) burn = currentReserve;

                // Fixed linear conversion: 20 gold = 1 construction point (Halved from 10:1)
                int bonus = burn / 20;

                this._cachedBonus = bonus;
                this._cachedCost = burn;
            }
        }

        public override int GetBoostAmount(Town town)
        {
            this.CalculateCostAndBonus(town);
            return this._cachedBonus;
        }

        public override int GetBoostCost(Town town)
        {
            this.CalculateCostAndBonus(town);
            return this._cachedCost;
        }

        public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
        {
            return base.CalculateDailyConstructionPowerWithoutBoost(town);
        }
    }

    [HarmonyPatch(typeof(TownManagementReserveControlVM))]
    public static class TownReserveMaxPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Settlement), typeof(Action) })]
        public static void CtorPostfix(TownManagementReserveControlVM __instance)
        {
            int heroGold = Hero.MainHero.Gold;
            __instance.MaxReserveAmount = Math.Min(heroGold, 1000000);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ExecuteConfirm")]
        public static void ExecuteConfirmPostfix(TownManagementReserveControlVM __instance)
        {
            int heroGold = Hero.MainHero.Gold;
            __instance.MaxReserveAmount = Math.Min(heroGold, 1000000);
        }
    }
}