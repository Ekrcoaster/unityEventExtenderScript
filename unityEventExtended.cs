/*
    Author: Ekrcoaster
    Date: 7/30/2023

    This script, when placed in an editor folder will add a bunch of new functionality to the UnityEvent in the inspector

    Hover over it to:
    - press "a" to add a blank one
    - press "d" to duplicate selected one
    - drag and drop a game object / script ontop of it to add a new one with that target
*/
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(UnityEventBase), true)]
public class unityEventExtended : UnityEventDrawer {

    // this is the last known selected item, so we can duplicate it
    // something like this is stored internally in the UnityEventDrawer, but idk how to access it
    int lastKnownSelected = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        base.OnGUI(position, property, label);

        // grab the current event
        Event e = Event.current;

        // check if the mouse overlaps myself
        if (e.mousePosition.x > position.x && e.mousePosition.x < position.x + position.width && e.mousePosition.y > position.y && e.mousePosition.y < position.y + position.height) {
            // get the event
            SerializedProperty persistentCalls = property.FindPropertyRelative("m_PersistentCalls.m_Calls");

            //
            // KEYBINDS
            //
            if (e.type == EventType.KeyDown) {
                // the A key should add a brand new element, the same way the + does
                if (e.keyCode == KeyCode.A || (e.keyCode == KeyCode.D && persistentCalls.arraySize == 0)) {
                    CreateNewItem(persistentCalls, null);

                // this should copy the current selected element
                } else if (e.keyCode == KeyCode.D) {
                    int selected = lastKnownSelected;
                    if (selected == -1) selected = persistentCalls.arraySize - 1;
                    DuplicateItem(persistentCalls, selected);
                }
            }

            // if something is being dragged, update the visual
            if (e.type == EventType.DragUpdated) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }

            // once the drag has been dropped, then add the new elements
            if (e.type == EventType.DragPerform) {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
                    CreateNewItem(persistentCalls, DragAndDrop.objectReferences[i]);
                }
            }
        }
    }

    /// <summary>
    /// Here, we just call the default boring old draw event, but if this event is selected, then we store the index
    /// </summary>
    protected override void DrawEvent(Rect rect, int index, bool isActive, bool isFocused) {
        base.DrawEvent(rect, index, isActive, isFocused);
        if (isFocused) lastKnownSelected = index;
    }

    /// <summary>
    /// This will create a new item with the given target
    /// </summary>
    private void CreateNewItem(SerializedProperty property, Object target) {
        // insert the elemenet
        property.InsertArrayElementAtIndex(property.arraySize);

        // configure it
        SerializedProperty justAdded = property.GetArrayElementAtIndex(property.arraySize - 1);
        justAdded.FindPropertyRelative("m_Target").objectReferenceValue = target;
        justAdded.FindPropertyRelative("m_CallState").intValue = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
        justAdded.FindPropertyRelative("m_MethodName").stringValue = "";
        property.serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// This will duplicate the item at the given index
    /// It will not, however duplicate the arguments. for 2 reasons
    ///     - if you are duplicating an event call, its prob cause you want a different argument, so just reset it or whatever
    ///     - im lazy
    /// </summary>
    private void DuplicateItem(SerializedProperty property, int index) {
        SerializedProperty copyFrom = property.GetArrayElementAtIndex(index);

        property.InsertArrayElementAtIndex(property.arraySize);
        SerializedProperty justAdded = property.GetArrayElementAtIndex(property.arraySize - 1);

        justAdded.FindPropertyRelative("m_CallState").intValue = copyFrom.FindPropertyRelative("m_CallState").intValue;
        justAdded.FindPropertyRelative("m_Target").objectReferenceValue = copyFrom.FindPropertyRelative("m_Target").objectReferenceValue;
        justAdded.FindPropertyRelative("m_MethodName").stringValue = copyFrom.FindPropertyRelative("m_MethodName").stringValue;
        justAdded.FindPropertyRelative("m_Mode").intValue = copyFrom.FindPropertyRelative("m_Mode").intValue;

        property.serializedObject.ApplyModifiedProperties();
    }
}
