using AlistarMod.Survivors.Alistar.SkillStates;

namespace AlistarMod.Survivors.Alistar
{
    public static class AlistarStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Shoot));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
