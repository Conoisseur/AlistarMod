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

            damageTimer += Time.fixedDeltaTime;
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
            if (isAuthority)
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

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, LayerIndex.entityPrecise.mask);
                foreach (Collider hitCollider in hitColliders)
                {
                    HurtBox hurtBox = hitCollider.GetComponent<HurtBox>();
                    if (hurtBox && Util.IsValid(hurtBox) && hurtBox.healthComponent && hurtBox.healthComponent.body != characterBody)
                    {
                        if (!hitTargets.Contains(hurtBox.healthComponent))
                        {
                            hitTargets.Add(hurtBox.healthComponent); // Add the target to the HashSet

                            AddDebuff(hurtBox.healthComponent.body);
                            hurtBox.healthComponent.body.RecalculateStats();

                            DamageInfo damageInfo = new DamageInfo
                            {
                                attacker = gameObject,
                                inflictor = gameObject,
                                damage = damageCoefficient * damageStat,
                                position = hurtBox.transform.position,
                                procCoefficient = procCoefficient,
                                crit = RollCrit(),
                                damageColorIndex = DamageColorIndex.Default,
                                force = Vector3.zero
                            };
                            hurtBox.healthComponent.TakeDamage(damageInfo);
                            GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox.healthComponent.gameObject);

                            // Apply knock-up force only to grounded enemies
                            bool isGrounded = hurtBox.healthComponent.body.characterMotor && hurtBox.healthComponent.body.characterMotor.isGrounded;
                            bool hasRigidbody = hurtBox.healthComponent.body.rigidbody != null;
                            bool isOnGround = hasRigidbody && Physics.Raycast(hurtBox.transform.position, Vector3.down, 0.1f, LayerIndex.world.mask);

                            if (isGrounded || isOnGround)
                            {
                                if (hurtBox.healthComponent.body.characterMotor)
                                {
                                    hurtBox.healthComponent.body.characterMotor.ApplyForce(Vector3.up * knockupForce, true, false);
                                }
                                else if (hurtBox.healthComponent.body.rigidbody)
                                {
                                    hurtBox.healthComponent.body.rigidbody.AddForce(Vector3.up * knockupForce, ForceMode.Impulse);
                                }
                            }

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
            characterBody.AddTimedBuff(RoR2Content.Buffs.WarCryBuff, AlistarStaticValues.trampleBuffDuration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
