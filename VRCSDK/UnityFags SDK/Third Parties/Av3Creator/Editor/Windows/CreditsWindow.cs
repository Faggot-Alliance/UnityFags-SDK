#region
using Av3Creator.Utils;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Windows
{
    public class CreditsWindow : EditorWindow
    {
        private static readonly string BannerGUID = "fc863a53b2a5068499dbdfba619dfdb2";

        [MenuItem("UnityFags SDK/Avatar Tools/Av3Creator/Info/Credits", priority = 100)]
        public static void ShowWindow()
        {
            var creditsWindow = CreateWindow<CreditsWindow>("Av3Creator Credits");
            creditsWindow.minSize = new Vector2(386, 470);
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

                EditorGUILayout.LabelField("<b>Av3Creator</b> has made by <b>Rafa#0069</b>\n" +
                    "A project to help our creators grow up and create new things!\n\n" +

                    "<b><i>Here it is some names that need a special thanks:</i></b>\n" +
                    "<b>Cam#1959</b> - Great 3.0 tutorials and a BIG SUPPORT!\n" +
                    "<b>Akami#0066</b> - Being with me since the early stage! ❤\n" +
                    "<b>Dreadrith#3238</b> - Open-source repos I used to study\n" +
                    "<b>Thryrallo#0001</b> - Poiyomi Support\n" +
                    "<b>Creators</b> - Amazing creators that helped me reach more people, together we are building a amazing community!\n" +
                    "<b>Beta Testers</b> - Bug bounty and some amazing ideas!\n" +
                    "<b>You</b> - <b>This project would never be possible without your support, thank you so much!</b>", Av3StyleManager.Styles.CreditsText);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("  Patreon", Av3IconManager.PatreonLogo,
                     "Support this project on patreon, you will receive closed beta access, exclusive WIPs and early access to updates (1 week before them go public!)"),
                     GUILayout.Height(30)))
                    Application.OpenURL("https://www.patreon.com/rafacasari");

                if (GUILayout.Button(new GUIContent("  Discord Server", Av3IconManager.DiscordLogo,
                      "Our discord have more information about the tool, some updates alerts and WIP pics/videos."),
                      GUILayout.Height(30)))
                    Application.OpenURL("https://discord.gg/3ZpeG5yahd");

                if (GUILayout.Button(new GUIContent("  Trello", Av3IconManager.TrelloLogo,
                  "We constantly are updating our trello page with all the features that we are working on."),
                  GUILayout.Height(30)))
                    Application.OpenURL("https://trello.com/b/6ZX55QkS/av3creator");
            }
        }
    }
}
