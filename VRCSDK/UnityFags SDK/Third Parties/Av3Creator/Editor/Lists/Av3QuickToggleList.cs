#region
using Av3Creator.Utils;
using Av3Creator.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#endregion

namespace Av3Creator.Lists
{
    public class Av3QuickToggleList
    {
        public Av3QuickToggleList(Av3CreatorWindow creator, SerializedProperty property, string[] headers, float?[] columnWidth = null, float columnSpacing = 10f)
        {
            list = CreateAutoLayout(property, false, true, true, true, headers, columnWidth, columnSpacing);
            this.creator = creator;
        }

        public ReorderableList list;
        public Av3CreatorWindow creator = null;
        public ReorderableList CreateAutoLayout(SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton, string[] headers, float?[] columnWidth = null, float columnSpacing = 10f)
        {
            var list = new ReorderableList(property.serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);
            var colmuns = new List<Column>();

            list.drawElementCallback = DrawElement(list, GetColumnsFunc(list, headers, columnWidth, colmuns), columnSpacing);
            list.drawHeaderCallback = DrawHeader(list, GetColumnsFunc(list, headers, columnWidth, colmuns), columnSpacing);
            list.drawFooterCallback += DrawFooter(list);
            list.elementHeightCallback += (idx) =>
            {
                var myProp = list.serializedProperty.GetArrayElementAtIndex(idx).FindPropertyRelative("Elements");
                var height = EditorGUI.GetPropertyHeight(myProp, GUIContent.none, true);

                if (myProp.isArray)
                    for (int i = 0; i < myProp.arraySize - 1; i++) height += 22;

                if (MultiToggle) height += 18;

                return Mathf.Max(EditorGUIUtility.singleLineHeight, height) + 10f;
            };

            return list;
        }


        public GUIContent iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to the list");
        public GUIContent iconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from the list");
        public GUIContent removeFromToggles = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove GameObject");

        public bool scheduleRemove;
        public bool MultiToggle;
        public ReorderableList.FooterCallbackDelegate DrawFooter(ReorderableList list)
        {
            return (rect) =>
            {

                var toggleRect = new Rect(rect.x + 5, rect.y, 100, 20);
                var toggleStyle = new GUIStyle(EditorStyles.toggle)
                {
                    fontSize = 11,
                    stretchWidth = false,
                    padding = new RectOffset(20, 0, 0, 2)
                };
                MultiToggle = GUI.Toggle(toggleRect, MultiToggle, new GUIContent("Multi Toggle", "Multi toggle is enabled"), toggleStyle);

                GUIStyle preButton = "RL FooterButton";
                GUIStyle footerBackground = "RL Footer";
                float rightEdge = rect.xMax - 10f;
                float leftEdge = rightEdge - 8f;
                leftEdge -= 155;

                rect = new Rect(leftEdge, rect.y, rightEdge - leftEdge, rect.height);

                Rect removeRect = new Rect(rightEdge - 30, rect.y, 25, 16);
                Rect addRect = new Rect(removeRect.x - 30, rect.y, 25, 16);
                Rect textRect = new Rect(addRect.x - 95, rect.y, 90, 16);
                if (Event.current.type == EventType.Repaint) footerBackground.Draw(rect, false, false, false, false);


                if (GUI.Button(addRect, iconToolbarPlus, preButton))
                    DoAddButton();


                if (GUI.Button(removeRect, iconToolbarMinus, preButton))
                    DoRemoveButton();


                GUIStyle preButtonEdited = new GUIStyle(preButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                if (GUI.Button(textRect, "Clear All Toggles", preButtonEdited))
                {
                    var _toggles = creator.toggleObjects.ToList();

                    _toggles.Clear();
                    creator.toggleObjects = _toggles.ToArray();
                    GUI.changed = true;
                }
            };
        }


        public void DoAddButton(params MultiToggleCell[] options)
        {
            var _toggles = creator.toggleObjects.ToList();
            if (options == null || options.Length == 0)
                _toggles.Add(new QuickToggleData()
                {
                    Elements = new MultiToggleCell[]
                    {
                    new MultiToggleCell()
                    },
                    Name = "Toggle " + _toggles.Count,
                    SaveParameter = true
                });
            else _toggles.Add(new QuickToggleData()
            {
                Elements = options,
                Name = options.Length > 1 ? "Toggle " + _toggles.Count : options[0].Object.name
            });

            creator.toggleObjects = _toggles.ToArray();
            GUI.changed = true;
        }

        public void DoRemoveButton()
        {
            if (list.index == -1) return;
            if (list.serializedProperty != null)
            {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                if (list.index >= list.serializedProperty.arraySize - 1)
                    list.index = list.serializedProperty.arraySize - 1;
            }
            else
            {
                list.list.RemoveAt(list.index);
                if (list.index >= list.list.Count - 1)
                    list.index = list.list.Count - 1;
            }
            GUI.changed = true;
        }


        public bool DrawList(string label = null, string description = "")
        {
            var property = list.serializedProperty;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label ?? property.displayName, Av3StyleManager.Styles.BoldFoldout);

            if (property.isExpanded)
            {
                if (!string.IsNullOrEmpty(description)) GUILayout.Label(description, Av3StyleManager.Styles.SettingsDescription);
                if (creator != null)
                {
                    var dragAndDropStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 12,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true
                    };
                    GUILayout.Label("<i><b>PRO TIP</b>: Drag and drop objects below to quickly add them into list.</i>", dragAndDropStyle);
                }

                int level = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                {
                    margin = new RectOffset(15, 15, 0, 0),

                };
                Rect rect = GUILayoutUtility.GetRect(100, list.GetHeight(), style);
                list.DoList(rect);


                EditorGUI.indentLevel = level;
            }

            #region Drag and Drop
            if (creator != null)
            {
                Rect dropRect = GUILayoutUtility.GetLastRect();
                Event evt = Event.current;
                if (evt == null) return true;

                if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropRect.Contains(evt.mousePosition))
                {

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        var toggleObjectsList = creator.toggleObjects.Distinct().ToList();
                        foreach (var dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object.GetType() != typeof(UnityEngine.GameObject))
                                continue;

                            var gameObject = (GameObject)dragged_object;

                            if (toggleObjectsList.Any(x => x.Elements[0].Object == gameObject))
                            {
                                foreach (var selectedToggle in toggleObjectsList.Where(x => x.Elements[0].Object == gameObject))
                                    selectedToggle.Elements[0].Enabled = gameObject.activeSelf;

                            }
                            else if (toggleObjectsList.Any(x => x.Elements[0].Object == null))
                            {
                                var obj = toggleObjectsList.First(x => x.Elements[0].Object == null);
                                obj.Elements[0].Object = gameObject;
                                obj.Elements[0].Enabled = gameObject.activeSelf;
                                obj.Name = gameObject.name;
                            }
                            else
                            {
                                toggleObjectsList.Add(new QuickToggleData()
                                {
                                    Elements = new MultiToggleCell[]
                                    {
                                    new MultiToggleCell()
                                    {
                                        Object = gameObject,
                                        Enabled = gameObject.activeSelf
                                    }
                                    },
                                    Name = gameObject.name,
                                    SaveParameter = true
                                });
                            }

                        }
                        creator.toggleObjects = toggleObjectsList.ToArray();

                        evt.Use();
                    }
                }
            }
            #endregion
            return property.isExpanded;
        }

        private ReorderableList.ElementCallbackDelegate DrawElement(ReorderableList list, System.Func<List<Column>> getColumns, float columnSpacing)
        {
            return (rect, index, isActive, isFocused) =>
            {

                var property = list.serializedProperty;
                var columns = getColumns();
                var layouts = CalculateColumnLayout(columns, rect, columnSpacing);

                GUIStyle preButton = "RL FooterButton";

                var arect = rect;
                arect.height = EditorGUIUtility.singleLineHeight;
                for (var ii = 0; ii < columns.Count; ii++)
                {
                    var c = columns[ii];

                    arect.width = layouts[ii];

                    var myProperty = property.GetArrayElementAtIndex(index).FindPropertyRelative(c.PropertyName);
                    switch (myProperty.name)
                    {
                        case "Name":
                            {
                                myProperty.stringValue = GUI.TextField(new Rect(arect.x, arect.y + (rect.height / 2) - 10, arect.width, 20), myProperty.stringValue);
                                break;
                            }

                        case "SaveParameter":
                            {
                                EditorGUI.PropertyField(new Rect(arect.x + (arect.width / 2) - 7, arect.y + (rect.height / 2) - 7, 14, 14), myProperty, GUIContent.none);
                                break;
                            }

                        case "Elements":
                            {
                                var arrayRect = new Rect(arect.x, arect.y, arect.width - 20, arect.height);
                                for (int arrayIndex = 0; arrayIndex < myProperty.arraySize; arrayIndex++)
                                {
                                    arrayRect.y += 4;
                                    var arrayProperty = myProperty.GetArrayElementAtIndex(arrayIndex);

                                    var toggleRect = new Rect(arrayRect.x, arrayRect.y, 20, 20);
                                    var toggleProperty = arrayProperty.FindPropertyRelative("Enabled");
                                    EditorGUI.PropertyField(toggleRect, toggleProperty, GUIContent.none);

                                    var objectRect = new Rect(arrayRect.x + 20, arrayRect.y, arrayRect.width - 20, arrayRect.height);
                                    var objectProperty = arrayProperty.FindPropertyRelative("Object");
                                    EditorGUI.PropertyField(objectRect, objectProperty, GUIContent.none);

                                    if (arrayIndex > 0 && GUI.Button(new Rect(arrayRect.width + 25, arrayRect.y + 1, 25, 16), removeFromToggles, preButton))
                                    {
                                        myProperty.DeleteArrayElementAtIndex(arrayIndex);
                                        GUI.changed = true;
                                    }
                                    arrayRect.y += 18f;
                                }

                                if (MultiToggle)
                                {
                                    GUIStyle preButtonEdited = new GUIStyle(preButton)
                                    {
                                        alignment = TextAnchor.MiddleLeft,
                                        fontSize = 10,
                                        fontStyle = FontStyle.Bold
                                    };
                                    var content = new GUIContent("+ GameObject", "Add a gameobject to make a toggle with multiple objects handling");
                                    var buttonRect = new Rect(rect.x, rect.y + rect.height - 18, preButton.CalcSize(content).x, 18);

                                    if (GUI.Button(buttonRect, content, preButtonEdited))
                                        myProperty.arraySize += 1;

                                }
                                break;
                            }

                        default:
                            break;
                    }

                    arect.x += arect.width + columnSpacing;
                }
            };
        }

        private ReorderableList.HeaderCallbackDelegate DrawHeader(ReorderableList list, System.Func<List<Column>> getColumns, float columnSpacing)
        {
            return (rect) =>
            {

                var columns = getColumns();

                if (list.draggable)
                {
                    rect.width -= 15;
                    rect.x += 15;
                }

                var layouts = CalculateColumnLayout(columns, rect, columnSpacing);
                var arect = rect;
                arect.height = EditorGUIUtility.singleLineHeight;
                for (var ii = 0; ii < columns.Count; ii++)
                {

                    var c = columns[ii];

                    arect.width = layouts[ii];
                    EditorGUI.LabelField(arect, c.DisplayName);
                    arect.x += arect.width + columnSpacing;
                }

            };
        }

        private System.Func<List<Column>> GetColumnsFunc(ReorderableList list, string[] headers, float?[] columnWidth, List<Column> output)
        {
            var property = list.serializedProperty;
            return () =>
            {
                if (output.Count <= 0 || list.serializedProperty != property)
                {
                    output.Clear();
                    property = list.serializedProperty;

                    if (property.isArray && property.arraySize > 0)
                    {
                        var it = property.GetArrayElementAtIndex(0).Copy();
                        var prefix = it.propertyPath;
                        var index = 0;
                        if (it.Next(true))
                        {
                            do
                            {
                                if (!it.propertyPath.StartsWith(prefix)) break;

                                var c = new Column
                                {
                                    DisplayName = (headers != null && headers.Length > index) ? headers[index] : it.displayName,
                                    PropertyName = it.propertyPath.Substring(prefix.Length + 1),
                                    Width = (columnWidth != null && columnWidth.Length > index) ? columnWidth[index] : null
                                };

                                output.Add(c);


                                index += 1;
                            }
                            while (it.Next(false));
                        }
                    }
                }

                return output;
            };
        }

        private List<float> CalculateColumnLayout(List<Column> columns, Rect rect, float columnSpacing)
        {
            var autoWidth = rect.width;
            var autoCount = 0;
            foreach (var column in columns)
            {
                if (column.Width.HasValue)
                {
                    autoWidth -= column.Width.Value;
                }
                else
                {
                    autoCount += 1;
                }
            }

            autoWidth -= (columns.Count - 1) * columnSpacing;
            autoWidth /= autoCount;

            var widths = new List<float>(columns.Count);
            foreach (var column in columns)
            {
                if (column.Width.HasValue)
                {
                    widths.Add(column.Width.Value);
                }
                else
                {
                    widths.Add(autoWidth);
                }
            }

            return widths;
        }

        private struct Column
        {
            public string DisplayName;
            public string PropertyName;
            public float? Width;
        }
    }
}