using AlistarMod.Survivors.Alistar.SkillStates;
using R2API;

namespace AlistarMod.Survivors.Alistar
{
    public static class AlistarStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(UnbreakableWill));

            Modules.Content.AddEntityState(typeof(Pulverize));

            Modules.Content.AddEntityState(typeof(Headbutt));

            Modules.Content.AddEntityState(typeof(Trample));
        }
    }
}
