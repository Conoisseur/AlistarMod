using System;

namespace AlistarMod.Survivors.Alistar
{
    public static class AlistarStaticValues
    {
        public const float swordDamageCoefficient = 2.8f;
        public const float gunDamageCoefficient = 4.2f;
        public const float bombDamageCoefficient = 16f;

        // Pulverize values
        public const float pulverizeDamageCoefficient = 5.5f;
        public const float pulverizeProcCoefficient = 1.0f;
        public const float pulverizeCooldown = 1.0f;  // 3f

        // Headbutt values
        public const float headbuttDamageCoefficient = 6.0f;
        public const float headbuttProcCoefficient = 1.0f;
        public const float headbuttCooldown = 4.0f;


        // Trample values
        public const float trampleDamageCoefficient = 0.5f;
        public const float trampleProcCoefficient = 0.1f;
        public const float trampleCooldown = 10.0f;
        public static float trampleBaseDuration = 5f;
        public static float trampleBaseDamageInterval = 0.5f; // How often damage is applied during ability duration
        public static float trampleRadius = 12f;
        public static float trampleKnockupForce = 2000f;
        public static float trampleBuffDuration = 5f;
        public static float trampleDebuffDuration = 8f;

        // UnbreakableWill values
        public const float unbreakableWillDamageCoefficient = 2.8f;
        public const float UnbreakableWillProcCoefficient = 1.0f;
        public const float UnbreakableWillCooldown = 4.0f;

    }
}