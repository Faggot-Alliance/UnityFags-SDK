#region
using Av3Creator.Core;
using Av3Creator.Utils;
using System;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.AdvancedToggles.Modules
{
    [Serializable]
    public class Av3AdvancedToggleObject : IAv3AdvancedModule
    {
        public string TargetPath;
        public GameObject Target;
        public bool ToggleState = true;

        public void OnImport() => Target = Av3AdvancedSettings.Instance.VRCAvatarDescriptor.gameObject.transform.Find(TargetPath)?.gameObject;
        public void Generate(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(Target.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);

            anim.SetCurve(path, typeof(GameObject), "m_IsActive", 
                new AnimationCurve(new Keyframe(0, ToggleState ? 1 : 0)));
        }

        public void GenerateDefault(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(Target.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);

            anim.SetCurve(path, typeof(GameObject), "m_IsActive", 
                new AnimationCurve(new Keyframe(0, ToggleState ? 0 : 1)));
        }

        public void DrawGUI(Av3AdvancedToggle toggle)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                Target = (GameObject)EditorGUILayout.ObjectField(Target, typeof(GameObject), true, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck() && Target != null)
                {
                    Target.FixPath(ref TargetPath);
                    ToggleState = !Target.activeSelf;
                }

                ToggleState = !Av3StyleManager.ToggleLeft(!ToggleState, "Default State");
                Av3StyleManager.DrawLabel(">");
                ToggleState = Av3StyleManager.ToggleLeft(ToggleState, "Toggled State");
                //Av3StyleManager.DrawLabel("Toggled State", "The target state when the toggle is ENABLED");
                //defaultState = EditorGUILayout.Toggle(defaultState, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                    toggle.Modules.Remove(this);
            }


        }

        public void InitializeModule()
        {
            if (string.IsNullOrEmpty(TargetPath)) Target.FixPath(ref TargetPath);
        }
    }
}