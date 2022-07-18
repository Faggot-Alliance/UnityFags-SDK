#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;


using UnityEditor.AnimatedValues;
using UnityEngine.Events;



namespace GabSith.PasswordGen
{

    [CustomEditor(typeof(Password))]
    public class PasswordEditor : Editor
    {
        List<int> passwordDigits = new List<int>();

        public AnimationClip proxy;

        float loadProgress;

        bool lockMenuRequirements = true;

        bool digits8 = false;
        bool digits0 = false;
        bool digitsCon = false;
        bool digitsLength = false;
        bool noMemory = false;

        SerializedObject so;
        SerializedProperty propAvatar;
        SerializedProperty propPasswordToEdit;
        SerializedProperty propBaseMenu;
        SerializedProperty propLockAnimation;

        AnimBool drop;
        AnimBool dropLockAnim;

        public Texture header;
        public Texture2D lockIcon;

        bool isInPlayMode;

        readonly string direct = "Assets/Password Lock - GabSith/Assets/Generated/";
        readonly string templateDirect = "Assets/Password Lock - GabSith/Assets/Template/";
        readonly string backupDirect = "Assets/Password Lock - GabSith/Assets/Backups/";

        string menuDir;

        List<string> menuItemsList = new List<string> {  };
        List<VRCExpressionsMenu> menuItems = new List<VRCExpressionsMenu> { };

        private void OnEnable()
        {
            so = serializedObject;
            propAvatar = so.FindProperty("descriptor");
            propPasswordToEdit = so.FindProperty("passwordToEdit");
            propBaseMenu = so.FindProperty("passwordBaseMenu");
            propLockAnimation = so.FindProperty("lockAnimation");

            drop = new AnimBool(((Password)target).extrasFold);
            drop.valueChanged.AddListener(new UnityAction(base.Repaint));

            dropLockAnim = new AnimBool(((Password)target).extrasFold);
            dropLockAnim.valueChanged.AddListener(new UnityAction(base.Repaint));


            if (Application.isPlaying) isInPlayMode = true;
            else isInPlayMode = false;


            menuItemsList.Add("Main Menu");

            if (((Password)target).gameObject.GetComponent<VRCAvatarDescriptor>() != null)
                ((Password)target).descriptor = ((Password)target).gameObject.GetComponent<VRCAvatarDescriptor>();


            RefreshMenuLists();


            if (ScriptFunctions.HasMixedWriteDefaults(((Password)target).descriptor) == ScriptFunctions.WriteDefaults.Off)
                ((Password)target).useWriteDefaults = false;
            else
                ((Password)target).useWriteDefaults = true;
        }


        public override void OnInspectorGUI()
        {
            Rect re = new Rect(20, 15, Screen.width - 30, 100);
            GUILayout.Space(120);
            EditorGUI.DrawPreviewTexture(re, header, null, ScaleMode.ScaleAndCrop, 0f);


            GUIStyle buttonPass = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 35 };
            GUIStyle discordSpam = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                hover = { textColor = Color.red },
                normal = { textColor = Color.grey },
                alignment = TextAnchor.MiddleRight,
                margin = { right = 10, bottom = 5 },

            };
            GUIStyle buttonExtras = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 35 };



            // Play Mode Warning
            if (isInPlayMode)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("\n\n  Exit Play mode to use this tool\n\n", MessageType.Error, true);
                GUILayout.Space(20);
                return;
            }


            if (!AssetDatabase.IsValidFolder("Assets/Password Lock - GabSith/Assets/Template"))
            {
                EditorGUILayout.HelpBox("Lock directory was not found! Make sure you don't move or delete the folder containing the lock's assets before installing!", MessageType.Error, true);
                EditorGUILayout.HelpBox("If you want better organization you can move the assets after setting the the password up", MessageType.Info, true);
                GUILayout.Space(30);
                return;
            }



            GUILayout.Space(20);

            so.Update();
            // Avatar slot
            EditorGUILayout.PropertyField(propAvatar, new GUIContent("Avatar", "Drag your avatar to modify here"));

            so.ApplyModifiedProperties();
            if (((Password)target).descriptor == null)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("\n\n  Avatar Descriptor not found!\n\n", MessageType.Error, true);
                GUILayout.Space(20);
                return;
            }

            GUILayout.Space(10);

            if (noMemory)
                EditorGUILayout.HelpBox("You don't have enough free memory in your avatar's Expression Parameters to generate. You need at least 9 bits of parameter memory available.", MessageType.Error, true);

            GUILayout.Space(10);

            WriteDefaultsWarning();

            so.Update();

            GUILayout.Space(15);

            using (new EditorGUILayout.HorizontalScope())
            {
                so.Update();
                EditorGUILayout.PropertyField(propPasswordToEdit, new GUIContent("Password:", "The password. Must be up to 8 digits long, using non-consecutive numbers from 1 to 8."));

                if (GUILayout.Button("Random", GUILayout.MaxWidth(100f)))
                {
                    ((Password)target).passwordToEdit = RandomPassword();
                    CheckPassword();
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Password must be up to 8 digits long, using non-consecutive numbers from 1 to 8", MessageType.Info, true);

            so.ApplyModifiedProperties();
            
            FunnyHahas();
            CheckPassword();


            if (!((Password)target).passwordRequirements)
            {
                if (digitsLength)
                    EditorGUILayout.HelpBox("Password must be 8 digits long at most", MessageType.Error, true);
                if (digits8)
                    EditorGUILayout.HelpBox("Password digits must be 8 at most", MessageType.Error, true);
                if (digits0)
                    EditorGUILayout.HelpBox("Password digits must be 1 at least", MessageType.Error, true);
                if (digitsCon)
                    EditorGUILayout.HelpBox("Password digits cannot be consecutives", MessageType.Error, true);
            }

            if (((Password)target).passwordRequirements)
            {
                passwordDigits.Clear();
                for (int i = 0; i < ((Password)target).passwordToEdit.ToString().Length; i++)
                {
                    passwordDigits.Add(((Password)target).passwordToEdit.ToString()[i] - 48);
                }
            }

            GUILayout.Space(20);

            ((Password)target).alsoLockToggles = EditorGUILayout.ToggleLeft(new GUIContent("Lock Toggles", "By default the password only locks the movement of the avatar, " +
                "when this is on the FX layer will have it's weight set to zero"), ((Password)target).alsoLockToggles);

            GUILayout.Space(10);

            ((Password)target).useLockAnimation = EditorGUILayout.ToggleLeft(new GUIContent("Add a custom lock animation", "Changes the " +
                "animation when the avatar is locked. By dafault, the avatar is only unable to move"), ((Password)target).useLockAnimation);

            dropLockAnim.target = ((Password)target).useLockAnimation;

            using (var group = new EditorGUILayout.FadeGroupScope(dropLockAnim.faded))
            {
                if (group.visible)
                {
                    so.Update();
                    EditorGUILayout.PropertyField(propLockAnimation, new GUIContent(" └   Lock Animation (action layer):", "The animation that will be played when the avatar is locked"));
                    so.ApplyModifiedProperties();
                }
            }
            GUILayout.Space(20);


            using (new EditorGUILayout.HorizontalScope())
            {
                so.Update();
                EditorGUILayout.PropertyField(propBaseMenu, new GUIContent("Add in:", "A 'Lock' submenu will be added in the menu placed here"));

                ((Password)target).menuInt = EditorGUILayout.Popup(((Password)target).menuInt, menuItemsList.ToArray(), GUILayout.MaxWidth(100f));
                if (menuItems.Count != 0)
                    ((Password)target).passwordBaseMenu = menuItems[((Password)target).menuInt];
                else
                    ((Password)target).passwordBaseMenu = null;


                so.ApplyModifiedProperties();

            }
            GUILayout.Space(10);
            
            CheckPasswordMenu();

            GUILayout.Space(10);


            // Password button
            if (((Password)target).descriptor != null)
            {
                GUILayout.Space(15);
                if (!((Password)target).hasPasswordMenu && !lockMenuRequirements || !((Password)target).passwordRequirements)
                    GUI.enabled = false;
                
                
                if (GUILayout.Button(new GUIContent("Apply Password", "Applies the password, with the selected settings above, to the selected avatar"), buttonPass))
                {
                    loadProgress = 0.2f;
                    EditorUtility.DisplayProgressBar("Applying Password", "Creating FX controllers", loadProgress);

                    if (((Password)target).makeBackup)
                        MakeBackup();

                    GeneratePasswordFX();

                    AddLockAnimation();

                    loadProgress = 0.99f;
                    EditorUtility.DisplayProgressBar("Applying Password", "Adding Menu", loadProgress);
                    AddPasswordMenu();

                    EditorUtility.ClearProgressBar();
                }
                GUI.enabled = true;
            }

            GUILayout.Space(25);            

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
            drop.target = ((Password)target).extrasFold;

            EditorGUI.indentLevel++;
            ((Password)target).extrasFold = EditorGUILayout.Foldout(((Password)target).extrasFold, " Extra");
            EditorGUI.indentLevel--;

                using (var group = new EditorGUILayout.FadeGroupScope(drop.faded))
                {
                    if (group.visible)
                    {
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Remove All Components", "Removes all related Parameters, FX layers, submenus"), buttonExtras))
                        {
                            if (EditorUtility.DisplayDialog("Remove Password", "Remove Password from the avatar? (this will remove all related Parameters, FX layers, etc.)", "Yes", "No"))
                                RemovePasswordThings();
                        }
                        if (GUILayout.Button(new GUIContent("Remove Script", "Removes this script from the avatar"), buttonExtras))
                            ((Password)target).DestroyScriptInstance();
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.Separator();

                        ((Password)target).useWriteDefaults = EditorGUILayout.ToggleLeft(new GUIContent("Use Write Defaults"), ((Password)target).useWriteDefaults);

                        ((Password)target).makeBackup = EditorGUILayout.ToggleLeft(new GUIContent("Make Backups"), ((Password)target).makeBackup);


                        EditorGUILayout.Separator();

                        if (!lockMenuRequirements)
                            GUI.enabled = false;
                        if (GUILayout.Button(new GUIContent("Apply Only Lock Submenu", "Removes the 'Lock' submenu from the previous menu and adds it to the new one")))
                        {
                            AddPasswordMenu();
                            RefreshMenuLists();
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent("Remove Lock Submenu", "Finds the lock submenu in the selected avatar and removes it")))
                        {
                            RemovePasswordMenu(((Password)target).descriptor.expressionsMenu);
                            RefreshMenuLists();
                        }

                        EditorGUILayout.Separator();


                        if (!((Password)target).useLockAnimation || ((Password)target).lockAnimation == null)
                            GUI.enabled = false;
                        if (GUILayout.Button(new GUIContent("Apply Only Lock Animation", "Adds a lock animation to the avatar's action layer")))
                        {
                            if (((Password)target).makeBackup)
                                MakeBackup(false, true);

                            ScriptFunctions.UninstallControllerByPrefix(((Password)target).descriptor, "   ", ScriptFunctions.PlayableLayer.Action);
                            AddLockAnimation();
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent("Remove Lock Animation", "Removes only the lock animation from the action layer")))
                        {
                            ScriptFunctions.UninstallControllerByPrefix(((Password)target).descriptor, "   ", ScriptFunctions.PlayableLayer.Action);
                        }

                        EditorGUILayout.Separator();

                        if (GUILayout.Button(new GUIContent("Create Backup", "Creates a backup of the FX and Action layers. " +
                            "This is done automatically when apllying the password, if the 'Make Backups' toggle is set to on")))
                            MakeBackup();

                        if (GUILayout.Button(new GUIContent("Flush Backups", "Deletes all previous backups")))
                        {
                            if (Directory.Exists(backupDirect))
                            {
                                Directory.Delete(backupDirect, true);
                                Directory.CreateDirectory(backupDirect);
                                MakeSureItDoesTheThing(null);
                            }
                        }

                        EditorGUILayout.Separator();

                        if (GUILayout.Button(new GUIContent("Refresh Menu List", "Realoads the list found next to the menu slot. " +
                            "Useful when the avatar's menu has changed and re-selecting it is too much work")))
                        {
                            RefreshMenuLists();
                        }
                        EditorGUILayout.Space(5);
                    }
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button(new GUIContent("Discord", "Problems? Feedback? Here's my discord server"), discordSpam))
                Application.OpenURL("https://discord.gg/uvYW2N4eW9");


            EditorUtility.ClearProgressBar();
        }


        void CheckPassword()
        {
            ((Password)target).passwordRequirements = true;

            digits8 = false;
            digits0 = false;
            digitsCon = false;
            digitsLength = false;
            noMemory = false;

            int prev = 0;

            if (((Password)target).descriptor.expressionParameters != null && ((Password)target).descriptor.expressionParameters.CalcTotalCost() > (256 - 9))
            {
                noMemory = true;
                ((Password)target).passwordRequirements = false;
            }


            if (((Password)target).passwordToEdit.ToString().Length > 8)
            {
                digitsLength = true;
                ((Password)target).passwordRequirements = false;
            }

            for (int i = 0; i < (((Password)target).passwordToEdit.ToString().Length); i++)
            {
                if (((Password)target).passwordToEdit.ToString()[i] - 48 > 8)
                {
                    digits8 = true;
                    ((Password)target).passwordRequirements = false;
                }

                if (((Password)target).passwordToEdit.ToString()[i] - 48 < 1)
                {

                    digits0 = true;
                    ((Password)target).passwordRequirements = false;

                }

                if (((Password)target).passwordToEdit.ToString()[i] != prev)
                {
                    prev = ((Password)target).passwordToEdit.ToString()[i];
                }
                else
                {
                    digitsCon = true;
                    ((Password)target).passwordRequirements = false;
                }
            }

        }
        
        void WriteDefaultsWarning()
        {
            var wd = ScriptFunctions.HasMixedWriteDefaults(((Password)target).descriptor);

            if (wd == ScriptFunctions.WriteDefaults.On)
            {
                EditorGUILayout.HelpBox("This avatar uses write defaults set to on, wich is not recommended by VRChat. " +
                    "The animations' settings will change to avoid problems.", MessageType.Info, true);
                GUILayout.Space(20);
            }
            else if (wd == ScriptFunctions.WriteDefaults.Mixed)
            {
                EditorGUILayout.HelpBox("This avatar uses mixed write defaults settings. " +
                    "This can cause some strange behaviors, such as facial animations getting stuck. " +
                    "\nTo mitigate possible issues, the animations' settings will change.", MessageType.Info, true);
                GUILayout.Space(20);
            }
        }
        
        void Password(AnimatorController control)
        {
            loadProgress = 0.45f;
            EditorUtility.DisplayProgressBar("Applying Password", "Creating Password Animations", loadProgress);

            var root = control.layers[0].stateMachine;

            List<AnimatorState> locks = new List<AnimatorState>();

            for (int i = 0; i < passwordDigits.Count; i++)
            {
                string nameSpaces = new string('‎', i);
                locks.Add(root.AddState("‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎‎" + nameSpaces));
            }

            ChildAnimatorState[] endRef = control.layers[1].stateMachine.states;
            var end = endRef[2].state;

            root.AddState(end, new Vector3(30, 400, 0));
            end.name = "unlocked";
            end.motion = proxy;

            List<AnimatorStateTransition> trans = new List<AnimatorStateTransition>();

            for (int i = 0; i < passwordDigits.Count; i++)
            {
                if (i != passwordDigits.Count - 1)
                    trans.Add(locks[i].AddTransition(locks[i + 1]));
                else
                    trans.Add(locks[i].AddTransition(end));
            }

            List<AnimatorStateTransition> exits = new List<AnimatorStateTransition>();


            for (int i = 0; i < passwordDigits.Count; i++)
            {
                exits.Add(locks[i].AddExitTransition(false));
            }


            if (!((Password)target).useWriteDefaults)
            {
                for (int i = 0; i < locks.Count; i++)
                {
                    locks[i].writeDefaultValues = false;
                    locks[i].motion = proxy;
                }
            }
            else
            {
                for (int i = 0; i < locks.Count; i++)
                {
                    locks[i].writeDefaultValues = true;
                    locks[i].motion = proxy;
                }
            }


            for (int i = 0; i < trans.Count; i++)
            {
                trans[i].duration = 0;
                trans[i].AddCondition(AnimatorConditionMode.Equals, passwordDigits[i], "   ");

                exits[i].duration = 0;
                if (i != 0)
                {
                    exits[i].AddCondition(AnimatorConditionMode.NotEqual, passwordDigits[i - 1], "   ");
                    exits[i].AddCondition(AnimatorConditionMode.NotEqual, 0, "   ");
                    exits[i].AddCondition(AnimatorConditionMode.NotEqual, passwordDigits[i], "   ");
                }
                else
                {
                    exits[i].AddCondition(AnimatorConditionMode.NotEqual, passwordDigits[0], "   ");
                    exits[i].AddCondition(AnimatorConditionMode.NotEqual, 0, "   ");
                }
            }

        }

        void GeneratePasswordFX()
        {

            RemovePasswordThings();

            string dir = direct + ((Password)target).descriptor.name + "/";

            AnimatorController passwordCopy;

            // Password Controller Clone
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            if (((Password)target).alsoLockToggles) {
                AssetDatabase.CopyAsset(templateDirect + "Password Template w locked toggles.controller", dir + "Password Copy.controller");
                passwordCopy = AssetDatabase.LoadAssetAtPath(dir + "Password Copy.controller", typeof(AnimatorController)) as AnimatorController;
            }
            else
            {
                AssetDatabase.CopyAsset(templateDirect + "Password Template.controller", dir + "Password Copy.controller");
                passwordCopy = AssetDatabase.LoadAssetAtPath(dir + "Password Copy.controller", typeof(AnimatorController)) as AnimatorController;

            }


            Password(passwordCopy);


            loadProgress = 0.75f;
            EditorUtility.DisplayProgressBar("Applying Password", "Creating New Controller", loadProgress);


            VRCAvatarDescriptor aviDesc = ((Password)target).descriptor.GetComponent<VRCAvatarDescriptor>();
            ScriptFunctions.MergeController(aviDesc, passwordCopy, ScriptFunctions.PlayableLayer.FX, dir);

            loadProgress = 0.9f;
            EditorUtility.DisplayProgressBar("Applying Password", "Almost done...", loadProgress);


            // Parameters
            VRCExpressionParameters.Parameter
                par1 = new VRCExpressionParameters.Parameter
                { name = "   ", valueType = VRCExpressionParameters.ValueType.Int, saved = false, defaultValue = 0 },
                par2 = new VRCExpressionParameters.Parameter
                { name = "   locked", valueType = VRCExpressionParameters.ValueType.Bool, saved = true, defaultValue = 1 };

            ScriptFunctions.AddParameter(((Password)target).descriptor, par1, dir);
            ScriptFunctions.AddParameter(((Password)target).descriptor, par2, dir);

            AssetDatabase.DeleteAsset(dir + "Password Copy.controller");

            MakeSureItDoesTheThing(null);
        }

        void MakeBackup(bool fx = true, bool action = true)
        {
            string backup = backupDirect + ((Password)target).descriptor.name + "/";

            Directory.CreateDirectory(backup);
            AssetDatabase.Refresh();

            if (((Password)target).descriptor.baseAnimationLayers[4].animatorController != null)
            {
                if (fx)
                {
                    string dir = AssetDatabase.GenerateUniqueAssetPath((backup + ((Password)target).descriptor.baseAnimationLayers[4].animatorController.name + " Backup.controller"));
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(((Password)target).descriptor.baseAnimationLayers[4].animatorController), dir);
                }
            }
            if (((Password)target).descriptor.baseAnimationLayers[3].animatorController != null && (((Password)target).useLockAnimation && (((Password)target).lockAnimation != null)))
            {
                if (action)
                {
                    string dir = AssetDatabase.GenerateUniqueAssetPath((backup + ((Password)target).descriptor.baseAnimationLayers[3].animatorController.name + " Backup.controller"));
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(((Password)target).descriptor.baseAnimationLayers[3].animatorController), dir);
                }
            }

            // Base = 0
            // Additive = 1
            // Gesture = 2
            // Action = 3
            // FX = 4

            MakeSureItDoesTheThing(null);
        }

        void RemovePasswordThings()
        {
            VRCAvatarDescriptor descriptor = ((Password)target).descriptor;

            ScriptFunctions.UninstallControllerByPrefix(descriptor, "   ", ScriptFunctions.PlayableLayer.FX);
            ScriptFunctions.UninstallControllerByPrefix(descriptor, "   ", ScriptFunctions.PlayableLayer.Action);
            ScriptFunctions.UninstallParametersByPrefix(descriptor, "   ");
            RemovePasswordMenu(descriptor.expressionsMenu);
            RefreshMenuLists();
        }


        int RandomPassword()
        {
            List<int> randomNums = new List<int>();

            randomNums.Add(RandomPasswordOptions(100, 999));
            randomNums.Add(RandomPasswordOptions(1000, 9999));
            randomNums.Add(RandomPasswordOptions(10000, 99999));
            randomNums.Add(RandomPasswordOptions(100000, 999999));

            ((Password)target).passwordToEdit = randomNums[Random.Range(0, randomNums.Count)];


            if (CheckRandomRequirements(((Password)target).passwordToEdit))
                return ((Password)target).passwordToEdit;
            else
                return RandomPassword();
        }

        int RandomPasswordOptions(int range1, int range2)
        {
            int rand = Random.Range(range1, range2);

            if (CheckRandomRequirements(rand))
                return rand;
            else
                return RandomPasswordOptions(range1, range2);

        }

        bool CheckRandomRequirements(int randomPass)
        {
            ((Password)target).passwordRequirements = true;
            int prev = 0;

            if (randomPass.ToString().Length > 8)
                return false;

            for (int i = 0; i < randomPass.ToString().Length; i++)
            {
                if (randomPass.ToString()[i] - 48 > 8)
                    return false;

                if (randomPass.ToString()[i] - 48 < 1)
                    return false;

                if (randomPass.ToString()[i] != prev)
                    prev = randomPass.ToString()[i];
                else
                    return false;
            }
            return true;
        }


        void AddPasswordMenu()
        {
            VRCExpressionsMenu lockMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(templateDirect + "lock toCopy.asset");


            if (((Password)target).descriptor.expressionsMenu != null)
                RemovePasswordMenu(((Password)target).descriptor.expressionsMenu);
            else
            {
                ScriptFunctions.AddSubMenu(((Password)target).descriptor, lockMenu.controls[0].subMenu, "Lock", direct + ((Password)target).descriptor.name + "/", null, lockIcon);
            }


            if (((Password)target).passwordBaseMenu != null)
            {
                ((Password)target).passwordBaseMenu.controls.Add(lockMenu.controls[0]);
                ((Password)target).hasPasswordMenu = true;
                Debug.Log("Lock menu added");
            }
            RefreshMenuLists();
            MakeSureItDoesTheThing(((Password)target).passwordBaseMenu);
        }

        void RemovePasswordMenu(VRCExpressionsMenu control)
        {
            if (control != null)
            {
                for (int i = 0; i < control.controls.Count; i++)
                {
                    if (control.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                    {
                        if (control.controls[i].subMenu != null)
                        {
                            if (control.controls[i].name == "Lock")
                            {
                                Debug.Log("Menu Found! Removing");
                                control.controls.RemoveAt(i);

                                ((Password)target).hasPasswordMenu = false;
                                MakeSureItDoesTheThing(control);
                                return;
                            }
                            RemovePasswordMenu(control.controls[i].subMenu);
                        }
                    }
                }
            }
        }

        void CheckPasswordMenu()
        {
            lockMenuRequirements = true;
            bool requiredSpace = true;
            bool slotNotEmply = true;


            if (((Password)target).passwordBaseMenu == null)
            {
                slotNotEmply = false;
            }
            else
            {
                for (int i = 0; i < ((Password)target).passwordBaseMenu.controls.Count; i++)
                {
                    if (((Password)target).passwordBaseMenu.controls.Count >= 8)
                    {
                        lockMenuRequirements = false;
                        requiredSpace = false;
                    }
                }
            }
            if (!requiredSpace && !((Password)target).hasPasswordMenu)
                EditorGUILayout.HelpBox("Not enough space in the selected menu", MessageType.Error, true);
            if (!slotNotEmply)
                EditorGUILayout.HelpBox("Lock submenu will be added in the main menu", MessageType.Info, true);

        }

        void MakeMenuList(VRCExpressionsMenu control)
        {
            for (int i = 0; i < control.controls.Count; i++)
            {
                string menuDirOld = menuDir;

                if (i == 0)
                {
                    if (menuItems.Contains(control))
                        return;

                    menuItemsList.Add(menuDir + "  ▶  " + control.name + "  ◀");
                    menuItems.Add(control);
                }

                if (control.controls[i].name == "Lock")
                {
                    menuItemsList.Add(menuDir + "---Lock is here---");
                    menuItems.Add(control);
                    ((Password)target).hasPasswordMenu = true;
                    continue;
                }

                if (control.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                {
                    if (control.controls[i].subMenu != null)
                    {
                        menuDir += control.controls[i].name + " / ";
                        MakeMenuList(control.controls[i].subMenu);
                    }
                }
                menuDir = menuDirOld;
            }
        }

        void RefreshMenuLists()
        {
            if (((Password)target).descriptor.expressionsMenu != null)
            {
                menuItemsList.Clear();
                menuItems.Clear();
                ((Password)target).hasPasswordMenu = false;
                MakeMenuList(((Password)target).descriptor.expressionsMenu);
            }
        }


        void AddLockAnimation()
        {
            string dir = direct + ((Password)target).descriptor.name + "/";

            if (!((Password)target).useLockAnimation || ((Password)target).lockAnimation == null)
                return;

            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            AssetDatabase.CopyAsset(templateDirect + "Action Template.controller", dir + "Action Copy.controller");
            AnimatorController actionCopy = AssetDatabase.LoadAssetAtPath(dir + "Action Copy.controller", typeof(AnimatorController)) as AnimatorController;

            ChildAnimatorState[] animRef = actionCopy.layers[0].stateMachine.states;
            var anim = animRef[0].state;

            anim.motion = ((Password)target).lockAnimation;

            if (((Password)target).useWriteDefaults)
            {
                for (int i = 0; i < animRef.Length; i++)
                {
                    animRef[i].state.writeDefaultValues = true;
                }
            }

            VRCAvatarDescriptor aviDesc = ((Password)target).descriptor.GetComponent<VRCAvatarDescriptor>();
            ScriptFunctions.MergeController(aviDesc, actionCopy, ScriptFunctions.PlayableLayer.Action, dir);

            AssetDatabase.DeleteAsset(dir + "Action Copy.controller");

            MakeSureItDoesTheThing(null);
        }

        void FunnyHahas()
        {
            // You're not supposed to look over here you cheater!

            if (((Password)target).passwordToEdit.ToString().Length == 1 && ((Password)target).passwordToEdit != 0)
            {
                EditorGUILayout.HelpBox("Not exactly what you'd call secure huh", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 123 || ((Password)target).passwordToEdit == 1234 || ((Password)target).passwordToEdit == 12345)
            {
                EditorGUILayout.HelpBox("come on...", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 69 || ((Password)target).passwordToEdit == 420)
            {
                EditorGUILayout.HelpBox("nice", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 69420)
            {
                EditorGUILayout.HelpBox("my dude... really?", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 666)
            {
                EditorGUILayout.HelpBox("oh god oh fu-", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 1337)
            {
                EditorGUILayout.HelpBox("dead meme", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 911)
            {
                EditorGUILayout.HelpBox("911 what's your emergency?", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 404)
            {
                EditorGUILayout.HelpBox("password not found", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 87)
            {
                EditorGUILayout.HelpBox("was that the bite of 87??", MessageType.Info, true);
            }
            else if (((Password)target).passwordToEdit == 2147483647)
            {
                EditorGUILayout.HelpBox("you really just went over unity's limit huh. did you not see the red warning below?", MessageType.Info, true);
            }
        }

        void MakeSureItDoesTheThing(VRCExpressionsMenu dirtyBoy)
        {
            if (dirtyBoy != null)
                EditorUtility.SetDirty(dirtyBoy);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}


#endif