// Code written by Gemini 2.5 Pro
// With the following starting code: https://gist.github.com/elaberge/36e43c1f459ee36cde64dc35bf54c312

// C# Editor Script
// Save this script as "Matrix4x4Drawer.cs" inside a folder named "Editor" in your Assets folder.
// This combines the grid layout with TRS decomposition.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Matrix4x4))]
public class Matrix4x4Drawer : PropertyDrawer
{
    private const float CellHeight = 18f; // A bit more padding
    private const float CellPadding = 2f;
    private const int Rows = 4;
    private const int Cols = 4;

    // We store the matrix property names here to avoid string allocation in OnGUI
    private static readonly string[,] s_PropertyNames = new string[Rows, Cols];

    // Dictionary to store the foldout state for the "Info" section of each property.
    private static readonly Dictionary<string, bool> s_InfoFoldouts = new Dictionary<string, bool>();

    // Static constructor to initialize the property names once
    static Matrix4x4Drawer()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                // Correct property names are e<row><col>
                s_PropertyNames[r, c] = $"e{r}{c}";
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Base height for the grid and the Info foldout.
        float height = (CellHeight * (Rows + 1)) + (CellPadding * 2);

        // Add height for the TRS fields if the Info foldout is expanded
        string key = property.propertyPath;
        if (s_InfoFoldouts.ContainsKey(key) && s_InfoFoldouts[key])
        {
            height += (CellHeight * 4) + (CellPadding * 4); // 4 read-only fields
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // --- Draw Label and First Row ---
        // Use PrefixLabel to draw the label and get the rect for the content on the same line.
        var lineRect = new Rect(position.x, position.y, position.width, CellHeight - CellPadding);
        var contentRect = EditorGUI.PrefixLabel(lineRect, label);

        var matrix = Matrix4x4.identity;
        var cellWidth = (contentRect.width - (CellPadding * (Cols - 1))) / Cols;

        // Draw the first row of the matrix
        for (int c = 0; c < Cols; c++)
        {
            var cellRect = new Rect(contentRect.x + c * (cellWidth + CellPadding), contentRect.y, cellWidth, CellHeight - CellPadding);
            var cellProp = property.FindPropertyRelative(s_PropertyNames[0, c]);
            EditorGUI.PropertyField(cellRect, cellProp, GUIContent.none);
            matrix[0, c] = cellProp.floatValue;
        }

        // --- Draw Remaining Rows ---
        // These rows are drawn on subsequent lines, aligned with the contentRect from above.
        for (int r = 1; r < Rows; r++)
        {
            lineRect.y += CellHeight;
            for (int c = 0; c < Cols; c++)
            {
                var cellRect = new Rect(contentRect.x + c * (cellWidth + CellPadding), lineRect.y, cellWidth, CellHeight - CellPadding);
                var cellProp = property.FindPropertyRelative(s_PropertyNames[r, c]);
                EditorGUI.PropertyField(cellRect, cellProp, GUIContent.none);
                matrix[r, c] = cellProp.floatValue;
            }
        }

        // --- Draw "Info" Foldout ---
        lineRect.y += CellHeight + CellPadding;
        // Use a rect with a slight indent for the foldout
        var infoRect = new Rect(position.x + 15f, lineRect.y, position.width - 15f, CellHeight);

        string key = property.propertyPath;
        if (!s_InfoFoldouts.ContainsKey(key)) s_InfoFoldouts[key] = false;

        s_InfoFoldouts[key] = EditorGUI.Foldout(infoRect, s_InfoFoldouts[key], "Matrix Info", true);

        if (s_InfoFoldouts[key])
        {
            // --- Draw TRS Decomposition (Read-only) ---
            GUI.enabled = false;
            // Indent the content of the foldout
            var infoContentRect = EditorGUI.IndentedRect(infoRect);
            infoContentRect.y += CellHeight;

            EditorGUI.Vector3Field(infoContentRect, "Translation", matrix.GetColumn(3));

            infoContentRect.y += CellHeight;
            Quaternion rotation = matrix.rotation;
            EditorGUI.Vector3Field(infoContentRect, "Rotation (Euler)", rotation.eulerAngles);

            infoContentRect.y += CellHeight;
            EditorGUI.Vector4Field(infoContentRect, "Rotation (Quaternion)", new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));

            infoContentRect.y += CellHeight;
            EditorGUI.Vector3Field(infoContentRect, "Scale", matrix.lossyScale);

            GUI.enabled = true;
        }

        EditorGUI.EndProperty();
    }
}
