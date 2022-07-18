#region
using Av3Creator.AdvancedToggles;
using Av3Creator.AdvancedToggles.Modules;
using Av3Creator.Core;
using Av3Creator.Utils;
using Av3Creator.Utils.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endregion

namespace Av3Creator.Windows
{
    [Serializable]
    public class Av3AdvancedToggleWindow : EditorWindow
    {
        private string ProjectPath;

        [SerializeField] [SerializeReference] public static Av3AdvancedToggleWindow Instance;

        [SerializeReference] private AnimatorController fxLayer;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMainMenu;

        [MenuItem("UnityFags SDK/Avatar Tools/Av3Creator/Advanced Toggle Creator", priority = 2)]
        public static Av3AdvancedToggleWindow ShowAdvancedToggleCreator()
        {
            if (!Instance)
            {
                Instance = GetWindow<Av3AdvancedToggleWindow>("Advanced Toggle Creator");
                Instance.autoRepaintOnSceneChange = true;
                Instance.minSize = new Vector2(500, 600);
            }
            Instance.Show();
            return Instance;
        }

        public void OnEnable()
        {
            if (EditorApplication.isPlaying) return;
            ProjectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            var data = EditorPrefs.GetString("Av3AdvancedToggle", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            LoadAvatar();
        }

        //[SerializeReference] public VRCAvatarDescriptor vrcAvatarDescriptor;


        public void OnDisable()
        {
            if (EditorApplication.isPlaying) return;
            EditorPrefs.SetString("Av3AdvancedToggle", JsonUtility.ToJson(this, false)); // Save window properties
            Av3AdvancedSettings.Save();
        }

        public readonly int DEFAULT_SPACEMENT = 3;

        private bool missingDirectoryError;

        private GameObject lastLoadedAvatar;
        private bool isAvatarLoaded = false;
        private bool LoadAvatar()
        {
            if (EditorApplication.isPlaying) return false;

            var vrcAvatarDescriptor = Av3AdvancedSettings.Instance.VRCAvatarDescriptor;
            if (vrcAvatarDescriptor == null)
            {
                ResetConfigs();
                return false;
            }

            if (lastLoadedAvatar == null) lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
            else if (lastLoadedAvatar != vrcAvatarDescriptor.gameObject)
            {
                lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
                ResetConfigs();
            }

            CheckAvatar();
            var toggles = Av3AdvancedSettings.Instance.AdvancedToggles;
            if (toggles != null && toggles.Count > 0)
                foreach (var toggle in toggles)
                    foreach (var module in toggle.Modules)
                        module.OnImport();
            return isAvatarLoaded;
        }

        private void CheckAvatar()
        {
            var vrcAvatarDescriptor = Av3AdvancedSettings.Instance.VRCAvatarDescriptor;
            if (vrcAvatarDescriptor == null) return;

            if (vrcAvatarDescriptor.customizeAnimationLayers && vrcAvatarDescriptor.baseAnimationLayers[Av3Layers.FX].animatorController is AnimatorController layer) fxLayer = layer;
            else fxLayer = null;

            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcMainMenu = vrcAvatarDescriptor.expressionsMenu;
            isAvatarLoaded = (fxLayer && vrcParameters && vrcMainMenu);
        }

        private void ResetConfigs()
        {
            fxLayer = null;
            vrcParameters = null;
            vrcMainMenu = null;
            isAvatarLoaded = false;
        }

        Vector2 scrollPos;

        public void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                Av3StyleManager.DrawError("Please leave play mode to create toggles");
                return;
            }


            CheckAvatar();
            var vrcAvatarDescriptor = Av3AdvancedSettings.Instance.VRCAvatarDescriptor;
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, Av3StyleManager.Styles.ContentView))
            {
                scrollPos = scroll.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(Av3StyleManager.Styles.Header))
                    EditorGUILayout.LabelField($"<b><size=15>Av3Creator {Av3CreatorWindow.VERSION} - Advanced Toggle</size></b>\n" +
                                               $"<size=11>rafacasari.gumroad.com • rafa.booth.pm</size>",
                                                Av3StyleManager.Styles.CenteredText, GUILayout.MinHeight(40f));
                EditorGUILayout.Space(DEFAULT_SPACEMENT);

                if (vrcAvatarDescriptor == null)
                    Av3StyleManager.DrawError("Please, select your avatar or use the auto-fill.");
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var checkChanges = new EditorGUI.ChangeCheckScope())
                    {
                        Av3AdvancedSettings.Instance.VRCAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(Av3AdvancedSettings.Instance.VRCAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));
                        if (checkChanges.changed)
                            LoadAvatar();
                    }


                    if (GUILayout.Button("Auto-Fill", GUILayout.Height(24)))
                    {
                        Av3AdvancedSettings.Instance.VRCAvatarDescriptor = Av3Utils.SelectCurrentAvatar();
                        if (Av3AdvancedSettings.Instance.VRCAvatarDescriptor != null) LoadAvatar();
                    }
                }

                EditorGUILayout.Space(DEFAULT_SPACEMENT);

                if (missingDirectoryError)
                    Av3StyleManager.DrawError("ERROR: Select a directory!");


                var Settings = Av3AdvancedSettings.Instance.Settings;
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    if (missingDirectoryError)
                    {
                        int padding = 2;
                        var highlightRect = new Rect(horizontalScope.rect.x - padding, horizontalScope.rect.y - padding, horizontalScope.rect.width + (padding * 2), horizontalScope.rect.height + padding * 2);
                        EditorGUI.DrawRect(highlightRect, new Color(1, 0, 0, 0.2f));
                    }

                    Av3StyleManager.DrawLabel("Output Directory ", "Choose a folder where the assets will be saved in.\nThis create simple 3.0 folders to better organization.", 10);

                    int avatarDirectoryHeight = 22;
                    using (new EditorGUI.DisabledScope(true)) EditorGUILayout.TextField(Settings.OutputDirectory, GUILayout.Height(avatarDirectoryHeight));

                    if (GUILayout.Button(Av3StyleManager.Icons.OpenFolder, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70), GUILayout.Height(avatarDirectoryHeight)))
                    {
                        var path = EditorUtility.OpenFolderPanel("Select Output Directory", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(path) && path.StartsWith(ProjectPath))
                        {
                            path = path.Substring(ProjectPath.Length + 1);
                            Settings.OutputDirectory = path;
                            missingDirectoryError = false;
                        }
                    }
                }

                if (vrcAvatarDescriptor != null && !isAvatarLoaded)
                {
                    EditorGUILayout.Space(DEFAULT_SPACEMENT);

                    if (!fxLayer) Av3StyleManager.DrawIssueBox(MessageType.Warning, "Missing FX Controller", () =>
                    {
                        if (string.IsNullOrEmpty(Settings.OutputDirectory)) missingDirectoryError = true;

                        vrcAvatarDescriptor.CreateFXLayer(Settings.OutputDirectory);
                    });

                    if (!vrcParameters) Av3StyleManager.DrawIssueBox(MessageType.Warning, "Missing Parameters", () =>
                    {
                        if (string.IsNullOrEmpty(Settings.OutputDirectory)) missingDirectoryError = true;

                        vrcAvatarDescriptor.CreateParameters(Settings.OutputDirectory);
                    });

                    if (!vrcMainMenu) Av3StyleManager.DrawIssueBox(MessageType.Warning, "Missing Main Menu", () =>
                    {
                        if (string.IsNullOrEmpty(Settings.OutputDirectory)) missingDirectoryError = true;

                        vrcAvatarDescriptor.CreateMainMenu(Settings.OutputDirectory);
                    });
                }

                using (new EditorGUI.DisabledScope(vrcAvatarDescriptor == null || !isAvatarLoaded))
                {
                    Settings.SettingsExpanded = EditorGUILayout.Foldout(Settings.SettingsExpanded, "Settings", true, Av3StyleManager.Styles.BoldFoldout);
                    if (Settings.SettingsExpanded)
                    {
                        using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.AdvancedToggle_Settings))
                        {
                            Settings.WriteDefaults = EditorGUILayout.ToggleLeft(new GUIContent("Write Defaults", "Only check this if your avatar use Write Defaults, if you don't know what is this, just keep this unchecked."),
                                Settings.WriteDefaults);
                            Settings.Overwrite = EditorGUILayout.ToggleLeft(new GUIContent("Overwrite", "If checked, will replace any layers, animations, menus or conditions that use same names. Useful for updates."),
                               Settings.Overwrite);


                            using (new EditorGUILayout.HorizontalScope())
                            {
                                Settings.CreateANewMenu = Av3StyleManager.ToggleLeft(Settings.CreateANewMenu, "Create New Menu", "If checked, the script will create a new menu with the given name. If not, you can select the target menu.", 10);


                                if (!Settings.CreateANewMenu) Settings.CustomMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(Settings.CustomMenu, typeof(VRCExpressionsMenu), false);
                                else
                                {
                                    GUILayout.Space(20);
                                    Av3StyleManager.DrawLabel("Name:");
                                    Settings.MenuName = EditorGUILayout.TextField(Settings.MenuName);
                                }
                            }

                            Settings.AddToMainMenu = EditorGUILayout.ToggleLeft(new GUIContent("Add Menu to Main Menu", "If checked, will automatically add the generated menu in your Main Menu (if have space avaiable).\nIf not, will just create the menu and you should manually put the menu wherever you want."),
                                Settings.AddToMainMenu);

                            Settings.DisableCredits = EditorGUILayout.ToggleLeft(new GUIContent("Disable Credits", "If checked, will not create additional layers for credits in FX Layers.\n\nChecking this will not have any difference in avatar aparence, just the layer. Not break anything, just my heart. :c"),
                                Settings.DisableCredits);

                            Settings.SyncInMenu = EditorGUILayout.ToggleLeft(new GUIContent("Sync in Menu (not recommended to disable)", "If checked, your default state will be applied in avatar menus too, so theoretically menus and toggles would be \"synced\". (e.g. Toggle OFF = Object OFF/ Toggle ON = Object ON)\n" +
                                "That way it would be easier to know what is on and off in-game without having to look in the mirror or camera."),
                               Settings.SyncInMenu);

                            Settings.PingFolder = EditorGUILayout.ToggleLeft(new GUIContent("Ping Folder", "Ping the selected folder after generating toggles"),
                             Settings.PingFolder);

                        } 
                    }

                    EditorGUILayout.Space(5);

                    try
                    {
                        DrawList();
                    } catch(Exception error)
                    {
                        Debug.LogException(error);
                    }

                    var AdvancedToggles = Av3AdvancedSettings.Instance.AdvancedToggles;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add", EditorStyles.miniButtonLeft))
                        {
                            Av3AdvancedSettings.Instance.AdvancedToggles.Add(new Av3AdvancedToggle());
                            Av3AdvancedSettings.Save();
                        }

                        using (new EditorGUI.DisabledGroupScope(AdvancedToggles == null || AdvancedToggles.Count == 0))
                        {
                            using (new EditorGUI.DisabledGroupScope(!(AdvancedToggles != null && AdvancedToggles.Any(x => x.IsSelected))))
                            {
                                if (GUILayout.Button("Clear Selected", EditorStyles.miniButtonMid)
                                    && EditorUtility.DisplayDialog(
                                        "Clear Confirmation",
                                "Do you really want to clear all the selected toggles?",
                                "Yes, clear selected toggles", "Cancel"))
                                {
                                    Av3AdvancedSettings.Instance.AdvancedToggles = AdvancedToggles.Where(x => !x.IsSelected).ToList();
                                    Av3AdvancedSettings.Save();
                                }
                            }

                            if (GUILayout.Button("Clear All", EditorStyles.miniButtonRight)
                                && EditorUtility.DisplayDialog(
                                    "Clear Confirmation",
                                "Do you really want to CLEAR ALL toggles?",
                                "Clear Toggles", "Cancel"))
                            {
                                Av3AdvancedSettings.Instance.AdvancedToggles.Clear();
                                Av3AdvancedSettings.Save();
                            }
                        }
                    }


                    var togglesToGenerate = AdvancedToggles?.Where(x => x.Modules.Count > 0 || (x.UseBaseAnimation && (x.DefaultAnimation != null || x.ToggledAnimation != null))).ToList();
                    var disableButton = Av3AdvancedToggleCore.InWork || vrcAvatarDescriptor == null || string.IsNullOrEmpty(Settings.OutputDirectory) || AdvancedToggles == null || togglesToGenerate.Count == 0 || hasLockedMaterials;
                    using (new EditorGUI.DisabledScope(disableButton))
                        if (GUILayout.Button("Create Toggles", GUILayout.Height(36))
                                && EditorUtility.DisplayDialog("Are you ready?", "Do you really want to create your toggles?",
                                "Yes, create my toggles", "No, I'm not ready yet"))
                        {
                            Av3AdvancedToggleCore.SetupThings(vrcAvatarDescriptor, AdvancedToggles, Settings);
                        }

                    if(string.IsNullOrEmpty(Settings.OutputDirectory)) Av3StyleManager.DrawError("ERROR: Please select a Output Folder!");
                    if (togglesToGenerate.Count == 0) Av3StyleManager.DrawError("ERROR: You don't have any toggle with a module or a base animation");
                    CalculeAndDrawPoiyomiStuff();

                    if (!PoiyomiInteractions.IsPoiyomiPresent())
                        EditorGUILayout.HelpBox("Poiyomi V7 integration failed, dissolve toggles may not work properly!\nPlease download Poiyomi V7 and import it in your project!", MessageType.Warning);
                    

                    GUILayout.FlexibleSpace();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Save Toggles", EditorStyles.miniButtonLeft))
                        {
                            var myAsset = CreateInstance<Av3TogglesScriptableObject>();
                            myAsset.ToggleList = Av3AdvancedSettings.Instance.AdvancedToggles.ToList();
                            EditorUtility.SetDirty(myAsset);

                            var path = EditorUtility.SaveFilePanel("Save preferences as", Application.dataPath, "", "a3c.asset");
                            if (!string.IsNullOrEmpty(path) && path.StartsWith(ProjectPath))
                            {
                                path = path.Substring(ProjectPath.Length + 1);
                                AssetDatabase.CreateAsset(myAsset, path);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }     
                        }
                        if (GUILayout.Button("Load Toggles", EditorStyles.miniButtonRight))
                        {
                            var path = EditorUtility.OpenFilePanel("Open Toggle List", Application.dataPath, "a3c.asset");
                            if (!string.IsNullOrEmpty(path) && path.StartsWith(ProjectPath))
                            {
                                path = path.Substring(ProjectPath.Length + 1);
                                var myAsset = AssetDatabase.LoadAssetAtPath<Av3TogglesScriptableObject>(path);
                                Av3AdvancedSettings.Instance.AdvancedToggles = myAsset.ToggleList.ToList();
                                Av3AdvancedSettings.Save();

                                foreach (var x in Av3AdvancedSettings.Instance.AdvancedToggles)
                                    foreach (var y in x.Modules)
                                        y.OnImport();
                            }
                        }
                    }
                }
            }
        }

        private bool hasLockedMaterials = false;
        private void CalculeAndDrawPoiyomiStuff()
        {
            try
            {
                if(PoiyomiInteractions.IsPoiyomiPresent())
                {
                    var lockedMaterials = (from toggle in Av3AdvancedSettings.Instance.AdvancedToggles
                                     select toggle.Modules into MyModules
                                     from module in MyModules
                                     where module is Av3AdvancedObjectDissolve
                                     select (module as Av3AdvancedObjectDissolve)?.SelectedMaterials into SelectedMaterials
                                     from Material in SelectedMaterials
                                     where Material.IsMaterialLocked()
                                     select Material).Distinct().ToList();

                    if (lockedMaterials != null && lockedMaterials.Count > 0)
                    {
                        hasLockedMaterials = true;
                        Av3StyleManager.DrawIssueBox(MessageType.Error, "<b>[Poiyomi Dissolve]</b> You have some <b>materials</b> that are <b>currently locked</b>, you have to unlock them before create your toggles.", () =>
                        {
                            foreach(var material in lockedMaterials) material.Unlock();
                            hasLockedMaterials = false;
                        });
                    }
                    else hasLockedMaterials = false;
                }

                var opaqueMaterials = (from toggle in Av3AdvancedSettings.Instance.AdvancedToggles
                                       where toggle.Modules != null
                                       let modules = toggle.Modules
                                       from currentModule in modules
                                       where modules.Any(x => x is Av3AdvancedObjectDissolve)
                                       where (currentModule as Av3AdvancedObjectDissolve)?.SelectedMaterials != null
                                       let dissolveMaterials = (currentModule as Av3AdvancedObjectDissolve).SelectedMaterials
                                       from dissolveMaterial in dissolveMaterials
                                       where dissolveMaterial.shader.name.StartsWith(".poiyomi/") && dissolveMaterial.GetInt("_Mode") == 0
                                       select dissolveMaterial).Distinct().ToList();

                if (opaqueMaterials != null && opaqueMaterials.Count > 0)
                {
                    Av3StyleManager.DrawIssueBox(MessageType.Info, $"<b>[Poiyomi Dissolve]</b> You have some <b>opaque materials</b> that don't work with transparent dissolves. " +
                        $"Please change the RenderType of this Materials or slap the auto-fix button.", 
                        () =>
                        {
                            foreach (Material material in opaqueMaterials)
                            {
                                material.SetOverrideTag("RenderType", "TransparentCutout");
                                material.SetInt("_Mode", 1);
                                material.SetInt("_BlendOp", 0);
                                material.SetFloat("_Cutoff", 0.5f);
                                material.SetInt("_SrcBlend", 1);
                                material.SetInt("_BlendOpAlpha", 0);
                                material.SetInt("_DstBlend", 0);
                                material.SetInt("_AlphaToMask", 1);
                                material.SetInt("_ZWrite", 1);
                                material.SetInt("_ZTest", 4);
                                material.SetInt("_AlphaPremultiply", 0);
                                material.SetInt("_ForceOpaque", 0);

                                material.renderQueue = 2450;
                                EditorUtility.SetDirty(material);
                            }
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        });
                }
            }
            catch
            {

            }
        }

        
        public void DrawList()
        {
            foreach (var (advancedToggle, index) in Av3AdvancedSettings.Instance.AdvancedToggles.ToList().Select((option, index) => (option, index)))
            {
                var foldoutRect = Av3StyleManager.FoldoutWithToggle(ref advancedToggle.IsExpanded, ref advancedToggle.IsSelected, (string.IsNullOrEmpty(advancedToggle.Name) ? $"Toggle ({index})" : "Toggle: " + advancedToggle.Name),
                    () => {
                        Av3AdvancedSettings.Instance.AdvancedToggles.Remove(advancedToggle);
                        Av3AdvancedSettings.Save();
                    });

                if (advancedToggle.IsExpanded)
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.BoxNormal))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
                            {
                                Av3StyleManager.DrawLabel("Toggle Name", "Toggle name will be displayed in Menu and FX Layers");
                                Av3StyleManager.DrawLabel("Parameter", "Now you can change the generated parameter name!");

                                if (advancedToggle.UseBaseAnimation)
                                {
                                    Av3StyleManager.DrawLabel("");
                                    Av3StyleManager.DrawLabel("Base Default", "");
                                    Av3StyleManager.DrawLabel("Base Toggled", "");
                                }
                            }

                            using (new EditorGUILayout.VerticalScope())
                            {
                                EditorGUI.BeginChangeCheck();
                                advancedToggle.Name = EditorGUILayout.TextField(advancedToggle.Name);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    advancedToggle.GenerateParameterName();
                                }


                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    advancedToggle.Parameter = EditorGUILayout.TextField(advancedToggle.Parameter);
                                    advancedToggle.IsLocal = Av3StyleManager.ToggleLeft(advancedToggle.IsLocal, "Is Local?", "Local will only appear to you, so only you will can see the toggle on/off.");
                                }

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    advancedToggle.Saved = Av3StyleManager.ToggleLeft(advancedToggle.Saved, "Saved", "Save state when changes avatar/world");
                                    advancedToggle.DefaultValue = Av3StyleManager.ToggleLeft(advancedToggle.DefaultValue, "Default State", "The default state of the toggle");
                                    advancedToggle.UseBaseAnimation = Av3StyleManager.ToggleLeft(advancedToggle.UseBaseAnimation, "Custom Base Anim", "Optional field to use a already made AnimationClip as base of toggle. If checked, all the modules will be added into this animation and saved in the output folder.");
                                }

                                if (advancedToggle.UseBaseAnimation)
                                {
                                    advancedToggle.ToggledAnimation = (AnimationClip)EditorGUILayout.ObjectField(advancedToggle.ToggledAnimation, typeof(AnimationClip), false);
                                    advancedToggle.DefaultAnimation = (AnimationClip)EditorGUILayout.ObjectField(advancedToggle.DefaultAnimation, typeof(AnimationClip), false);
                                }
                            }

                            using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.RemovePaddingAndMargin))
                            {
                                advancedToggle.UseTexture = Av3StyleManager.ToggleLeft(advancedToggle.UseTexture, "Icon", "Icon for easily find your toggle in menu");

                                using (new EditorGUI.DisabledGroupScope(!advancedToggle.UseTexture))
                                    advancedToggle.Texture = (Texture2D)EditorGUILayout.ObjectField(advancedToggle.Texture, typeof(Texture2D), true, GUILayout.Height(45), GUILayout.MaxWidth(45));
                            }
                        }

                        if (advancedToggle.UseBaseAnimation)
                            GUILayout.Space(5);

                        Av3StyleManager.DrawFoldout("Modules", ref advancedToggle.ModulesExpanded, () =>
                        {
                            DrawProperty(ref advancedToggle.toggleObjectsExpanded, advancedToggle, "Toggle Objects", typeof(Av3AdvancedToggleObject));
                            DrawProperty(ref advancedToggle.blendshapesExpanded, advancedToggle, "Blendshapes", typeof(Av3AdvancedBlendshape));
                            DrawProperty(ref advancedToggle.materialSwapExpanded, advancedToggle, "Material Swaps", typeof(Av3AdvancedMaterialSwap));
                            DrawProperty(ref advancedToggle.objectDissolveExpanded, advancedToggle, "Poiyomi Integration: Dissolve", typeof(Av3AdvancedObjectDissolve));


                            using (new EditorGUILayout.HorizontalScope(Av3StyleManager.Styles.Padding5))
                            {
                                Av3StyleManager.DrawLabel("Add Module");

                                advancedToggle.SelectedModule = (Av3ToggleModuleTypes) EditorGUILayout.EnumPopup(advancedToggle.SelectedModule);
                                if (GUILayout.Button("Add"))
                                {
                                    AddModule(advancedToggle);
                                }

                            }
                        });

                        Av3StyleManager.DrawFoldout("Parameter Drivers", ref advancedToggle.ParameterDriverIsExpanded, () =>
                        {
                            advancedToggle.SelectedParameterType = GUILayout.Toolbar(advancedToggle.SelectedParameterType, new string[] { "On Default (when OFF)", "On Toggled (when ON)" });
                            if (advancedToggle.SelectedParameterType == 0)
                                DrawParameterDriver(advancedToggle, ref advancedToggle.ParameterDriversDefault);
                            else if(advancedToggle.SelectedParameterType == 1)
                                DrawParameterDriver(advancedToggle, ref advancedToggle.ParameterDriversToggled);

                        });
                    }
                    EditorGUILayout.Space(5);
                };
            }
        }

        private void DrawParameterDriver(Av3AdvancedToggle advancedToggle, ref List<Av3Preset> presetList)
        {
            var newList = presetList;
            if (advancedToggle != null && newList != null && newList.Count > 0)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.LabelField("No Change", GUILayout.Width(80));
                        EditorGUILayout.LabelField(" Default", GUILayout.Width(70));
                        EditorGUILayout.LabelField("Toggled", GUILayout.Width(70));
                    }

                    EditorGUILayout.Space(5);

                    foreach (var preset in newList)
                    {
                        using (new EditorGUILayout.HorizontalScope(new GUIStyle(GUI.skin.box)
                        {
                            padding = new RectOffset(20, 0, 1, 1),
                            margin = new RectOffset(0, 0, 0, 0)
                        }))
                        {
                            string parameterName = preset.Parameter.name;
                            if (vrcParameters == null || !vrcParameters.parameters.Any(x => x == preset.Parameter))
                            {
                                EditorGUILayout.LabelField(new GUIContent(parameterName, "Advanced Toggle (non-generated yet)"), Av3StyleManager.Styles.YellowLabel);
                            }
                            else EditorGUILayout.LabelField(parameterName);

                            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(80)))
                            {
                                EditorGUILayout.Space();
                                if (EditorGUILayout.Toggle(preset.State == Av3PresetOption.NoChange, GUILayout.ExpandWidth(false)))
                                    preset.State = Av3PresetOption.NoChange;
                                EditorGUILayout.Space();
                            }

                            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(70)))
                            {
                                EditorGUILayout.Space();
                                if (EditorGUILayout.Toggle(preset.State == Av3PresetOption.Default, GUILayout.ExpandWidth(false)))
                                    preset.State = Av3PresetOption.Default;
                                EditorGUILayout.Space();
                            }

                            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(70)))
                            {
                                EditorGUILayout.Space();
                                if (EditorGUILayout.Toggle(preset.State == Av3PresetOption.Toggled, GUILayout.ExpandWidth(false)))
                                    preset.State = Av3PresetOption.Toggled;
                                EditorGUILayout.Space();
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("Refresh Parameters"))
            {
                var existentParameters = vrcParameters?.parameters.Where(x => x.name != advancedToggle.Parameter).ToList();
                var newParameters = Av3AdvancedSettings.Instance.AdvancedToggles
                .Where(x => x != advancedToggle)
                .Select(x => new VRCExpressionParameters.Parameter()
                {
                    name = x.Parameter,
                    defaultValue = x.DefaultValue ? 1 : 0,
                    valueType = VRCExpressionParameters.ValueType.Bool
                }).ToList();

                var allParameters = new List<VRCExpressionParameters.Parameter>((existentParameters != null ? existentParameters.Count : 0) + newParameters.Count);
                if (existentParameters != null) allParameters.AddRange(existentParameters);
                allParameters.AddRange(newParameters);
                newList = allParameters.GroupBy(x => x.name).Select(x => x.Last()).Select(x => new Av3Preset()
                {
                    Parameter = x,
                    State = Av3PresetOption.NoChange
                }).ToList();
            }

            presetList = newList;
        }

        private static void DrawProperty(ref bool isExpanded, Av3AdvancedToggle advancedToggle, string propertyName, Type propertyType)
        {
            if (!advancedToggle.Modules.Any(x => x.GetType() == propertyType)) return;
            using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.ModuleBox, GUILayout.ExpandWidth(true)))
            {
                Av3StyleManager.DrawFoldout(propertyName, ref isExpanded, () =>
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle()
                    {
                        padding = new RectOffset(5, 5, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    }))
                    {
                        foreach (var currentToggle in advancedToggle.Modules.Where(x => x.GetType() == propertyType).ToList())
                        {
                            currentToggle.DrawGUI(advancedToggle);
                        }
                    }
                }, toggleLabelOnClick: true);
            }
        }

        private void AddModule(Av3AdvancedToggle advancedToggle)
        {
            if (advancedToggle == null) return;
            if (advancedToggle.Modules == null) advancedToggle.Modules = new List<IAv3AdvancedModule>();
            advancedToggle.ModulesExpanded = true;

            switch (advancedToggle.SelectedModule)
            {
                case Av3ToggleModuleTypes.SimpleToggle:
                    {
                        AddModuleToToggle(advancedToggle, new Av3AdvancedToggleObject());
                        advancedToggle.toggleObjectsExpanded = true;
                        break;
                    }
                case Av3ToggleModuleTypes.ChangeMaterial:
                    {
                        AddModuleToToggle(advancedToggle, new Av3AdvancedMaterialSwap());
                        advancedToggle.materialSwapExpanded = true;
                        break;
                    }

                case Av3ToggleModuleTypes.PoiyomiDissolve:
                    {
                        AddModuleToToggle(advancedToggle, new Av3AdvancedObjectDissolve());
                        advancedToggle.objectDissolveExpanded = true;
                        break;
                    }

                case Av3ToggleModuleTypes.Blendshape:
                    {
                        AddModuleToToggle(advancedToggle, new Av3AdvancedBlendshape());
                        advancedToggle.blendshapesExpanded = true;
                        break;
                    }

                default:
                    break;
            }
        }

        private static void AddModuleToToggle(Av3AdvancedToggle advancedToggle, IAv3AdvancedModule module)
        {
            advancedToggle.Modules.Add(module);
            module.InitializeModule();
        }
    }
}