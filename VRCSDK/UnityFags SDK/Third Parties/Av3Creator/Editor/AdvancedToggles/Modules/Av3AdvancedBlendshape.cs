#region
using Av3Creator.Core;
using Av3Creator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.AdvancedToggles.Modules
{
    [Serializable]
    public class Av3AdvancedBlendshape : IAv3AdvancedModule
    {
        public SkinnedMeshRenderer TargetRenderer;

        public string TargetPath;
        public void OnImport() => TargetRenderer = Av3AdvancedSettings.Instance.VRCAvatarDescriptor.gameObject.transform.Find(TargetPath)?.GetComponent<SkinnedMeshRenderer>();

        public bool Expanded;
        public bool IsAnimated = false;
        public float Duration = 0.9f;
        public List<Av3Blendshape> SelectedBlendshapes = new List<Av3Blendshape>();

        public void Generate(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);

            foreach (var blendshape in SelectedBlendshapes)
            {
                var curve = IsAnimated ? AnimationCurve.Linear(0, blendshape.OriginalValue, Duration, blendshape.Value) : new AnimationCurve(new Keyframe(0, blendshape.Value));
                anim.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + blendshape.Name, curve);
            }
        }

        public void GenerateDefault(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);

            foreach (var blendshape in SelectedBlendshapes)
            {     
                var curve = IsAnimated ? AnimationCurve.Linear(0, blendshape.Value, Duration, blendshape.OriginalValue) : new AnimationCurve(new Keyframe(0, blendshape.OriginalValue));
                anim.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + blendshape.Name, curve);   
            }
        }

        private string searchTerm = "";
        public void DrawGUI(Av3AdvancedToggle toggle)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var newTarget = EditorGUILayout.ObjectField(TargetRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
                if (EditorGUI.EndChangeCheck() && TargetRenderer != newTarget)
                {
                    TargetRenderer = newTarget;
                    if (TargetRenderer != null) TargetRenderer.gameObject.FixPath(ref TargetPath);
                }

                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                    toggle.Modules.Remove(this);
            }
 
            using (new EditorGUILayout.HorizontalScope())
            {
                IsAnimated = EditorGUILayout.ToggleLeft("Animated Blendshape", IsAnimated);
                if (IsAnimated)
                {
                    Av3StyleManager.DrawLabel("Animation Duration", "This field will define the duration of the smooth transition.\nTime is in seconds and also acceps non-integer numbers. E.g. 1.5", 0);
                    Duration = EditorGUILayout.FloatField(Duration, GUILayout.Width(50));
                    Av3StyleManager.DrawLabel("s", padding: 0);
                }
            }

            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
            {
                var sizeButton = GUI.skin.button.CalcSize(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"));
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Name", GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("Default", GUILayout.Width(60));
                    EditorGUILayout.LabelField("Toggled", GUILayout.Width(60));
                    EditorGUILayout.LabelField("", GUILayout.Width(sizeButton.x));
                }

                foreach (var blendshape in SelectedBlendshapes.ToList())
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(blendshape.Name, GUILayout.ExpandWidth(true));
                        blendshape.OriginalValue = EditorGUILayout.FloatField(blendshape.OriginalValue, GUILayout.Width(60));
                        blendshape.Value = EditorGUILayout.FloatField(blendshape.Value, GUILayout.Width(60));
                        if (SelectedBlendshapes.Any(x => x.Index == blendshape.Index) && GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), GUILayout.Width(sizeButton.x), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            SelectedBlendshapes.RemoveAll(x => x.Index == blendshape.Index);

                    }
            }

            if (TargetRenderer != null && TargetRenderer.sharedMesh != null && TargetRenderer.sharedMesh.blendShapeCount > 0)
            {
                Expanded = EditorGUILayout.Foldout(Expanded, new GUIContent($"{TargetRenderer.name} Blendshapes"), true);
                if (Expanded)
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle()
                    {
                        padding = new RectOffset(10, 10, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    }))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            Av3StyleManager.DrawLabel("Search", padding: 20);
                            searchTerm = EditorGUILayout.TextField(searchTerm);
                        }

                        for (int i = 0; i < TargetRenderer.sharedMesh.blendShapeCount; i++)
                        {
                            var currentBlendShape = TargetRenderer.sharedMesh.GetBlendShapeName(i);
                          
                            if (string.IsNullOrEmpty(searchTerm) || currentBlendShape.ToLower().Contains(searchTerm.ToLower()))
                            {
                                using (new EditorGUI.DisabledScope(SelectedBlendshapes.Any(x => x.Index == i)))
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    var currentBlendshapeValue = TargetRenderer.GetBlendShapeWeight(i);
                                    TargetRenderer.SetBlendShapeWeight(i, EditorGUILayout.Slider(new GUIContent(currentBlendShape), currentBlendshapeValue, 0, 100f));
                                    
                                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus@2x"), GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                                        SelectedBlendshapes.Add(new Av3Blendshape(i, currentBlendShape, currentBlendshapeValue, 0));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void InitializeModule()
        {
            if (TargetRenderer != null && string.IsNullOrEmpty(TargetPath)) TargetRenderer.gameObject.FixPath(ref TargetPath);
        }
    }
    public class Av3BlendshapeCreator : EditorWindow
    {
        private SkinnedMeshRenderer Source;
        private Av3Blendshape blendshape;
        private Av3AdvancedToggle AdvancedToggle;
        private bool IsAnimated = false;
        public float Duration = 0.9f;

        public void SetParameter(SkinnedMeshRenderer source, Av3Blendshape originalBlendshape, Av3AdvancedToggle toggle = null)
        {
            if (source == null) return;
            Source = source;
            blendshape = originalBlendshape;

            AdvancedToggle = toggle;
        }

        public void OnGUI()
        {
            if (Source == null || blendshape == null)
            {
                Close();
                return;
            }

            using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.ContentView))
            {
                
                IsAnimated = EditorGUILayout.ToggleLeft("Animated Blendshape", IsAnimated, GUILayout.ExpandWidth(false));
                if (IsAnimated)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Duration = EditorGUILayout.FloatField(new GUIContent("Duration"), Duration);
                        Av3StyleManager.DrawLabel("s", padding: 0);
                    }
                
                }
                

                using (new EditorGUILayout.HorizontalScope())
                {
                    Av3StyleManager.DrawLabel("From:", padding: 5);
                    blendshape.OriginalValue = EditorGUILayout.FloatField(blendshape.OriginalValue);

                    Av3StyleManager.DrawLabel("To:", padding: 5);
                    blendshape.Value = EditorGUILayout.FloatField(blendshape.Value);
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Add Blendshape"))
                {
                    var module = new Av3AdvancedBlendshape()
                    {
                        TargetRenderer = Source,
                        SelectedBlendshapes = new List<Av3Blendshape>()
                        {
                            new Av3Blendshape(blendshape.Index, blendshape.Name, blendshape.OriginalValue, blendshape.Value)
                        }, 
                        IsAnimated = IsAnimated,
                        Duration = Duration
                    };

                    if (AdvancedToggle == null)  
                        Av3AdvancedSettings.Instance.AdvancedToggles.Add(new Av3AdvancedToggle()
                        {
                            Name = Source.name,
                            Modules = new List<IAv3AdvancedModule>()
                            {
                                module
                            }
                        });
                    else AdvancedToggle.Modules.Add(module);
                    module.InitializeModule();

                    Av3AdvancedSettings.Save();
                    Close();
                }

            }
        }
    }

}
