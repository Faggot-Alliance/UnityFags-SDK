#region
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using Object = UnityEngine.Object;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.ScriptableObjects;
using Av3Creator.Lists;
using Av3Creator.Utils;
#endregion

namespace Av3Creator.Core
{
    public class Av3Organizer
    {
        private static readonly string OrganizedFolderPath = "Assets/Avatars/";

        private string ProjectPath;
        private Av3Settings Settings;
        private List<Av3OrganizeItem> OrganizeItems = new List<Av3OrganizeItem>();
        private List<Av3OrganizerData> Items = new List<Av3OrganizerData>();
        private Av3OrganizerList Av3OrganizerList;

        public Av3Organizer(Av3Settings settings)
        {
            ProjectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            Settings = settings;

            // Basics
            OrganizeItems.Add(new Av3OrganizeItem(typeof(GameObject), "FBX", Av3OrganizeInputType.Move, "GameObject (FBX)"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(Texture2D), "Textures"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(Cubemap), "Textures/Cubemap"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(Material), "Materials"));

            // 3.0
            OrganizeItems.Add(new Av3OrganizeItem(typeof(VRCExpressionParameters), "3.0", iconName: "ScriptableObject"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(VRCExpressionsMenu), "3.0/Menus", iconName: "ScriptableObject"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(BlendTree), "3.0/BlendTrees"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(AnimationClip), "3.0/Animations"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(AnimatorOverrideController), "3.0/Layers"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(AnimatorController), "3.0/Layers"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(AvatarMask), "3.0/AvatarMasks"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(AudioClip), "Others"));

            // Others, not recomended
            OrganizeItems.Add(new Av3OrganizeItem(typeof(Shader), "Shaders", Av3OrganizeInputType.Ignore));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(MonoScript), "Scripts", Av3OrganizeInputType.Ignore, iconName: "cs Script"));
            OrganizeItems.Add(new Av3OrganizeItem(typeof(DefaultAsset), "Others", Av3OrganizeInputType.Ignore, "DefaultAsset (DLLs)"));

            Av3OrganizerList = new Av3OrganizerList(Items);
        }


        private bool missingDirectoryError;

        private string[] BlackListedPaths =
        {
            "VRCSDK",
            "_PoiyomiShaders",
            "DynamicPenetrationSystem",
            "RalivDynamicPenetrationSystem"
        };

        public void DrawGUI()
        {
            GUILayout.Label("Does your avatar have files scattered throughout your project? This tool will help you organize your project, having a cleaner unitypackage.", Av3StyleManager.Styles.Description);

            EditorGUILayout.Space(10);
            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                if (missingDirectoryError)
                {
                    int padding = 2;
                    Rect highlightRect = new Rect(horizontalScope.rect.x - padding, horizontalScope.rect.y - padding, horizontalScope.rect.width + (padding * 2), horizontalScope.rect.height + padding * 2);
                    EditorGUI.DrawRect(highlightRect, new Color(1, 0, 0, 0.2f));
                }

                int avatarDirectoryHeight = 20;
                EditorGUILayout.LabelField("Output Directory ", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(150), GUILayout.Height(avatarDirectoryHeight));

                using (new EditorGUI.DisabledScope(true)) EditorGUILayout.TextField(Settings.OrganizeSettings.CustomPath, GUILayout.Height(avatarDirectoryHeight));

                if (GUILayout.Button(Av3StyleManager.Icons.OpenFolder, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70), GUILayout.Height(avatarDirectoryHeight)))
                {
                    var path = EditorUtility.OpenFolderPanel("Select Output Directory", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(path) && path.StartsWith(ProjectPath))
                    {
                        path = path.Substring(ProjectPath.Length + 1);
                        Settings.OrganizeSettings.CustomPath = path;
                        missingDirectoryError = false;
                    }
                }
            }

            EditorGUILayout.Space(5);
            Av3OrganizerList.DrawList();

            Av3StyleManager.DrawFoldout("Settings", ref Settings.OrganizeSettings.SettingsExpanded, () =>
            {
                using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.SettingsMargin))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(14, false);
                        var icon = EditorGUIUtility.IconContent("SceneAsset Icon");
                        EditorGUILayout.LabelField(icon, GUILayout.Width(20), GUILayout.Height(20));
                        EditorGUILayout.LabelField(new GUIContent("Scene Name", "Output scene"), GUILayout.MaxWidth(120), GUILayout.Height(20));
                        Settings.OrganizeSettings.SceneName = EditorGUILayout.TextField(Settings.OrganizeSettings.SceneName, GUILayout.Height(20));
                    }

                    Settings.OrganizeSettings.CreatePrefabInstance = EditorGUILayout.ToggleLeft(new GUIContent(" Create Prefab", "")
                    {
                        image = EditorGUIUtility.IconContent("Prefab Icon").image,
                    }, Settings.OrganizeSettings.CreatePrefabInstance, Av3StyleManager.Styles.ToggleLeft, GUILayout.MaxWidth(200));


                    Av3StyleManager.DrawFoldout("Export List", ref Settings.OrganizeSettings.ExportListExpanded, () =>
                    {
                        foreach (var (option, index) in OrganizeItems.Select((option, index) => (option, index)))
                        {

                            using (new EditorGUILayout.HorizontalScope(index % 2 == 0 ? Av3StyleManager.Styles.BoxEven : Av3StyleManager.Styles.BoxOdd))
                            {
                                EditorGUILayout.LabelField(new GUIContent(" " + option.Name, "")
                                {
                                    image = EditorGUIUtility.IconContent($"{(string.IsNullOrEmpty(option.IconName) ? option.ItemType.Name : option.IconName)} Icon").image
                                });
                                option.InputType = (Av3OrganizeInputType)EditorGUILayout.EnumPopup(option.InputType);
                            }
                        }
                    }, isBold: false);

                    EditorGUILayout.Space(5);
                    GUILayout.Label("Use the ignore list for anything that you dont want to be in organized folder, like Paid Assets that can not be included in the unitypackage. Just drag and drop into \"Ignore list\", this also accepts entire folders.",
                        Av3StyleManager.Styles.Description);

                    EditorGUI.BeginChangeCheck();
                    if (Settings.SerializedSettings.GetProperty(nameof(Settings.SerializedSettings.Organizer_IgnoreList), out SerializedProperty property))
                        EditorGUILayout.PropertyField(property, new GUIContent("Ignore List"));
                    if (EditorGUI.EndChangeCheck())
                        Settings.SerializedSettings.SerializedSettings.ApplyModifiedProperties();


                }
            });

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(Items == null || !Items.Any(x => x.Descriptor != null)))
                if (GUILayout.Button("Organize my Assets!", GUILayout.Height(26))) OrganizeAssets();


        }

        
        public void OrganizeAssets()
        {
            if (Items == null || Items.Count == 0) return;
            Items = Items.Where(x => x.Descriptor != null).ToList();
            var guidsMap = new List<(string, string)>();

            string currentPath = OrganizedFolderPath.ToString();
            if (!string.IsNullOrEmpty(Settings.OrganizeSettings.CustomPath)) currentPath = Settings.OrganizeSettings.CustomPath;

            if (Directory.GetFiles(currentPath).Length > 0)
            {
                currentPath = AssetDatabase.GenerateUniqueAssetPath(currentPath);
                Debug.Log($"Specified folder is not empty, generated a new folder. ({currentPath})");
            }
            Directory.CreateDirectory(currentPath);

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            foreach (var (target, index) in Items.Select((value, i) => (value, i)))
            {
                var targetObject = target.Descriptor.gameObject;

                var avatarName = Av3Core.RemoveInvalidChars(target.Name);
                if (string.IsNullOrEmpty(avatarName)) avatarName = "Avatar";

                GameObject go = null;

                go = Object.Instantiate(targetObject, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)).gameObject;


                SceneManager.MoveGameObjectToScene(go, newScene);
                go.name = target.Name;
                go.SetActive(true);

                if (Settings.OrganizeSettings.CreatePrefabInstance)
                {
                    var prefabDirectory = Path.Combine(currentPath, "Prefabs");
                    Directory.CreateDirectory(prefabDirectory);

                    var prefabPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(prefabDirectory, avatarName + ".prefab"));
                    PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                }

                go.SetActive(index == 0);
            }



            var scenePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(currentPath, $"{(Av3Core.RemoveInvalidChars(Settings.OrganizeSettings.SceneName))}.unity"));
            EditorSceneManager.SaveScene(newScene, scenePath);

            var dependencies = AssetDatabase.GetDependencies(scenePath)
                .Distinct()
                .Where(x =>
                {
                    bool blacklisted = false;
                    foreach (var blacklist in BlackListedPaths)
                    {
                        blacklisted = x.Split('/', '\\').Any(y => y == blacklist);
                        if (blacklisted) break;

                    }
                    return !blacklisted;
                });


            EditorSceneManager.CloseScene(newScene, true);
            foreach (var dependencePath in dependencies)
            {
                var loadedObject = AssetDatabase.LoadAssetAtPath<Object>(dependencePath);
                if (loadedObject == null) continue;

                Av3OrganizeItem myItem = null;
                if (OrganizeItems.Any(y => y.ItemType == loadedObject.GetType()))
                    myItem = OrganizeItems?.First(y => y.ItemType == loadedObject.GetType());
                else if (loadedObject is SceneAsset) continue;
                if (myItem == null || myItem.InputType == Av3OrganizeInputType.Ignore) continue;

                if (Settings?.SerializedSettings?.Organizer_IgnoreList != null)
                {
                    if (Settings.SerializedSettings.Organizer_IgnoreList.Contains(loadedObject)) continue;
                    if (Settings.SerializedSettings.Organizer_IgnoreList.Any(x => x != null && dependencePath.StartsWith(AssetDatabase.GetAssetPath(x)))) continue;
                }

                var folderName = "Others";
                if (myItem != null && !string.IsNullOrEmpty(myItem.FolderName)) folderName = myItem.FolderName;

                var targetPath = Path.Combine(currentPath, folderName);

                if (myItem.InputType == Av3OrganizeInputType.Move && !Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                    AssetDatabase.Refresh();
                }
                else Directory.CreateDirectory(targetPath);

                var fileName = Path.GetFileName(dependencePath);
                var fileTargetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(targetPath, fileName));

                if (myItem.InputType == Av3OrganizeInputType.Copy)
                {
                    AssetDatabase.CopyAsset(dependencePath, fileTargetPath);
                    var oldGUID = AssetDatabase.AssetPathToGUID(dependencePath);
                    var newGUID = AssetDatabase.AssetPathToGUID(fileTargetPath);
                    guidsMap.Add((oldGUID, newGUID));
                }
                else if (myItem.InputType == Av3OrganizeInputType.Move)
                {
                    var result = AssetDatabase.MoveAsset(dependencePath, fileTargetPath);
                    if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                }
            }

            UpdateGUIDs(currentPath, guidsMap);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(scenePath));
        }

        private void UpdateGUIDs(string path, List<(string, string)> guidTable)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                List<string> allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();

                foreach (string fileToModify in allFiles)
                {
                    var metaAttributes = File.GetAttributes(fileToModify);
                    var isHidden = false;
                    if (metaAttributes.HasFlag(FileAttributes.Hidden))
                    {
                        isHidden = true;
                        HideFile(fileToModify, metaAttributes);
                    }

                    string content = File.ReadAllText(fileToModify);

                    bool containsModify = false;
                    foreach (var (originalGuid, newGuid) in guidTable)
                    {
                        var oldContent = content.ToString();
                        content = content.Replace(originalGuid, newGuid);
                        if (content != oldContent) containsModify = true;
                    }

                    if (containsModify) File.WriteAllText(fileToModify, content.ToString());
                    if (isHidden) UnhideFile(fileToModify, metaAttributes);
                }
            }

            catch (Exception error)
            {
                Debug.LogError(error);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void HideFile(string path, FileAttributes attributes)
        {
            attributes &= ~FileAttributes.Hidden;
            File.SetAttributes(path, attributes);
        }

        private static void UnhideFile(string path, FileAttributes attributes)
        {
            attributes |= FileAttributes.Hidden;
            File.SetAttributes(path, attributes);
        }
    }

    public class Av3OrganizeItem
    {

        public Type ItemType;
        //public bool Includes;
        public string FolderName;
        public string IconName;
        public string Name;
        public Av3OrganizeInputType InputType;

        //public Av3OrganizeItem(Type itemType, string folderName, bool includes = true, string name = "", string iconName = "")
        public Av3OrganizeItem(Type itemType, string folderName, Av3OrganizeInputType inputType = Av3OrganizeInputType.Copy, string name = "", string iconName = "")
        {
            ItemType = itemType;
            //Includes = includes;
            InputType = inputType;
            FolderName = folderName;
            IconName = iconName;
            if (string.IsNullOrEmpty(name)) Name = itemType.Name;
            else Name = name;
        }
    }

    public enum Av3OrganizeInputType
    {
        Copy,
        Move,
        Ignore
    }
}