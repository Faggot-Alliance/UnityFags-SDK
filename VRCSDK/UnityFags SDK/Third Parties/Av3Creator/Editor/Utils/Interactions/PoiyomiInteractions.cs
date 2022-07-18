#region
using HarmonyLib;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Utils.Interactions
{
    // Thanks to Thryrallo#0001 for some help with ThryOptimizer implementation!
    public static class PoiyomiInteractions
    {
        private static readonly Type ShaderOptimizer = AccessTools.TypeByName("Thry.ShaderOptimizer");
        //private static readonly MethodInfo SetLockedForAllMaterials = AccessTools.Method(ShaderOptimizer, "SetLockedForAllMaterials");
        private static readonly MethodInfo LockMethod = AccessTools.Method(ShaderOptimizer, "Lock");
        private static readonly MethodInfo UnlockMethod = AccessTools.Method(ShaderOptimizer, "Unlock");

        //private static bool LockApplyShader(Material material)
        private static readonly MethodInfo _LockApplyShader = AccessTools.Method(ShaderOptimizer, "LockApplyShader", new Type[] {
            typeof(Material)
        });

        //  public static string GetOptimizerPropertyName(Shader shader)
        private static readonly MethodInfo _GetOptimizerPropertyName = AccessTools.Method(ShaderOptimizer, "GetOptimizerPropertyName");

        public static bool IsPoiyomiPresent() => ShaderOptimizer != null;


        private static readonly MethodInfo _IsShaderUsingThryOptimizer = AccessTools.Method(ShaderOptimizer, "IsShaderUsingThryOptimizer");
        public static bool IsShaderUsingThryOptimizer(Shader shader) => (bool)_IsShaderUsingThryOptimizer.Invoke(null, new object[] { shader });

        public static void Lock(this Material material, bool dontApplyShader = true)
        {
            if (ShaderOptimizer == null || LockMethod == null) return;
            AssetDatabase.StartAssetEditing();

            var materialProperties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { material });
            LockMethod.Invoke(null, new object[] {
                material,
                materialProperties,
                dontApplyShader
            });
            AssetDatabase.StopAssetEditing();

            if (dontApplyShader)
            {
                if (LockApplyShader(material))
                {
                    var propertyName = GetOptimizerPropertyName(material.shader);
                    if(!string.IsNullOrEmpty(propertyName))
                        material.SetFloat(propertyName, 1);
                }
               
            }
        }

        public static void Unlock(this Material material, MaterialProperty shaderOptimizer = null)
        {
            if (ShaderOptimizer == null || LockMethod == null) return;

            AssetDatabase.StartAssetEditing();
            UnlockMethod.Invoke(null, new object[]
            {
                material,
                shaderOptimizer
            });

            AssetDatabase.StopAssetEditing();
        }

        public static bool LockApplyShader(Material material) => (bool)_LockApplyShader?.Invoke(null, new object[] { material });
        public static string GetOptimizerPropertyName(Shader shader) => (string)_GetOptimizerPropertyName?.Invoke(null, new object[] { shader });

        public static bool IsPoiyomi(this Material material) => material.shader.name.StartsWith(".poiyomi/")
            || material.shader.name.StartsWith("Hidden/Locked/.poiyomi/")
            || material.shader.name.StartsWith("Hidden/.poiyomi/");
        public static bool IsMaterialLocked(this Material material) => material.shader.name.StartsWith("Hidden/Locked/.poiyomi/") && material.GetTag("OriginalShader", false, "") != "";

    }
}