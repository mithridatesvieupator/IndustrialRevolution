using System;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace IndustrialRevolution.Philanthropist
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "IndustrialRevolution_Settings";
        public override string DisplayName => "Industrial Revolution";
        public override string FolderName => "IndustrialRevolution";
        public override string FormatType => "json";

        // --- Philanthropist Settings ---
        [SettingPropertyFloatingInteger("{=IR_MCM_DONATE_TOWN_MAX}Donate Town Prosperity Max", 2000f, 20000f, "0.00", HintText = "{=IR_MCM_DONATE_TOWN_MAX_HINT}Max town prosperity via donation.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_PHILANTHROPY}Philanthropy")]
        public float DonateTownProsperityMax { get; set; } = 10000f;

        [SettingPropertyFloatingInteger("{=IR_MCM_DONATE_CASTLE_MAX}Donate Castle Prosperity Max", 1000f, 10000f, "0.00", HintText = "{=IR_MCM_DONATE_CASTLE_MAX_HINT}Max castle prosperity via donation.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_PHILANTHROPY}Philanthropy")]
        public float DonateCastleProsperityMax { get; set; } = 5000f;

        [SettingPropertyFloatingInteger("{=IR_MCM_DONATE_VILLAGE_MAX}Donate Village Hearth Max", 1000f, 10000f, "0.00", HintText = "{=IR_MCM_DONATE_VILLAGE_MAX_HINT}Max village hearths via donation.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_PHILANTHROPY}Philanthropy")]
        public float DonateVillageProsperityMax { get; set; } = 5000f;

        [SettingPropertyInteger("{=IR_MCM_GOLD_PROSPERITY_RATIO}Gold To Prosperity Ratio", 20, 200, "0", HintText = "{=IR_MCM_GOLD_PROSPERITY_RATIO_HINT}Gold required to increase prosperity/hearths by 1.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_PHILANTHROPY}Philanthropy")]
        public int GoldToProsperityRatio { get; set; } = 100;

        [SettingPropertyInteger("{=IR_MCM_PROSPERITY_RELATIONS_RATIO}Prosperity To Relations Ratio", 10, 1000, "0", HintText = "{=IR_MCM_PROSPERITY_RELATIONS_RATIO_HINT}Prosperity gain required to increase relations by 1.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_PHILANTHROPY}Philanthropy")]
        public int ProsperityToRelationsRatio { get; set; } = 500;

        // --- Troop Selling Settings ---
        [SettingPropertyFloatingInteger("{=IR_MCM_PRICE_MULTIPLIER}Price Multiplier", 0.1f, 2f, "0.00", HintText = "{=IR_MCM_PRICE_MULTIPLIER_HINT}Multiplies the base troop sale price.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_TROOP_SELLING}Troop Selling")]
        public float SoldierSellingPriceMultiplier { get; set; } = 0.5f;

        // --- Financial Support ---
        [SettingPropertyInteger("{=IR_MCM_GOLD_PER_RELATION}Gold per Relation Point", 50, 200, "0", HintText = "{=IR_MCM_GOLD_PER_RELATION_HINT}Amount of gold received per 1 relation point.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_FINANCIAL}Financial Support")]
        public int FinancialSupportGoldPerRelation { get; set; } = 100;

        // --- Economy Tweaks ---
        [SettingPropertyFloatingInteger("{=IR_MCM_VILLAGE_PRICE_PREMIUM}Village Price Premium", 1f, 3f, "0.0", HintText = "{=IR_MCM_VILLAGE_PRICE_PREMIUM_HINT}Multiplier for manufactured goods and draft animals in villages.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_ECONOMY}Economy Tweaks")]
        public float VillagePricePremiumMultiplier { get; set; } = 2f;

        [SettingPropertyFloatingInteger("{=IR_MCM_HEARTH_GROWTH}Hearth Growth Multiplier", 0.5f, 2f, "0.0", HintText = "{=IR_MCM_HEARTH_GROWTH_HINT}Multiplier for village hearth growth based on bound town prosperity.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_ECONOMY}Economy Tweaks")]
        public float HearthGrowthFromProsperityMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=IR_MCM_VILLAGE_FOOD_BONUS}Village Food Bonus Multiplier", 0.25f, 1f, "0.00", HintText = "{=IR_MCM_VILLAGE_FOOD_BONUS_HINT}Multiplier for the food bonus towns receive from bound villages.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_ECONOMY}Economy Tweaks")]
        public float VillageFoodBonusMultiplier { get; set; } = 0.5f;

        // --- Discard XP Settings ---
        [SettingPropertyBool("{=IR_MCM_DISCARD_XP_ALL}XP for all items, proportional to gold value", HintText = "{=IR_MCM_DISCARD_XP_ALL_HINT}Overrides native discard system.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_DISCARD_XP}Discard XP", GroupOrder = 10)]
        public bool EnableDiscardXpTweak { get; set; } = true;

        [SettingPropertyInteger("{=IR_MCM_DISCARD_GOLD_PER_XP}Gold Value Per XP", 10, 40, "0", HintText = "{=IR_MCM_DISCARD_GOLD_PER_XP_HINT}Gold value of item donations required for 1 XP.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_DISCARD_XP}Discard XP", GroupOrder = 10)]
        public int DiscardGoldPerXp { get; set; } = 20;

        [SettingPropertyBool("{=IR_MCM_DISCARD_PERKS_DOUBLE}Paid In Promise, Giving Hands both double XP reward", HintText = "{=IR_MCM_DISCARD_PERKS_DOUBLE_HINT}Possessing these perks doubles XP.", RequireRestart = false)]
        [SettingPropertyGroup("{=IR_MCM_GROUP_DISCARD_XP}Discard XP", GroupOrder = 10)]
        public bool DiscardPerksDoubleXp { get; set; } = true;
    }
}
