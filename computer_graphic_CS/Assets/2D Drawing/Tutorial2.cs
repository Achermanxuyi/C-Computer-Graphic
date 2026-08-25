using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tutorial2 : MonoBehaviour
{
    Texture2D texture;
    Color[] background;

    public Vector4[] hexagonVertices;

    public Vector4 offset = new Vector4(0, 0, 0, 0);

    public Matrix4x4 R;

    private Vector2 hexCenter;

    // Awake is called when the game object is initialised
    // This is perhaps not the best way to setup a full-screen quad but it is sufficient for our purposes
    private void Awake()
    {
        // set up the camera to look perfectly at the quad (which we are about to make)
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 360;
        Camera.main.rect = new Rect(0, 0, 1, 1);

        // create and postion a quad
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        surface.transform.localPosition = new Vector3(0, 0, 0);
        surface.transform.localRotation = Quaternion.identity;
        surface.transform.localScale = new Vector3(1280, 720, 1);

        // create a texture to display on the quad (that we can also draw into directly)
        texture = new Texture2D(1280, 720);
        texture.filterMode = FilterMode.Point;
        surface.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
        surface.GetComponent<Renderer>().material.mainTexture = texture;
        background = texture.GetPixels();
    }

    // Start is called before the first frame update
    void Start()
    {
        // nothing to do here
        int centerX = texture.width / 2;
        int centerY = texture.height / 2;

        int radius = 100;
        int sides = 6; // Hexagon  

        hexagonVertices = new Vector4[sides];

        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI / sides * i;
            float x = centerX + radius * Mathf.Cos(angle);
            float y = centerY + radius * Mathf.Sin(angle);

            hexagonVertices[i] = new Vector4(x, y, 0, 1);
        }

        Vector2 center = Vector2.zero;
        foreach (var vertex in hexagonVertices)
        {
            center += new Vector2(vertex.x, vertex.y);
        }
        center /= hexagonVertices.Length;

        hexCenter = center;
    }

    // Update is called once per frame
    void Update()
    {
        // this needs to happen _before_ your code
        texture.SetPixels(background); // DO NOT MOVE OR DELETE

        // *******************************************************
        // Write your Tutorial2 code here
        // *******************************************************

        // Animate offset
        offset.x += 1f;
        offset.y += 0.5f;

        // for Scale
        float scale = 1f + Time.time * 0.1f;

        Matrix4x4 S = Matrix4x4.identity;
        S.SetRow(0, new Vector4(scale, 0, 0, 0));
        S.SetRow(1, new Vector4(0, scale, 0, 0));

        // Translate hexagon to origin
        Matrix4x4 T1 = Matrix4x4.identity;
        T1.SetRow(0, new Vector4(1, 0, 0, -hexCenter.x));
        T1.SetRow(1, new Vector4(0, 1, 0, -hexCenter.y));

        // for Rotation
        float cos = Mathf.Cos(Time.time * 0.5f);
        float sin = Mathf.Sin(Time.time * 0.5f);

        R = Matrix4x4.identity;
        R.SetRow(0, new Vector4(cos, -sin, 0, 0));
        R.SetRow(1, new Vector4(sin, cos, 0, 0));


        // Translate back to center
        Matrix4x4 T2 = Matrix4x4.identity;
        T2.SetRow(0, new Vector4(1, 0, 0, hexCenter.x));
        T2.SetRow(1, new Vector4(0, 1, 0, hexCenter.y));

        // Combine transformations
        Matrix4x4 T = T2 * R * S * T1;

        for (int i = 0; i < hexagonVertices.Length; i++)
        {
            Vector4 p1 = hexagonVertices[i];
            Vector4 p2 = hexagonVertices[(i + 1) % hexagonVertices.Length];

            Vector4 tp1 = T * p1;
            Vector4 tp2 = T * p2;

            // Draw line between p1 and p2
            texture.DrawDDALine(
                Mathf.RoundToInt(tp1.x),
                Mathf.RoundToInt(tp1.y),
                Mathf.RoundToInt(tp2.x),
                Mathf.RoundToInt(tp2.y),
                Color.black
            );
        }

        // this needs to happen _after_ your code
        texture.Apply(); // DO NOT MOVE OR DELETE
    }
}



