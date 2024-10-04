using EntityStates;
using AlistarMod.Survivors.Alistar;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using HG;

namespace AlistarMod.Survivors.Alistar.SkillStates
{
    public class Headbutt : BaseSkillState
    {
        public static float procCoefficient = AlistarStaticValues.headbuttProcCoefficient;
        public static float damageCoefficient = AlistarStaticValues.headbuttDamageCoefficient;
        public static float duration = 0.75f;  
        public static float initialSpeedCoefficient = 6.5f;
        public static float finalSpeedCoefficient = 2.5f;
        public static float baseKnockbackForce = 800f;
        public static float tailoredKnockbackForceMultiplier = 60f; // Used to calculate force to apply to enemy based on it's mass and other features

        public static float hitboxRadius = 4f; 


        private float headbuttSpeed;
        private Vector3 headbuttDirection;
        //private Animator animator;
        private Vector3 previousPosition;

        public static GameObject onHitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/DiamondDamageBonusEffect.prefab").WaitForCompletion();


        public override void OnEnter()
        {
            base.OnEnter();
           // animator = GetModelAnimator();

            if (isAuthority && inputBank)
            {
                // Get the direction the player is aiming
                headbuttDirection = (inputBank.aimDirection == Vector3.zero ? characterDirection.forward : inputBank.aimDirection).normalized;
            }

            RecalculateHeadbuttSpeed();

            if (characterMotor)
            {
                // Apply velocity in the aiming direction
                characterMotor.velocity = headbuttDirection * headbuttSpeed;
            }

            Vector3 currentVelocity = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - currentVelocity;

            PlayAnimation("FullBody, Override", "Headbutt", "Headbutt.playbackRate", duration); 
           // Util.PlaySound("Play_moonBrother_dash", gameObject);
            Util.PlaySound("Play_bison_headbutt_attack_swing", gameObject);

        }

        private void RecalculateHeadbuttSpeed()
        {
            headbuttSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateHeadbuttSpeed();

            if (characterDirection) characterDirection.forward = headbuttDirection;

            // Calculate the direction of movement since the last frame
            Vector3 movementDirection = (transform.position - previousPosition).normalized;

            if (characterMotor && characterDirection && movementDirection != Vector3.zero)
            {
                // Calculate the velocity based on the movement direction and headbutt speed
                Vector3 desiredVelocity = movementDirection * headbuttSpeed;
                characterMotor.velocity = desiredVelocity;
            }
            previousPosition = transform.position;


            // Check for collisions
            if (isAuthority)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, hitboxRadius, LayerIndex.entityPrecise.mask);
                foreach (Collider hitCollider in hitColliders)
                {
                    HurtBox hurtBox = hitCollider.GetComponent<HurtBox>();
                    if (hurtBox && Util.IsValid(hurtBox) && hurtBox.healthComponent && hurtBox.healthComponent.body != characterBody)
                    {

                        CharacterBody hitBody = hurtBox.healthComponent.body;

                        // Create damaging blast in area
                        BlastAttack headbutAttack = new BlastAttack();
                        headbutAttack.radius = 5f;
                        headbutAttack.procCoefficient = procCoefficient;
                        headbutAttack.position = transform.position;
                        headbutAttack.attacker = characterBody.gameObject;
                        headbutAttack.crit = RollCrit();
                        headbutAttack.baseDamage = damageCoefficient * damageStat;
                        headbutAttack.canRejectForce = false;
                        headbutAttack.falloffModel = BlastAttack.FalloffModel.None;
                        headbutAttack.baseForce = 0f;
                        headbutAttack.bonusForce = headbuttDirection * (baseKnockbackForce + (hitBody.characterMotor.mass * tailoredKnockbackForceMultiplier));
                        headbutAttack.teamIndex = characterBody.teamComponent.teamIndex;
                        headbutAttack.damageType = DamageType.Generic;
                        headbutAttack.attackerFiltering = AttackerFiltering.Default;
                        BlastAttack.Result trampleAttackDamageResult = headbutAttack.Fire();

                        EffectManager.SpawnEffect(onHitEffectPrefab, new EffectData
                        {
                            origin = hitBody.corePosition,
                            scale = 1
                        }, true);

                        Util.PlaySound("Play_bison_headbutt_attack_hit", gameObject);

                        // Stop the dash on hit
                        outer.SetNextStateToMain();
                        return;
                    }
                }
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(headbuttDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            headbuttDirection = reader.ReadVector3();
        }
    }
}
