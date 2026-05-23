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
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.OnDailyTickSettlement));
        }

        public override void SyncData(IDataStore store) { }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement != null && settlement.Town != null)
            {
                Town town = settlement.Town;
                if (town.BoostBuildingProcess > 0)
                {
                    ConstructionBoostModel model = Campaign.Current.Models.BuildingConstructionModel as ConstructionBoostModel;
                    if (model != null)
                    {
                        int bonus = model.GetBoostAmount(town);
                        int burn = model.GetBoostCost(town) - (town.IsCastle ? 250 : 500);

                        if (bonus > 0 && burn > 0)
                        {
                            if (town.BuildingsInProgress != null && town.BuildingsInProgress.Count > 0)
                            {
                                Building current = town.BuildingsInProgress.Peek();
                                current.BuildingProgress += (float)bonus;
                                town.BoostBuildingProcess -= burn;

                                if (town.BoostBuildingProcess < 0)
                                {
                                    town.BoostBuildingProcess = 0;
                                }

                                BuildingHelper.CheckIfBuildingIsComplete(current);
                            }
                        }
                    }
                }
            }
        }
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
                this._cachedBonus = 0;
                this._cachedCost = 0;
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

        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.CalculateDailyConstructionPower(town, includeDescriptions);
            this.CalculateCostAndBonus(town);

            int bonus = this._cachedBonus;
            int burn = this._cachedCost;

            if (bonus > 0 && burn > 0 && town.BoostBuildingProcess > 0)
            {
                result.Add((float)bonus, new TextObject("{=!}Gold Reserve Boost", null), null);
            }
            return result;
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