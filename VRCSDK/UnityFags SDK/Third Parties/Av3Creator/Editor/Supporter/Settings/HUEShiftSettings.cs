#region
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
#endregion

namespace Av3Creator.Supporter.Settings
{
    [Serializable]
    [InitializeOnLoad]
    internal class HUEShiftSettings
    {
        static HUEShiftSettings()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private static void EditorApplication_quitting()
        {
            Debug.Log("Saving");
            Save();
        }

        private static readonly string DATA_KEY = "Av3Creator_" + nameof(HUEShiftSettings);

        private static HUEShiftSettings _instance;
        public static HUEShiftSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HUEShiftSettings();
                    var data = EditorPrefs.GetString(DATA_KEY, JsonUtility.ToJson(_instance, false));
                    JsonUtility.FromJsonOverwrite(data, _instance);
                }

                return _instance;
            }
        }

        public static void Save() => EditorPrefs.SetString(DATA_KEY, JsonUtility.ToJson(Instance, false));


        //public Renderer Renderer = null;
        public List<Renderer> Renderers = new List<Renderer>() { null };
        public List<Material> SelectedMaterials = new List<Material>();

        public string Name;
        public bool WriteDefaults = true;
        public VRCExpressionsMenu TargetMenu; 
        public bool SettingsIsExpanded;
        public bool CommonIsExpanded;
        public bool UncommonIsExpanded;

        public bool Main;
        public bool Decal1;
        public bool Decal2;
        public bool Decal3;
        public bool Decal4;
        public bool BackFace;
        public bool RimLight;
        public bool Matcap;
        public bool Matcap2;
        public bool Emission1;
        public bool Emission2;
        public bool Flipbook;
        public bool Dissolve;
        public bool DissolveEdge;
        public bool Glitter;
        public bool Outline;
    }
}
