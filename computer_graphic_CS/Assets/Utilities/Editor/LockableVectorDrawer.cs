// Code 100% written by Gemini 2.5 Pro

// C# Editor Script
// Save this as "LockableVectorDrawer.cs" inside a folder named "Editor" in your Assets folder.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(LockableVectorAttribute))]
public class LockableVectorDrawer : PropertyDrawer
{
    // A dictionary to store the lock state of each property, keyed by its property path.
    private static readonly Dictionary<string, bool> s_LockStates = new Dictionary<string, bool>();

    // Constants for GUI layout
    private const float LockButtonWidth = 20f;
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure this drawer is only used for vector types.
        if (property.propertyType != SerializedPropertyType.Vector2 &&
            property.propertyType != SerializedPropertyType.Vector3 &&
            property.propertyType != SerializedPropertyType.Vector4)
        {
            EditorGUI.LabelField(position, label.text, "Use LockableVector with Vector2, 3, or 4.");
            return;
        }

        // Get the lock state for this specific property.
        string propertyPath = property.propertyPath;
        if (!s_LockStates.ContainsKey(propertyPath))
        {
            s_LockStates[propertyPath] = false;
        }
        bool isLocked = s_LockStates[propertyPath];

        // Begin the property drawing.
        EditorGUI.BeginProperty(position, label, property);

        // The value fields should start where they normally would.
        // The space for this is defined by EditorGUIUtility.labelWidth.
        float valueXStart = position.x + EditorGUIUtility.labelWidth;
        float valueWidth = position.width - EditorGUIUtility.labelWidth;

        // Define the rect for the vector fields. This ensures they align correctly.
        var vectorRect = new Rect(valueXStart, position.y, valueWidth, position.height);

        // The lock button will be placed just to the left of the value fields, inside the label's space.
        var lockButtonRect = new Rect(valueXStart - LockButtonWidth - Spacing, position.y, LockButtonWidth, position.height);

        // The label will take up the remaining space on the left.
        var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - LockButtonWidth - Spacing, position.height);

        // --- Draw the Label ---
        EditorGUI.LabelField(labelRect, label);

        // --- Draw the Lock Button ---
        GUIContent lockIcon = isLocked ? EditorGUIUtility.IconContent("d_Linked") : EditorGUIUtility.IconContent("d_Unlinked");

        if (GUI.Button(lockButtonRect, lockIcon, GUIStyle.none))
        {
            isLocked = !isLocked;
        }
        s_LockStates[propertyPath] = isLocked; // Save the updated state.

        // --- Prepare for Vector Field Drawing ---
        GUIContent[] componentLabels;
        string[] componentPropertyNames;
        int numComponents;

        // Set up arrays based on the vector type.
        switch (property.propertyType)
        {
            case SerializedPropertyType.Vector2:
                numComponents = 2;
                componentLabels = new[] { new GUIContent("X"), new GUIContent("Y") };
                componentPropertyNames = new[] { "x", "y" };
                break;
            case SerializedPropertyType.Vector3:
                numComponents = 3;
                componentLabels = new[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };
                componentPropertyNames = new[] { "x", "y", "z" };
                break;
            default: // Vector4
                numComponents = 4;
                componentLabels = new[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") };
                componentPropertyNames = new[] { "x", "y", "z", "w" };
                break;
        }

        var componentProperties = new SerializedProperty[numComponents];
        var oldValues = new float[numComponents];
        var newValues = new float[numComponents];

        // Get the SerializedProperty and float value for each component.
        for (int i = 0; i < numComponents; i++)
        {
            componentProperties[i] = property.FindPropertyRelative(componentPropertyNames[i]);
            oldValues[i] = componentProperties[i].floatValue;
            newValues[i] = oldValues[i];
        }

        // --- Draw the Vector Field ---
        EditorGUI.BeginChangeCheck();

        // Setting indentLevel to 0 is crucial when using manually calculated rects
        int originalIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUI.MultiFloatField(vectorRect, componentLabels, newValues);

        EditorGUI.indentLevel = originalIndent;

        if (EditorGUI.EndChangeCheck())
        {
            // If the value was changed by the user...
            if (isLocked)
            {
                // Find which component was changed and use it as the master value.
                float masterValue = newValues[0]; // Default to the first component
                for (int i = 0; i < numComponents; i++)
                {
                    if (!Mathf.Approximately(oldValues[i], newValues[i]))
                    {
                        masterValue = newValues[i];
                        break; // Found the changed value
                    }
                }

                // Apply the master value to all components.
                for (int i = 0; i < numComponents; i++)
                {
                    componentProperties[i].floatValue = masterValue;
                }
            }
            else
            {
                // If not locked, just apply the new values directly.
                for (int i = 0; i < numComponents; i++)
                {
                    componentProperties[i].floatValue = newValues[i];
                }
            }
        }

        EditorGUI.EndProperty();
    }
}
