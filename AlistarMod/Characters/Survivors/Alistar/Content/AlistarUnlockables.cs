using AlistarMod.Survivors.Alistar.Achievements;
using RoR2;
using UnityEngine;

namespace AlistarMod.Survivors.Alistar
{
    public static class AlistarUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                AlistarMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(AlistarMasteryAchievement.identifier),
                AlistarSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
