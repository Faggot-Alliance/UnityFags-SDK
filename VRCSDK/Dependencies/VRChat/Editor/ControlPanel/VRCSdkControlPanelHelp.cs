using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

public partial class VRCSdkControlPanel : EditorWindow
{
    [MenuItem("UnityFags SDK/Help/Developer FAQ")]
    public static void ShowDeveloperFAQ()
    {
        if (!ConfigManager.RemoteConfig.IsInitialized())
        {
            ConfigManager.RemoteConfig.Init(() => ShowDeveloperFAQ());
            return;
        }

        Application.OpenURL(ConfigManager.RemoteConfig.GetString("sdkDeveloperFaqUrl"));
    }

    [MenuItem("UnityFags SDK/Help/VRChat Discord")]
    public static void ShowVRChatDiscord()
    {
        if (!ConfigManager.RemoteConfig.IsInitialized())
        {
            ConfigManager.RemoteConfig.Init(() => ShowVRChatDiscord());
            return;
        }

        Application.OpenURL(ConfigManager.RemoteConfig.GetString("sdkDiscordUrl"));
    }

    [MenuItem("UnityFags SDK/Help/Avatar Optimization Tips")]
    public static void ShowAvatarOptimizationTips()
    {
        if (!ConfigManager.RemoteConfig.IsInitialized())
        {
            ConfigManager.RemoteConfig.Init(() => ShowAvatarOptimizationTips());
            return;
        }

        Application.OpenURL(AVATAR_OPTIMIZATION_TIPS_URL);
    }

    [MenuItem("UnityFags SDK/Help/Avatar Rig Requirements")]
    public static void ShowAvatarRigRequirements()
    {
        if (!ConfigManager.RemoteConfig.IsInitialized())
        {
            ConfigManager.RemoteConfig.Init(() => ShowAvatarRigRequirements());
            return;
        }

        Application.OpenURL(AVATAR_RIG_REQUIREMENTS_URL);
    }
}
