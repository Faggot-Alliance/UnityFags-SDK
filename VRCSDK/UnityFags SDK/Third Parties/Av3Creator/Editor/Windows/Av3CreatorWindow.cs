#define Av3Creator
//#define Av3Creator_BASIC
#define Av3Creator_SUPPORTER

#region
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using Av3Creator.Utils;
using Av3Creator.Lists;
using Av3Creator.Core;
#if Av3Creator_SUPPORTER
using Av3Creator.Supporter;
#endif

#endregion

namespace Av3Creator.Windows
{
    public class Av3CreatorWindow : EditorWindow
    {
        public static Av3CreatorWindow Av3Instance;
        public readonly static string VERSION = "v1.2.1";

        private bool isAvatarLoaded = false;

#region Serializable Variables
        [SerializeReference]
        private VRCAvatarDescriptor vrcAvatarDescriptor;
        public VRCAvatarDescriptor GetSelectedDescriptor() => vrcAvatarDescriptor;

        private static string ProjectPath; // Initialized when Enabled

        [SerializeReference] public Av3Settings Settings = new Av3Settings();

        [SerializeReference] private AnimatorController fxLayer;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMainMenu;
#endregion

        [MenuItem("UnityFags SDK/Avatar Tools/Av3Creator/Av3Creator", priority = 1)]
        public static void ShowAv3Creator()
        {
            if (!Av3Instance)
            {
                Av3Instance = (Av3CreatorWindow)GetWindow(typeof(Av3CreatorWindow));
                Av3Instance.autoRepaintOnSceneChange = true;
                Av3Instance.minSize = new Vector2(500, 600);
            }
            Av3Instance.Show();
        }

        [MenuItem("GameObject/Av3Creator/Quick/Quick Toggle", false, -100)]
        public static void AddToToggle(MenuCommand menuCommand)
        {
            if (Selection.objects.Length > 1 && menuCommand.context != Selection.objects[0]) return;

            ShowAv3Creator();
            var selectedObjects = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
            foreach (var selected in selectedObjects)
                Av3Instance.AddObjectToToggleList(new QuickToggleData()
                {
                    Elements = new MultiToggleCell[] {
                    new MultiToggleCell()
                    {
                         Enabled = selected.activeSelf,
                        Object = selected
                    }
                },
                    Name = selected.name
                });

        }

        private SerializedObject serializedObject;

        private Av3QuickToggleList quickToggleList;
        public QuickToggleData[] toggleObjects = new QuickToggleData[] { new QuickToggleData() };
        private SerializedProperty _toggleObjectsProperty;

        private Av3PresetCreator Av3PresetCreator;
        private Av3Organizer Av3Organizer;

        public void AddObjectToToggleList(QuickToggleData newElement)
        {
            if (toggleObjects.Any(x => x == newElement)) return;
            var newList = toggleObjects.ToList();
            newList.Add(newElement);

            toggleObjects = newList.Distinct().ToArray();
        }

        public void OnEnable()
        {
            ProjectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            var data = EditorPrefs.GetString("Av3Creator", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            serializedObject = new SerializedObject(this);
            _toggleObjectsProperty = serializedObject.FindProperty(nameof(toggleObjects));

            quickToggleList = new Av3QuickToggleList(this, _toggleObjectsProperty, new string[] {
            "Default State | Toggles", "Name", "   Saved"
        }, new float?[] { null, 100, 60 });

            serializedObject.Update();

            Av3Organizer = new Av3Organizer(Settings);
            LoadAvatar();
        }

        public void OnDisable() => EditorPrefs.SetString("Av3Creator", JsonUtility.ToJson(this, false)); // Save window properties

        private GameObject lastLoadedAvatar;
        private bool LoadAvatar()
        {
            if (!vrcAvatarDescriptor)
            {
                ResetConfigs(true);
                return false;
            }

            if (lastLoadedAvatar == null) lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
            else if (lastLoadedAvatar != vrcAvatarDescriptor.gameObject)
            {
                lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
                ResetConfigs();
            }

            CheckAvatar();

            Av3PresetCreator = new Av3PresetCreator(vrcAvatarDescriptor, Settings);

            return isAvatarLoaded;
        }

        private void ResetConfigs(bool foldoutOptions = false)
        {
            if (foldoutOptions)
            {
                Settings.QuickToggleExpanded = false;
                Settings.OtherOptionsExpanded = false;
            }

            fxLayer = null;
            vrcParameters = null;
            vrcMainMenu = null;
            isAvatarLoaded = false;

            toggleObjects = new QuickToggleData[] { new QuickToggleData() };

            Settings.OutputDirectory = "";

            Av3PresetCreator = null;
        }

        private void CheckAvatar()
        {
            if (vrcAvatarDescriptor == null) return;

            if (vrcAvatarDescriptor.customizeAnimationLayers &&
                vrcAvatarDescriptor.baseAnimationLayers.Any(x => x.type == VRCAvatarDescriptor.AnimLayerType.FX) &&
                vrcAvatarDescriptor.baseAnimationLayers.Single(x => x.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController is AnimatorController layer &&
                layer != null)
                fxLayer = layer;
            else fxLayer = null;

            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcMainMenu = vrcAvatarDescriptor.expressionsMenu;
            isAvatarLoaded = (fxLayer && vrcParameters && vrcMainMenu);
        }

        private bool missingDirectoryError;
        private readonly int DEFAULT_SPACEMENT = 3;

        Vector2 scrollPos;
        private bool planedFeaturesExpanded;
        private bool moreInfoExpanded;
#if !Av3Creator_SUPPORTER
        private bool supporterFeatures;
#endif

        public void OnGUI()
        {
            CheckAvatar();

            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, Av3StyleManager.Styles.ContentView))
            {
                scrollPos = scroll.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(Av3StyleManager.Styles.Header))
                    EditorGUILayout.LabelField($"<b><size=15>Av3Creator {VERSION}</size></b>\n" +
                                               $"<size=11>rafacasari.gumroad.com • rafa.booth.pm</size>",
                                                Av3StyleManager.Styles.CenteredText, GUILayout.MinHeight(40f));
                EditorGUILayout.Space(DEFAULT_SPACEMENT);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var checkChanges = new EditorGUI.ChangeCheckScope())
                    {
                        vrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(vrcAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));
                        if (checkChanges.changed)
                            LoadAvatar();
                    }


                    if (GUILayout.Button("Auto-Fill", GUILayout.Height(24)))
                    {
                        vrcAvatarDescriptor = Av3Utils.SelectCurrentAvatar();
                        LoadAvatar();
                    }
                }

                EditorGUILayout.Space(DEFAULT_SPACEMENT);

                if (missingDirectoryError)
                    Av3StyleManager.DrawError("ERROR: Select a directory!");

                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    if (missingDirectoryError)
                    {
                        int padding = 2;
                        Rect highlightRect = new Rect(horizontalScope.rect.x - padding, horizontalScope.rect.y - padding, horizontalScope.rect.width + (padding * 2), horizontalScope.rect.height + padding * 2);
                        EditorGUI.DrawRect(highlightRect, new Color(1, 0, 0, 0.2f));
                    }

                    int avatarDirectoryHeight = 22;
                    EditorGUILayout.LabelField("Avatar Directory ", GUILayout.ExpandWidth(false), GUILayout.Height(avatarDirectoryHeight));

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

                        Directory.CreateDirectory(Settings.OutputDirectory);

                        var parameters = CreateInstance<VRCExpressionParameters>();
                        parameters.parameters = new VRCExpressionParameters.Parameter[0];
                        AssetDatabase.CreateAsset(parameters, Settings.OutputDirectory + "/Parameters.asset");
                        vrcAvatarDescriptor.customExpressions = true;
                        vrcAvatarDescriptor.expressionParameters = parameters;

                        EditorUtility.SetDirty(parameters);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    });

                    if (!vrcMainMenu) Av3StyleManager.DrawIssueBox(MessageType.Warning, "Missing Main Menu", () =>
                    {
                        if (string.IsNullOrEmpty(Settings.OutputDirectory)) missingDirectoryError = true;

                        Directory.CreateDirectory(Settings.OutputDirectory);

                        var menu = CreateInstance<VRCExpressionsMenu>();
                        AssetDatabase.CreateAsset(menu, Settings.OutputDirectory + "/Main Menu.asset");
                        vrcAvatarDescriptor.customExpressions = true;
                        vrcAvatarDescriptor.expressionsMenu = menu;

                        EditorUtility.SetDirty(menu);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    });

                    Settings.QuickToggleExpanded = false;
                }

                Av3StyleManager.DrawLine(1, DEFAULT_SPACEMENT);

                using (new EditorGUI.DisabledScope(!isAvatarLoaded))
                {
                    DrawQuickToggleCreator();
                    DrawPresetCreator();
                    DrawOtherOptions();

#if Av3Creator_SUPPORTER
                    DrawHUEShift();
#endif
                }

                DrawOrganizer();

                moreInfoExpanded = Av3StyleManager.Foldout("More Info", moreInfoExpanded, 5);
                if (moreInfoExpanded)
                {

                    if (GUILayout.Button(new GUIContent(" Join the Official Av3Creator Discord Server", Av3IconManager.DiscordLogo,
                        "Our discord have more information about the tool, some updates alerts and WIP pics/videos."),
                        GUILayout.Height(30)))
                        Application.OpenURL("https://discord.gg/3ZpeG5yahd");

                    if (GUILayout.Button(new GUIContent(" Check out our Trello page - Future Features", Av3IconManager.TrelloLogo,
                      "We constantly are updating our trello page with all the features that we are working on."),
                      GUILayout.Height(30)))
                        Application.OpenURL("https://trello.com/b/6ZX55QkS/av3creator");

                    if (GUILayout.Button(new GUIContent(" Support on Patreon (become a beta tester)", Av3IconManager.PatreonLogo,
                        "I'am always making some new thing for this script, if you support me on patreon you'll receive some exclusive benefits, including Early Access to Updates and Beta Testing Some Features."),
                        GUILayout.Height(30)))
                        Application.OpenURL("https://patreon.com/rafacasari");

                    using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                    {
                        planedFeaturesExpanded = Av3StyleManager.Foldout("Planned Features", planedFeaturesExpanded);
                        if (planedFeaturesExpanded)
                        {
                            using (new EditorGUI.DisabledScope(true))
                            {
                                using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                                {
                                    GUILayout.Button("Simple PIN Creator");
                                    GUILayout.Button("Easy Expressions");
                                    GUILayout.Button("Swap Toggle Creator (int toggles)");
                                }
                            }
                        }

#if !Av3Creator_SUPPORTER
                        supporterFeatures = Av3StyleManager.Foldout("Supporter Edition Features", supporterFeatures);
                        if (supporterFeatures)
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.Padding5))
                            {
                                GUILayout.Button("Auto HUE Shifter");
                            }

                        }
#endif
                    }
                }
            }
        }

#region Options
        private void DrawQuickToggleCreator()
        {
            if (Settings.QuickToggleExpanded = Av3StyleManager.Foldout("Quick Toggle Creator", Settings.QuickToggleExpanded, 5))
            {
                GUILayout.Label("Quick Toggle Creator can easily setup toggles for your avatar, creating the animation, setting-up the layers and parameters, and adding them to the avatar menu.", Av3StyleManager.Styles.Description);

                EditorGUILayout.Space(10);
#region QUICK TOGGLE CREATOR
                using (new EditorGUI.DisabledScope(!(fxLayer && vrcParameters && vrcMainMenu)))
                {
                    Settings.QTSettings.SettingsExpanded = EditorGUILayout.Foldout(Settings.QTSettings.SettingsExpanded, "Settings", Av3StyleManager.Styles.BoldFoldout);
                    if (Settings.QTSettings.SettingsExpanded)
                    {

                        EditorGUILayout.LabelField("<b>The default settings are good to go, if you don't know what you doing, don't change anything here.</b>\nYou can hover a option for more information about it.",
                            Av3StyleManager.Styles.SettingsDescription, GUILayout.MaxWidth(position.width - 40));

                        using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.SettingsMargin))
                        {
                            Settings.QTSettings.WriteDefaults = EditorGUILayout.ToggleLeft(new GUIContent("» Write Defaults", "Only check this if your avatar use Write Defaults, if you don't know what is this, just keep this unchecked."),
                                Settings.QTSettings.WriteDefaults, Av3StyleManager.Styles.ToggleLeft);
                            Settings.QTSettings.Overwrite = EditorGUILayout.ToggleLeft(new GUIContent("» Overwrite", "If checked, will replace any layers, animations, menus or conditions that use same names. Useful for updates."),
                                Settings.QTSettings.Overwrite, Av3StyleManager.Styles.ToggleLeft);
                            EditorGUILayout.BeginHorizontal();
                            Settings.QTSettings.CustomMenuName = EditorGUILayout.ToggleLeft(new GUIContent("» Custom menu name", "Check this if you want to rename the menu"),
                                Settings.QTSettings.CustomMenuName, Av3StyleManager.Styles.ToggleLeft);

                            using (new EditorGUI.DisabledScope(!Settings.QTSettings.CustomMenuName))
                            {
                                EditorGUILayout.LabelField("Name:", Av3StyleManager.Styles.RightLabelField);
                                Settings.QTSettings.MenuName = Av3Core.RemoveInvalidChars(EditorGUILayout.TextField(Settings.QTSettings.MenuName));
                            }

                            EditorGUILayout.EndHorizontal();
                            Settings.QTSettings.AddToMainMenu = EditorGUILayout.ToggleLeft(new GUIContent("» Add Menu to Main Menu", "If checked, will automatically add Toggles Menu in Main Menu.\nIf not, will just create the menu and you should manually put the menu wherever you want."),
                                Settings.QTSettings.AddToMainMenu, Av3StyleManager.Styles.ToggleLeft);
                            using (new EditorGUI.DisabledScope(true))
                                Settings.QTSettings.SyncMenuStates = EditorGUILayout.ToggleLeft(new GUIContent("» Sync in Menu (WIP)", "If checked, the object will be \"synced\" with the menu, that is, if a gameobject is enabled by default, the option in menu will be enabled too."),
                                    Settings.QTSettings.SyncMenuStates, Av3StyleManager.Styles.ToggleLeft);

                            Settings.QTSettings.DisableCredits = EditorGUILayout.ToggleLeft(new GUIContent("» Disable Credits", "If checked, will not create additional layers for credits in FX Layers.\n\nChecking this will not have any difference in avatar aparence, just the layer. Not break anything, just my heart. :c"),
                                Settings.QTSettings.DisableCredits, Av3StyleManager.Styles.ToggleLeft);

                        }
                    }

                    GUILayout.Space(5f);

                    serializedObject.Update();
                    quickToggleList.DrawList("Toggles",
                        "» <b>Default State:</b> The default state of toggle (<b>before</b> enabling toggle)\n" +
                        "» <b>Toggles:</b> Target gameobject for toggling\n" +
                        "» <b>Name:</b> Used for Parameters, Layers and Menu\n" +
                        "» <b>Saved:</b> Saves parameter in memory (<i>aka \"Memory System\"</i>)\n");

                    serializedObject.ApplyModifiedProperties();


                    GUILayout.Space(10f);
                    var canCreateParams = Av3QuickToggle.CanCreateParams(vrcParameters, toggleObjects.Length, out int sizeNeeded);
                    if (!canCreateParams)
                        Av3StyleManager.DrawIssueBox(MessageType.Warning, "Insuficient space to create this toggles, please consider to delete some unused parameters." +
                            "<b>You need more " + sizeNeeded + " byte(s).</b>");

                    bool disableCreateButton = toggleObjects.Where(x => x.Elements.Any(obj => obj.Object != null)).Count() <= 0 || !canCreateParams || string.IsNullOrEmpty(Settings.OutputDirectory);
                    using (new EditorGUI.DisabledScope(disableCreateButton))
                        if (GUILayout.Button("Create Toggles!", GUILayout.Height(40f)))
                        {
                            var toggleCreator = new Av3QuickToggle(vrcAvatarDescriptor, toggleObjects, Settings);
                            toggleCreator.SetupThings();
                        }

                    // ERRORS
                    if (string.IsNullOrEmpty(Settings.OutputDirectory))
                        Av3StyleManager.DrawError("> ERROR: Missing avatar directory, please select the main folder of your avatar.", 5);

                    if (vrcMainMenu.controls.Count >= 8 && Settings.QTSettings.AddToMainMenu)
                        Av3StyleManager.DrawWarning("> Menu doesn't have enough space, generated menu will not be automatically added!", 5);
                }
#endregion
            }
        }

        private void DrawPresetCreator() => Av3StyleManager.DrawOption("Preset Creator", ref Settings.PresetCreatorExpanded, () => Av3PresetCreator?.DrawGUI());
        private void DrawOrganizer() => Av3StyleManager.DrawOption("Project Organizer", ref Settings.ProjectOrganizerExpanded, () => Av3Organizer?.DrawGUI());
#if Av3Creator_SUPPORTER
        private void DrawHUEShift() => Av3StyleManager.DrawOption("Auto HUE Shifter (Supporter Exclusive)", ref Settings.EasyHUEShifterExpanded, () => Av3AutoHUEShift.DrawGUI(vrcAvatarDescriptor, Settings));
#endif

        private void DrawOtherOptions()
        {
            Av3StyleManager.DrawOption("Other Options", ref Settings.OtherOptionsExpanded, () =>
            {
                GUILayout.Space(5);
                GUILayout.Label("This options can help you with very boring stuff.", Av3StyleManager.Styles.Description);
                GUILayout.Space(5);

                if (GUILayout.Button(new GUIContent("Fix Overlap Expressions",
                    "Simple fix to block different expressions in FX Layer.\n\n" +
                    "This will create a new condition for every gesture in Right and Left hand"), GUILayout.Height(40f)))
                    vrcAvatarDescriptor.FixExpressions();

                if (GUILayout.Button(new GUIContent("Remove Unused Parameters", "This will search and remove unused parameters in your descriptor and layers."),
                    GUILayout.Height(40f)))
                    vrcAvatarDescriptor?.RemoveUnusedParams();
            });
        }
#endregion
    }
}