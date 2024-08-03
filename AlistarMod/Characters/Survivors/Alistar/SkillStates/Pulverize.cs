using EntityStates;
using AlistarMod.Survivors.Alistar;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace AlistarMod.Survivors.Alistar.SkillStates
{
    public class Pulverize : BaseSkillState
    {
        public static float damageCoefficient = AlistarStaticValues.pulverizeDamageCoefficient;
        public static float procCoefficient = AlistarStaticValues.pulverizeProcCoefficient;
        public static float baseDuration = 0.2f;
        public static float baseKnockupForce = 1000f; // Minimum knockup force applied to enemies
        public static float tailoredKnockupForceMultiplier = 17f; // Used to calculate force to apply to enemy based on it's mass and other features
        public static float abilityRadius = 30f;
        public static GameObject rubbleScatterEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/PodGroundImpact.prefab").WaitForCompletion();
        public static GameObject groundSlamEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion();

        private float duration;
        private bool hasFired;
        private HashSet<HealthComponent> hitTargets;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("FullBody, Override", "Pulverize", "Pulverize.playbackRate", this.duration);
            hitTargets = new HashSet<HealthComponent>();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration * 0.5f && !hasFired)
            {
                Slam();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Slam()
        {
            if (!hasFired)
            {
                hasFired = true;

                // Play ground slam effect
                EffectManager.SpawnEffect(rubbleScatterEffectPrefab, new EffectData
                {
                    origin = characterBody.footPosition,
                    scale = abilityRadius
                }, true);

                EffectManager.SpawnEffect(groundSlamEffectPrefab, new EffectData
                {
                    origin = characterBody.footPosition,
                    scale = abilityRadius
                }, true);

                Util.PlaySound("Play_imp_overlord_attack2_smash", gameObject);

                // Apply damage and knockup in a radius
                if (isAuthority)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, abilityRadius, LayerIndex.entityPrecise.mask);
                    foreach (Collider hitCollider in hitColliders)
                    {
                        HurtBox hurtBox = hitCollider.GetComponent<HurtBox>();
                        if (hurtBox && Util.IsValid(hurtBox) && !hitTargets.Contains(hurtBox.healthComponent))
                        {
                            TeamIndex team = base.GetTeam();
                            if (FriendlyFireManager.ShouldSplashHitProceed(hurtBox.healthComponent, team))
                            {
                                hitTargets.Add(hurtBox.healthComponent); // Add to hit targets to ensure it's hit only once

                                DamageInfo damageInfo = new DamageInfo
                                {
                                    attacker = base.gameObject,
                                    damage = damageCoefficient * damageStat,
                                    position = hurtBox.transform.position,
                                    procCoefficient = procCoefficient,
                                    crit = base.RollCrit(),
                                    damageColorIndex = DamageColorIndex.Default,
                                    force = Vector3.zero  // Not applying any knockup here
                                };
                                hurtBox.healthComponent.TakeDamage(damageInfo);
                                GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox.healthComponent.gameObject);

                                CharacterBody body = hurtBox.healthComponent.body;
                                if (body)
                                {
                                    if (body.characterMotor && body.characterMotor.isGrounded)
                                    {
                                        body.characterMotor.ApplyForce(Vector3.up * (baseKnockupForce + (body.characterMotor.mass*tailoredKnockupForceMultiplier)), true, false);
                                    }
                                    else if (body.rigidbody)
                                    {
                                        body.rigidbody.AddForce(Vector3.up * (baseKnockupForce + (body.characterMotor.mass * tailoredKnockupForceMultiplier)), ForceMode.Impulse);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
