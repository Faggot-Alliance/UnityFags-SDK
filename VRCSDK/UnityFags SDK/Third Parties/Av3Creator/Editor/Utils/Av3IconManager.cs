#region
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Utils
{
    [InitializeOnLoad, ExecuteInEditMode]
    public class Av3IconManager
    {
        public static Texture2D DiscordLogo;
        public static Texture2D TrelloLogo;
        public static Texture2D PatreonLogo;

        static Av3IconManager()
        {
            var discordLogoPath = AssetDatabase.GUIDToAssetPath("f8f5d62b1f272c942a104b390913568b");
            DiscordLogo = LoadTexture(discordLogoPath);

            var trelloLogoPath = AssetDatabase.GUIDToAssetPath("c7909e8bba3d5e74b89de22a1046b243");
            TrelloLogo = LoadTexture(trelloLogoPath);

            var patreonLogoPath = AssetDatabase.GUIDToAssetPath("0883572c88ea0574c9b6561695f059ca");
            PatreonLogo = LoadTexture(patreonLogoPath);
        }


        public static Texture2D LoadTexture(string path)
            => string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}