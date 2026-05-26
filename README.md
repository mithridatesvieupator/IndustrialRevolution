Industrial Revolution

https://github.com/mithridatesvieupator/IndustrialRevolution

I hated that no matter how rich or powerful I got, I could never keep my peasants safe. 
I discovered The Philanthropist by Faraonj, and loved that I could rebuild villages and donate gold to strengthen them and grow their militias. But it wasn’t enough. 
So I tried Give Troops to Notables by Lliud, which allowed me to donate some troops to militias. But only some, not enough to reliably protect all villages. 
I tried to focus on towns and castles, train elite governors, used Gold Rush Construction by Remosewa to rapidly build great cities and boost villages that way. I used Improved Garrisons by Sidies to automate patrols. That protected my towns better, but suddenly my castles starved! Villages couldn’t provide more food beyond 800 hearths. 
I found Food Demand Tweaks by moechar, which promised a fix but was abandoned in 2021 and not working on new game versions. I used dnSpy to unpack the dll and worked with AI to rebuild it for versions 1.3-1.4. Finally I had a sustainable solution. 
But I felt my options game was still limited. I was finding lots of loot but could only donate half to troops, and had plenty of top tier troops anyway so no need to upgrade or recruit. So I found Sell Your Troops by bearTokken. But I also wanted to sell in villages. I found Olto Discard Mod by Oltopeteeh which allows all items to convert to XP, based on gold value, not tier. 
So I decided to take all these features, extend them, and bundle them into one. Generally, I wanted more ways to exchange resources - troops for gold and vice versa, troops for relations, loot for troop quality, gold for relations and vice versa. I’m not a skilled coder, but AI helped me refactor and reform these features to unlock fresh new playstyles. 

Donate gold via menu option to towns, castles, or villages. Adds to prosperity/hearths and gives relations boost to notables in ratios controllable by sliders in Mod Options MCM menu
Rebuild razed villages with the time and labor of your troops. Takes about a day with 100 low tier troops, a third of that with 200 mid tier troops, a third of that again with 300 elite troops. Less if they’ve had time to partly recover on their own. 
Give troops to notables in towns or villages via conversation. Any troops, any number, added to militia and rewarded in relations boost proportional to tier and quantity, plus renown to your clan and power to notable. 
Request financial support from notables - conversation option allows you to sacrifice 20 relations points for 2000 gold by default, ratio controllable by slider.
Boost town/castle construction now scales exponentially with gold in reserve, so that double the gold adds double the speed, instead of the flat boost in native. 
Villages continue to provide more production and food to bound settlements beyond 800 hearths, which capped out at +18 in native, multiple controllable by slider. If you invest in village hearths and militias, they can thus provide reliable food surpluses for thriving towns and castles to keep growing indefinitely. 
High prosperity towns and castles boost village hearth growth, so that a well-protected and invested region can enter a positive feedback loop, multiplier controllable by slider.
If towns starve, they will slaughter livestock for meat, and even horses eventually
High prosperity cities boost workshop output greatly - 2x at 6000, 3x at 9000, 4x at 12000 etc. 
Villages pay double price for goods manufactured in towns - Tools, Beer, Wine, Oil, Pottery, Cloth, Leather, Velvet, Jewelry. Better reflects realistic split of villages supplying primary industry and demanding secondary industry, vice versa for towns. Also doubled price for horses (excepting horse breeding towns) because I was having trouble trading off horses and thought villages would realistically demand lots of beasts of burden, and wear through them quickly. Price multiplier controllable by slider, and all these goods are consumed at 25% of stock per day. 
Villages have more gold, 1000 + (2*Hearths), and towns too, 100,000 + (50*Prosperity). Incentivizes player to trade with villages more.
All discarded loot gives XP, not just weapons and armor, based on gold value, rather than tier. 0.5 gold value to XP by default, controllable by slider in Mod Options. Paid in Promise and Giving Hands perks in your Quartermaster each double XP gained, controllable by tickbox in Mod Options.
Sell troops via menu option in villages or towns. Price for each troop is 20x troop level by default, multiplier controllable by slider. 
Sell prisoners in villages and castles (sell all, or select via exchange screen)
Localization support for 11 languages (English, German, French, Spanish, Portuguese BR, Chinese Simplified & Traditional, Japanese, Korean, Polish, Russian, Turkish)

Compatibility

Game Versions
Built and tested on v1.3.x (current: 1.3.15). Compatible with 1.2.x for most features — prisoner exchange screen unconfirmed. 1.4.x is untested; a rebuild against 1.4 DLLs will likely be required once that version stabilises in the mod community.

Likely Compatible Mods
Mods that do not alter core settlement logic, troop management, gold/trade, or loot-to-XP conversion should be compatible. This includes armories, cosmetic mods, UI overhauls, most total overhauls, and mods that edit characters or tournaments or battle mechanics. I have personally tested it on the following without any obvious problems:
Realm of Thrones
Empires of Europe 1100 + Eriks Troops
War Sails
Fourberie
Retinues
Improved Garrisons
Horses
Hot Butter
Character Reload
Character Manager
True Relations
Both Perks
Party AI Controls
Esoteric Knowledge
Champion’s Relics
Tutelage
Bannerlord Expanded: Spouses Expanded and Settlement Interactions
Armories: Bahamut, Westeros, TA Weapons and Shields, EOE Armoury, Horse Armoury, Weaponry 

Likely Incompatible Mods
Mods that make changes to the economy and dialogue with notables may clash, including the original mods that inspired it, as well as:
Banner Kings
Dramalord (reportedly interfered with dialogue with The Philanthropist) 
True Town Gold

Savegame Compatibility
Adding the Mod: safe to add to an existing save game.
Removing the Mod: this mod does not inject custom variables into save files (all SyncData protocols are intentionally empty). If you remove the mod, the game should remove custom dialogue and menu options and seamlessly revert to native economy and construction formulas. Always make a backup saves before changing mods mod.

Requirements
To allow you to customize your experience with in-game sliders and toggles, this mod requires the standard Bannerlord modding stack:
Harmony: (Required for core engine patches)
ButterLib: (Required by MCM)
UIExtenderEx: (Required by MCM)
Mod Configuration Menu (MCM v5+): (Generates the in-game settings menu)

Recommended Load Order
[Native game modules]
Harmony
ButterLib
UIExtenderEx
Mod Configuration Menu
[Your other mods]
Industrial Revolution (Place near the bottom to ensure economy overrides take effect)

Troubleshooting Guide and Error Reports
In the event of crash or error, please write up game version, modlist and load order, exact nature of the crash, screenshots or text of any error message, relevant extract of rgl_log from C:\ProgramData\Mount and Blade II Bannerlord\logs, and ideally use Better Exception Window mod to save exception report, and upload to Github along with your savegame and steps to replicate. What I’ll be doing is just feeding that into an LLM, so you could also try that yourself, use Visual Studio to alter the files on Github, rebuild it yourself, test it, and let me know the outcome. I’m not a professional coder and I have an unrelated full time job and life so won’t always be quick to respond.

Acknowledgements
This project was built to consolidate and expand upon creative ideas from the Bannerlord modding community. Thanks again to these mods and authors, for the great fun they gave me and for making me realize that we can make this game whatever we want 
Give Troops to Notables by Lliud
The Philanthropist by Faraonj
Gold Rush Construction by remosewa
Food Demand Tweaks by moechar
MB2_Olto_Discard by Oltopeteeh
Sell Your Troops by bearTokken























