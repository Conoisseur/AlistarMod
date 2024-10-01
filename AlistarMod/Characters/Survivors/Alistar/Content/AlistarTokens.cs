using System;
using AlistarMod.Modules;
using AlistarMod.Survivors.Alistar.Achievements;

namespace AlistarMod.Survivors.Alistar
{
    public static class AlistarTokens
    {
        public static void Init()
        {
            AddHenryTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddHenryTokens()
        {
            string prefix = AlistarSurvivor.ALISTAR_PREFIX;

            string desc = "Alistar is a raging bull with a focus on melee combat and crowd control.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
                         + "< ! > Unbreakable Will is great for beating down enemies while granting a defensive buff." + Environment.NewLine + Environment.NewLine
                         + "< ! > Pulverize knocks enemies into the air, allowing for a follow-up attack." + Environment.NewLine + Environment.NewLine
                         + "< ! > Headbutt shoves an enemy away and deals a chunk of damage." + Environment.NewLine + Environment.NewLine
                         + "< ! > Trample unleashes a series of knock-ups, granting a huge buff if enough enemies are hit." + Environment.NewLine + Environment.NewLine;

            string outro = "Mess with the bull and you get the horns!";
            string outroFailure = "Now I'm angry...";

            Language.Add(prefix + "NAME", "Alistar");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Minotaur");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Henry passive");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Sample text.");
            #endregion

            #region Primary
            Language.Add(prefix + "UNBREAKABLE_WILL_NAME", "Unbreakable Will");
            Language.Add(prefix + "UNBREAKABLE_WILL_DESCRIPTION", $"Alistar delivers a series of punches for <style=cIsDamage>{100f * AlistarStaticValues.unbreakableWillDamageCoefficient}% damage</style> each. Landing the third strike empowers him with a short <style=cIsUtility>armor buff.</style>");
            #endregion

            #region Secondary
            Language.Add(prefix + "PULVERIZE_NAME", "Pulverize");
            Language.Add(prefix + "PULVERIZE_DESCRIPTION", $"Alistar smashes the ground, dealing <style=cIsDamage>{100f * AlistarStaticValues.pulverizeDamageCoefficient}% damage</style> to nearby enemies and <style=cIsUtility>knocking them into the air</style>.");
            #endregion

            #region Utility
            Language.Add(prefix + "HEADBUTT_NAME", "Headbutt");
            Language.Add(prefix + "HEADBUTT_DESCRIPTION", $"Alistar rams a target with his head, dealing <style=cIsDamage>{100f * AlistarStaticValues.headbuttDamageCoefficient}% damage</style> and <style=cIsUtility>knocking them back</style>.");
            #endregion

            #region Special
            Language.Add(prefix + "TRAMPLE_NAME", "Trample");
            Language.Add(prefix + "TRAMPLE_DESCRIPTION", $"Alistar tramples the ground, dealing <style=cIsDamage>{100f * AlistarStaticValues.trampleDamageCoefficient}% damage</style> 10 times to nearby enemies.Additionally, Alistar gains a stack per hit, and at 5 stacks, Alistar buffs his <style=cIsUtility>movement speed</style> and <style=cIsUtility>attack speed</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(AlistarMasteryAchievement.identifier), "Alistar: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(AlistarMasteryAchievement.identifier), "As Alistar, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
