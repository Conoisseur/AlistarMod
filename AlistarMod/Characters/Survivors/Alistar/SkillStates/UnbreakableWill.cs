using AlistarMod.Modules.BaseStates;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AlistarMod.Survivors.Alistar.SkillStates
{
    public class UnbreakableWill : BaseMeleeAttack
    {
        private Vector3 initialForward;
        private bool hasMovedForward = false;
        private float moveSpeed = 20f; // Speed Alistar is propelled by when swinging
        private InputBankTest inputBank;
        private const float armorBuffDuration = 2f; 

        public override void OnEnter()
        {
            hitboxGroupName = "AlistarMeleeGroup";

            damageType = DamageType.Generic;
            damageCoefficient = AlistarStaticValues.unbreakableWillDamageCoefficient;
            procCoefficient = AlistarStaticValues.UnbreakableWillProcCoefficient;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;
            earlyExitPercentTime = 0.6f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;

            swingSoundString = "Play_clayBruiser_step";
            hitSoundString = "Play_roboBall_takehit";
            muzzleString = "AlistarMeleeHitbox";
            playbackRateParam = "UnbreakableWill.playbackRate";

            swingEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressGlaiveSwing.prefab").WaitForCompletion(); ;
            hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBruiser/ClayShockwaveEffect.prefab").WaitForCompletion();

            impactSound = AlistarAssets.swordHitSoundEvent.index;

            // Store the initial forward direction
            initialForward = characterDirection.forward;

            // Cache the input bank
            inputBank = characterBody.inputBank;

            // Disable player input
            if (inputBank != null)
            {
                inputBank.moveVector = Vector3.zero;
            }

            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Ensure player input remains disabled during the attack
            if (inputBank != null)
            {
                inputBank.moveVector = Vector3.zero;
            }

            // Apply forward movement during the attack
            if (characterMotor && !hasMovedForward && fixedAge >= baseDuration * attackStartPercentTime)
            {
                Vector3 forwardMovement = initialForward * moveSpeed;
                characterMotor.velocity += forwardMovement;
                hasMovedForward = true;
            }
        }

        protected override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "UnbreakableWill" + (1 + swingIndex), "UnbreakableWill.playbackRate", duration, 0.1f * duration);
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            // Apply armor buff if this is the third punch (swingIndex 2)
            if (swingIndex == 2)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, armorBuffDuration);
            }
        }

        public override void OnExit()
        {
            // Do not reset velocity to ensure existing movement continues after the attack
            // Re-enable player input
            if (inputBank != null)
            {
                inputBank.moveVector = Vector3.zero;
            }

            base.OnExit();
        }
    }
}
