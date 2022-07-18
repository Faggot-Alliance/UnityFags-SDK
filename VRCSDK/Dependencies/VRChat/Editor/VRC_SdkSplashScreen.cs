#define COMMUNITY_LABS_SDK

using UnityEngine;
using UnityEditor;
using System.Net;

namespace VRCSDK2
{
    [InitializeOnLoad]
    public class VRC_SdkSplashScreen : EditorWindow
    {

        static VRC_SdkSplashScreen()
        {
            EditorApplication.update -= DoSplashScreen;
            EditorApplication.update += DoSplashScreen;
        }

        private static void DoSplashScreen()
        {
            EditorApplication.update -= DoSplashScreen;
            if (EditorApplication.isPlaying)
                return;

            #if UDON
                if (!EditorPrefs.GetBool("VRCSDK_ShowedSplashScreenFirstTime", false))
                {
                    OpenSplashScreen();
                    EditorPrefs.SetBool("VRCSDK_ShowedSplashScreenFirstTime", true);
                }
                else
            #endif
                if (EditorPrefs.GetBool("VRCSDK_ShowSplashScreen", true))
                    OpenSplashScreen();
        }

        private static GUIStyle vrcSdkHeader;
        private static GUIStyle vrcSdkLabelCatalog;
        private static GUIStyle vrcSdkBottomHeader;
        private static GUIStyle vrcHeaderLearnMoreButton;
        private static GUIStyle vrcBottomHeaderLearnMoreButton;
        private static Vector2 changeLogScroll;
        [MenuItem("UnityFags SDK/Splash Screen", false, 0)]
        public static void OpenSplashScreen()
        {
            GetWindow<VRC_SdkSplashScreen>(true);
        }
        
        public static void Open()
        {
            OpenSplashScreen();
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("UnityFags SDK");

#if UDON
            maxSize = new Vector2(400, 360);
#else
            maxSize = new Vector2(400, 600);
#endif
            minSize = maxSize;

            vrcSdkHeader = new GUIStyle
            {
                normal =
                    {
#if UDON
                            background = Resources.Load("vrcSdkSplashUdon1") as Texture2D,
#elif COMMUNITY_LABS_SDK
                            background = Resources.Load("vrcSdkHeaderWithCommunityLabs") as Texture2D,
#else
                            background = Resources.Load("vrcSdkHeader") as Texture2D,
#endif
                        textColor = Color.white
                    },
                fixedHeight = 200
            };

            vrcSdkBottomHeader = new GUIStyle
            {
                normal =
                {
#if UDON
                        background = Resources.Load("vrcSdkSplashUdon2") as Texture2D,
#else
                        background = Resources.Load("vrcSdkBottomHeader") as Texture2D,
#endif

                    textColor = Color.white
                },
                fixedHeight = 100
            };

        }

        public void OnGUI()
        {
            GUILayout.Box("", vrcSdkHeader);

                vrcHeaderLearnMoreButton = EditorStyles.miniButton;
                vrcHeaderLearnMoreButton.normal.textColor = Color.black;
                vrcHeaderLearnMoreButton.fontSize = 12;
                vrcHeaderLearnMoreButton.border = new RectOffset(10, 10, 10, 10);
                Texture2D texture = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Texture2D>("UI/Skin/UISprite.psd");
                vrcHeaderLearnMoreButton.normal.background = texture;
                vrcHeaderLearnMoreButton.active.background = texture;
#if UDON
            if (GUI.Button(new Rect(20, 165, 185, 25), "Get Started with Udon", vrcHeaderLearnMoreButton))
                    Application.OpenURL("https://docs.vrchat.com/docs/getting-started-with-udon");
#elif COMMUNITY_LABS_SDK

#endif

#if !UDON
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.black;
            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = Color.gray;

            if (GUILayout.Button("SDK Docs",style))
            {
                Application.OpenURL("https://docs.vrchat.com/");
            }
            if (GUILayout.Button("VRChat FAQ",style))
            {
                Application.OpenURL("https://vrchat.com/developer-faq");
            }
            if (GUILayout.Button("Help Center", style))
            {
                Application.OpenURL("http://help.vrchat.com");
            }
            if(GUILayout.Button("Examples", style))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/vrchat-kits");
            }
            if (GUILayout.Button("Discord", style))
            {
                Application.OpenURL("https://discord.gg/9MFmcsJDVx");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
#endif
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.black;
#if UDON

            if(GUILayout.Button("Udon Examples"))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/examples#udon--sdk3");
            };
#endif
            if (GUILayout.Button("Building VRChat Quest Content",style))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/creating-content-for-the-oculus-quest");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
#if !UDON
            changeLogScroll = GUILayout.BeginScrollView(changeLogScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(395));
            EditorGUILayout.LabelField("Changelog", EditorStyles.boldLabel);
            string NF = "";
            WebClient webClient = new WebClient();
            NF = webClient.DownloadString("https://raw.githubusercontent.com/Faggot-Alliance/UnityFags-SDK/master/api/v1/changelog");
            GUILayout.Label(NF);

            GUILayout.EndScrollView();
#endif

            GUILayout.Space(4);

            //GUILayout.Box("", vrcSdkBottomHeader);
            //vrcBottomHeaderLearnMoreButton = EditorStyles.miniButton;
            //vrcBottomHeaderLearnMoreButton.normal.textColor = Color.black;
            //vrcBottomHeaderLearnMoreButton.fontSize = 10;
            //vrcBottomHeaderLearnMoreButton.border = new RectOffset(10, 10, 10, 10);
            //vrcBottomHeaderLearnMoreButton.normal.background = texture;
            //vrcBottomHeaderLearnMoreButton.active.background = texture;

#if UDON
            if (GUI.Button(new Rect(100, 270, 200, 60), "Join other Creators in our Discord", vrcBottomHeaderLearnMoreButton))
                Application.OpenURL("https://discord.gg/9MFmcsJDVx");
#else
            //if (GUI.Button(new Rect(110, 525, 180, 42), "Click Here to see great\nassets for VRChat creation", vrcBottomHeaderLearnMoreButton))
            //    Application.OpenURL("https://assetstore.unity.com/lists/vrchat-picks-125734?aid=1101l7yuQ");
#endif

            //if (GUI.Button(new Rect(80, 540, 240, 30), "Learn how to create for VRChat Quest!", vrcBottomHeaderLearnMoreButton))
            //{
            //    Application.OpenURL("https://docs.vrchat.com/docs/creating-content-for-the-oculus-quest");
            //}

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            EditorPrefs.SetBool("VRCSDK_ShowSplashScreen", GUILayout.Toggle(EditorPrefs.GetBool("VRCSDK_ShowSplashScreen"), "Show at Startup"));

            GUILayout.EndHorizontal();
        }

    }
}