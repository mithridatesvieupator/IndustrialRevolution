using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using IndustrialRevolution.Philanthropist; // <-- Added to access MCM Settings

namespace IndustrialRevolution.Economy
{
    // --- RURAL PEASANTS CONSUME GOODS & DRAFT ANIMALS ---
    public class VillageConsumptionBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.OnDailyTickSettlement));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement.IsVillage && settlement.ItemRoster != null)
            {
                string villageType = settlement.Village.VillageType.StringId.ToLower();
                bool isMountBreeder = villageType.Contains("horse") || villageType.Contains("camel") || villageType.Contains("mule");

                for (int i = settlement.ItemRoster.Count - 1; i >= 0; i--)
                {
                    var element = settlement.ItemRoster.GetElementCopyAtIndex(i);
                    if (element.EquipmentElement.Item != null)
                    {
                        // 1. Consume Manufactured Goods
                        ItemCategory category = element.EquipmentElement.Item.ItemCategory;
                        if (category != null)
                        {
                            string id = category.StringId.ToLower();
                            if (id.Contains("tools") || id.Contains("beer") || id.Contains("wine") ||
                                id.Contains("oil") || id.Contains("pottery") || id.Contains("linen") ||
                                id.Contains("velvet") || id.Contains("leather") || id.Contains("jewelry") ||
                                id.Contains("garment") || id.Contains("cloth"))
                            {
                                int amountToConsume = (int)Math.Ceiling(element.Amount * 0.25f);
                                if (amountToConsume > 0)
                                {
                                    settlement.ItemRoster.AddToCounts(element.EquipmentElement, -amountToConsume);
                                }
                            }
                        }

                        // 2. Consume Draft Animals (Hardcoded to 25% daily)
                        if (!isMountBreeder && element.EquipmentElement.Item.HasHorseComponent)
                        {
                            int animalConsumption = (int)Math.Ceiling(element.Amount * 0.25f);
                            if (animalConsumption > 0)
                            {
                                settlement.ItemRoster.AddToCounts(element.EquipmentElement, -animalConsumption);
                            }
                        }
                    }
                }
            }
        }
    }

    // --- ORIGINAL ECONOMY MODEL ---
    public class Economy : DefaultSettlementEconomyModel
    {
        public override int GetTownGoldChange(Town town)
        {
            float targetGold = 100000f + (town.Prosperity * 50f);
            float delta = targetGold - (float)town.Gold;
            return (int)MathF.Round(0.25f * delta);
        }
    }

    // --- ORIGINAL VILLAGE GOLD INJECTION ---
    public class VillageGold : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.OnDailyTickSettlement));
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement.IsVillage)
            {
                int targetGold = 1000 + (int)(settlement.Village.Hearth * 2f);
                int delta = targetGold - settlement.Village.Gold;

                if (delta != 0)
                {
                    settlement.Village.ChangeGold((int)(delta * 0.25f));
                }
            }
        }

        public override void SyncData(IDataStore dataStore) { }
    }

    // --- HARMONY PATCHES ---

    // BRUTE-FORCE PRICE MULTIPLIER FOR VILLAGES
    [HarmonyPatch(typeof(DefaultTradeItemPriceFactorModel), "GetPrice")]
    public static class VillagePricePatch
    {
        public static void Postfix(EquipmentElement itemRosterElement, MobileParty clientParty, ref int __result)
        {
            Settlement settlement = null;

            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsVillage)
            {
                settlement = Settlement.CurrentSettlement;
            }
            else if (clientParty != null && clientParty.CurrentSettlement != null && clientParty.CurrentSettlement.IsVillage)
            {
                settlement = clientParty.CurrentSettlement;
            }

            if (settlement != null && itemRosterElement.Item != null)
            {
                bool applyPremium = false;

                // 1. Check Manufactured Goods
                if (itemRosterElement.Item.ItemCategory != null)
                {
                    string categoryId = itemRosterElement.Item.ItemCategory.StringId.ToLower();
                    if (categoryId.Contains("tools") || categoryId.Contains("beer") || categoryId.Contains("wine") ||
                        categoryId.Contains("oil") || categoryId.Contains("pottery") || categoryId.Contains("linen") ||
                        categoryId.Contains("velvet") || categoryId.Contains("leather") || categoryId.Contains("jewelry") ||
                        categoryId.Contains("garment") || categoryId.Contains("cloth"))
                    {
                        applyPremium = true;
                    }
                }

                // 2. Check Draft Animals (Only in non-breeding villages)
                if (!applyPremium && itemRosterElement.Item.HasHorseComponent)
                {
                    string villageType = settlement.Village.VillageType.StringId.ToLower();
                    bool isMountBreeder = villageType.Contains("horse") || villageType.Contains("camel") || villageType.Contains("mule");

                    if (!isMountBreeder)
                    {
                        applyPremium = true;
                    }
                }

                // Apply shared multiplier if either condition is met
                if (applyPremium)
                {
                    __result = (int)(__result * Settings.Instance.VillagePricePremiumMultiplier);
                }
            }
        }
    }

    public static class FoodModelPatch
    {
        public static void Postfix(Town __0, ref ExplainedNumber __result)
        {
            if (__0 != null && __0.Owner != null && __0.Owner.Settlement != null && !__0.IsUnderSiege)
            {
                float num = 0f;
                foreach (Village village in __0.Owner.Settlement.BoundVillages)
                {
                    if (village.VillageState == Village.VillageStates.Normal)
                    {
                        num += (float)Math.Floor((double)(Math.Max(0f, village.Hearth - 500f) / 50f)) + (float)Math.Floor((double)(village.Hearth / 100f));
                    }
                }

                // Hooked to MCM slider (Default 0.5x halving the native calculation)
                num *= Settings.Instance.VillageFoodBonusMultiplier;

                if (num > 0.1f)
                {
                    __result.Add(num, new TextObject("{=!}Industrial Revolution", null), null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(DefaultVillageProductionCalculatorModel), "CalculateDailyProductionAmount")]
    public static class VillageProductionPatch
    {
        private static void Postfix(ref ExplainedNumber __result, Village village, ItemObject item)
        {
            if (village.VillageState == Village.VillageStates.Normal)
            {
                float num = Math.Max(0f, village.Hearth - 500f) / 50f * 0.15f;
                if (num > 0f)
                {
                    __result.Add(num, new TextObject("{=!}Industrial Revolution", null), null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(DefaultSettlementProsperityModel), "CalculateHearthChange")]
    public static class HearthChangePatch
    {
        public static void Postfix(Village village, ref ExplainedNumber __result)
        {
            if (village != null && village.Bound != null && village.Bound.Town != null)
            {
                // Hooked to MCM slider (Default 1x)
                float num = (village.Bound.Town.Prosperity / 1000f) * Settings.Instance.HearthGrowthFromProsperityMultiplier;
                if (num > 0.1f)
                {
                    __result.Add(num, new TextObject("{=!}Industrial Revolution", null), null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(DefaultWorkshopModel), "GetEffectiveConversionSpeedOfProduction")]
    public static class WorkshopProductionPatch
    {
        public static void Postfix(Workshop workshop, ref ExplainedNumber __result)
        {
            if (workshop != null && workshop.Settlement != null && workshop.Settlement.IsTown)
            {
                Town town = workshop.Settlement.Town;
                float bonus = (town.Prosperity - 3000f) / 3000f;

                if (bonus != 0f)
                {
                    __result.Add(bonus, new TextObject("{=!}Industrial Revolution", null), null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemConsumptionBehavior), "MakeConsumption")]
    public static class ItemConsumptionPatch
    {
        private static void Postfix(Town town)
        {
            if (town != null && town.Settlement != null && town.Settlement.ItemRoster != null)
            {
                float num = (float)town.FoodStocksUpperLimit();
                if ((double)town.FoodStocks < (double)num * 0.1)
                {
                    ButcherExcessCattle(town);
                }
            }
        }

        private static void ButcherExcessCattle(Town town)
        {
            ItemRoster itemRoster = town.Settlement.ItemRoster;
            ItemObject itemObject = MBObjectManager.Instance.GetObjectTypeList<ItemObject>().FirstOrDefault((ItemObject x) => x.ItemCategory == DefaultItemCategories.Meat);
            if (itemObject != null)
            {
                for (int i = itemRoster.Count - 1; i >= 0; i--)
                {
                    ItemObject item = itemRoster.GetElementCopyAtIndex(i).EquipmentElement.Item;
                    if (item != null && item.HasHorseComponent && item.HorseComponent.MeatCount > 0)
                    {
                        itemRoster.AddToCounts(item, -1);
                        itemRoster.AddToCounts(itemObject, item.HorseComponent.MeatCount);
                    }
                }
            }
        }
    }
}