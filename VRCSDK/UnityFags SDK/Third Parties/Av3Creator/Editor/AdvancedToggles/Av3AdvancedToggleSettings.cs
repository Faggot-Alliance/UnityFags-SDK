#region
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Av3Creator.AdvancedToggles.Modules;
using System.Linq;
using Av3Creator.Core;
#endregion

namespace Av3Creator.AdvancedToggles
{
    public enum Av3ToggleModuleTypes
    {
        None,
        SimpleToggle,
        ChangeMaterial,
        Blendshape,
        //AnimatedBlendshape,
        PoiyomiDissolve
    }

    [Serializable]
    public class Av3Blendshape
    {
        public int Index;
        public string Name;
        public float OriginalValue;
        public float Value;

        public Av3Blendshape(int originalIndex, string name, float originalValue, float value)
        {
            Index = originalIndex;
            Name = name;
            OriginalValue = originalValue;
            Value = value;
        }
    }

    [Serializable]
    public class Av3AdvancedToggleSettings
    {
        public string OutputDirectory;
        public bool SettingsExpanded;
        public bool WriteDefaults = true;
        public bool Overwrite = true;
        public bool AddToMainMenu = true;
        public bool DisableCredits;
        public bool PingFolder = true;

        public bool SyncInMenu = true;
        public VRCExpressionsMenu CustomMenu;
        public bool CreateANewMenu = true;
        public string MenuName = "Toggles";
    }


    [Serializable]
    [InitializeOnLoad]
    internal class Av3AdvancedSettings
    {
        static Av3AdvancedSettings()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private static void EditorApplication_quitting()
        {
            Save();
        }

        private static readonly string DATA_KEY = "Av3Creator_AdvancedToggle_Settings";

        private static Av3AdvancedSettings _instance;
        public static Av3AdvancedSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Av3AdvancedSettings();
                    var data = EditorPrefs.GetString(DATA_KEY, JsonUtility.ToJson(_instance, false));
                    JsonUtility.FromJsonOverwrite(data, _instance);
                }

                return _instance;
            }
        }

        public static void Save() => EditorPrefs.SetString(DATA_KEY, JsonUtility.ToJson(Instance, false));

        [SerializeReference] public VRCAvatarDescriptor VRCAvatarDescriptor;
        public List<Av3AdvancedToggle> AdvancedToggles = new List<Av3AdvancedToggle>();
        public Av3AdvancedToggleSettings Settings = new Av3AdvancedToggleSettings();

        public static void AddBlendshape(SkinnedMeshRenderer renderer, int blendshapeKey, bool isAnimated = false, float? offWeight = 0, float? onWeight = 100)
        {
            if (renderer == null || renderer.sharedMesh == null) return;
           var blendshapeName = renderer.sharedMesh.GetBlendShapeName(blendshapeKey);

            Instance.AdvancedToggles.Add(new Av3AdvancedToggle()
            {
                Name = blendshapeName
            });
            var lastToggle = Instance.AdvancedToggles.Last();
            lastToggle.ModulesExpanded = true;
            lastToggle.IsExpanded = true;

            var module = new Av3AdvancedBlendshape()
            {
                TargetRenderer = renderer,
                Expanded = true, 
                IsAnimated = isAnimated
            };
            lastToggle.Modules.Add(module);

            //var lastBlendshapeModule = lastToggle.Modules.Last(x => x is Av3AdvancedBlendshape) as Av3AdvancedBlendshape;
            var originalWeight = offWeight ?? renderer.GetBlendShapeWeight(blendshapeKey);

            if (module == null) return;
            if (module.SelectedBlendshapes == null) module.SelectedBlendshapes = new List<Av3Blendshape>();

            module.SelectedBlendshapes.Add(new Av3Blendshape(blendshapeKey, blendshapeName, originalWeight, onWeight ?? (originalWeight == 100 ? 0 : 100)));
            module.InitializeModule();
        }

        //public static void AddAnimatedBlendshape(SkinnedMeshRenderer renderer, int blendshapeKey)
        //{
        //    if (renderer == null || renderer.sharedMesh == null) return;
        //    var blendshapeName = renderer.sharedMesh.GetBlendShapeName(blendshapeKey);

        //    Instance.AdvancedToggles.Add(new Av3AdvancedToggle()
        //    {
        //        Name = blendshapeName
        //    });
        //    var lastToggle = Instance.AdvancedToggles.Last();
        //    lastToggle.ModulesExpanded = true;
        //    lastToggle.IsExpanded = true;

        //    var module = new Av3AdvancedAnimatedBlendshape()
        //    {
        //        TargetRenderer = renderer,
        //        Expanded = true
        //    };
        //    lastToggle.Modules.Add(module);

        //    //var lastBlendshapeModule = lastToggle.Modules.Last(x => x is Av3AdvancedAnimatedBlendshape) as Av3AdvancedAnimatedBlendshape;
        //    var originalWeight = renderer.GetBlendShapeWeight(blendshapeKey);

        //    if (module == null) return;
        //    if (module.SelectedBlendshapes == null) module.SelectedBlendshapes = new List<Av3Blendshape>();

        //    module.SelectedBlendshapes.Add(new Av3Blendshape(blendshapeKey, blendshapeName, originalWeight, originalWeight == 100 ? 0 : 100));
        //    module.InitializeModule();
        //}
    }
}