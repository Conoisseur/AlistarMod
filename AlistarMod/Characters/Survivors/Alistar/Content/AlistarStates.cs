using AlistarMod.Survivors.Alistar.SkillStates;

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
