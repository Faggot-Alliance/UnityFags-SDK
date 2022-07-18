#region
using Av3Creator.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Av3Creator.Core;
using Av3Creator.Supporter.Settings;
using Av3Creator.Utils.Interactions;
#endregion

namespace Av3Creator.Supporter
{
    public static class Av3AutoHUEShift
    {
        public static void DrawGUI(VRCAvatarDescriptor descriptor, Av3Settings settings)
        {
            var renderers = HUEShiftSettings.Instance.Renderers;
            EditorGUI.BeginChangeCheck();
            Av3StyleManager.DrawLabel("1. Select target renderers");
            using (new GUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
            {
                if (renderers == null)
                    HUEShiftSettings.Instance.Renderers = new System.Collections.Generic.List<Renderer>(1);

                for (int i = 0; i < renderers.Count; i++)
                {

                    using (new GUILayout.HorizontalScope()) 
                    {
                        HUEShiftSettings.Instance.Renderers[i] = (Renderer)EditorGUILayout.ObjectField(HUEShiftSettings.Instance.Renderers[i], typeof(Renderer), true);

                        using(new EditorGUI.DisabledScope(renderers.Count <= 1))
                            if(GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(30)))
                            {
                                HUEShiftSettings.Instance.Renderers.RemoveAt(i);
                                break;
                            }

                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus@2x"), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(30)))
                        {
                            HUEShiftSettings.Instance.Renderers.Add(null);
                            break;
                        }
                    } 
                }
            }

            
            if (EditorGUI.EndChangeCheck())
            {
                HUEShiftSettings.Instance.SelectedMaterials = HUEShiftSettings.Instance.SelectedMaterials.Where(x => HUEShiftSettings.Instance.Renderers.Any(y => y.sharedMaterials.Contains(x))).ToList();
                HUEShiftSettings.Instance.CommonIsExpanded = false;
                HUEShiftSettings.Instance.UncommonIsExpanded = false;
                HUEShiftSettings.Instance.SettingsIsExpanded = false;
                HUEShiftSettings.Save();            
            }

            if (renderers?.Count(x => x != null) > 0)
            {
                for (int i = 0; i < HUEShiftSettings.Instance.Renderers.Count; i++)
                {
                    var x = HUEShiftSettings.Instance.Renderers[i];
                    if (x != null && !(x is SkinnedMeshRenderer || x is MeshRenderer || x is LineRenderer || x is TrailRenderer))
                        HUEShiftSettings.Instance.Renderers[i] = null;

                }
                Av3StyleManager.DrawLabel("2. Select target materials");
                using (new GUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                {
                    var sharedMaterials = 
                        (from _renderer in HUEShiftSettings.Instance.Renderers
                         where _renderer != null
                         let _materials = _renderer.sharedMaterials
                         from _material in _materials
                         where _material.IsPoiyomi()
                         select _material).Distinct().ToArray();

                    foreach (var material in sharedMaterials)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var isSelected = HUEShiftSettings.Instance.SelectedMaterials.Contains(material);
                            EditorGUI.BeginChangeCheck();
                            isSelected = EditorGUILayout.ToggleLeft("", isSelected, GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (isSelected) HUEShiftSettings.Instance.SelectedMaterials.Add(material);
                                else HUEShiftSettings.Instance.SelectedMaterials.Remove(material);
                                HUEShiftSettings.Save();
                            }

                            EditorGUILayout.ObjectField(material, typeof(Material), false);
                        }
                    }

                    using(new GUILayout.HorizontalScope())
                    {
                        if(GUILayout.Button("Select None", EditorStyles.miniButtonLeft))
                        {
                            HUEShiftSettings.Instance.SelectedMaterials.Clear();
                            HUEShiftSettings.Save();
                        }    

                        if(GUILayout.Button("Select All", EditorStyles.miniButtonRight))
                        {
                            HUEShiftSettings.Instance.SelectedMaterials = sharedMaterials.ToList();
                            HUEShiftSettings.Save();
                        }
                    }
                }


                var selectedMaterials = HUEShiftSettings.Instance.SelectedMaterials;
                if (selectedMaterials != null && selectedMaterials.Count > 0)
                {
                    Av3StyleManager.DrawLabel("3. Settings");
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    using (new GUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                    {
                        using(new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                Av3StyleManager.DrawLabel("Name", padding: 20);
                                Av3StyleManager.DrawLabel("Menu", padding: 20);

                            }

                            using (new GUILayout.VerticalScope())
                            {
                                HUEShiftSettings.Instance.Name = EditorGUILayout.TextField(HUEShiftSettings.Instance.Name);
                                HUEShiftSettings.Instance.TargetMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(HUEShiftSettings.Instance.TargetMenu, typeof(VRCExpressionsMenu), false);
                            }
                        }
                       
                        HUEShiftSettings.Instance.WriteDefaults = EditorGUILayout.ToggleLeft("Write Defaults", HUEShiftSettings.Instance.WriteDefaults);
                        Av3StyleManager.DrawFoldout("Properties to HUE Shift (most common)", ref HUEShiftSettings.Instance.CommonIsExpanded, () => {
                            using (new GUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                            {
                                HUEShiftSettings.Instance.Main = EditorGUILayout.ToggleLeft("Main (Main Texture/Color)", HUEShiftSettings.Instance.Main);
                                HUEShiftSettings.Instance.Emission1 = EditorGUILayout.ToggleLeft("Emission 1", HUEShiftSettings.Instance.Emission1);
                                HUEShiftSettings.Instance.Emission2 = EditorGUILayout.ToggleLeft("Emission 2", HUEShiftSettings.Instance.Emission2);
                                HUEShiftSettings.Instance.Dissolve = EditorGUILayout.ToggleLeft("Dissolve", HUEShiftSettings.Instance.Dissolve);
                                HUEShiftSettings.Instance.DissolveEdge = EditorGUILayout.ToggleLeft("Dissolve Edge", HUEShiftSettings.Instance.DissolveEdge);
                            }
                        }, false);

                        Av3StyleManager.DrawFoldout("Other Properties", ref HUEShiftSettings.Instance.UncommonIsExpanded, () => {
                            using (new GUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                            {
                                HUEShiftSettings.Instance.Decal1 = EditorGUILayout.ToggleLeft("Decal 1", HUEShiftSettings.Instance.Decal1);
                                HUEShiftSettings.Instance.Decal2 = EditorGUILayout.ToggleLeft("Decal 2", HUEShiftSettings.Instance.Decal2);
                                HUEShiftSettings.Instance.Decal3 = EditorGUILayout.ToggleLeft("Decal 3", HUEShiftSettings.Instance.Decal3);
                                HUEShiftSettings.Instance.Decal4 = EditorGUILayout.ToggleLeft("Decal 4", HUEShiftSettings.Instance.Decal4);
                                HUEShiftSettings.Instance.BackFace = EditorGUILayout.ToggleLeft("Backface", HUEShiftSettings.Instance.BackFace);
                                HUEShiftSettings.Instance.RimLight = EditorGUILayout.ToggleLeft("Rim Light", HUEShiftSettings.Instance.RimLight);
                                HUEShiftSettings.Instance.Matcap = EditorGUILayout.ToggleLeft("Matcap", HUEShiftSettings.Instance.Matcap);
                                HUEShiftSettings.Instance.Matcap2 = EditorGUILayout.ToggleLeft("Matcap 2", HUEShiftSettings.Instance.Matcap2);
                                HUEShiftSettings.Instance.Flipbook = EditorGUILayout.ToggleLeft("Flipbook", HUEShiftSettings.Instance.Flipbook);
                                HUEShiftSettings.Instance.Glitter = EditorGUILayout.ToggleLeft("Glitter", HUEShiftSettings.Instance.Glitter);
                                HUEShiftSettings.Instance.Outline = EditorGUILayout.ToggleLeft("Outline", HUEShiftSettings.Instance.Outline);
                            }
                        }, false);

                        if (changeScope.changed) HUEShiftSettings.Save();
                    }



                    GUILayout.Space(5);

                    bool hasLockedMaterials = false;
                    if (PoiyomiInteractions.IsPoiyomiPresent())
                    {
                        var lockedMaterials = (from Material in HUEShiftSettings.Instance.SelectedMaterials
                                               where Material.IsMaterialLocked()
                                               select Material).Distinct().ToList();

                        if (lockedMaterials != null && lockedMaterials.Count > 0)
                        {
                            hasLockedMaterials = true;
                            Av3StyleManager.DrawIssueBox(MessageType.Error, "<b>[Poiyomi Dissolve]</b> You have some <b>materials</b> that are <b>currently locked</b>, you have to unlock them before create a HUE Shift.", () =>
                            {
                                foreach (var material in lockedMaterials) material.Unlock();
                                hasLockedMaterials = false;
                            });
                        }
                    }
                    // todo: check if the parameters have space
                    using(new EditorGUI.DisabledScope(hasLockedMaterials))
                    if (GUILayout.Button("Generate HUE Shift", GUILayout.Height(26)))
                    {
                        Generate(descriptor, HUEShiftSettings.Instance, settings);
                    }
                }
            }
        }

        private static void SetAnimatedTag(this Material material, string name) => material.SetOverrideTag(name, "2"); 

        private static void Generate(VRCAvatarDescriptor descriptor, HUEShiftSettings settings, Av3Settings mainSettings)
        {
            if (string.IsNullOrEmpty(settings.Name)) return;

            if (string.IsNullOrEmpty(mainSettings.OutputDirectory)) throw new System.Exception("Avatar Directory can not be null");

            var fxController = (descriptor?.baseAnimationLayers[4].animatorController is AnimatorController _fxLayer) ? _fxLayer : null;
            if (fxController == null) throw new Exception("FX Layer cant be null!");

            var vrcParams = descriptor.expressionParameters;
            if (vrcParams == null)
                throw new Exception("Your avatar dont have a VRCExpressionParameters, please use the quick fix to automatically add one");

            var totalSize = vrcParams.parameters.Aggregate(VRCExpressionParameters.TypeCost(VRCExpressionParameters.ValueType.Float),
                                               (total, param) => total + VRCExpressionParameters.TypeCost(param.valueType));

            if (totalSize > VRCExpressionParameters.MAX_PARAMETER_COST)
            {
                var requiredSize = (VRCExpressionParameters.MAX_PARAMETER_COST - totalSize) * -1;
                throw new Exception("You dont have enough space to add a Float parameter (you need more " + requiredSize + " bits), please remove some parameters and try again!");
            }

            AnimationClip anim = new AnimationClip()
            {
                wrapMode = WrapMode.Once
            };

            
            var curve = AnimationCurve.Linear(0f, 0f, 1f / 60f, 1f);

            foreach (var currentRenderer in HUEShiftSettings.Instance.Renderers.Where(x => x != null))
            {
                var path = AnimationUtility.CalculateTransformPath(currentRenderer.transform, descriptor.transform);
                // loop selected materials
                foreach (var material in settings.SelectedMaterials.Where(x => currentRenderer.sharedMaterials.Contains(x)))
                {
                    string animPropertySuffix = new string(material.name.Trim().ToLower().Where(char.IsLetter).ToArray());
                    if (settings.Main)
                    {
                        material.SetFloat("_MainHueShiftToggle", 1);
                        material.SetFloat("_MainHueShiftReplace", 1);
                        material.SetAnimatedTag("_MainHueShiftAnimated");
                    }

                    if (settings.Decal1)
                    {
                        material.SetFloat("_DecalHueShiftEnabled", 1);
                        material.SetAnimatedTag("_DecalHueShiftAnimated");
                    }

                    if (settings.Decal2)
                    {
                        material.SetFloat("_DecalHueShiftEnabled1", 1);
                        material.SetAnimatedTag("_DecalHueShift1Animated");
                    }

                    if (settings.Decal3)
                    {
                        material.SetFloat("_DecalHueShiftEnabled2", 1);
                        material.SetAnimatedTag("_DecalHueShift2Animated");
                    }

                    if (settings.Decal4)
                    {
                        material.SetFloat("_DecalHueShiftEnabled3", 1);
                        material.SetAnimatedTag("_DecalHueShift3Animated");
                    }

                    if (settings.BackFace)
                    {
                        material.SetAnimatedTag("_BackFaceHueShiftAnimated");
                    }

                    if (settings.RimLight)
                    {
                        material.SetFloat("_RimHueShiftEnabled", 1);
                        material.SetAnimatedTag("_RimHueShiftAnimated");
                    }

                    if (settings.Matcap)
                    {
                        material.SetFloat("_MatcapHueShiftEnabled", 1);
                        material.SetAnimatedTag("_MatcapHueShiftAnimated");
                    }

                    if (settings.Matcap2)
                    {
                        material.SetFloat("_Matcap2HueShiftEnabled", 1);
                        material.SetAnimatedTag("_Matcap2HueShiftAnimated");
                    }

                    if (settings.Emission1)
                    {
                        material.SetFloat("_EmissionHueShiftEnabled", 1);
                        material.SetAnimatedTag("_EmissionHueShiftAnimated");
                    }

                    if (settings.Emission2)
                    {
                        material.SetFloat("_EmissionHueShiftEnabled1", 1);
                        material.SetAnimatedTag("_EmissionHueShift1Animated");
                    }

                    if (settings.Flipbook)
                    {
                        material.SetFloat("_FlipbookHueShiftEnabled", 1);
                        material.SetAnimatedTag("_FlipbookHueShiftAnimated");
                    }

                    if (settings.Dissolve)
                    {
                        material.SetFloat("_DissolveHueShiftEnabled", 1);
                        material.SetAnimatedTag("_DissolveHueShiftAnimated");
                    }

                    if (settings.DissolveEdge)
                    {
                        material.SetFloat("_DissolveEdgeHueShiftEnabled", 1);
                        material.SetAnimatedTag("_DissolveEdgeHueShiftAnimated");
                    }

                    if (settings.Glitter)
                    {
                        material.SetFloat("_GlitterHueShiftEnabled", 1);
                        material.SetAnimatedTag("_GlitterHueShiftAnimated");
                    }

                    if (settings.Outline)
                    {
                        material.SetFloat("_OutlineHueShift", 1);
                        material.SetAnimatedTag("_OutlineHueOffsetAnimated");
                    }

                    material.EnableKeyword("COLOR_GRADING_HDR");
                    EditorUtility.SetDirty(material);

                    #region
                    if (settings.Main)
                        anim.SetCurve(path, typeof(Renderer), "material._MainHueShift_" + animPropertySuffix, curve);


                    if (settings.Decal1)
                        anim.SetCurve(path, typeof(Renderer), "material._DecalHueShift_" + animPropertySuffix, curve);


                    if (settings.Decal2)
                        anim.SetCurve(path, typeof(Renderer), "material._DecalHueShift1_" + animPropertySuffix, curve);


                    if (settings.Decal3)
                        anim.SetCurve(path, typeof(Renderer), "material._DecalHueShift2_" + animPropertySuffix, curve);


                    if (settings.Decal4)
                        anim.SetCurve(path, typeof(Renderer), "material._DecalHueShift3_" + animPropertySuffix, curve);


                    if (settings.BackFace)
                        anim.SetCurve(path, typeof(Renderer), "material._BackFaceHueShift_" + animPropertySuffix, curve);


                    if (settings.RimLight)
                        anim.SetCurve(path, typeof(Renderer), "material._RimHueShift_" + animPropertySuffix, curve);


                    if (settings.Matcap)
                        anim.SetCurve(path, typeof(Renderer), "material._MatcapHueShift_" + animPropertySuffix, curve);


                    if (settings.Matcap2)
                        anim.SetCurve(path, typeof(Renderer), "material._Matcap2HueShift_" + animPropertySuffix, curve);


                    if (settings.Emission1)
                        anim.SetCurve(path, typeof(Renderer), "material._EmissionHueShift_" + animPropertySuffix, curve);


                    if (settings.Emission2)
                        anim.SetCurve(path, typeof(Renderer), "material._EmissionHueShift1_" + animPropertySuffix, curve);


                    if (settings.Flipbook)
                        anim.SetCurve(path, typeof(Renderer), "material._FlipbookHueShift_" + animPropertySuffix, curve);


                    if (settings.Dissolve)
                        anim.SetCurve(path, typeof(Renderer), "material._DissolveHueShift_" + animPropertySuffix, curve);


                    if (settings.DissolveEdge)
                        anim.SetCurve(path, typeof(Renderer), "material._DissolveEdgeHueShift_" + animPropertySuffix, curve);


                    if (settings.Glitter)
                        anim.SetCurve(path, typeof(Renderer), "material._GlitterHueShift_" + animPropertySuffix, curve);


                    if (settings.Outline)
                        anim.SetCurve(path, typeof(Renderer), "material._OutlineHueOffset_" + animPropertySuffix, curve);

                    #endregion
                }
            }

            var directory = mainSettings.OutputDirectory + "/Animations";
            Directory.CreateDirectory(directory);

            var animationOutputPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{settings.Name}.anim");

            AssetDatabase.CreateAsset(anim, animationOutputPath);

            string parameterName = "HUE/" + settings.Name;
            if (!descriptor.expressionParameters.AddParameter(parameterName, VRCExpressionParameters.ValueType.Float)) return;
            bool existParam = fxController.parameters.Any(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Bool);

            if (existParam) fxController.RemoveParameter(fxController.parameters.Single(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Float));
            fxController.AddParameter(parameterName, AnimatorControllerParameterType.Float);

            //create layer
            var layerName = "HUE: " + settings.Name;
            bool existLayer = fxController.layers.Any(x => x.name == layerName);

            if (existLayer) fxController.RemoveLayer(layerName);
            fxController.AddLayer(layerName);
            

            var layers = fxController.layers;
            var layer = layers[fxController.layers.Length - 1];
            layer.defaultWeight = 1;

            var state = layer.stateMachine.AddState(settings.Name, new Vector3(150, 70));
            state.writeDefaultValues = settings.WriteDefaults;
            state.motion = AssetDatabase.LoadAssetAtPath<Motion>(animationOutputPath);
            state.timeParameterActive = true;
    
            state.timeParameter = parameterName;

            layer.stateMachine.entryPosition = layer.stateMachine.anyStatePosition + new Vector3(0, -10);
            layer.stateMachine.anyStatePosition = layer.stateMachine.entryPosition + new Vector3(0, 40);
            layer.stateMachine.exitPosition = layer.stateMachine.anyStatePosition + new Vector3(0, 40);

            if(settings.TargetMenu != null) settings.TargetMenu.AddToMenu(settings.Name, VRCExpressionsMenu.Control.ControlType.RadialPuppet, null, parameterName);
            else descriptor.expressionsMenu?.AddToMenu(settings.Name, VRCExpressionsMenu.Control.ControlType.RadialPuppet, null, parameterName);
            
            EditorUtility.SetDirty(state);

            fxController.layers = layers;
            EditorUtility.SetDirty(fxController);


            if (PoiyomiInteractions.IsPoiyomiPresent() && EditorUtility.DisplayDialog("Lock Materials?",
            "You have to lock your materials to hue shift work properly, do you want to lock them now?\n", "Yes", "No"))
            {
                foreach (var material in settings.SelectedMaterials)
                    material.Lock();
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}