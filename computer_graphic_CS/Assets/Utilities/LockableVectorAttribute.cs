// Code 100% written by Gemini 2.5 Pro

// C# Script
// Save this as "LockableVectorAttribute.cs" in any folder EXCEPT an "Editor" folder.

using UnityEngine;

/// <summary>
/// Add this attribute to a Vector2, Vector3, or Vector4 field to add a lock button
/// that enforces uniform values across all components.
/// Example: [LockableVector] public Vector3 myScale;
/// </summary>
public class LockableVectorAttribute : PropertyAttribute { }
