#region
using Av3Creator.AdvancedToggles.Modules;
using Av3Creator.Core;
using Av3Creator.Utils;
using Av3Creator.Utils.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;
#endregion

namespace Av3Creator.AdvancedToggles
{
    public class Av3AdvancedToggleCore
    {

        public static bool InWork = false;
        public static void SetupThings(VRCAvatarDescriptor descriptor, List<Av3AdvancedToggle> toggles, Av3AdvancedToggleSettings Settings)
        {
            InWork = true;
            try
            {             
                if (!(descriptor.baseAnimationLayers[Av3Layers.FX].animatorController is AnimatorController fxLayer) || fxLayer == null)
                    throw new Exception("Your avatar need to have a valid FX layer, ensure that you have setup correctly your descriptor.");

                //toggles = toggles.Where(x => x.Modules != null && x.Modules.Count > 0).ToList();
                toggles = toggles?.Where(x =>  (x.Modules != null && x.Modules.Count > 0) || (x.UseBaseAnimation && (x.DefaultAnimation != null || x.ToggledAnimation != null))).ToList();

                if (toggles == null || toggles.Count == 0)
                    throw new Exception("You do not have any toggle to be generated.");


                if (descriptor.expressionParameters == null || descriptor.expressionsMenu == null) throw new Exception("You avatar needs to have a VRCExpressionsParamaters and a VRCExpressionsMenu");


                // Setup missing things
                foreach(var toggle in toggles)
                {
                    if(string.IsNullOrWhiteSpace(toggle.Name)) toggle.Name = "Toggle " + toggles.IndexOf(toggle);
                    if (string.IsNullOrWhiteSpace(toggle.Parameter)) toggle.GenerateParameterName();

                    if (!Settings.Overwrite)
                        toggle.Parameter = ObjectNames.GetUniqueName(descriptor.expressionParameters.parameters.Select(x => x.name).ToArray(), toggle.Parameter);
                }

                if (!CanCreateParams(descriptor.expressionParameters, toggles.Count, out int sizeNeeded))
                    EditorUtility.DisplayDialog("Av3Creator: ERROR",
                        "Avatar doesn't have space to create this toggles, please consider to delete some unused parameters.\n" +
                        "You need more " + sizeNeeded + " byte(s).", "Close");


                // Get avatar name without invalid chars
                var avatarName = Av3Core.RemoveInvalidChars(descriptor.transform.root.name);
                var OutputDirectory = Settings.OutputDirectory;
                if (string.IsNullOrEmpty(OutputDirectory)) throw new System.Exception("Avatar Directory can not be null");

                var MenusDirectory = OutputDirectory + "/Menus/";
                var AnimationsDirectory = OutputDirectory + "/Animations/";

                // CREATE DIRECTORIES
                Directory.CreateDirectory(MenusDirectory);
                Directory.CreateDirectory(AnimationsDirectory);

                // Create Empty Anim
                var originalEmptyAnimationPath = AssetDatabase.GUIDToAssetPath("b146fcd37dbc7734dadd974b87aec8df");
                var emptyAnimationPath = AnimationsDirectory + "Empty Animation.anim";
                if (!string.IsNullOrEmpty(originalEmptyAnimationPath) && !File.Exists(emptyAnimationPath))
                    AssetDatabase.CopyAsset(originalEmptyAnimationPath, emptyAnimationPath);
                else if(string.IsNullOrEmpty(originalEmptyAnimationPath))
                    Debug.LogWarning("Can't find Av3Creator Empty Animation, this can break some things!");

                SetupAnimations(toggles, Settings, AnimationsDirectory);
                SetupFX(fxLayer, toggles, Settings, emptyAnimationPath);
                SetupParameterDrivers(fxLayer, Settings, toggles, emptyAnimationPath);
                SetupParameters(descriptor.expressionParameters, toggles);
                SetupMenu(toggles, Settings, MenusDirectory, descriptor.expressionsMenu ?? null);
                if (!Settings.DisableCredits) Av3Utils.GenerateCredits(fxLayer);
                AfterComplete(toggles);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                if(Settings.PingFolder) EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(OutputDirectory));
            }
            catch(Exception error)
            {
                Debug.LogException(error);
            }

            InWork = false;
        }

        public static bool CanCreateParams(VRCExpressionParameters vrcParams, int togglesToAdd, out int sizeNeeded)
        {
            sizeNeeded = 0;
            if (vrcParams == null) return false;

            var totalSize = vrcParams.parameters.Aggregate(togglesToAdd * VRCExpressionParameters.TypeCost(VRCExpressionParameters.ValueType.Bool),
                                                   (total, param) => total + VRCExpressionParameters.TypeCost(param.valueType));

            if (totalSize > VRCExpressionParameters.MAX_PARAMETER_COST) sizeNeeded = (VRCExpressionParameters.MAX_PARAMETER_COST - totalSize) * -1;

            return sizeNeeded <= 0;
        }

        private static void SetupParameterDrivers(AnimatorController fxController, Av3AdvancedToggleSettings Settings, List<Av3AdvancedToggle> toggles, string emptyAnimationPath)
        {
          
            foreach (var toggle in toggles.Where(x => x.ParameterDriversDefault.Any(y => y.State != Av3PresetOption.NoChange) || x.ParameterDriversToggled.Any(y => y.State != Av3PresetOption.NoChange)))
            {
                string layerName = $"A3C: {toggle.Name} - Drivers";
                if (Settings.Overwrite)
                {
                    if (fxController.layers.Any(x => x.name == layerName)) fxController.RemoveLayer(layerName); // just replace this shit
                    fxController.AddLayer(layerName);
                } else
                {
                    layerName = ObjectNames.GetUniqueName(fxController.layers.Select(x => x.name).ToArray(), layerName);
                    fxController.AddLayer(layerName);
                }

                var layers = fxController.layers;
                var layer = layers[fxController.layers.Length - 1];
                layer.defaultWeight = 1f;

                var waitingState = layer.stateMachine.AddState("Wait", new Vector3(250, -20));
                if (!string.IsNullOrEmpty(emptyAnimationPath))
                    waitingState.motion = AssetDatabase.LoadAssetAtPath<Motion>(emptyAnimationPath);

                EditorUtility.SetDirty(waitingState);


                var stateOff = layer.stateMachine.AddState("Default State", new Vector3(250, 30));
                stateOff.writeDefaultValues = Settings.WriteDefaults;
                stateOff.motion = AssetDatabase.LoadAssetAtPath<Motion>(emptyAnimationPath);
                EditorUtility.SetDirty(stateOff);

                if (toggle.ParameterDriversDefault.Any(x => x.State != Av3PresetOption.NoChange))
                {
                    var parameterDriver = stateOff.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    var clonedList = parameterDriver.parameters;
                    foreach (var preset in toggle.ParameterDriversDefault.Where(x => x.State != Av3PresetOption.NoChange))
                        clonedList.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
                        {
                            name = preset.Parameter.name,
                            type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                            value = (preset.State == Av3PresetOption.Toggled ? 1 : 0)
                        });

                    parameterDriver.parameters = clonedList.ToList();
                    EditorUtility.SetDirty(parameterDriver);
                }
                EditorUtility.SetDirty(stateOff);

                var stateOn = layer.stateMachine.AddState("Toggled State", new Vector3(250, 80));
                stateOn.writeDefaultValues = Settings.WriteDefaults;
                stateOn.motion = AssetDatabase.LoadAssetAtPath<Motion>(emptyAnimationPath);
                EditorUtility.SetDirty(stateOn);

                if (toggle.ParameterDriversToggled.Any(x => x.State != Av3PresetOption.NoChange))
                {
                    var parameterDriver = stateOn.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    var clonedList = parameterDriver.parameters;
                    foreach (var preset in toggle.ParameterDriversToggled.Where(x => x.State != Av3PresetOption.NoChange))
                        clonedList.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
                        {
                            name = preset.Parameter.name,
                            type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                            value = (preset.State == Av3PresetOption.Toggled ? 1 : 0)
                        });

                    parameterDriver.parameters = clonedList.ToList();
                    EditorUtility.SetDirty(parameterDriver);
                }
                EditorUtility.SetDirty(stateOn);

                layer.stateMachine.entryPosition = layer.stateMachine.anyStatePosition + new Vector3(0, -10);
                layer.stateMachine.anyStatePosition = layer.stateMachine.entryPosition + new Vector3(0, 40);
                layer.stateMachine.exitPosition = layer.stateMachine.anyStatePosition + new Vector3(0, 40);

                var off = Av3AdvancedSettings.Instance.Settings.SyncInMenu ? AnimatorConditionMode.If : toggle.DefaultValue ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If;
                var on = Av3AdvancedSettings.Instance.Settings.SyncInMenu ? AnimatorConditionMode.IfNot : toggle.DefaultValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;


                var transitionToOn = layer.stateMachine.AddAnyStateTransition(stateOn);
                transitionToOn.AddCondition(off, 0, toggle.Parameter);
                transitionToOn.duration = 0.1f;
                transitionToOn.canTransitionToSelf = false;
                EditorUtility.SetDirty(transitionToOn);

                var transitionToOff = layer.stateMachine.AddAnyStateTransition(stateOff);
                transitionToOff.AddCondition(on, 0, toggle.Parameter);
                transitionToOff.duration = 0.1f;
                transitionToOff.canTransitionToSelf = false;
                EditorUtility.SetDirty(transitionToOff);

                fxController.layers = layers;
            }

         
            EditorUtility.SetDirty(fxController);
        }

        private static void AfterComplete(List<Av3AdvancedToggle> toggles)
        {
       
            if (PoiyomiInteractions.IsPoiyomiPresent() && toggles.Any(x => x.Modules.Any(y => y is Av3AdvancedObjectDissolve)) &&
                EditorUtility.DisplayDialog("Lock Materials?", 
                "You have to lock your poiyomi materials to dissolve work properly, do you want to lock them now?\n", "Yes", "No"))
            {
                var materials = (from toggle in toggles
                                 select toggle.Modules into MyModules
                                 from module in MyModules
                                 where module is Av3AdvancedObjectDissolve
                                 select (module as Av3AdvancedObjectDissolve)?.SelectedMaterials into SelectedMaterials
                                 from Material in SelectedMaterials
                                 select Material).Distinct().ToList();

                foreach (var material in materials) material.Lock();
            }
            
        }

        private static void SetupAnimations(List<Av3AdvancedToggle> toggles,  Av3AdvancedToggleSettings Settings, string animationDirectory)
        {
            void CopyAnim(ref AnimationClip targetAnim, AnimationClip sourceAnim)
            {
                Debug.Log("Copying " + sourceAnim.name);
                AnimationUtility.SetAnimationClipSettings(targetAnim, AnimationUtility.GetAnimationClipSettings(sourceAnim));
                targetAnim.frameRate = sourceAnim.frameRate;

                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(sourceAnim);

                for (int i = 0; i < curveBindings.Length; i++)
                    AnimationUtility.SetEditorCurve(targetAnim, curveBindings[i], AnimationUtility.GetEditorCurve(sourceAnim, curveBindings[i]));
            }

            foreach (var toggleObject in toggles)
            {
                if (toggleObject == null) continue;

                var DefaultAnimation = new AnimationClip();
                var ToggledAnimation = new AnimationClip();

                if (toggleObject.UseBaseAnimation)
                {
                    if (toggleObject.DefaultAnimation != null) CopyAnim(ref DefaultAnimation, toggleObject.DefaultAnimation);
                    if (toggleObject.ToggledAnimation != null) CopyAnim(ref ToggledAnimation, toggleObject.ToggledAnimation);
                }

                if (toggleObject.Modules != null && toggleObject.Modules.Count > 0)
                    foreach (var toggleElement in toggleObject.Modules)
                    {
                        toggleElement.GenerateDefault(ref DefaultAnimation);
                        toggleElement.Generate(ref ToggledAnimation);
                    }
                

                string objectName = Av3Core.RemoveInvalidChars(toggleObject.Name).Trim();
               
                var defaultAnimPath = $"{animationDirectory}{objectName} Default.anim";
                if (!Settings.Overwrite) defaultAnimPath = AssetDatabase.GenerateUniqueAssetPath(defaultAnimPath);

                var toggledAnimPath = $"{animationDirectory}{objectName} Toggled.anim";
                if (!Settings.Overwrite) toggledAnimPath = AssetDatabase.GenerateUniqueAssetPath(toggledAnimPath);

                toggleObject.DefaultAnimationPath = defaultAnimPath;
                toggleObject.ToggledAnimationPath = toggledAnimPath;

                AssetDatabase.CreateAsset(DefaultAnimation, toggleObject.DefaultAnimationPath);
                AssetDatabase.CreateAsset(ToggledAnimation, toggleObject.ToggledAnimationPath);
            }

        }

        private static void SetupFX(AnimatorController fxLayer, List<Av3AdvancedToggle> toggles, Av3AdvancedToggleSettings Settings, string emptyAnimationPath)
        {
            foreach (var toggleObject in toggles)
            {
                string normalizedName = Av3Core.RemoveInvalidChars(toggleObject.Name);
                string objectName = Av3Core.RemoveInvalidChars(toggleObject.Name);

                string parameterName = toggleObject.Parameter;
                bool existParam = fxLayer.parameters.Any(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Bool);
                if (!existParam || Settings.Overwrite)
                {
                    if (existParam) fxLayer.RemoveParameter(fxLayer.parameters.First(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Bool));
                    //fxLayer.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
                    fxLayer.AddParameter(new AnimatorControllerParameter()
                    {
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = toggleObject.DefaultValue,
                        name = parameterName
                    });
                }

                string layerName = "A3C: " + toggleObject.Name.Replace(".", "_");
                if (Settings.Overwrite && fxLayer.layers.Any(x => x.name == layerName))
                    fxLayer.RemoveLayer(layerName);
                else
                    layerName = ObjectNames.GetUniqueName(fxLayer.layers.Select(x => x.name).ToArray(), layerName);
          
                fxLayer.AddLayer(layerName);

                var layers = fxLayer.layers;
                var layer = layers[fxLayer.layers.Length - 1];
                layer.defaultWeight = 1f;

                var emptyAnimation = layer.stateMachine.AddState("Initialize", new Vector3(250, 45));
                emptyAnimation.writeDefaultValues = Settings.WriteDefaults;
                if (!string.IsNullOrEmpty(emptyAnimationPath))
                    emptyAnimation.motion = AssetDatabase.LoadAssetAtPath<Motion>(emptyAnimationPath);
                EditorUtility.SetDirty(emptyAnimation);

                var stateOff = layer.stateMachine.AddState("Default State", new Vector3(500, 20));
                stateOff.writeDefaultValues = Settings.WriteDefaults;
                stateOff.motion = AssetDatabase.LoadAssetAtPath<Motion>(toggleObject.DefaultAnimationPath);
                EditorUtility.SetDirty(stateOff);

                var stateOn = layer.stateMachine.AddState("Toggled State", new Vector3(500, 70));
                stateOn.writeDefaultValues = Settings.WriteDefaults;
                stateOn.motion = AssetDatabase.LoadAssetAtPath<Motion>(toggleObject.ToggledAnimationPath);
                EditorUtility.SetDirty(stateOn);


                var off = Av3AdvancedSettings.Instance.Settings.SyncInMenu ? AnimatorConditionMode.If : toggleObject.DefaultValue ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If;
                var on = Av3AdvancedSettings.Instance.Settings.SyncInMenu ? AnimatorConditionMode.IfNot : toggleObject.DefaultValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;

                AddTransition(toggleObject, emptyAnimation, stateOff, on, true);
                AddTransition(toggleObject, emptyAnimation, stateOn, off, true);
                AddTransition(toggleObject, stateOn, stateOff, on);
                AddTransition(toggleObject, stateOff, stateOn, off);

                // maybe i'm a bit of a perfectionist
                layer.stateMachine.anyStatePosition -= new Vector3(0, 10);
                layer.stateMachine.entryPosition = layer.stateMachine.anyStatePosition + new Vector3(0, 35);
                layer.stateMachine.exitPosition = layer.stateMachine.entryPosition + new Vector3(0, 35);

                fxLayer.layers = layers; // Save? :think:
                
            }

            EditorUtility.SetDirty(fxLayer);
        }

        private static AnimatorStateTransition AddTransition(Av3AdvancedToggle toggleObject, AnimatorState source, AnimatorState target, AnimatorConditionMode condition, bool skipTransition = false)
        {
            var transition = source.AddTransition(target);
         
            transition.AddCondition(condition, 0, toggleObject.Parameter);
            transition.duration = 0;
            transition.name = $"{source.name} -> {target.name} (by Av3Creator)";
            if (toggleObject.IsLocal) transition.AddCondition(AnimatorConditionMode.If, 1, "IsLocal");
            if (skipTransition) transition.offset = 1;
            EditorUtility.SetDirty(transition);
            return transition;
        }

        private static void SetupParameters(VRCExpressionParameters vrcParameters, List<Av3AdvancedToggle> toggles)
        {
            var parameters = vrcParameters.parameters.ToList();

            foreach (var parameterToRemove in toggles)
                if (parameters.Any(x => x.name == parameterToRemove.Parameter && x.valueType == VRCExpressionParameters.ValueType.Bool))
                    parameters.Remove(parameters.Find(x => x.name == parameterToRemove.Parameter && x.valueType == VRCExpressionParameters.ValueType.Bool));

            vrcParameters.parameters = parameters.ToArray();

            foreach (var toggleObject in toggles)
            {

                string parameterName = toggleObject.Parameter;

                var newParameter = new VRCExpressionParameters.Parameter()
                {
                    name = parameterName,
                    valueType = VRCExpressionParameters.ValueType.Bool,
                    defaultValue = !Av3AdvancedSettings.Instance.Settings.SyncInMenu ? 0 : toggleObject.DefaultValue ? 1 : 0,
                    saved = toggleObject.Saved

                };

                if ((vrcParameters.CalcTotalCost() + VRCExpressionParameters.TypeCost(newParameter.valueType)) > VRCExpressionParameters.MAX_PARAMETER_COST) return;

                parameters.Add(newParameter);
                vrcParameters.parameters = parameters.ToArray();
            }

            EditorUtility.SetDirty(vrcParameters);
        }

        private static void SetupMenu(List<Av3AdvancedToggle> toggles, Av3AdvancedToggleSettings Settings, string MenusDirectory, VRCExpressionsMenu mainMenu = null)
        {
            var menu = !Settings.CreateANewMenu && Settings.CustomMenu !=null ? Settings.CustomMenu : ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var menuName = string.IsNullOrEmpty(Settings.MenuName) ? "Toggles" : Settings.MenuName;

            if (Settings.CreateANewMenu || Settings.CustomMenu == null)
            {
                var menuPath = $"{MenusDirectory}{menuName}.asset";
                if(!Settings.Overwrite) menuPath = AssetDatabase.GenerateUniqueAssetPath(menuPath);
                AssetDatabase.CreateAsset(menu, menuPath);
            }

            menu.AddToPaginatorMenu(MenusDirectory + menuName, toggles.Select(x => new VRCExpressionsMenu.Control()
            {
                name = x.Name,
                parameter = new VRCExpressionsMenu.Control.Parameter()
                {
                    name = x.Parameter
                },
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                icon = x.Icon
            }).ToArray(), 1, Settings.Overwrite);

            if (Settings.AddToMainMenu && mainMenu)
            {
                if (mainMenu.controls.Count < 8)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = menuName,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = menu
                    });
                }
                else
                {
                    Debug.LogWarning("Main Menu doesn't have enough space, please add the generated menu manually!");
                    return;
                }
            }

            EditorUtility.SetDirty(mainMenu);
        }
    }
}