﻿using BepInEx.Configuration;
using AlistarMod.Modules;
using AlistarMod.Modules.Characters;
using AlistarMod.Survivors.Alistar.Components;
using AlistarMod.Survivors.Alistar.SkillStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlistarMod.Survivors.Alistar
{
    public class AlistarSurvivor : SurvivorBase<AlistarSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "alistarassetbundle"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "AlistarBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "AlistarMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlAlistar";
        public override string displayPrefabName => "DisplayAlistar";

        public const string ALISTAR_PREFIX = AlistarPlugin.DEVELOPER_PREFIX + "_ALISTAR_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => ALISTAR_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = ALISTAR_PREFIX + "NAME",
            subtitleNameToken = ALISTAR_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("alistar_icon"),
            bodyColor = Color.magenta,
            sortPosition = 100,

            crosshair = MyAssets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 200f,
            healthRegen = 1.5f,
            armor = 0f,
            moveSpeed = 8f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "AlistarModel",
                }
        };

        public override UnlockableDef characterUnlockableDef => AlistarUnlockables.characterUnlockableDef;
        
        public override ItemDisplaysBase itemDisplays => new AlistarItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            AlistarUnlockables.Init();

            base.InitializeCharacter();

            AlistarConfig.Init();
            AlistarStates.Init();
            AlistarTokens.Init();

            AlistarAssets.Init(assetBundle);
            AlistarBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<AlistarWeaponComponent>();
            //bodyPrefab.AddComponent<HuntressTrackerComopnent>();
            //anything else here
        }

        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "AlistarMeleeGroup", "AlistarMeleeHitbox");
        }

        public override void InitializeEntityStateMachines() 
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(EntityStates.GenericCharacterMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
                //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            //AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = ALISTAR_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = ALISTAR_PREFIX + "PASSIVE_DESCRIPTION",
                keywordToken = "KEYWORD_STUNNING",
                icon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
            };

            //option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "AlistarPassive",
                skillNameToken = ALISTAR_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = ALISTAR_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            SteppedSkillDef unbreakableWillSkillDef = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "UnbreakableWill",
                    ALISTAR_PREFIX + "UNBREAKABLE_WILL_NAME",
                    ALISTAR_PREFIX + "UNBREAKABLE_WILL_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("unbreakable_will_icon"),
                    new EntityStates.SerializableEntityStateType(typeof(SkillStates.UnbreakableWill)),
                    "Weapon2",
                    true
                ));
            //custom Skilldefs can have additional fields that you can set manually
            unbreakableWillSkillDef.stepCount = 3;
            unbreakableWillSkillDef.stepGraceDuration = 0.5f;
            unbreakableWillSkillDef.keywordTokens = new string[] { };

            Skills.AddPrimarySkills(bodyPrefab, unbreakableWillSkillDef);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            SkillDef pulverizeSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Pulverize",
                skillNameToken = ALISTAR_PREFIX + "PULVERIZE_NAME",
                skillDescriptionToken = ALISTAR_PREFIX + "PULVERIZE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("pulverize_icon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Pulverize)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = AlistarStaticValues.pulverizeCooldown,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, pulverizeSkillDef);
        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            SkillDef headbuttSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Headbutt",
                skillNameToken = ALISTAR_PREFIX + "HEADBUTT_NAME",
                skillDescriptionToken = ALISTAR_PREFIX + "HEADBUTT_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("headbutt_icon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Headbutt)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = AlistarStaticValues.headbuttCooldown,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, headbuttSkillDef);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            SkillDef trampleSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Trample",
                skillNameToken = ALISTAR_PREFIX + "TRAMPLE_NAME",
                skillDescriptionToken = ALISTAR_PREFIX + "TRAMPLE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("trample_icon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Trample)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = AlistarStaticValues.trampleCooldown,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddSpecialSkills(bodyPrefab, trampleSkillDef);
        }
        #endregion skills
        
        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texMainSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            //defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySword",
            //    "meshHenryGun",
            //    "meshHenry");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            
            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);
            
            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            AlistarAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(AlistarBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }
        }
    }
}