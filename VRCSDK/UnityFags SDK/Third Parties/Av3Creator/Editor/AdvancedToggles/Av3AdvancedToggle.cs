#region
using Av3Creator.AdvancedToggles.Modules;
using Av3Creator.Core;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
#endregion

namespace Av3Creator.AdvancedToggles
{
    [Serializable]
    public class Av3AdvancedToggle
    {
        public bool IsExpanded = true;
        public bool ModulesExpanded = false;

        public bool ParameterDriverIsExpanded;
        public int SelectedParameterType = 0;
        [SerializeReference] public List<Av3Preset> ParameterDriversToggled = new List<Av3Preset>();
        [SerializeReference] public List<Av3Preset> ParameterDriversDefault = new List<Av3Preset>();

        [SerializeReference] public List<IAv3AdvancedModule> Modules = new List<IAv3AdvancedModule>();
        public bool materialSwapExpanded;
        public bool toggleObjectsExpanded;
        public bool objectDissolveExpanded;
        public bool blendshapesExpanded;
        public bool animatedBlendshapesExpanded;

        public string Name;
        public string Parameter;
        public bool IsLocal;
        public Texture2D Icon;
        public bool DefaultValue;
        public bool Saved = true;

        public bool UseTexture;
        public Texture2D Texture;

        public bool UseCustomMenu;
        public VRCExpressionsMenu CustomMenu;
        public bool IsPaginator = true;

        public bool TransitionSettingsIsExpanded;
        public AnimatorCondition[] ExtraConditions;

        [NonSerialized] public Av3ToggleModuleTypes SelectedModule = Av3ToggleModuleTypes.None;

        public bool IsSelected;
        public bool UseBaseAnimation;
        public AnimationClip DefaultAnimation;
        public string DefaultAnimationPath;
        public AnimationClip ToggledAnimation;
        public string ToggledAnimationPath;

        internal string GenerateParameterName()
        {
            var settings = Av3AdvancedSettings.Instance.Settings;
            var menuName = !settings.CreateANewMenu && settings.CustomMenu != null ? settings.CustomMenu.name : string.IsNullOrEmpty(settings.MenuName) ? "Toggles" : settings.MenuName;
            Parameter = $"{menuName}/{Name}";
            return Parameter;
        }

        public void AddBlendshape(SkinnedMeshRenderer meshRenderer, int blendshapeIndex, bool isAnimated = false, float? defaultValue = null,  float? toggledValue = null)
        {
            if (meshRenderer == null || meshRenderer.sharedMesh == null) return;

            if (Modules == null) Modules = new List<IAv3AdvancedModule>();
            var module = new Av3AdvancedBlendshape()
            {
                TargetRenderer = meshRenderer,
                Expanded = true,
                IsAnimated = isAnimated
            };
            Modules.Add(module);
            ModulesExpanded = true;
            IsExpanded = true;

            var blendshapeName = meshRenderer.sharedMesh.GetBlendShapeName(blendshapeIndex);
            var originalWeight = defaultValue ?? meshRenderer.GetBlendShapeWeight(blendshapeIndex);

            if (module == null) return;
            if (module.SelectedBlendshapes == null) module.SelectedBlendshapes = new List<Av3Blendshape>();

            module.SelectedBlendshapes.Add(new Av3Blendshape(blendshapeIndex, blendshapeName, originalWeight, toggledValue ?? (originalWeight == 100 ? 0 : 100))); 
        }
    }

    public interface IAv3AdvancedModule
    {
        void OnImport();
        void Generate(ref AnimationClip anim);
        void GenerateDefault(ref AnimationClip anim);
        void DrawGUI(Av3AdvancedToggle toggle);
        void InitializeModule();
    }
}