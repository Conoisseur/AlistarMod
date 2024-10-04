using EntityStates;
using AlistarMod.Survivors.Alistar;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace AlistarMod.Survivors.Alistar.SkillStates
{
    public class Trample : BaseSkillState
    {
        public static float damageCoefficient = AlistarStaticValues.trampleDamageCoefficient;
        public static float procCoefficient = AlistarStaticValues.trampleProcCoefficient;
        public static float baseDuration = AlistarStaticValues.trampleBaseDuration;
        public static float baseDamageInterval = AlistarStaticValues.trampleBaseDamageInterval;
        public static float radius = AlistarStaticValues.trampleRadius;
        public static float knockupForce = AlistarStaticValues.trampleKnockupForce;

        public static float trampleBuffDuration = AlistarStaticValues.trampleKnockupForce;
        public static float trampleDebuffDuration = AlistarStaticValues.trampleKnockupForce;

        private float duration;
        private float damageInterval;
        private float damageTimer;
        private int hitStackCount; // Stack counter for hits
        private const int maxHitStacks = 5; // Max stacks before granting buff
        private HashSet<HealthComponent> hitTargets; // Store hit targets for the current damage interval

        public static GameObject groundSlamEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBruiser/ClayShockwaveEffect.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration;
            damageInterval = baseDamageInterval / attackSpeedStat;
            damageTimer = 0f;
            hitStackCount = 0;
            hitTargets = new HashSet<HealthComponent>();

            DealDamage();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Recalculate damage interval based on the current attack speed
            damageInterval = baseDamageInterval / characterBody.attackSpeed;

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                DealDamage();
                damageTimer = 0f;
                hitTargets.Clear();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void DealDamage()
        {
            PlayAnimation("FullBody, Override", "Trample", "Trample.playbackRate", damageInterval);

            Util.PlaySound("Play_bison_headbutt_attack_hit", gameObject);

            // Spawn effects randomly in radius around player
            for (int i = 0; i < 12; i++)
            {
                Vector3 randomPosition = GetRandomPositionWithinRadius(characterBody.footPosition, radius);
                EffectManager.SpawnEffect(groundSlamEffectPrefab, new EffectData
                {
                    origin = randomPosition,
                    scale = AlistarStaticValues.trampleRadius,
                    rotation = Quaternion.Euler(90f, 0f, 0f)
                }, true);
            }

            if (isAuthority)
            {
                // Create damaging blast in area
                BlastAttack trampleAttackDamage = new BlastAttack();
                trampleAttackDamage.radius = radius;
                trampleAttackDamage.procCoefficient = procCoefficient;
                trampleAttackDamage.position = transform.position;
                trampleAttackDamage.attacker = characterBody.gameObject;
                trampleAttackDamage.crit = RollCrit();
                trampleAttackDamage.baseDamage = damageCoefficient * damageStat;
                trampleAttackDamage.canRejectForce = false;
                trampleAttackDamage.falloffModel = BlastAttack.FalloffModel.None;
                trampleAttackDamage.baseForce = 0;
                trampleAttackDamage.bonusForce = Vector3.zero;
                trampleAttackDamage.teamIndex = characterBody.teamComponent.teamIndex;
                trampleAttackDamage.damageType = DamageType.Generic;
                trampleAttackDamage.attackerFiltering = AttackerFiltering.Default;
                BlastAttack.Result trampleAttackDamageResult = trampleAttackDamage.Fire();

                // Create individual blast for each hit enemy which knocks them up if they are grounded
                foreach (BlastAttack.HitPoint hit_enemy in trampleAttackDamageResult.hitPoints)
                {
                    CharacterBody enemy_body = hit_enemy.hurtBox.healthComponent.body;

                    // Add debuff which locks enemy movement
                    // AddDebuff(enemy_body);

                    // Apply knock-up force only to grounded enemies
                    bool isGrounded = hit_enemy.hurtBox.healthComponent.body.characterMotor && hit_enemy.hurtBox.healthComponent.body.characterMotor.isGrounded;
                    bool hasRigidbody = hit_enemy.hurtBox.healthComponent.body.rigidbody != null;
                    bool isOnGround = hasRigidbody && Physics.Raycast(hit_enemy.hurtBox.transform.position, Vector3.down, 0.1f, LayerIndex.world.mask);

                    if (isGrounded || isOnGround)
                    {
                        BlastAttack trampleKnockupAttack = new BlastAttack();
                        trampleKnockupAttack.radius = 1f;
                        trampleKnockupAttack.procCoefficient = 0f;
                        trampleKnockupAttack.position = hit_enemy.hitPosition;
                        trampleKnockupAttack.attacker = characterBody.gameObject;
                        trampleKnockupAttack.crit = false;
                        trampleKnockupAttack.baseDamage = 0f;
                        trampleKnockupAttack.canRejectForce = false;
                        trampleKnockupAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                        trampleKnockupAttack.baseForce = 0;
                        trampleKnockupAttack.bonusForce = Vector3.up * knockupForce;
                        trampleKnockupAttack.teamIndex = characterBody.teamComponent.teamIndex;
                        trampleKnockupAttack.damageType = DamageType.NonLethal;
                        trampleKnockupAttack.attackerFiltering = AttackerFiltering.Default;
                        trampleKnockupAttack.Fire();
                    }

                    // Increment stacks to know when to apply buff
                    // Stack for each enemy hit
                    hitStackCount++;
                    if (hitStackCount >= maxHitStacks)
                    {
                        Util.PlaySound("Play_teamWarCry_activate", gameObject);
                        AddBuff();
                        hitStackCount = 0;
                    }
                }
            }
        }

        private Vector3 GetRandomPositionWithinRadius(Vector3 center, float radius)
        {
            float randomAngle = Random.Range(0f, 360f);
            float randomRadius = Random.Range(0f, radius);
            Vector3 offset = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(randomAngle * Mathf.Deg2Rad)) * randomRadius;
            return center + offset;
        }

        private void AddDebuff(CharacterBody body)
        {
            body.AddTimedBuff(RoR2Content.Buffs.NullifyStack, AlistarStaticValues.trampleDebuffDuration);
            SetStateOnHurt component = body.healthComponent.GetComponent<SetStateOnHurt>();
            if (component == null)
            {
                return;
            }
            component.SetStun(-1f);
        }

        private void AddBuff()
        {
            characterBody.AddTimedBuff(RoR2Content.Buffs.Energized, AlistarStaticValues.trampleBuffDuration);
            characterBody.AddTimedBuff(RoR2Content.Buffs.WhipBoost, AlistarStaticValues.trampleBuffDuration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
