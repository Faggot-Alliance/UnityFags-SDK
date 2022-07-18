#region
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using ReorderableList = UnityEditorInternal.ReorderableList;
#endregion

namespace Av3Creator.Lists
{
    public class Av3OrganizerData
    {
        public VRCAvatarDescriptor Descriptor;
        public string Name;
    }

    public class Av3OrganizerList
    {
        private ReorderableList reorderableList;
        private List<Av3OrganizerData> Items;

        public Av3OrganizerList(List<Av3OrganizerData> data)
        {
            Items = data;
            reorderableList = new ReorderableList(data, typeof(Av3OrganizerData), false, true, true, true);

            reorderableList.drawElementCallback = DrawElement;
            reorderableList.drawHeaderCallback += DrawHeader;
        }

        public void DrawList()
        {
            Rect rect = GUILayoutUtility.GetRect(100, reorderableList.GetHeight());
            reorderableList.DoList(rect);

            #region Drag and Drop
            Rect dropRect = GUILayoutUtility.GetLastRect();
            Event evt = Event.current;
            if (evt == null) return;

            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropRect.Contains(evt.mousePosition))
            {

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();


                    foreach (var dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object.GetType() != typeof(GameObject))
                            continue;

                        var gameObject = (GameObject)dragged_object;
                        var descriptor = gameObject.GetComponent<VRCAvatarDescriptor>();
                        if (gameObject == null || descriptor == null) continue;

                        if (Items.Any(x => x.Descriptor == descriptor))
                        {
                            foreach (var selectedToggle in Items.Where(x => x.Descriptor == descriptor))
                                selectedToggle.Name = gameObject.name;

                        }
                        else if (Items.Any(x => x.Descriptor == null))
                        {
                            var obj = Items.First(x => x.Descriptor == null);
                            obj.Descriptor = descriptor;
                            obj.Name = gameObject.name;
                        }
                        else
                        {
                            Items.Add(new Av3OrganizerData()
                            {
                                Descriptor = descriptor,
                                Name = gameObject.name
                            });
                        }

                    }

                    evt.Use();
                }
            }
            #endregion
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Items == null || Items[index] == null) return;
            var currentItem = Items[index];

            var slices = rect.width / 9;
            var separator = 5;

            rect.width = (slices * 6) - (separator / 2);
            EditorGUI.BeginChangeCheck();
            currentItem.Descriptor = (VRCAvatarDescriptor)EditorGUI.ObjectField(rect, currentItem.Descriptor, typeof(VRCAvatarDescriptor), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (currentItem.Descriptor != null)
                    currentItem.Name = currentItem.Descriptor.gameObject.name;
            }
            rect.x += rect.width + separator;
            rect.width = slices * 3 - (separator / 2);
            currentItem.Name = EditorGUI.TextField(rect, currentItem.Name);
        }

        private void DrawHeader(Rect rect)
        {
            var slices = rect.width / 9;
            var separator = 5;

            rect.width = (slices * 6) - (separator / 2);
            EditorGUI.LabelField(rect, "Avatar");
            rect.x += rect.width + separator;
            rect.width = slices * 3 - (separator / 2);
            EditorGUI.LabelField(rect, "Name");
        }
    }
}