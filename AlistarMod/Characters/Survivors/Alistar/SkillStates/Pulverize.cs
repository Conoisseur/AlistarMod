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
                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.radius = abilityRadius;
                    blastAttack.procCoefficient = 0f;
                    blastAttack.position = transform.position;
                    blastAttack.attacker = characterBody.gameObject;
                    blastAttack.crit = false;
                    blastAttack.baseDamage = damageCoefficient * damageStat;
                    blastAttack.canRejectForce = false;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    blastAttack.baseForce = 0;
                    blastAttack.bonusForce = Vector3.zero;
                    //blastAttack.bonusForce = Vector3.up * baseKnockupForce;
                    blastAttack.teamIndex = characterBody.teamComponent.teamIndex;
                    blastAttack.damageType = DamageType.Generic;
                    blastAttack.attackerFiltering = AttackerFiltering.Default;
                    BlastAttack.Result blastResult = blastAttack.Fire();
                    
                    // Create individual blast for each hit enemy which knocks them up
                    foreach (BlastAttack.HitPoint hit_enemy in blastResult.hitPoints)
                    {

                        CharacterBody enemy_body = hit_enemy.hurtBox.healthComponent.body;

                        BlastAttack blastAttackKnockup = new BlastAttack();
                        blastAttackKnockup.radius = 1f;
                        blastAttackKnockup.procCoefficient = 0f;
                        blastAttackKnockup.position = hit_enemy.hitPosition;
                        blastAttackKnockup.attacker = characterBody.gameObject;
                        blastAttackKnockup.crit = false;
                        blastAttackKnockup.baseDamage = 0f;
                        blastAttackKnockup.canRejectForce = false;
                        blastAttackKnockup.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                        blastAttackKnockup.baseForce = 0;
                        blastAttackKnockup.bonusForce = Vector3.up * (baseKnockupForce + (enemy_body.characterMotor.mass * tailoredKnockupForceMultiplier));
                        blastAttackKnockup.teamIndex = characterBody.teamComponent.teamIndex;
                        blastAttackKnockup.damageType = DamageType.NonLethal;
                        blastAttackKnockup.attackerFiltering = AttackerFiltering.Default;
                        blastAttackKnockup.Fire();
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
