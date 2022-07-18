#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace FACSSafeImport
{
    public class SafeImport
    {
        private static int ticks = 0;
        public static void OnUpdate() // workaround for bug in EditorApplication.LockReloadAssemblies
        {
            if (ticks < 500)
            {
                ticks++;
            }
            else
            {
                ticks = 0;
                AssetDatabase.Refresh();
            }
        }

        [InitializeOnLoadMethod]
        public static void ApplySafeModeOnStartup()
        {
            if (PlayerPrefs.GetInt("FACSSafeImport_SafeMode", 0) == 1)
            {
                EditorApplication.LockReloadAssemblies();
                EditorApplication.update += OnUpdate;
                if (SessionState.GetBool("FACSSafeImport_OnFirstLaunch", true)) Debug.Log($"[<color=cyan>FACS Safe Import</color>] Safe Mode Started.");
            }
            else if (SessionState.GetBool("FACSSafeImport_OnFirstLaunch", true))
            {
                Debug.LogWarning($"[<color=cyan>FACS Safe Import</color>] Safe Mode hasn't been started.");
            }
            SessionState.SetBool("FACSSafeImport_OnFirstLaunch", false);
        }

        //

        [MenuItem("UnityFags SDK/Safe Import (FACS)/Start Safe Mode", true)]
        public static bool CanStartSafeMode()
        {
            if (PlayerPrefs.GetInt("FACSSafeImport_SafeMode", 0) == 0)
            {
                return true;
            }
            return false;
        }

        [MenuItem("UnityFags SDK/Safe Import (FACS)/Start Safe Mode")]
        public static void StartSafeMode()
        {
            PlayerPrefs.SetInt("FACSSafeImport_SafeMode", 1);
            EditorApplication.LockReloadAssemblies();
            EditorApplication.update += OnUpdate;
            Debug.Log($"[<color=cyan>FACS Safe Import</color>] Safe Mode Started.");
        }

        //

        [MenuItem("UnityFags SDK/Safe Import (FACS)/Exit Safe Mode", true)]
        public static bool CanExitSafeMode()
        {
            if (PlayerPrefs.GetInt("FACSSafeImport_SafeMode", 0) == 1)
            {
                return true;
            }
            return false;
        }

        [MenuItem("UnityFags SDK/Safe Import (FACS)/Exit Safe Mode")]
        public static void ExitSafeMode()
        {
            PlayerPrefs.SetInt("FACSSafeImport_SafeMode", 0);
            EditorApplication.UnlockReloadAssemblies();
            EditorApplication.update -= OnUpdate;
            Debug.LogWarning($"[<color=cyan>FACS Safe Import</color>] Safe Mode Disabled.");
        }

        //

        [MenuItem("UnityFags SDK/Force Scripts Reload")]
        public static void ReloadScripts()
        {
            if (PlayerPrefs.GetInt("FACSSafeImport_SafeMode", 0) == 0)
            {
                CompilationPipeline.RequestScriptCompilation();
            }
            else
            {
                EditorApplication.UnlockReloadAssemblies();
                CompilationPipeline.RequestScriptCompilation();
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            }
            Debug.Log($"[<color=cyan>FACS Safe Import</color>] Scripts reloading...");
        }

        //

        public static void OnAfterAssemblyReload()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.Refresh();
        }
    }

    public class DetectScriptChanges : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (PlayerPrefs.GetInt("FACSSafeImport_SafeMode", 0) == 0) { return; }

            if (importedAssets.Length > 0)
            {
                List<string> importeds = new List<string>();
                foreach (string asset in importedAssets)
                {
                    if (asset.EndsWith(".cs") || asset.EndsWith(".dll"))
                    {
                        importeds.Add(asset);
                    }
                }
                if (importeds.Count > 0)
                {
                    importeds.Sort();
                    string output = String.Join("\n", importeds);
                    Debug.LogWarning($"[<color=cyan>FACS Safe Import</color>] Some scripts ({importeds.Count}) were added/modified:\n" + output + "\n");
                }
            }

            if (deletedAssets.Length > 0)
            {
                List<string> deleteds = new List<string>();
                foreach (string asset in deletedAssets)
                {
                    if (asset.EndsWith(".cs") || asset.EndsWith(".dll"))
                    {
                        deleteds.Add(asset);
                    }
                }
                if (deleteds.Count > 0)
                {
                    deleteds.Sort();
                    string output = String.Join("\n", deleteds);
                    Debug.LogWarning($"[<color=cyan>FACS Safe Import</color>] Some scripts ({deleteds.Count}) were deleted:\n" + output + "\n");
                }
            }
        }
    }
}
#endif