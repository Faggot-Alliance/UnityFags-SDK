#region
using Av3Creator.Utils;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Windows
{
    public class RulesWindow : EditorWindow
    {
        private static readonly string BannerGUID = "fc863a53b2a5068499dbdfba619dfdb2";

        [MenuItem("UnityFags SDK/Avatar Tools/Av3Creator/Info/Commercial Rules", priority = 100)]
        public static void ShowWindow()
        {
            var creditsWindow = CreateWindow<RulesWindow>("Av3Creator Rules");
            creditsWindow.minSize = new Vector2(386, 380);
            creditsWindow.maxSize = creditsWindow.minSize;
            creditsWindow.Show();
        }

        private Vector2 scrollPos;
        public void OnGUI()
        {
            var bannerPath = AssetDatabase.GUIDToAssetPath(BannerGUID);
            if (!string.IsNullOrEmpty(bannerPath))
            {
                var bannerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
                GUILayout.Label(bannerTexture, new GUIStyle());
            }
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, Av3StyleManager.Styles.ContentView))
            {
                scrollPos = scroll.scrollPosition;

                EditorGUILayout.LabelField("<b>Av3Creator</b> have some rules that you need to follow when selling a avatar, since Av3Creator is a one-time purchase with a cheap price, I hope that you follow this base rules.\n\n" +
                    "1. Do NOT redistribute or resell.\n" +
                    "2. Do NOT claims as your own.\n" +
                    "3. Do NOT attempt to change or modify the script.\n" +
                    "4. Do NOT use any of my code as your own.\n" +
                    "5. Credit Av3Creator in your post or site, preferably with a hyperlink. (E.g.: \"3.0 by Av3Creator\")\n"+
                    "6. <b>DO NOT INCLUDE THE AV3CREATOR PACKAGE IN YOUR AVATAR WHEN YOU SELL IT.</b>", Av3StyleManager.Styles.CreditsText);

            }
        }
    }
}
