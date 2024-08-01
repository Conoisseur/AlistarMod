using RoR2;
using AlistarMod.Modules.Achievements;

namespace AlistarMod.Survivors.Alistar.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class AlistarMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = AlistarSurvivor.ALISTAR_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = AlistarSurvivor.ALISTAR_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => AlistarSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}