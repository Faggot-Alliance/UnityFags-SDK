#region
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using VRC.SDK3.Avatars.Components;
using ReorderableList = UnityEditorInternal.ReorderableList;
using Av3Creator.Windows;
#endregion

namespace Av3Creator.Lists
{
    public class Av3ParameterList
    {
        [SerializeField] private Av3ParameterEditorWindow Av3Window;
        [SerializeField] private ReorderableList reorderableList;
        public Av3ParameterList(Av3ParameterEditorWindow window, SerializedProperty property, string[] headers, float?[] columnWidth = null, float columnSpacing = 10f)
        {
            Av3Window = window;
            reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);
            var colmuns = new List<Column>();

            reorderableList.drawElementCallback = DrawElement(reorderableList, GetColumnsFunc(reorderableList, headers, columnWidth, colmuns), columnSpacing);
            reorderableList.drawHeaderCallback = DrawHeader(reorderableList, GetColumnsFunc(reorderableList, headers, columnWidth, colmuns), columnSpacing);
            reorderableList.onAddCallback = (list) =>
            {
                var parameters = Av3Window.GetSelectedDescriptor()?.expressionParameters;
                if (parameters == null) return;

                var newParams = parameters.parameters.ToList();
                newParams.Add(new ExpressionParameter());

                Av3Window.GetSelectedDescriptor().expressionParameters.parameters = newParams.ToArray();
                EditorUtility.SetDirty(Av3Window.GetSelectedDescriptor().expressionParameters);
                AssetDatabase.SaveAssets();
            };

            reorderableList.onRemoveCallback = (list) =>
            {
                var parameters = Av3Window.GetSelectedDescriptor()?.expressionParameters;
                if (parameters == null) return;
                if (list.index == -1) return;

                var newParams = parameters.parameters.ToList();

                newParams.RemoveAt(list.index);

                Av3Window.GetSelectedDescriptor().expressionParameters.parameters = newParams.ToArray();
                EditorUtility.SetDirty(Av3Window.GetSelectedDescriptor().expressionParameters);
                AssetDatabase.SaveAssets();

                CachedNames.Remove(list.index);
            };

            reorderableList.onReorderCallbackWithDetails += (list, oldIndex, newIndex) =>
            {
                (bool, string) oldCache = (false, null);
                (bool, string) newCache = (false, null);
                if (CachedNames.ContainsKey(oldIndex)) oldCache = CachedNames[oldIndex];
                if (CachedNames.ContainsKey(newIndex)) newCache = CachedNames[newIndex];

                if (newCache.Item2 != null && CachedNames.ContainsKey(oldIndex)) CachedNames[oldIndex] = newCache;
                if (oldCache.Item2 != null && CachedNames.ContainsKey(newIndex)) CachedNames[newIndex] = oldCache;
            };
        }

        public void DrawList()
        {
            int level = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(5, 5, 10, 0)

            };
            Rect rect = GUILayoutUtility.GetRect(100, reorderableList.GetHeight(), style);
            reorderableList.DoList(rect);


            EditorGUI.indentLevel = level;
        }

        public void ClearCache() => CachedNames.Clear();
        [SerializeField] private Dictionary<int, (bool, string)> CachedNames = new Dictionary<int, (bool, string)>();
        private ReorderableList.ElementCallbackDelegate DrawElement(ReorderableList list, System.Func<List<Column>> getColumns, float columnSpacing)
        {
            return (rect, index, isActive, isFocused) =>
            {

                var property = list.serializedProperty;
                var columns = getColumns();
                var layouts = CalculateColumnLayout(columns, rect, columnSpacing);


                var arect = rect;
                arect.height = EditorGUIUtility.singleLineHeight;
                for (var ii = 0; ii < columns.Count; ii++)
                {
                    var c = columns[ii];

                    arect.width = layouts[ii];

                    var item = property.GetArrayElementAtIndex(index);
                    var name = item.FindPropertyRelative("name");
                    var valueType = item.FindPropertyRelative("valueType");
                    var defaultValue = item.FindPropertyRelative("defaultValue");
                    var saved = item.FindPropertyRelative("saved");

                    var myProperty = item.FindPropertyRelative(c.PropertyName);

                    if (CachedNames.TryGetValue(index, out (bool, string) testValue) && string.IsNullOrEmpty(testValue.Item2)) CachedNames.Remove(index);

                    if (c.PropertyName == "name" && !string.IsNullOrEmpty(myProperty.stringValue) && !CachedNames.ContainsKey(index))
                        CachedNames.Add(index, (false, myProperty.stringValue));

                    switch (c.PropertyName)
                    {
                        case "name":
                            {
                                var controlName = "Av3Parameter" + index;
                                GUI.SetNextControlName(controlName);
                                EditorGUI.BeginChangeCheck();
                                var newRect = new Rect(arect.x, arect.y, arect.width - 20, arect.height);
                                EditorGUI.PropertyField(newRect, myProperty, new GUIContent(""));

                                if (EditorGUI.EndChangeCheck() && CachedNames.ContainsKey(index))
                                {
                                    Debug.Log("Changed");
                                    CachedNames[index] = (true, CachedNames[index].Item2);
                                }


                                var newName = myProperty.stringValue;
                            //if (Av3Window.EnableParameterRenaming && (GUI.GetNameOfFocusedControl() == controlName && Event.current.keyCode == KeyCode.Return))
                            //{
                            //    if (CachedNames.TryGetValue(index, out (bool, string) test))
                            //    {
                            //        Debug.Log(test.Item1);
                            //        Debug.Log(test.Item2);
                            //        Debug.Log(newName);
                            //    }
                            //}
                            if (Av3Window.EnableParameterRenaming && (GUI.GetNameOfFocusedControl() != controlName
                                || (GUI.GetNameOfFocusedControl() == controlName && Event.current.keyCode == KeyCode.Return))
                                && CachedNames.TryGetValue(index, out (bool, string) cachedValue) && cachedValue.Item1 && cachedValue.Item2 != newName)
                                {
                                    var descriptor = Av3Window.GetSelectedDescriptor();

                                    void CheckLayer(VRCAvatarDescriptor.CustomAnimLayer currentLayer)
                                    {
                                        if (currentLayer.isDefault) return;

                                        var type = (ExpressionParameters.ValueType)valueType.intValue;
                                        AnimatorControllerParameterType animatorType = 0;

                                        if (type == ExpressionParameters.ValueType.Int) animatorType = AnimatorControllerParameterType.Int;
                                        else if (type == ExpressionParameters.ValueType.Bool) animatorType = AnimatorControllerParameterType.Bool;
                                        else if (type == ExpressionParameters.ValueType.Float) animatorType = AnimatorControllerParameterType.Float;

                                        var controller = (AnimatorController)currentLayer.animatorController;
                                        var parameters = controller.parameters;
                                        var layers = controller.layers;

                                        if (!parameters.Any(x => x.name == cachedValue.Item2 && x.type == animatorType)) return;
                                        var parameter = parameters.First(x => x.name == cachedValue.Item2 && x.type == animatorType);

                                        var oldName = parameter.name;
                                        parameter.name = newName;

                                        void GetSubStateMachinesAndRename(AnimatorStateMachine stateMachine)
                                        {
                                            void RenameTransitionParams(AnimatorTransitionBase[] transitions)
                                            {
                                                if (transitions == null || transitions.Length == 0) return;
                                                foreach (var transition in transitions)
                                                {
                                                    var conditions = transition.conditions.ToArray();
                                                    for (int i = 0; i < conditions.Length; i++)
                                                    {
                                                        var condition = conditions[i];
                                                        if (condition.parameter == oldName)
                                                        {
                                                            conditions[i] = new AnimatorCondition()
                                                            {
                                                                mode = condition.mode,
                                                                parameter = newName,
                                                                threshold = condition.threshold
                                                            };
                                                        }
                                                    }

                                                    transition.conditions = conditions;
                                                }
                                            }

                                            if (stateMachine.defaultState != null)
                                            {
                                                if (stateMachine.defaultState.cycleOffsetParameter == oldName) stateMachine.defaultState.cycleOffsetParameter = newName;
                                                if (stateMachine.defaultState.mirrorParameter == oldName) stateMachine.defaultState.mirrorParameter = newName;
                                                if (stateMachine.defaultState.speedParameter == oldName) stateMachine.defaultState.speedParameter = newName;
                                                if (stateMachine.defaultState.timeParameter == oldName) stateMachine.defaultState.timeParameter = newName;
                                                if (stateMachine.defaultState.transitions != null && stateMachine.defaultState.transitions.Length > 0)
                                                    RenameTransitionParams(stateMachine.defaultState.transitions);
                                            }

                                            RenameTransitionParams(stateMachine.entryTransitions);
                                            RenameTransitionParams(stateMachine.anyStateTransitions);

                                            foreach (var state in stateMachine.states)
                                            {
                                                if (state.state.cycleOffsetParameter == oldName) state.state.cycleOffsetParameter = newName;
                                                if (state.state.mirrorParameter == oldName) state.state.mirrorParameter = newName;
                                                if (state.state.speedParameter == oldName) state.state.speedParameter = newName;
                                                if (state.state.timeParameter == oldName) state.state.timeParameter = newName;
                                                RenameTransitionParams(state.state.transitions);
                                            }
                                            foreach (var newStateMachine in stateMachine.stateMachines) GetSubStateMachinesAndRename(newStateMachine.stateMachine);

                                            EditorUtility.SetDirty(stateMachine);
                                        }

                                        foreach (var layer in layers) GetSubStateMachinesAndRename(layer.stateMachine);


                                        controller.parameters = parameters;
                                        controller.layers = layers;
                                        EditorUtility.SetDirty(controller);
                                    }

                                    foreach (var currentLayer in descriptor.baseAnimationLayers) CheckLayer(currentLayer);
                                    foreach (var currentLayer in descriptor.specialAnimationLayers) CheckLayer(currentLayer);

                                    CachedNames[index] = (false, newName);

                                    EditorUtility.SetDirty(descriptor.expressionParameters);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();

                                }
                                break;
                            }
                        case "valueType":
                            {
                                EditorGUI.PropertyField(arect, myProperty, new GUIContent(""));
                                break;
                            }

                        case "saved":
                            {
                                EditorGUI.PropertyField(new Rect(arect.x + (arect.width / 2) - 7, arect.y + (rect.height / 2) - 7, 14, 14), myProperty, new GUIContent(""));
                                break;
                            }

                        case "defaultValue":
                            {
                                var type = (ExpressionParameters.ValueType)valueType.intValue;
                                switch (type)
                                {
                                    case ExpressionParameters.ValueType.Int:
                                        defaultValue.floatValue = Mathf.Clamp(EditorGUI.IntField(arect, (int)defaultValue.floatValue), 0, 255);
                                        break;
                                    case ExpressionParameters.ValueType.Float:
                                        defaultValue.floatValue = Mathf.Clamp(EditorGUI.FloatField(arect, defaultValue.floatValue), -1f, 1f);
                                        break;
                                    case ExpressionParameters.ValueType.Bool:
                                        defaultValue.floatValue = EditorGUI.Toggle(new Rect(arect.x + (arect.width / 2) - 7, arect.y + (rect.height / 2) - 7, 14, 14), defaultValue.floatValue != 0 ? true : false) ? 1f : 0f;
                                        break;
                                }
                                break;
                            }
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
                if (column.Width.HasValue) autoWidth -= column.Width.Value;
                else autoCount += 1;

            }

            autoWidth -= (columns.Count - 1) * columnSpacing;
            autoWidth /= autoCount;

            var widths = new List<float>(columns.Count);
            foreach (var column in columns)
            {
                if (column.Width.HasValue) widths.Add(column.Width.Value);
                else widths.Add(autoWidth);
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