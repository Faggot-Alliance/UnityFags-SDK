using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Core;
using VRC.SDKBase.Editor;

[ExecuteInEditMode]
    public class UnityFagsDownloadableWindow : EditorWindow
    {
     public static UnityFagsDownloadableWindow window;
    [MenuItem("UnityFags SDK/Import Panel", false, 1)]
    public static void OpenImportPanel()
    {
        GetWindow<UnityFagsDownloadableWindow>(true);
    }

    public static void Open()
    {
        OpenImportPanel();
    }

    public void OnEnable()
    {
        titleContent = new GUIContent("Import Panel");
    }
     
    public const int SdkWindowWidth = 518;
    // Update is called once per frame
    
    void OnGUI()

        {
        
            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.gray;
       
            EditorGUILayout.LabelField("Shaders", EditorStyles.boldLabel);
             GUI.backgroundColor = Color.black;
            if (GUILayout.Button("Download Newest Poiyomi Pro",style, GUILayout.Height(40),GUILayout.Width(200)))
            {

            }
            if (GUILayout.Button("Download Lastest Bloodborne SDK", style, GUILayout.Height(40)))
            {

            }
            if (GUILayout.Button("Doppelgänger's Shaders", style, GUILayout.Height(40)))
            {

            }
            if (GUILayout.Button("Download Newest Poiyomi Pro", style, GUILayout.Height(40)))
            {

            }
            if (GUILayout.Button("Download Newest Poiyomi Pro", style, GUILayout.Height(40)))
            {

            }
            GUI.backgroundColor = Color.red;
    }
    }
