using UnityEditor;
using UnityEngine;

namespace Navigation
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Save the current GUI enabled state
            GUI.enabled = false;

            // Draw the property as usual
            EditorGUI.PropertyField(position, property, label);

            // Restore the GUI enabled state
            GUI.enabled = true;
        }
    }

}