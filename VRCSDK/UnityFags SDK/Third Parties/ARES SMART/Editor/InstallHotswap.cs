#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class InstallHotswap
{
    private static readonly string TempFolderPath = Path.GetTempPath();
    private static readonly string ProjectPath = Application.dataPath;

    static InstallHotswap()
    {
        if (SessionState.GetBool("ARESInstallationUseCustomSDK", false))
        {
            CompilationPipeline.compilationFinished += OnCompilationFinishedCustomSDK;
        }
        if (!File.Exists(ProjectPath + "/VRCSDK/Plugins/VRCSDK3A.dll"))
        {
            if (SessionState.GetBool("ARESInstallationAskSDK", true))
            {
                SessionState.SetBool("ARESInstallationAskSDK", false);
                bool customSDK = EditorUtility.DisplayDialog("ARES HSB Setup", "Do you want to use the original VRChat SDK (+FACS Tools),\nor a custom SDK?\n", "Original", "Custom");
                if (!customSDK)
                {
                    string customSDKpath = EditorUtility.OpenFilePanelWithFilters("ARES HSB Setup - Custom SDK", "", new string[] { "Unity Package", "unitypackage", "All files", "*" });
                    if (!string.IsNullOrEmpty(customSDKpath) && customSDKpath.EndsWith(".unitypackage"))
                    {
                        SessionState.SetBool("ARESInstallationUseCustomSDK", true);
                        CustomSDK(customSDKpath);
                        return;
                    }
                }
            }
        } else SessionState.SetBool("ARESInstallationAskSDK", false);


        if (SessionState.GetBool("ARESInstallationUseCustomSDK", false)) return;

        if (File.Exists(ProjectPath + "/VRCSDK/Plugins/VRCSDK3A.dll"))
        {
            SessionState.SetBool("ARESInstallHotswap", false);
            if (SessionState.GetBool("ARESafterInstallation", false))
            {
                CompilationPipeline.compilationFinished += OnCompilationFinished;
            }
        }
        else if (SessionState.GetBool("ARESInstallHotswap", true))
        {
            SessionState.SetBool("ARESInstallHotswap", false);
            Run();
        }
    }
    private static void CustomSDK(string customSDKpath)
    {
        AssetDatabase.ImportPackage(customSDKpath, false);
        EditorUtility.ClearProgressBar();
        //FACS01 was here ewe
    }
    public static void OnCompilationFinishedCustomSDK(object value)
    {
        if (File.Exists(ProjectPath + "/VRCSDK/Plugins/VRCSDK3A.dll"))
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinishedCustomSDK;
            ARESCubeSpawn();
            SessionState.SetBool("ARESafterInstallationClose", true);
        }
    }
    private static void Run()
    {
        EditorUtility.DisplayProgressBar("ARES Installing Hotswap", "Fetching latest VRC SDK3", 0.0f);

        string vrcSDKs = "";
        try
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                vrcSDKs = wc.DownloadString(new Uri("https://api.vrchat.cloud/api/1/config"));
            }
        }
        catch (WebException)
        {
            ErrorMessage("An error occurred while fetching latest VRC SDK3. Internet down?");
            return;
        }
        catch (NotSupportedException)
        {
            ErrorMessage("Unexpected error occurred while fetching latest VRC SDK3.");
            return;
        }

        EditorUtility.DisplayProgressBar("ARES Installing Hotswap", "Parcing latest VRC SDK3", 0.2f);
        string pattern = @"https:\/\/files\.vrchat\.cloud\/sdk\/VRCSDK3-AVATAR-(([0-9]+\.*)+)_Public.unitypackage";
        Regex rg = new Regex(pattern);
        MatchCollection matchedSDKURL = rg.Matches(vrcSDKs);
        if (!(matchedSDKURL.Count == 1))
        {
            ErrorMessage("Couldn't parse latest VRC SDK3 version.", 2);
            return;
        }
        string SDK3URL = matchedSDKURL[0].Value;
        string SDK3Ver = matchedSDKURL[0].Groups[1].Value;

        string SDK3Filepath = Path.Combine(TempFolderPath, "SDK3Avatars_" + SDK3Ver + ".unitypackage");
        if (!File.Exists(SDK3Filepath))
        {
            EditorUtility.DisplayProgressBar("ARES Installing Hotswap", $"Downloading latest VRC SDK3 (v{SDK3Ver})", 0.4f);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    wc.DownloadFile(new Uri(SDK3URL), SDK3Filepath);
                }
            }
            catch (WebException)
            {
                ErrorMessage($"An error occurred while downloading latest VRC SDK3 (v{SDK3Ver}). Internet down?");
                return;
            }
            catch (NotSupportedException)
            {
                ErrorMessage($"Unexpected error occurred while downloading latest VRC SDK3 (v{SDK3Ver}).");
                return;
            }
		}
		
		string CustomPrevImagesFilepath = Path.Combine(TempFolderPath, "VRC Custom Preview Images.unitypackage");
        if (!File.Exists(CustomPrevImagesFilepath))
        {
            EditorUtility.DisplayProgressBar("ARES Installing Hotswap", "Downloading FACS Custom Preview Images", 0.6f);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    wc.DownloadFile(new Uri("https://github.com/FACS01-01/FACS_Utilities/raw/main/Plugins/VRC%20Custom%20Preview%20Images.unitypackage"), CustomPrevImagesFilepath);
                }
            }
            catch (WebException)
            {
                ErrorMessage($"An error occurred while downloading FACS Custom Preview Images. Internet down?", 1);
            }
            catch (NotSupportedException)
            {
                ErrorMessage($"Unexpected error occurred while downloading FACS Custom Preview Images.", 1);
            }
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.ImportPackage(SDK3Filepath, false);
		try{
		AssetDatabase.ImportPackage(CustomPrevImagesFilepath, false);
		} catch {}

        var tmp = 0;
        
        SessionState.SetInt("ARESPreOnCompilationFinished", tmp);
        SessionState.SetBool("ARESafterInstallation", true);

        EditorUtility.DisplayProgressBar("ARES Installing Hotswap", "please wait", 1.0f);
        EditorUtility.ClearProgressBar();
    }

    private static void ErrorMessage(string msg, int retry = 0)
    {
        Debug.LogError(msg);
        EditorUtility.ClearProgressBar();
        if (retry == 1) return;
        else if (retry == 0)
        {
            if (EditorUtility.DisplayDialog("ARES Hotswap Installation Failed", msg + "\n\nRetry?", "Yes", "No"))
            {
                if (Directory.Exists(ProjectPath + "/VRCSDK"))
                {
                    Directory.Delete(ProjectPath + "/VRCSDK");
                }
                Run();
            }
            else
            {
                if (Directory.Exists(ProjectPath + "/VRCSDK"))
                {
                    Directory.Delete(ProjectPath + "/VRCSDK");
                }
            }
        }
        else
        {
            EditorUtility.DisplayDialog("ARES Hotswap Installation Failed", msg, "Ok", "Also Ok");
            if (Directory.Exists(ProjectPath + "/VRCSDK"))
            {
                Directory.Delete(ProjectPath + "/VRCSDK");
            }
        }
    }
    private static void ARESCubeSpawn()
    {
        var ARESCube = GameObject.Find("ARES Cube");
        if (!ARESCube)
        {
            ARESCube = UnityEngine.Object.Instantiate(Resources.Load("ARES Cube Prefab") as GameObject);
            ARESCube.name = "ARES Cube";
        }
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARES SMART/Resources/ARESLogoMat.mat");
        Shader vrcshader = Shader.Find("VRChat/Mobile/Toon Lit");
        if (vrcshader) mat.shader = vrcshader;
    }
    public static void OnCompilationFinished(object value)
    {
        int tmp = SessionState.GetInt("ARESPreOnCompilationFinished", 0);
        if (File.Exists(ProjectPath + "/VRCSDK/Plugins/VRCSDK3A.dll"))
        {
            if (tmp > 0)
            {
                SessionState.SetInt("ARESPreOnCompilationFinished", tmp-1);
                return;
            }
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            ARESCubeSpawn();
            SessionState.SetBool("ARESafterInstallationClose", true);
        }
    }
}
#endif