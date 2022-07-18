#region
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;
using Av3Creator.Utils;
using Av3Creator.Lists;
using Av3Creator.Core;
#endregion

namespace Av3Creator.Windows
{
    public class Av3ParameterEditorWindow : EditorWindow
    {
        private static EditorWindow Av3Window;

        #region Serializable Variables
        [SerializeReference] private VRCAvatarDescriptor _vrcAvatarDescriptor;
        private VRCAvatarDescriptor vrcAvatarDescriptor
        {
            get { return _vrcAvatarDescriptor; }
            set
            {
                _vrcAvatarDescriptor = value;
                LoadAvatar();
            }
        }

        public string CustomPath { get; private set; }

        public VRCAvatarDescriptor GetSelectedDescriptor() => _vrcAvatarDescriptor;

        private static string ProjectPath; // Initialized when Enabled

        [SerializeReference] private VRCExpressionParameters vrcParameters;

        #endregion

        [MenuItem("UnityFags SDK/Avatar Tools/Av3Creator/Parameter Editor", priority = 21)]
        static void ShowParameterEditor()
        {
            if (!Av3Window)
            {
                Av3Window = GetWindow(typeof(Av3ParameterEditorWindow));
                Av3Window.autoRepaintOnSceneChange = true;
                Av3Window.minSize = new Vector2(400, 0);
                Av3Window.titleContent = new GUIContent("Parameter Editor", "Av3Creator Parameter Editor by Rafa");
            }
            Av3Window.Show();
        }

        private SerializedObject serializedObject;
        [SerializeField] private Av3ParameterList Av3ParameterList;
        public VRCExpressionParameters.Parameter[] Av3Parameters = null;
        private SerializedProperty _vrcParametersProperty;

        public void OnEnable()
        {
            ProjectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            var data = EditorPrefs.GetString(nameof(Av3CreatorWindow), JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            serializedObject = new SerializedObject(this);
            _vrcParametersProperty = serializedObject.FindProperty(nameof(Av3Parameters));

            if (Av3ParameterList == null)
                Av3ParameterList = new Av3ParameterList(this, _vrcParametersProperty, new string[] {
                    "Name", "Type", "Saved", "   Default"
                }, new float?[] { null, 50, 40, 60 });

            serializedObject.Update();
            LoadAvatar(true);
        }

        public void OnDisable() => EditorPrefs.SetString(nameof(Av3CreatorWindow), JsonUtility.ToJson(this, false)); // Save window properties

        private GameObject lastLoadedAvatar;

        private bool LoadAvatar(bool isOnEnable = false)
        {
            if (!vrcAvatarDescriptor)
            {
                ResetConfigs(isOnEnable);
                return false;
            }

            if (lastLoadedAvatar == null) lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
            else if (lastLoadedAvatar != vrcAvatarDescriptor.gameObject)
            {
                lastLoadedAvatar = vrcAvatarDescriptor.gameObject;
                ResetConfigs(isOnEnable);
            }


            vrcParameters = vrcAvatarDescriptor.expressionParameters;

            isLoaded = vrcParameters;
            return isLoaded;
        }

        private void ResetConfigs(bool isOnEnable = false)
        {
            vrcParameters = null;
            isLoaded = false;

            CustomPath = "";
            missingDirectoryError = false;

            if (!isOnEnable) Av3ParameterList?.ClearCache();
        }

        private bool missingDirectoryError;
        private readonly int DEFAULT_SPACEMENT = 3;

        Vector2 scrollPos;
        private bool isLoaded;

        public bool SettingsExpanded;
        public bool EnableParameterRenaming;

        private VRCExpressionParameters cachedParameters;
        private VRCExpressionParameters parametersCopy;

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, Av3StyleManager.Styles.ContentView);
                {
                    Av3StyleManager.DrawIssueBox(MessageType.Info, "<b>This editor is a WIP and can have bugs and be laggy.</b>");
                    EditorGUILayout.Space(DEFAULT_SPACEMENT);   

                    if (!vrcAvatarDescriptor)
                    {
                        Av3StyleManager.DrawIssueBox(MessageType.Warning, "<b>Select an avatar to begin editing your parameters</b>");
                        EditorGUILayout.Space(DEFAULT_SPACEMENT);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        vrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(vrcAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));
                        if (GUILayout.Button("Auto-Fill", GUILayout.Height(24), GUILayout.MaxWidth(100)))
                            SelectAvatar();
                    }

                    if (vrcAvatarDescriptor != null)
                    {
                        EditorGUILayout.Space(DEFAULT_SPACEMENT);
                        vrcAvatarDescriptor.expressionParameters = (VRCExpressionParameters) EditorGUILayout.ObjectField(vrcAvatarDescriptor.expressionParameters, typeof(VRCExpressionParameters), false, GUILayout.Height(20));
                    }

                    if (vrcAvatarDescriptor != null && !isLoaded && !vrcParameters)
                    {
                        EditorGUILayout.Space(DEFAULT_SPACEMENT);
                        if (missingDirectoryError) Av3StyleManager.DrawError("ERROR: Select a directory!");

                        using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                        {
                            if (missingDirectoryError)
                            {
                                int padding = 2;
                                Rect highlightRect = new Rect(horizontalScope.rect.x - padding, horizontalScope.rect.y - padding, horizontalScope.rect.width + (padding * 2), horizontalScope.rect.height + padding * 2);
                                EditorGUI.DrawRect(highlightRect, new Color(1, 0, 0, 0.2f));
                            }

                            int avatarDirectoryHeight = 22;
                            EditorGUILayout.LabelField("Output Directory ", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(120), GUILayout.Height(avatarDirectoryHeight));

                            using (new EditorGUI.DisabledScope(true)) EditorGUILayout.TextField(CustomPath, GUILayout.Height(avatarDirectoryHeight));

                            if (GUILayout.Button(Av3StyleManager.Icons.OpenFolder, GUILayout.ExpandWidth(false), GUILayout.Height(avatarDirectoryHeight)))
                            {
                                var path = EditorUtility.OpenFolderPanel("Select Output Directory", Application.dataPath, "");
                                if (!string.IsNullOrEmpty(path) && path.StartsWith(ProjectPath))
                                {
                                    path = path.Substring(ProjectPath.Length + 1);
                                    CustomPath = path;
                                    missingDirectoryError = false;
                                }
                            }
                        }

                        EditorGUILayout.Space(3);
                        Av3StyleManager.DrawIssueBox(MessageType.Warning, "Avatar doesn't have VRCExpressionsParameters", () =>
                        {
                            if (string.IsNullOrEmpty(CustomPath)) missingDirectoryError = true;

                            Directory.CreateDirectory(CustomPath);

                            var parameters = CreateInstance<VRCExpressionParameters>();
                            parameters.parameters = new VRCExpressionParameters.Parameter[0];
                            AssetDatabase.CreateAsset(parameters, CustomPath + "/Parameters.asset");
                            vrcAvatarDescriptor.expressionParameters = parameters;

                            EditorUtility.SetDirty(parameters);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        });
                    }

                    using (new EditorGUI.DisabledScope(!isLoaded))
                    {
                        if (vrcAvatarDescriptor != null && vrcAvatarDescriptor.expressionParameters != null)
                        {
                            EditorGUILayout.Space(5);
                            //Av3StyleManager.DrawFoldout("Settings", ref SettingsExpanded, () =>
                            //{
                            //    EnableParameterRenaming = EditorGUILayout.ToggleLeft(new GUIContent("Parameter Auto Rename", ""), EnableParameterRenaming);
                            //});

                            EnableParameterRenaming = EditorGUILayout.ToggleLeft(new GUIContent("Parameter Auto Rename", ""), EnableParameterRenaming);

                            if (cachedParameters == null) cachedParameters = vrcAvatarDescriptor.expressionParameters;
                            else if (cachedParameters != vrcAvatarDescriptor.expressionParameters)
                            {
                                cachedParameters = vrcAvatarDescriptor.expressionParameters;
                                Av3ParameterList.ClearCache();
                            }

                            Av3Parameters = vrcAvatarDescriptor?.expressionParameters?.parameters;
                            serializedObject.Update();

                            EditorGUI.BeginChangeCheck();
                            Av3ParameterList.DrawList();
                            if (EditorGUI.EndChangeCheck() && serializedObject.hasModifiedProperties)
                            {
                                serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(vrcAvatarDescriptor.expressionParameters);
                            }


                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Add Int")) vrcAvatarDescriptor.expressionParameters.AddEmptyParameter(VRCExpressionParameters.ValueType.Int);
                                if (GUILayout.Button("Add Float")) vrcAvatarDescriptor.expressionParameters.AddEmptyParameter(VRCExpressionParameters.ValueType.Float);
                                if (GUILayout.Button("Add Bool")) vrcAvatarDescriptor.expressionParameters.AddEmptyParameter(VRCExpressionParameters.ValueType.Bool);
                            }

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Clear All")) vrcAvatarDescriptor.expressionParameters.ClearAll();
                                if (GUILayout.Button("Reset to Default")) vrcAvatarDescriptor.expressionParameters.ResetToDefault();
                            }

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Remove Unused Parameters")) vrcAvatarDescriptor.RemoveUnusedParams();
                                if (GUILayout.Button("Remove Duplicated Parameters")) vrcAvatarDescriptor.RemoveDuplicatedParams();
                            }
                           

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                parametersCopy = (VRCExpressionParameters)EditorGUILayout.ObjectField(parametersCopy, typeof(VRCExpressionParameters), false);
                                if (GUILayout.Button("Copy Parameters"))
                                {
                                    if(parametersCopy != null && parametersCopy != vrcParameters)
                                    {
                                        Undo.RecordObject(vrcAvatarDescriptor.expressionParameters, "Av3Creator - Merge Parameters");
                                        var newList = vrcParameters.parameters.ToList();
                                        newList.AddRange(parametersCopy.parameters);

                                        vrcAvatarDescriptor.expressionParameters.parameters = newList.Aggregate(new List<VRCExpressionParameters.Parameter>(),
                                        (d, e) =>
                                        {
                                            if (!d.Any(x => x.name == e.name && x.valueType == e.valueType))
                                                d.Add(e);
                                            return d;
                                        }).Where(x => !string.IsNullOrEmpty(x.name)).ToArray();

                                        EditorUtility.SetDirty(vrcAvatarDescriptor.expressionParameters);
                                    }
                                }
                            }



                            int cost = vrcAvatarDescriptor.expressionParameters.CalcTotalCost();
                            if (cost <= VRCExpressionParameters.MAX_PARAMETER_COST)
                                EditorGUILayout.HelpBox($"Total Memory: {cost}/{VRCExpressionParameters.MAX_PARAMETER_COST}", MessageType.Info);
                            else
                                EditorGUILayout.HelpBox($"Total Memory: {cost}/{VRCExpressionParameters.MAX_PARAMETER_COST}\nParameters use too much memory. Remove some parameters or use bools instead of int/floats to use less memory.", MessageType.Error);

                        }
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        private void SelectAvatar()
        {
            // Priorize selected transform
            if (Selection.activeTransform && Selection.activeTransform?.root?.gameObject?.GetComponent<VRCAvatarDescriptor>() != null)
            {
                vrcAvatarDescriptor = Selection.activeTransform.root.GetComponent<VRCAvatarDescriptor>();
                return;
            }
            // If dont have any transform selected, find all descriptors and order them by index
            var gameObjects = Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>().OrderBy(x => x.transform.GetSiblingIndex()).ToArray();
            if (gameObjects != null && gameObjects.Length > 0)
            {
                // Priorize active descriptors
                if (gameObjects.Any(x => x.gameObject.activeSelf))
                    vrcAvatarDescriptor = gameObjects.First(x => x.gameObject.activeSelf);
                // If dont have any active descriptor, use the first one
                else vrcAvatarDescriptor = gameObjects.First();
            }
        }

    }
}