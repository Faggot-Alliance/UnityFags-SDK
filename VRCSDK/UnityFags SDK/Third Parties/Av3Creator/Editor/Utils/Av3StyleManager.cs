#region
using System;
using UnityEditor;
using UnityEngine;
#endregion

namespace Av3Creator.Utils
{
    public class Av3Styles
    {
        public Av3Styles() => InitializeStyles();
        private void InitializeStyles()
        {
            // Label Colors
            ErrorLabel.normal.textColor = new Color(1, 0, 0, 0.65f);
            WarningLabel.normal.textColor = new Color(1, 1, 0, 0.65f);
            GreenLabel.normal.textColor = new Color(0, 1, 0);
            YellowLabel.normal.textColor = new Color(1, 0.7f, 0);

            BoxSelected.normal.background = MakeTexture(new Color(0.0f, 0.5f, 1f, 0.5f));

        }

        Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }


        public readonly GUIStyle ErrorLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            richText = true,
            wordWrap = true,
            fontSize = 11
        };

        public readonly GUIStyle WarningLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            richText = true,
            wordWrap = true,
            fontSize = 11
        };

        public readonly GUIStyle GreenLabel = new GUIStyle();
        public readonly GUIStyle YellowLabel = new GUIStyle();

        public readonly GUIStyle ContentView = new GUIStyle()
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(10, 10, 10, 10)
        };

        public readonly GUIStyle Padding5 = new GUIStyle()
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(5, 5, 0, 0)
        };

        public readonly GUIStyle AlignToCenter = new GUIStyle()
        {
            margin = new RectOffset(10, 10, 10, 0)
        };

        public readonly GUIStyle SettingsMargin = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(25, 0, 0, 0)
        };

        public readonly GUIStyle SettingsDescription = new GUIStyle(GUI.skin.label)
        {
            padding = new RectOffset(20, 0, 0, 0),
            richText = true,
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };

        public readonly GUIStyle RightLabelField = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true,
            margin = new RectOffset(0, 0, 5, 5),
            alignment = TextAnchor.MiddleRight
        };

        public readonly GUIStyle LeftLabelField = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true,
            margin = new RectOffset(0, 0, 5, 5),
            alignment = TextAnchor.MiddleLeft,
            stretchWidth = false
        };

        public readonly GUIStyle RemovePaddingAndMargin = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
            stretchHeight = false,
            stretchWidth = false
        };

        public readonly GUIStyle Header = new GUIStyle("box") { stretchWidth = true };
        public readonly GUIStyle CenteredText = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, richText = true };

        public readonly GUIStyle VersionHeader = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            margin = new RectOffset(5, 5, 0, 0)
        };

        public readonly GUIStyle Description = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true,
            margin = new RectOffset(5, 5, 0, 0)
        };

        public readonly GUIStyle CenteredDescription = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true,
            margin = new RectOffset(5, 5, 0, 0),
            alignment = TextAnchor.MiddleCenter
        };

        public readonly GUIStyle BoldFoldout = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        public readonly GUIStyle Foldout = new GUIStyle(EditorStyles.foldout)
        {
        };


        public readonly GUIStyle ToggleLeft = new GUIStyle(EditorStyles.label)
        {
            margin = new RectOffset(5, 5, 0, 0)
        };

        public readonly GUIStyle Button = new GUIStyle()
        {
            margin = new RectOffset(5, 5, 0, 0)
        };

        public readonly GUIStyle MiddleLeftButton = new GUIStyle("Button")
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 5, 5),
            fixedHeight = 26
        };

        public readonly GUIStyle TogglePadding = new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(5, 0, 0, 2)
        };

        public readonly GUIStyle AdvancedToggleBox = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public readonly GUIStyle BoxEven = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(3, 3, 3, 3),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public readonly GUIStyle BoxOdd = new GUIStyle()
        {
            padding = new RectOffset(3, 3, 3, 3),
            margin = new RectOffset(0, 0, 0, 0)
        };

        // Advanced Toggle
        public readonly GUIStyle AdvancedToggle_Settings = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(10, 0, 0, 0)
        };

        public readonly GUIStyle ModuleBox = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 2, 2),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public readonly GUIStyle BoxNormal = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public readonly GUIStyle BoxSelected = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public readonly GUIStyle ShurikenFoldout = new GUIStyle("ShurikenModuleTitle")
        {
            font = new GUIStyle(EditorStyles.boldLabel).font,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f)
        };

        public readonly GUIStyle CreditsText = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            wordWrap = true,
            richText = true,
            margin = new RectOffset(5, 5, 0, 0)
        };
    }

    public class Av3Icons
    {
        public readonly GUIContent OpenFolder = new GUIContent("", "Open Folder")
        {
            image = EditorGUIUtility.IconContent("d_FolderOpened Icon").image,
        };
    }

    public static class Av3StyleManager
    {
        private static Av3Styles _styles;
        public static Av3Styles Styles
        {
            get
            {
                if (_styles == null) _styles = new Av3Styles(); // Initialize Av3Styles
                return _styles;
            }
        }

        private static Av3Icons _icons;
        public static Av3Icons Icons
        {
            get
            {
                if (_icons == null) _icons = new Av3Icons();
                return _icons;
            }
        }

        public static void DrawFoldout(string title, ref bool variable, Action action, bool isBold = true, bool toggleLabelOnClick = true)
        {
            variable = EditorGUILayout.Foldout(variable, title, toggleLabelOnClick, isBold ? Av3StyleManager.Styles.BoldFoldout : Av3StyleManager.Styles.Foldout);

            if (variable) action();

        }

        public static Rect FoldoutWithToggle(ref bool isExpanded, ref bool toggleValue, string header, Action removeAction = null)
        {
            var rect = GUILayoutUtility.GetRect(16, 22, Styles.ShurikenFoldout);
            rect = EditorGUI.IndentedRect(rect);
            GUI.Box(rect, "", Styles.ShurikenFoldout);
            var toggleRect = new Rect(rect.x + 2, rect.y + 2f, 13, 13);
            Event e = Event.current;
            if (e.type == EventType.Repaint) EditorStyles.foldout.Draw(toggleRect, false, false, isExpanded, false);
         

            var style = new GUIStyle(GUI.skin.toggle)
            {
                padding = new RectOffset(20, 0, 0, 4)
            };


            var lastRect = GUILayoutUtility.GetLastRect();
            var newRect = new Rect(lastRect);
            newRect.x += 20;
            newRect.y += 1;
            newRect.width = 15;
            newRect.height -= 4;

            toggleValue = GUI.Toggle(newRect, toggleValue, "", style);
            var labelRect = new Rect(newRect);
            var labelContent = new GUIContent(header);
            var size = GUI.skin.label.CalcSize(labelContent);
            labelRect.x += 16;
            labelRect.width = size.x;
            labelRect.height = size.y;

            GUI.Label(labelRect, new GUIContent(header));
            Rect? removeButton = null;
            if (removeAction != null)
            {
                removeButton = new Rect(lastRect.width - 12, lastRect.y + 2, 20, 20);
                var removeIcon = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove Toggle");
                if (GUI.Button(removeButton.Value, removeIcon, new GUIStyle("RL FooterButton")))
                {
                    removeAction();
                }
            }

            if (e.type == EventType.MouseDown && (removeButton.HasValue && !removeButton.Value.Contains(e.mousePosition)) && (toggleRect.Contains(e.mousePosition) || rect.Contains(e.mousePosition)) && !e.alt)
            {
                isExpanded = !isExpanded;
                e.Use();
            }
            return lastRect;
        }

        public static void DrawLabel(string text, string description = null, int padding = 5, GUIStyle style = null, float height = 0)
        {
            if (height <= 0) height = EditorGUIUtility.singleLineHeight;

            var labelContent = new GUIContent(text, description);
            if (style == null) EditorGUILayout.LabelField(labelContent, GUILayout.MaxWidth(GUI.skin.label.CalcSize(labelContent).x + padding), GUILayout.Height(height));
            else EditorGUILayout.LabelField(labelContent, style, GUILayout.MaxWidth(GUI.skin.label.CalcSize(labelContent).x + padding), GUILayout.Height(height));
        }


        public static bool ToggleLeft(bool targetVariable, string text, string description = null, int padding = 5)
        {
            var labelContent = new GUIContent(text, description);
            return EditorGUILayout.ToggleLeft(labelContent, targetVariable, GUILayout.MaxWidth(GUI.skin.toggle.CalcSize(labelContent).x + padding));
        }

        public static void DrawOption(string title, ref bool variable, Action action, int margin = 5)
        {
            variable = Foldout(title, variable, margin);
            if (variable)
                action();
        }

        public static bool Foldout(string title, bool display, int margin = 0)
        {
            GUIStyle style = new GUIStyle("ShurikenModuleTitle")
            {
                margin = new RectOffset(margin, margin, 5, 0),
                font = new GUIStyle(EditorStyles.boldLabel).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f)
            };
            Rect rect = GUILayoutUtility.GetRect(16f, 22, style);
            GUI.Box(rect, title, style);

            Event e = Event.current;
            if (e.type == EventType.Repaint) EditorStyles.foldout.Draw(new Rect(rect.x + 2, rect.y + 2, 13, 13), false, false, display, false);

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            return display;
        }

        public static void DrawLine(int height = 1, int spacement = 5)
        {
            EditorGUILayout.Space(spacement);
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
            EditorGUILayout.Space(spacement);
        }

        public static void DrawIssueBox(MessageType msgType, string message, System.Action FixAction = null)
        {
            GUIStyle style = new GUIStyle("HelpBox");
            var oldRichTextOption = EditorStyles.helpBox.richText;
            EditorStyles.helpBox.richText = true; // Allow html tags
            using (new EditorGUILayout.HorizontalScope())
            {

                GUIContent c = new GUIContent(message);
                float height = style.CalcHeight(c, style.fixedWidth);
                var minHeight = GUILayout.MinHeight(Mathf.Max(40, height));
                Rect rt = GUILayoutUtility.GetRect(c, style, minHeight);
                EditorGUI.HelpBox(rt, message, msgType);    // note: EditorGUILayout resulted in uneven button layout in this case

                var buttonContent = new GUIContent("Auto Fix");
                var buttonWidth = GUILayout.Width( GUI.skin.button.CalcSize(buttonContent).x + 10);

                if (FixAction != null && GUILayout.Button(buttonContent, buttonWidth, minHeight)) FixAction();

            }
            EditorStyles.helpBox.richText = oldRichTextOption;
        }

        public static void DrawError(string error, int padding = 0)
        {
            Styles.ErrorLabel.padding = new RectOffset(padding, padding, 0, 0);
            GUILayout.Label(error, Styles.ErrorLabel);
        }

        public static void DrawWarning(string error, int padding = 0)
        {
            Styles.WarningLabel.padding = new RectOffset(padding, padding, 0, 3);
            GUILayout.Label(error, Styles.WarningLabel);
        }
    }

}