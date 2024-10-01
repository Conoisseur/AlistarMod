using EntityStates;
using AlistarMod.Survivors.Alistar;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.Networking;

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

        private float duration = baseDuration; // Default duration is base duration
        private bool hasFired = false;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("FullBody, Override", "Pulverize", "Pulverize.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration * 0.5f && !hasFired)
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

                // Play ground slam effect and sound
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

                // Create damaging blast in area
                if (isAuthority)
                {
                    BlastAttack pulverizeDamageAttack = new BlastAttack();
                    pulverizeDamageAttack.radius = abilityRadius;
                    pulverizeDamageAttack.procCoefficient = 0f;
                    pulverizeDamageAttack.position = transform.position;
                    pulverizeDamageAttack.attacker = characterBody.gameObject;
                    pulverizeDamageAttack.crit = false;
                    pulverizeDamageAttack.baseDamage = damageCoefficient * damageStat;
                    pulverizeDamageAttack.canRejectForce = false;
                    pulverizeDamageAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    pulverizeDamageAttack.baseForce = 0;
                    pulverizeDamageAttack.bonusForce = Vector3.zero;
                    pulverizeDamageAttack.teamIndex = characterBody.teamComponent.teamIndex;
                    pulverizeDamageAttack.damageType = DamageType.Generic;
                    pulverizeDamageAttack.attackerFiltering = AttackerFiltering.Default;
                    BlastAttack.Result pulverizeDamageAttackResult = pulverizeDamageAttack.Fire();
                    
                    // Create individual blast for each hit enemy which knocks them up
                    foreach (BlastAttack.HitPoint hit_enemy in pulverizeDamageAttackResult.hitPoints)
                    {
                        CharacterBody enemy_body = hit_enemy.hurtBox.healthComponent.body;

                        BlastAttack pulverizeKnockupAttack = new BlastAttack();
                        pulverizeKnockupAttack.radius = 1f;
                        pulverizeKnockupAttack.procCoefficient = 0f;
                        pulverizeKnockupAttack.position = hit_enemy.hitPosition;
                        pulverizeKnockupAttack.attacker = characterBody.gameObject;
                        pulverizeKnockupAttack.crit = false;
                        pulverizeKnockupAttack.baseDamage = 0f;
                        pulverizeKnockupAttack.canRejectForce = false;
                        pulverizeKnockupAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                        pulverizeKnockupAttack.baseForce = 0;
                        pulverizeKnockupAttack.bonusForce = Vector3.up * (baseKnockupForce + (enemy_body.characterMotor.mass * tailoredKnockupForceMultiplier));
                        pulverizeKnockupAttack.teamIndex = characterBody.teamComponent.teamIndex;
                        pulverizeKnockupAttack.damageType = DamageType.NonLethal;
                        pulverizeKnockupAttack.attackerFiltering = AttackerFiltering.Default;
                        pulverizeKnockupAttack.Fire();
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
