﻿using System;
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

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

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
            Language.Add(prefix + "PRIMARY_SLASH_NAME", "Sword");
            Language.Add(prefix + "PRIMARY_SLASH_DESCRIPTION", Tokens.agilePrefix + $"Swing forward for <style=cIsDamage>{100f * AlistarStaticValues.swordDamageCoefficient}% damage</style>.");
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
            Language.Add(prefix + "TRAMPLE_DESCRIPTION", $"Alistar tramples the ground, dealing <style=cIsDamage>{100f * AlistarStaticValues.trampleDamageCoefficient}% damage</style> to nearby enemies. Hits for 10 times, each applying a <style=cIsUtility>debuff stack</style>; at 3 stacks, enemies' movement is impaired. Alistar also gains a stack per hit, and at 5 stacks, he lets out a roar, increasing his <style=cIsUtility>movement speed</style> and <style=cIsUtility>attack speed</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(AlistarMasteryAchievement.identifier), "Henry: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(AlistarMasteryAchievement.identifier), "As Henry, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
