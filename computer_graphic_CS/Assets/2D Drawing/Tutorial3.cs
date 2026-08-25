using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tutorial3 : MonoBehaviour
{
    Texture2D texture;
    Color[] background;

    // Tower base position
    public int towerBaseX = 200;
    public int towerBaseY = 100;
    public int towerHeight = 400;
    public int towerWidth = 40;

    public int ropeLength = 200; // length of the rope 
    public int ballRadius = 30; // ball radius in pixels
    public float swingAngle = 30f; // max swing angle
    public float swingSpeed = 1f; // swing speed

    public Vector4[] hexagonVertices;
        
    private Vector2 hexCenter;

    public Vector4 offset = new Vector4(0, 0, 0, 0);

    public Matrix4x4 R;


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
        int fulcrumX = towerBaseX + towerWidth / 2;
        int fulcrumY = towerBaseY + towerHeight;

        int fulcrumRadius = 20;
        int fulcrumSides = 6;

        hexagonVertices = new Vector4[fulcrumSides];

        for (int i = 0; i < fulcrumSides; i++)
        {
            float angle = 2 * Mathf.PI / fulcrumSides * i;
            float x = fulcrumRadius * Mathf.Cos(angle);
            float y = fulcrumRadius * Mathf.Sin(angle);

            hexagonVertices[i] = new Vector4(x, y, 0, 1);  // Relative to center (0,0)
        }

        hexCenter = new Vector2(fulcrumX, fulcrumY);


    }

    // Update is called once per frame
    void Update()
    {
        // this needs to happen _before_ your code
        texture.SetPixels(background); // DO NOT MOVE OR DELETE


        texture.DrawFilledRect(towerBaseX, towerBaseY, towerBaseX + towerWidth, towerBaseY + towerHeight, Color.gray);

        int fulcrumX = towerBaseX + towerWidth / 2;
        int fulcrumY = towerBaseY + towerHeight;

        // Draw fulcrum
        int fulcrumSides = 6; // Hexagon
        int fulcrumRadius = 20;

        hexagonVertices = new Vector4[fulcrumSides];

        for (int i = 0; i < fulcrumSides; i++)
        {
            float fulcrumAngle = 2 * Mathf.PI / fulcrumSides * i;
            float x = fulcrumX + fulcrumRadius * Mathf.Cos(fulcrumAngle);
            float y = fulcrumY + fulcrumRadius * Mathf.Sin(fulcrumAngle);

            hexagonVertices[i] = new Vector4(x, y, 0, 1);
        }


       // Draw tower beam
        int beamLength = 400;
        int beamY = towerBaseY + towerHeight;
        texture.DrawFilledRect(towerBaseX, beamY, towerBaseX + beamLength, beamY + 20, Color.gray);

        // Pivot point end of beam 
        int pivotX = towerBaseX + beamLength;
        int pivotY = beamY + 10;

        // swing angle
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        // Convert angle to radians (Chatgpt suggested this)
        float rad = angle * Mathf.Deg2Rad;

        // Ball position from pivot
        int ballX = pivotX + Mathf.RoundToInt(Mathf.Sin(rad) * ropeLength);
        int ballY = pivotY - Mathf.RoundToInt(Mathf.Cos(rad) * ropeLength);

        // Draw rope
        texture.DrawDDALine(pivotX, pivotY, ballX, ballY, Color.black);

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
        Matrix4x4 T = T2 * R * T1;

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

        // Draw ball
        texture.DrawFilledCirclePythagoras(ballX, ballY, ballRadius, Color.red);

        int lift = 2;
        int liftScale = Mathf.RoundToInt(20 * Mathf.Sin(Time.time * 0.5f)) * lift;
        texture.DrawFilledRect(towerBaseX, towerBaseY + towerHeight, towerBaseX + towerWidth / 2, towerBaseY + towerHeight + liftScale, Color.gray);


        // this needs to happen _after_ your code
        texture.Apply(); // DO NOT MOVE OR DELETE
    }
    
}

public static class Texture2DExtensions
{
    public static void DrawFilledCirclePythagoras(this Texture2D texture, int centrex, int centrey, int radius, Color c)
    {
        for (int y = 0; y <= radius; y++)
        {
            int x = Mathf.RoundToInt((float)Math.Sqrt(radius * radius - y * y));
            texture.DrawDDALine(centrex - x, centrey + y, centrex + x, centrey + y, c);
            texture.DrawDDALine(centrex - x, centrey - y, centrex + x, centrey - y, c);
        }
    }

     public static void DrawFilledRect(this Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color c)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            texture.DrawDDALine(xMin, y, xMax, y, c);
        }
    }
}