using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

using IndustrialRevolution.Economy;
using IndustrialRevolution.GiveTroops;
using IndustrialRevolution.Philanthropist;
using IndustrialRevolution.Construction;
using IndustrialRevolution.SellTroops;

namespace IndustrialRevolution
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            Harmony harmony = new Harmony("com.industrialrevolution.patch");
            harmony.PatchAll();

            try
            {
                Type typeFromHandle = typeof(TaleWorlds.CampaignSystem.GameComponents.DefaultSettlementFoodModel);
                MethodInfo methodInfo = AccessTools.Method(typeFromHandle, "CalculateTownFoodChange", null, null) ?? AccessTools.Method(typeFromHandle, "CalculateTownFoodStocksChange", null, null);
                MethodInfo methodInfo2 = AccessTools.Method(typeof(FoodModelPatch), "Postfix", null, null);

                if (methodInfo != null && methodInfo2 != null)
                {
                    harmony.Patch(methodInfo, null, new HarmonyMethod(methodInfo2), null, null);
                }
            }
            catch { }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (gameStarterObject is CampaignGameStarter campaignStarter)
            {
                campaignStarter.AddModel(new IndustrialRevolution.Economy.Economy());
                campaignStarter.AddModel(new ConstructionBoostModel());
                campaignStarter.AddModel(new DiscardXpModel());

                campaignStarter.AddBehavior(new VillageGold());
                campaignStarter.AddBehavior(new GiveTroopsBehavior());
                campaignStarter.AddBehavior(new ThePhilanthropistCampaignBehavior());
                campaignStarter.AddBehavior(new ConstructionBoostBehavior());
                campaignStarter.AddBehavior(new IndustrialRevolution.SellTroops.SellTroops());
                campaignStarter.AddBehavior(new VillageConsumptionBehavior());
            }
        }
    }
}