// Code 100% written by Gemini 2.5 Pro

// C# Editor Script
// Save this script as "Vector4Drawer.cs" inside a folder named "Editor" in your Assets folder.
// Unity will automatically detect it and use it to draw all public Vector4 fields.

using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom PropertyDrawer for Vector4.
/// This drawer displays the Vector4 on a single line in the inspector,
/// similar to how Vector2 and Vector3 are drawn, making it much more
/// compact and readable, especially in lists.
/// </summary>
[CustomPropertyDrawer(typeof(Vector4))]
public class Vector4Drawer : PropertyDrawer
{
    // An array to hold the labels for each component of the vector.
    private static readonly GUIContent[] s_VectorLabels = { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") };

    // An array to hold the float values of the vector's components.
    private readonly float[] m_Values = new float[4];

    /// <summary>
    /// Override this method to make your own GUI for the property.
    /// </summary>
    /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
    /// <param name="property">The SerializedProperty to make the GUI for.</param>
    /// <param name="label">The label of this property.</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Begin the property drawing. This is important for prefab overrides and other editor features.
        label = EditorGUI.BeginProperty(position, label, property);

        // Find the child properties for x, y, z, and w.
        // Using FindPropertyRelative is robust and efficient.
        SerializedProperty x = property.FindPropertyRelative("x");
        SerializedProperty y = property.FindPropertyRelative("y");
        SerializedProperty z = property.FindPropertyRelative("z");
        SerializedProperty w = property.FindPropertyRelative("w");

        // Store the current values from the SerializedProperty into our float array.
        m_Values[0] = x.floatValue;
        m_Values[1] = y.floatValue;
        m_Values[2] = z.floatValue;
        m_Values[3] = w.floatValue;

        // Draw the main label for the property (e.g., "Element 0", "My Vector").
        // The MultiFloatField will be drawn in the remaining space.
        position = EditorGUI.PrefixLabel(position, label);

        // Prevent the field labels (X, Y, Z, W) from being drawn inside the MultiFloatField.
        // We provide our own labels.
        EditorGUI.indentLevel = 0;

        // Use a MultiFloatField to draw the four float fields in a single line.
        // This is the same control Unity uses for Vector2 and Vector3.
        EditorGUI.BeginChangeCheck();
        EditorGUI.MultiFloatField(position, s_VectorLabels, m_Values);
        if (EditorGUI.EndChangeCheck())
        {
            // If any of the float values were changed by the user,
            // update the SerializedProperty with the new values.
            x.floatValue = m_Values[0];
            y.floatValue = m_Values[1];
            z.floatValue = m_Values[2];
            w.floatValue = m_Values[3];
        }

        // End the property drawing.
        EditorGUI.EndProperty();
    }
}