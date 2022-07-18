using UnityEngine;
using UnityEditor;
using System;

namespace RiClothes {
    public class MissingRemover: MonoBehaviour {

        [MenuItem("DevloliSDK/Tools/MissingRemover")]
        static void Run() {
            EditorWindow.GetWindow<MissingRemoverGUI> (true, "MissingRemover");
        }
        
    }

    public class MissingRemoverGUI: EditorWindow {
        GameObject avatar = null;

        void OnGUI() {
            avatar = EditorGUILayout.ObjectField("Avatar", avatar, typeof(GameObject), true) as GameObject;

            GUILayout.Space(16);
            #if UNITY_2019_1_OR_NEWER
                GUILayout.Label("Missing Remover");
                GUILayout.Space(16);
            #endif

            GUI.backgroundColor = Color.grey;
            if(GUILayout.Button("Run")) {
                if(avatar != null) {
                    #if UNITY_2018
                        Run2018();
                    #endif
                    #if UNITY_2019_1_OR_NEWER
                        Run2019();
                    #endif
                }
            }
        }

        void Run2019() {
            try {
                PrefabUtility.UnpackPrefabInstance(avatar, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                } catch (ArgumentException) {
            }
            RemoveMissingScript_2019(avatar);
            Close();
        }

        void RemoveMissingScript_2019(GameObject gameObject) {

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

            if(gameObject.transform.childCount > 0) {
                for(int i = 0; i < gameObject.transform.childCount; i++) {
                    RemoveMissingScript_2019(gameObject.transform.GetChild(i).gameObject);
                }
            }
        }

        void Run2018() {
            try {
                PrefabUtility.UnpackPrefabInstance(avatar, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                } catch (ArgumentException) {
            }

            RemoveMissingScript_2018(avatar);
            EditorApplication.ExecuteMenuItem("Edit/Play");
            Close();
        }

        void RemoveMissingScript_2018(GameObject gameObject) {
            Component[] components = gameObject.GetComponents<Component>();
            int count = 0;
            for(int i = 0;i < components.Length; i++) {
                Component component = components[i];
                if(component == null) {
                    SerializedObject sObject = new SerializedObject(gameObject);
                    SerializedProperty property = sObject.FindProperty("m_Component");
                    property.DeleteArrayElementAtIndex(i - count);
                    count++;
                    sObject.ApplyModifiedProperties();
                }
            }

            if(gameObject.transform.childCount > 0) {
                for(int i = 0; i < gameObject.transform.childCount; i++) {
                    RemoveMissingScript_2018(gameObject.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    
}