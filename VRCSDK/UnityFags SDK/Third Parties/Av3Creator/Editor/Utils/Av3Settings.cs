#region
using Av3Creator.AdvancedToggles;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Utils
{
    [Serializable]
    public class QuickToggleData
    {
        public MultiToggleCell[] Elements = new MultiToggleCell[]
            {
                new MultiToggleCell()
            };

        public string Name = "Toggle";
        public bool SaveParameter = true;
        [NonSerialized] public string Parameter;

        [NonSerialized] public string DefaultAnimPath;
        [NonSerialized] public string ToggledAnimPath;
    }

    [Serializable]
    public class MultiToggleCell
    {
        [SerializeField] public GameObject Object = null;
        [SerializeField] public bool Enabled = true;

        [SerializeField] public (Material oldMaterial, Material newMaterial)[] Materials;

        public MultiToggleCell(GameObject source = null, bool defaultState = true)
        {
            Object = source;
            Enabled = defaultState;
        }
    }


    [Serializable]
    public class Av3Settings
    {
        [SerializeField] Av3SerializedSettings _serializedSettings;
        internal Av3SerializedSettings SerializedSettings
        {
            get
            {
                if (_serializedSettings == null) _serializedSettings = ScriptableObject.CreateInstance<Av3SerializedSettings>();
                return _serializedSettings;
            }
        }

        //===========>  SETTINGS  <===========//
        public QuickToggleSettings QTSettings = new QuickToggleSettings();
        public Av3OrganizeSettings OrganizeSettings = new Av3OrganizeSettings();

        //===========>  SETTINGS  <===========//
        public bool QuickToggleExpanded;
        public bool PresetCreatorExpanded;
        public bool OtherOptionsExpanded;
        public bool ProjectOrganizerExpanded;
        public bool EasyHUEShifterExpanded;

        //===========>  AVATARS  <===========//
        public string OutputDirectory;
    }

    [Serializable]
    public class QuickToggleSettings
    {
        public bool SettingsExpanded = false;
        public bool WriteDefaults = true;
        public bool Overwrite = true;
        public bool CustomMenuName = false;
        public string MenuName = "Toggles";
        public bool SyncMenuStates = false;
        public bool AddToMainMenu = true;
        public bool DisableCredits = false;
        public bool PingFolder = true;
    }

    [Serializable]
    public class Av3OrganizeSettings
    {
        public string CustomPath = "";
        public bool CreatePrefabInstance = false;
        public UnityEngine.Object[] IgnoreList;
        public string SceneName = "OPEN ME";

        public bool SettingsExpanded = false;
        public bool ExportListExpanded = false;
    }

    public static class Av3Layers
    {
        // Yes, i know enums, but i do class cause it is more beauty in the code :V
        public static int Base = 0;
        public static int Additive = 1;
        public static int Gesture = 2;
        public static int Action = 3;
        public static int FX = 4;
    }

    internal class Av3SerializedSettings : ScriptableObject
    {
        internal SerializedObject SerializedSettings
        {
            get
            {
                if (_serializedSettings == null)
                    _serializedSettings = new SerializedObject(this);
                return _serializedSettings;
            }
        }

        [SerializeField] private SerializedObject _serializedSettings;

        public UnityEngine.Object[] Organizer_IgnoreList;

        [SerializeField] public QuickToggleData[] QuickToggleList = new QuickToggleData[] { new QuickToggleData() };
        [SerializeField] public List<Av3AdvancedToggle> AdvancedToggles = new List<Av3AdvancedToggle>();

        private SerializedProperty _toggleObjectsProperty;
        internal SerializedProperty QuickToggleData
        {
            get
            {
                if (_toggleObjectsProperty == null)
                    GetProperty(nameof(QuickToggleList), out _toggleObjectsProperty);
                return _toggleObjectsProperty;
            }
        }

        public bool GetProperty(string propertyName, out SerializedProperty property)
        {
            property = null;
            if (SerializedSettings == null) return false;

            property = SerializedSettings.FindProperty(propertyName);
            return property != null;
        }
    }
}