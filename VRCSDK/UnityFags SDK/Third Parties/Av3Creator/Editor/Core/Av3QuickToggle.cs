#region
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using System.IO;
using Av3Creator.Utils;
#endregion

namespace Av3Creator.Core
{
    public class Av3QuickToggle
    {
        #region variables
        private string avatarName;

        private VRCAvatarDescriptor vrcAvatarDescriptor;
        private List<QuickToggleData> toggleObjects;

        private AnimatorController fxLayer;
        private VRCExpressionParameters vrcParameters;
        private VRCExpressionsMenu vrcMainMenu;

        private Av3Settings Av3Settings;
        private QuickToggleSettings Settings;
        #endregion

        public Av3QuickToggle(VRCAvatarDescriptor avatar, QuickToggleData[] data, Av3Settings settings)
        {
            vrcAvatarDescriptor = avatar;
            toggleObjects = data.ToList();

            fxLayer = (vrcAvatarDescriptor.baseAnimationLayers[4].animatorController is AnimatorController layer) ? layer : null;
            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcMainMenu = vrcAvatarDescriptor.expressionsMenu;

            Av3Settings = settings;
            Settings = settings.QTSettings;
        }

        public static bool CanCreateParams(VRCExpressionParameters vrcParams, int togglesToAdd, out int sizeNeeded)
        {
            sizeNeeded = 0;
            if (vrcParams == null) return false;

            var totalSize = vrcParams.parameters.Aggregate(togglesToAdd * VRCExpressionParameters.TypeCost(VRCExpressionParameters.ValueType.Bool),
                                                   (total, param) => total + VRCExpressionParameters.TypeCost(param.valueType));

            if (totalSize > VRCExpressionParameters.MAX_PARAMETER_COST)
                sizeNeeded = (VRCExpressionParameters.MAX_PARAMETER_COST - totalSize) * -1;

            return sizeNeeded <= 0;
        }

        private string MenusDirectory;
        private string AnimationsDirectory;
        public void SetupThings()
        {
            // Check if any toggle is invalid
            if (toggleObjects.Any(obj => string.IsNullOrWhiteSpace(obj.Name)))
            {
                EditorUtility.DisplayDialog("Av3Creator: ERROR", "Toggle names cannot be null, please make sure that you fill all the textfields.", "Close");
                return;
            }

            if (!CanCreateParams(vrcParameters, toggleObjects.Count, out int sizeNeeded))
                EditorUtility.DisplayDialog("Av3Creator: ERROR",
                    "Avatar doesn't have space to create this toggles, please consider to delete some unused parameters.\n" +
                    "You need more " + sizeNeeded + " byte(s).", "Close");


            // Get avatar name without invalid chars
            avatarName = Av3Core.RemoveInvalidChars(vrcAvatarDescriptor.transform.root.name);

            if (string.IsNullOrEmpty(Av3Settings.OutputDirectory)) throw new System.Exception("Avatar Directory can not be null");

            MenusDirectory = Av3Settings.OutputDirectory + "/Menus/";
            AnimationsDirectory = Av3Settings.OutputDirectory + "/Animations/";

            // CREATE DIRECTORIES
            Directory.CreateDirectory(MenusDirectory);
            Directory.CreateDirectory(AnimationsDirectory);

            // SETUP PARAMETERS
            foreach (var toggleObj in toggleObjects)
            {
                var validName = Av3Core.RemoveInvalidChars(toggleObj.Name);
                string parameterName = $"A3C/Toggle/{validName}";

                if(!Settings.Overwrite)
                    parameterName = ObjectNames.GetUniqueName(vrcParameters.parameters.Select(x => x.name).ToArray(), parameterName);

                toggleObj.Parameter = parameterName;
            }

            SetupAnimations();
            SetupFX();
            SetupParameters();
            SetupMenu();

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            if(Settings.PingFolder) EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(Av3Settings.OutputDirectory));
        }

        private void SetupAnimations()
        {
            foreach (var toggleObject in toggleObjects)
            {
                var OFF = new AnimationClip() { legacy = false, wrapMode = WrapMode.Once };
                var ON = new AnimationClip() { legacy = false, wrapMode = WrapMode.Once };


                foreach (var toggleElement in toggleObject.Elements)
                {
                    // Check if the current element is valid
                    if (!toggleElement.Object || !toggleElement.Object.transform.IsChildOf(vrcAvatarDescriptor.transform)) continue;

                    var path = AnimationUtility.CalculateTransformPath(toggleElement.Object.transform, vrcAvatarDescriptor.transform);

                    OFF.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, toggleElement.Enabled ? 1f : 0f, 0, 0)));
                    ON.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, toggleElement.Enabled ? 0f : 1f, 0, 0)));
                }

                string objectName = Av3Core.RemoveInvalidChars(toggleObject.Name).Trim();

                toggleObject.DefaultAnimPath = $"{AnimationsDirectory}{objectName} DEFAULT.anim";
                toggleObject.ToggledAnimPath = $"{AnimationsDirectory}{objectName} TOGGLED.anim";

                if (!Settings.Overwrite)
                {
                    toggleObject.DefaultAnimPath = AssetDatabase.GenerateUniqueAssetPath(toggleObject.DefaultAnimPath);
                    toggleObject.DefaultAnimPath = AssetDatabase.GenerateUniqueAssetPath(toggleObject.ToggledAnimPath);
                }

                AssetDatabase.CreateAsset(OFF, toggleObject.DefaultAnimPath);
                AssetDatabase.CreateAsset(ON, toggleObject.ToggledAnimPath);
            }
        }
        private void SetupFX()
        {
            foreach (var toggleObject in toggleObjects)
            {
                string normalizedName = Av3Core.RemoveInvalidChars(toggleObject.Name);
                string objectName = Av3Core.RemoveInvalidChars(toggleObject.Name);

                string parameterName = toggleObject.Parameter;
                bool existParam = fxLayer.parameters.Any(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Bool);
                if (!existParam || Settings.Overwrite)
                {
                    if (existParam) fxLayer.RemoveParameter(fxLayer.parameters.First(x => x.name == parameterName && x.type == AnimatorControllerParameterType.Bool));
                    fxLayer.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
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

                var stateOff = layer.stateMachine.AddState("Default State", new Vector3(250, 20));
                stateOff.writeDefaultValues = Settings.WriteDefaults;
                stateOff.motion = (Motion)AssetDatabase.LoadAssetAtPath(toggleObject.DefaultAnimPath, typeof(Motion));
                EditorUtility.SetDirty(stateOff);

                var stateOn = layer.stateMachine.AddState("Toggled State", new Vector3(250, 70));
                stateOn.writeDefaultValues = Settings.WriteDefaults;
                stateOn.motion = (Motion)AssetDatabase.LoadAssetAtPath(toggleObject.ToggledAnimPath, typeof(Motion));
                EditorUtility.SetDirty(stateOn);

                // maybe i'm a bit of a perfectionist
                layer.stateMachine.entryPosition = layer.stateMachine.anyStatePosition + new Vector3(0, -10);
                layer.stateMachine.anyStatePosition = layer.stateMachine.entryPosition + new Vector3(0, 40);
                layer.stateMachine.exitPosition = layer.stateMachine.anyStatePosition + new Vector3(0, 40);

                var anyStateOn = layer.stateMachine.AddAnyStateTransition(stateOn);
                anyStateOn.AddCondition(AnimatorConditionMode.If, 0f, parameterName);
                anyStateOn.duration = 0f;

                var anyStateOff = layer.stateMachine.AddAnyStateTransition(stateOff);
                anyStateOff.AddCondition(AnimatorConditionMode.IfNot, 0f, parameterName);
                anyStateOff.duration = 0f;

                fxLayer.layers = layers;     
            }

            if (!Settings.DisableCredits) Av3Utils.GenerateCredits(fxLayer);
          

            EditorUtility.SetDirty(fxLayer);
        }
        private void SetupParameters()
        {
            var parameters = vrcParameters.parameters.ToList();

            foreach (var parameterToRemove in toggleObjects)
                if (parameters.Any(x => x.name == parameterToRemove.Parameter && x.valueType == VRCExpressionParameters.ValueType.Bool))
                    parameters.Remove(parameters.Find(x => x.name == parameterToRemove.Parameter && x.valueType == VRCExpressionParameters.ValueType.Bool));

            vrcParameters.parameters = parameters.ToArray();

            foreach (var toggleObject in toggleObjects)
            {
                string parameterName = toggleObject.Parameter;

                var newParameter = new VRCExpressionParameters.Parameter()
                {
                    name = parameterName,
                    valueType = VRCExpressionParameters.ValueType.Bool,
                    defaultValue = 0
                };

                if ((vrcParameters.CalcTotalCost() + VRCExpressionParameters.TypeCost(newParameter.valueType)) > VRCExpressionParameters.MAX_PARAMETER_COST) return;

                parameters.Add(newParameter);
                vrcParameters.parameters = parameters.ToArray();
            }

            EditorUtility.SetDirty(vrcParameters);
        }

        private void SetupMenu()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();

            if (string.IsNullOrEmpty(Settings.MenuName) || !Settings.CustomMenuName) Settings.MenuName = "Toggles";
            var menuPath = $"{MenusDirectory}{Settings.MenuName}.asset";
            menuPath = Settings.Overwrite ? menuPath : AssetDatabase.GenerateUniqueAssetPath(menuPath);
            AssetDatabase.CreateAsset(menu, menuPath);

            menu.AddToPaginatorMenu(MenusDirectory + Settings.MenuName, toggleObjects.Select(x => new VRCExpressionsMenu.Control()
            {
                name = x.Name,
                parameter = new VRCExpressionsMenu.Control.Parameter()
                {
                    name = x.Parameter
                },
                type = VRCExpressionsMenu.Control.ControlType.Toggle
            }).ToArray(), 1, Settings.Overwrite);

            if (Settings.AddToMainMenu && vrcMainMenu != null)
            {
                if (vrcMainMenu.controls.Count < 8)
                {
                    vrcMainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = Settings.MenuName,
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

            EditorUtility.SetDirty(vrcMainMenu);
        }
    }
}