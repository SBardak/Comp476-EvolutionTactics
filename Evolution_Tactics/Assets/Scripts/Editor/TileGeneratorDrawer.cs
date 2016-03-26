using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TileGeneratorInspector))]
public class TileGeneratorDrawer: PropertyDrawer {

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        Rect codeRect = new Rect(position.x, position.y, 20, position.height);
        Rect typeRect = new Rect(position.x + 25, position.y, 50, position.height);
        Rect occRect = new Rect(position.x + 80, position.y, 30, position.height);
        Rect nameRect = new Rect(position.x + 115, position.y, position.width - 95, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(codeRect, property.FindPropertyRelative("charCode"), GUIContent.none);
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("Type"), GUIContent.none);
        //EditorGUI.IntSlider(occRect, property.FindPropertyRelative("occurence"), 0, 100, GUIContent.none);
        EditorGUI.PropertyField(occRect, property.FindPropertyRelative("occurence"), GUIContent.none);
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("Name"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
