#region
using Av3Creator.Core;
using Av3Creator.Supporter.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using HarmonyLib;
using Av3Creator.Windows;
#endregion

namespace Av3Creator.Supporter 
{

    [InitializeOnLoad, ExecuteInEditMode]
    public class SupporterFunctions
    {
        private static HarmonyMethod GetPatch(string name) => new HarmonyMethod(AccessTools.Method(typeof(SupporterFunctions), name));
        static SupporterFunctions() => Initialize();

        private static readonly Type SceneHierarchy = AccessTools.TypeByName("UnityEditor.SceneHierarchy");
        private static void Initialize()
        {
            var harmony = new Harmony("Av3Creator_SupporterPatcher");

            Av3Patches.TryToPatch(harmony, AccessTools.Method(SceneHierarchy, "AddCreateGameObjectItemsToMenu"),
              prefix: GetPatch(nameof(GameObjectInterceptor)));
        }

        public static void GameObjectInterceptor(GenericMenu menu)
        {
            var selection = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
            if (selection == null || selection.Length <= 0) return;
            var vrcDescriptor = selection[0].GetComponentInParent<VRCAvatarDescriptor>();
            if (vrcDescriptor == null || selection[0].GetComponent<VRCAvatarDescriptor>() == vrcDescriptor) return;
            bool hasRenderer = Selection.GetFiltered<Renderer>(SelectionMode.Editable).Length > 0;

            if (hasRenderer) {
                menu.AddItem(new GUIContent("Av3Creator/Quick/Add to Auto HUE Shift"), false, () =>
                {
                    foreach (var selected in selection)
                    {
                        if (selected == null || selected.GetComponent<VRCAvatarDescriptor>() == vrcDescriptor) continue;
                        var renderer = selected.GetComponent<Renderer>();
                        if (renderer == null || !(renderer is SkinnedMeshRenderer || renderer is MeshRenderer || renderer is LineRenderer || renderer is TrailRenderer)) continue;

                        if (HUEShiftSettings.Instance.Renderers == null) HUEShiftSettings.Instance.Renderers = new List<Renderer>();
                        HUEShiftSettings.Instance.Renderers.Add(renderer);
                    }

                    if(HUEShiftSettings.Instance.Renderers.Any(x => x != null))
                        HUEShiftSettings.Instance.Renderers = HUEShiftSettings.Instance.Renderers.Where(x => x != null).ToList();
                    HUEShiftSettings.Instance.Renderers = HUEShiftSettings.Instance.Renderers.Distinct().ToList();

                    HUEShiftSettings.Save();
                    Av3CreatorWindow.ShowAv3Creator();
                    Av3CreatorWindow.Av3Instance.Settings.EasyHUEShifterExpanded = true;
                });
            }
        }
    }
}
