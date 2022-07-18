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
    public class MaterialSwapData
    {
        public int Index;
        public Material MaterialOff;
        public Material MaterialOn;

        public MaterialSwapData(int index, Material defaultMaterial, Material toggledMaterial = null)
        {
            Index = index;
            MaterialOff = defaultMaterial;
            MaterialOn = toggledMaterial;
        }
    }

    [Serializable]
    public class Av3AdvancedMaterialSwap : IAv3AdvancedModule
    {
        public Renderer TargetRenderer;
        public string TargetPath;
        public void OnImport() => TargetRenderer = Av3AdvancedSettings.Instance.VRCAvatarDescriptor.gameObject.transform.Find(TargetPath)?.GetComponent<Renderer>();

        //public Material[] materials;
        [SerializeField] public List<MaterialSwapData> SelectedMaterials = new List<MaterialSwapData>();

        private bool Expanded;

        public void Generate(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);
            var type = TargetRenderer.GetType();

            foreach (var material in SelectedMaterials)
            {
                var property = "m_Materials.Array.data[" + material.Index + "]";

                List<ObjectReferenceKeyframe> frames = new List<ObjectReferenceKeyframe>
                    {
                        new ObjectReferenceKeyframe()
                        {
                            time = 0f,
                            value = material.MaterialOn
                        }
                    };

                EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(path, type, property);
                AnimationUtility.SetObjectReferenceCurve(anim, binding, frames.ToArray());
            }
        }

        public void GenerateDefault(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);
            var type = TargetRenderer.GetType();

            foreach (var material in SelectedMaterials)
            {
                var property = "m_Materials.Array.data[" + material.Index + "]";

                List<ObjectReferenceKeyframe> frames = new List<ObjectReferenceKeyframe>
                    {
                        new ObjectReferenceKeyframe()
                        {
                            time = 0f,
                            value = material.MaterialOff
                        }
                    };

                EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(path, type, property);
                AnimationUtility.SetObjectReferenceCurve(anim, binding, frames.ToArray());
            }
        }

        public void DrawGUI(Av3AdvancedToggle toggle)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Av3StyleManager.DrawLabel("Source");

                EditorGUI.BeginChangeCheck();
                TargetRenderer = (Renderer)EditorGUILayout.ObjectField(TargetRenderer, typeof(Renderer), true);
                if(EditorGUI.EndChangeCheck() && TargetRenderer !=null)
                   TargetRenderer.gameObject.FixPath(ref TargetPath);
                
                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                {
                    toggle.Modules.Remove(this);
                    TargetRenderer = null;
                }
            }

            if (SelectedMaterials == null) SelectedMaterials = new List<MaterialSwapData>();
            foreach (var material in SelectedMaterials.ToList())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    material.MaterialOff = (Material)EditorGUILayout.ObjectField(material.MaterialOff, typeof(Material), false);
                    Av3StyleManager.DrawLabel(">", padding: 0);
                    material.MaterialOn = (Material)EditorGUILayout.ObjectField(material.MaterialOn, typeof(Material), false);
                    if (SelectedMaterials.Any(x => x.Index == material.Index) && GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        SelectedMaterials.RemoveAll(x => x.Index == material.Index);

                }
            }

            if (TargetRenderer != null && TargetRenderer.sharedMaterials.Length > 0)
            {
                Expanded = EditorGUILayout.Foldout(Expanded, new GUIContent($"{TargetRenderer.name} Materials"), true);
                if (Expanded)
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle()
                    {
                        padding = new RectOffset(10, 10, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    }))
                    {

                        foreach (var (material, index) in TargetRenderer.sharedMaterials.Select((material, index) => (material, index)))
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.ObjectField(material, typeof(Material), false);
                                using (new EditorGUI.DisabledScope(SelectedMaterials.Any(x => x.Index == index)))
                                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus@2x"), GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                                        SelectedMaterials.Add(new MaterialSwapData(index, material));
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

    public class Av3MaterialSwapCreator : EditorWindow
    {
        private Renderer Source;
        private int materialIndex = -1;
        private Material offMaterial = null;
        private Material onMaterial = null;
        private Av3AdvancedToggle AdvancedToggle;

        public void SetParameter(Renderer source, Material originalMaterial, int index, Av3AdvancedToggle toggle = null)
        {
            if (source == null) return;
            Source = source;
            materialIndex = index;
            offMaterial = originalMaterial;

            AdvancedToggle = toggle;
        }

        public void OnGUI()
        {
            if (Source == null || materialIndex < 0)
            {
                Close();
                return;
            }

            using (new EditorGUILayout.VerticalScope(Av3StyleManager.Styles.ContentView))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Av3StyleManager.DrawLabel("Default (OFF):", padding: 5);
                    offMaterial = (Material)EditorGUILayout.ObjectField(offMaterial, typeof(Material), true);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    Av3StyleManager.DrawLabel("Toggled (ON):", padding: 5);
                    onMaterial = (Material)EditorGUILayout.ObjectField(onMaterial, typeof(Material), true);
                }

                GUILayout.Space(5);
                using (new EditorGUI.DisabledScope(offMaterial == null || onMaterial == null))
                {
                    if (GUILayout.Button("Add Material Swap"))
                    {
                        if (AdvancedToggle == null)
                        {
                            Av3AdvancedSettings.Instance.AdvancedToggles.Add(new Av3AdvancedToggle()
                            {
                                Name = Source.name + " Swap",
                                Modules = new List<IAv3AdvancedModule>()
                            {
                                new Av3AdvancedMaterialSwap()
                                {
                                    TargetRenderer = Source,
                                    SelectedMaterials = new List<MaterialSwapData>()
                                    {
                                        new MaterialSwapData(materialIndex, offMaterial, onMaterial)
                                    }
                                }
                            }
                            });
                        } else
                        {
                            AdvancedToggle.Modules.Add(new Av3AdvancedMaterialSwap()
                            {
                                TargetRenderer = Source,
                                SelectedMaterials = new List<MaterialSwapData>()
                                    {
                                        new MaterialSwapData(materialIndex, offMaterial, onMaterial)
                                    }
                            });
                        }

                        Av3AdvancedSettings.Save();
                        Close();
                    }
                }
            }
        }
    }
}