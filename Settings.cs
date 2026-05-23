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
        [SettingPropertyFloatingInteger("Donate Town Prosperity Max", 2000f, 20000f, "0.00", HintText = "Max town prosperity via donation.", RequireRestart = false)]
        [SettingPropertyGroup("Philanthropy")]
        public float DonateTownProsperityMax { get; set; } = 10000f;

        [SettingPropertyFloatingInteger("Donate Castle Prosperity Max", 1000f, 10000f, "0.00", HintText = "Max castle prosperity via donation.", RequireRestart = false)]
        [SettingPropertyGroup("Philanthropy")]
        public float DonateCastleProsperityMax { get; set; } = 5000f;

        [SettingPropertyFloatingInteger("Donate Village Hearth Max", 1000f, 10000f, "0.00", HintText = "Max village hearths via donation.", RequireRestart = false)]
        [SettingPropertyGroup("Philanthropy")]
        public float DonateVillageProsperityMax { get; set; } = 5000f;

        [SettingPropertyInteger("Gold To Prosperity Ratio", 1, 100, "0", HintText = "Gold required to increase prosperity/hearths by 1.", RequireRestart = false)]
        [SettingPropertyGroup("Philanthropy")]
        public int GoldToProsperityRatio { get; set; } = 50;

        [SettingPropertyInteger("Prosperity To Relations Ratio", 10, 1000, "0", HintText = "Prosperity gain required to increase relations by 1.", RequireRestart = false)]
        [SettingPropertyGroup("Philanthropy")]
        public int ProsperityToRelationsRatio { get; set; } = 500;

        // --- Troop Selling Settings ---
        [SettingPropertyFloatingInteger("Price Multiplier", 0.5f, 2f, "0.0", HintText = "Multiplies the base troop sale price.", RequireRestart = false)]
        [SettingPropertyGroup("Troop Selling")]
        public float SoldierSellingPriceMultiplier { get; set; } = 1f;

        // --- Financial Support ---
        [SettingPropertyInteger("Gold per Relation Point", 50, 200, "0", HintText = "Amount of gold received per 1 relation point.", RequireRestart = false)]
        [SettingPropertyGroup("Financial Support")]
        public int FinancialSupportGoldPerRelation { get; set; } = 100;

        // --- Economy Tweaks ---
        [SettingPropertyFloatingInteger("Village Price Premium", 1f, 3f, "0.0", HintText = "Multiplier for manufactured goods and draft animals in villages.", RequireRestart = false)]
        [SettingPropertyGroup("Economy Tweaks")]
        public float VillagePricePremiumMultiplier { get; set; } = 2f;

        [SettingPropertyFloatingInteger("Hearth Growth Multiplier", 0.5f, 2f, "0.0", HintText = "Multiplier for village hearth growth based on bound town prosperity.", RequireRestart = false)]
        [SettingPropertyGroup("Economy Tweaks")]
        public float HearthGrowthFromProsperityMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Village Food Bonus Multiplier", 0.25f, 1f, "0.00", HintText = "Multiplier for the food bonus towns receive from bound villages.", RequireRestart = false)]
        [SettingPropertyGroup("Economy Tweaks")]
        public float VillageFoodBonusMultiplier { get; set; } = 0.5f;

        // --- Discard XP Settings ---
        [SettingPropertyBool("XP for all items, proportional to gold value", HintText = "Overrides native discard system.", RequireRestart = false)]
        [SettingPropertyGroup("Discard XP tweak", GroupOrder = 10)]
        public bool EnableDiscardXpTweak { get; set; } = true;

        [SettingPropertyInteger("Gold Value Per XP", 10, 40, "0", HintText = "Gold value of item donations required for 1 XP.", RequireRestart = false)]
        [SettingPropertyGroup("Discard XP tweak", GroupOrder = 10)]
        public int DiscardGoldPerXp { get; set; } = 20;

        [SettingPropertyBool("Paid In Promise, Giving Hands both double XP reward", HintText = "Possessing these perks doubles XP.", RequireRestart = false)]
        [SettingPropertyGroup("Discard XP tweak", GroupOrder = 10)]
        public bool DiscardPerksDoubleXp { get; set; } = true;
    }
}