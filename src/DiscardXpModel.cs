using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using IndustrialRevolution.Philanthropist;

namespace IndustrialRevolution.Economy
{
    public class DiscardXpModel : DefaultItemDiscardModel
    {
        public override int GetXpBonusForDiscardingItem(ItemObject item, int amount)
        {
            if (!Settings.Instance.EnableDiscardXpTweak)
            {
                return base.GetXpBonusForDiscardingItem(item, amount);
            }

            if (item == null || amount <= 0) return 0;

            // Inverted math: divides total value by the required gold per XP point
            float xp = ((float)item.Value * (float)amount) / (float)Settings.Instance.DiscardGoldPerXp;

            if (Settings.Instance.DiscardPerksDoubleXp && MobileParty.MainParty != null && MobileParty.MainParty.EffectiveQuartermaster != null)
            {
                Hero quartermaster = MobileParty.MainParty.EffectiveQuartermaster;

                bool hasGivingHands = quartermaster.GetPerkValue(DefaultPerks.Steward.GivingHands);
                bool hasPaidInPromise = quartermaster.GetPerkValue(DefaultPerks.Steward.PaidInPromise);

                int multiplier = 1;
                if (hasGivingHands) multiplier *= 2;
                if (hasPaidInPromise) multiplier *= 2;

                xp *= (float)multiplier;
            }

            return (int)Math.Max(0f, xp);
        }
    }
}