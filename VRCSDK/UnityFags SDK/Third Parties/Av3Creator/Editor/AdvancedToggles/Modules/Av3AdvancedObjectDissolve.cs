#region
using Av3Creator.Core;
using Av3Creator.Utils;
using Av3Creator.Utils.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.AdvancedToggles.Modules
{
    [Serializable]
    public class Av3AdvancedObjectDissolve : IAv3AdvancedModule
    {
        public string TargetPath;
        public Renderer TargetRenderer;
        public void OnImport() => TargetRenderer = Av3AdvancedSettings.Instance.VRCAvatarDescriptor.gameObject.transform.Find(TargetPath)?.GetComponent<Renderer>();

        public bool dissolveSettingsExpanded;
        public Texture2D dissolveNoise;
        // edge
        public float edgeWidth = 0.025f;
        public float edgeHardness = 0f;
        public Color edgeColor = new Color(1f, 1f, 1f);

        public Color dissolvedColor = new Color(0, 0, 0, 0f); //

        public float edgeEmission = 0f;

        public float Duration = 1.2f;

        [SerializeReference] public List<Material> SelectedMaterials = new List<Material>();
        public void InitializeModule()
        {
            if (dissolveNoise == null)
            {
                var defaultNoisePath = AssetDatabase.GUIDToAssetPath("d5e4a521aa43ec742a51e66e8da2871a");
                if (!string.IsNullOrEmpty(defaultNoisePath)) dissolveNoise = AssetDatabase.LoadAssetAtPath<Texture2D>(defaultNoisePath);
            }

            if (TargetRenderer != null && string.IsNullOrEmpty(TargetPath)) TargetRenderer.gameObject.FixPath(ref TargetPath);
        }

        public void Generate(ref AnimationClip anim)
        {
            SetupMaterials();
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);
            var curve = AnimationCurve.Linear(0, 1, Duration, 0);
            var otherCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(Duration, 1f));

            foreach(var material in SelectedMaterials)
            {
                string animPropertySuffix = new string(material.name.Trim().ToLower().Where(char.IsLetter).ToArray());
                anim.SetCurve(path, typeof(Renderer), "material._DissolveAlpha_" + animPropertySuffix, curve);
            }
            anim.SetCurve(path, typeof(GameObject), "m_IsActive", otherCurve);
        }

        public void GenerateDefault(ref AnimationClip anim)
        {
            var path = AnimationUtility.CalculateTransformPath(TargetRenderer.transform, Av3AdvancedSettings.Instance.VRCAvatarDescriptor.transform);

            var curve = AnimationCurve.Linear(0, 0, Duration, 1);
            var otherCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(Duration, 0));

            foreach(var material in SelectedMaterials)
            {
                string animPropertySuffix = new string(material.name.Trim().ToLower().Where(char.IsLetter).ToArray());
                anim.SetCurve(path, typeof(Renderer), "material._DissolveAlpha_" + animPropertySuffix, curve);
            }
            anim.SetCurve(path, typeof(GameObject), "m_IsActive", otherCurve);
        }

        public void DrawGUI(Av3AdvancedToggle toggle)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Av3StyleManager.DrawLabel("Target");

                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    TargetRenderer = (Renderer) EditorGUILayout.ObjectField(TargetRenderer, typeof(Renderer), true);
                    if (c.changed)
                    {
                        //materials = new List<Av3DissolveMaterial>();

                        if (TargetRenderer != null)
                        {
                            SelectedMaterials = SelectedMaterials.Where(x => TargetRenderer.sharedMaterials.Contains(x)).ToList();
                            TargetRenderer.gameObject.FixPath(ref TargetPath);
                            //foreach (var material in TargetRenderer.sharedMaterials)
                            //    if (!materials.Any(x => x.Material == material)) materials.Add(new Av3DissolveMaterial(material, true));
                        }
                    }
                }

                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                    toggle.Modules.Remove(this);
            }

            if (TargetRenderer == null || TargetRenderer?.sharedMaterials == null || TargetRenderer.sharedMaterials.Length == 0) return;

            dissolveSettingsExpanded = EditorGUILayout.Foldout(dissolveSettingsExpanded, "Dissolve Settings", true);
            if (dissolveSettingsExpanded)
            {
                Av3StyleManager.DrawLabel("You can manually edit properties in material after the toggle been created.");
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
                    {
                        Av3StyleManager.DrawLabel("Noise", "Texture noise for dissolve", 20);
                        dissolveNoise = (Texture2D)EditorGUILayout.ObjectField(dissolveNoise, typeof(Texture2D), false, GUILayout.Width(50f), GUILayout.Height(50f));
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(new GUIContent("Duration", "The duration of the dissolve animation in seconds, this also can be non-integer numbers. E.g. 1.6"));
                            Duration = EditorGUILayout.FloatField(Duration);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Edge Width");
                            edgeWidth = EditorGUILayout.Slider(edgeWidth, 0f, 0.5f);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Edge Hardness");
                            edgeHardness = EditorGUILayout.Slider(edgeHardness, 0f, 1f);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Edge Color");
                            edgeColor = EditorGUILayout.ColorField(edgeColor);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Edge Emission");
                            edgeEmission = EditorGUILayout.Slider(edgeEmission, 0, 20);
                        }

                        if (GUILayout.Button(new GUIContent("Apply to others", "Apply the current settings on all other Poiyomi Dissolves in Advanced Toggles")))
                            foreach (var x in Av3AdvancedSettings.Instance.AdvancedToggles.Select(x => x.Modules))
                                foreach (Av3AdvancedObjectDissolve y in x.Where(y => y is Av3AdvancedObjectDissolve))
                                {
                                    if (y == this) continue;
                                    y.Duration = Duration;
                                    y.dissolveNoise = dissolveNoise;
                                    y.edgeWidth = edgeWidth;
                                    y.edgeHardness = edgeHardness;
                                    y.edgeColor = edgeColor;
                                    y.edgeEmission = edgeEmission;
                                }

                    }
                }
                GUILayout.Space(5);
            }

            foreach (var material in TargetRenderer.sharedMaterials.Distinct())
            {
                if (material == null) return;

                var isSelected = SelectedMaterials.Contains(material);
                var isLocked = material.IsMaterialLocked();
                var isPoiyomi = material.IsPoiyomi();
                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    //EditorGUILayout.ObjectField(material.Material, typeof(Material), false);
                    using (new EditorGUI.DisabledScope(!isPoiyomi))
                    using (var c = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUILayout.ToggleLeft(new GUIContent(material.name, "Apply dissolve on this material"), isSelected);
                        if(c.changed)
                        {
                            if (!isSelected) SelectedMaterials.Add(material);
                            else SelectedMaterials.Remove(material);
                        }
                    }
                    using (new EditorGUI.DisabledScope(!isPoiyomi || !isLocked))
                        if (material.IsMaterialLocked() && GUILayout.Button("Unlock", GUILayout.Width(90)))
                            material.Unlock();
                        

                }
            }
            //}
        }

        private void SetupMaterials()
        {
            foreach(var material in SelectedMaterials)
            {
                material.SetFloat("_EnableDissolve", 1f);
                material.EnableKeyword("DISTORT");
                material.SetFloat("_DissolveEdgeWidth", edgeWidth);
                material.SetFloat("_DissolveEdgeHardness", edgeHardness);
                material.SetColor("_DissolveEdgeColor", edgeColor);
                material.SetFloat("_DissolveEdgeEmission", edgeEmission);
                material.SetColor("_DissolveTextureColor", new Color(0, 0, 0, 0f));
                material.SetTexture("_DissolveNoiseTexture", dissolveNoise);
                material.SetOverrideTag("_DissolveAlphaAnimated", "2");
                EditorUtility.SetDirty(material);
            }
        }
    }
}