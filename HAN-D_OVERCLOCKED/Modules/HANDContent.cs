﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.ContentManagement;
using System.Collections;

namespace HandPlugin.Modules
{
    class HANDContent : IContentPackProvider
    {
        public static AssetBundle assets;
        internal static ContentPack contentPack = new ContentPack();

        public static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<Type> entityStates = new List<Type>();
        public static List<GameObject> masterPrefabs = new List<GameObject>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();

        public string identifier => "HAND_OVERCLOCKED.content";

        //TODO: UNCOMMENT
        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            Debug.Log("1");
            //contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            Debug.Log("2");
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());
            //contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
            //contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            //contentPack.survivorDefs.Add(survivorDefs.ToArray());
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
