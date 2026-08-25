using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Tutorial1 : MonoBehaviour
{
    Texture2D texture;
    Color[] background;

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
        // ******************************************************
        // Write your Tutorial1 code here

        //texture.DrawDDALine(300, 300, 300, 500, Color.black); // 1

        //texture.DrawDDALine(300, 300, 600, 300, Color.blue);  // 2

        //texture.DrawDDALine(300, 300, 300, 50, Color.green); // 3
        //texture.DrawDDALine(400, 600, 400, 100, Color.yellow);// 4

        //texture.DrawDDALine(300, 300, 300, 200, Color.black); // 
        //texture.DrawDDALine(300, 300, 350, 213, Color.red); // 6
        //texture.DrawDDALine(300, 300, 387, 250, Color.blue); // 7
        //texture.DrawDDALine(300, 300, 400, 300, Color.green); // 8
        //texture.DrawDDALine(300, 300, 387, 350, Color.yellow); // 9   
        //texture.DrawDDALine(300, 300, 350, 387, Color.cyan); // 10
        //texture.DrawDDALine(300, 300, 300, 400, Color.magenta);  // 11
        //texture.DrawDDALine(300, 300, 250, 387, Color.white); // 12
        //texture.DrawDDALine(300, 300, 213, 350, Color.gray); // 13
        //texture.DrawDDALine(300, 300, 200, 300, Color.orange); // 14
        //texture.DrawDDALine(300, 300, 213, 250, Color.purple);  // 15
        //texture.DrawDDALine(300, 300, 250, 213, Color.brown);  // 16  

        texture.DrawBresLine(300, 300, 300, 500, Color.black); // 1

        texture.DrawBresLine(300, 300, 600, 300, Color.blue);  // 2

        texture.DrawBresLine(300, 300, 300, 50, Color.green); // 3
        texture.DrawBresLine(400, 600, 400, 100, Color.yellow);// 4

        texture.DrawBresLine(300, 300, 300, 200, Color.black); // 
        texture.DrawBresLine(300, 300, 350, 213, Color.red); // 6
        texture.DrawBresLine(300, 300, 387, 250, Color.blue); // 7
        texture.DrawBresLine(300, 300, 400, 300, Color.green); // 8
        texture.DrawBresLine(300, 300, 387, 350, Color.yellow); // 9   
        texture.DrawBresLine(300, 300, 350, 387, Color.cyan); // 10
        texture.DrawBresLine(300, 300, 300, 400, Color.magenta);  // 11
        texture.DrawBresLine(300, 300, 250, 387, Color.white); // 12
        texture.DrawBresLine(300, 300, 213, 350, Color.gray); // 13
        texture.DrawBresLine(300, 300, 200, 300, Color.orange); // 14
        texture.DrawBresLine(300, 300, 213, 250, Color.purple);  // 15
        texture.DrawBresLine(300, 300, 250, 213, Color.brown);  // 16  


        // test DrawCircle
        DateTime before = DateTime.Now;


        texture.DrawCircle(300, 300, 100, Color.red);
        texture.DrawCircle(500, 300, 100, Color.black);
        texture.DrawCircle(400, 300, 200, Color.blue);

        for (int r = 5; r < 200; r += 5)
        {
            texture.DrawCircle(800, 500, r, Color.black);
        }


        for (float r = 0; r < 100; r += 0.5f)
        {
            texture.DrawCircle(100, 100, Mathf.RoundToInt(r), Color.red);
        }

        //texture.DrawCircleOpt4(300, 300, 100, Color.red);
        //texture.DrawCircleOpt4(500, 300, 100, Color.black);
        //texture.DrawCircleOpt4(400, 300, 200, Color.blue);

        //for (int r = 5; r < 200; r += 5)
        //{
        //    texture.DrawCircleOpt4(800, 500, r, Color.black);
        //}

        //texture.DrawCircleOpt8(300, 300, 100, Color.red);
        //texture.DrawCircleOpt8(500, 300, 100, Color.black);
        //texture.DrawCircleOpt8(400, 300, 200, Color.blue);

        //for (int r = 5; r < 200; r += 5)
        //{
        //    texture.DrawCircleOpt8(800, 500, r, Color.black);
        //}

        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("Duration in milliseconds: " + duration.Milliseconds);

        // test DrawCircleWithLines
        texture.DrawCircleWithLines(300, 600, 100, Color.red);
        texture.DrawCircleWithLines(250, 400, 160, Color.black);
        texture.DrawCircleWithLines(500, 400, 20, Color.blue);

        // test DrawCirclePythagoras
        texture.DrawCirclePythagoras(300, 100, 100, Color.red);
        texture.DrawCirclePythagoras(300, 400, 200, Color.red);

        // test DrawCircleBresenham
        texture.DrawCircleBresenham(500, 100, 50, Color.red);
        texture.DrawCircleBresenham(900, 300, 250, Color.red);

        // ******************************************************

        // this needs to happen _after_ your code
        texture.Apply(); // DO NOT MOVE OR DELETE
    }

    // Update is called once per frame
    void Update()
    {
        // nothing to update
    }
}

// in C# you can add methods to existing classes: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
public static class Texture2DExtensionsPrimitives
{
    // incomplete line drawing function, only handles a limited number of directions and line slopes
    public static void DrawDDALine(this Texture2D texture, int x1, int y1, int x2, int y2, Color c)
    {
        int deltax = (x2 - x1);
        int deltay = (y2 - y1);

        int steps = Mathf.Max(Mathf.Abs(deltay), Mathf.Abs(deltax));

        if (steps == 0)
        {
            // no line to draw
            texture.SetPixel(x1, y1, c);
            return;
        }

        float xIncrement = (float)deltax / steps;
        float yIncrement = (float)deltay / steps;

        float x = x1;
        float y = y1;

        for (int i = 0; i <= steps; i++)
        {
            texture.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y), c);
            x += xIncrement;
            y += yIncrement;
        }
    }


    public static void DrawBresLine(this Texture2D texture, int x1, int y1, int x2, int y2, Color c)
    {
        bool steep = Mathf.Abs(y2 - y1) > Mathf.Abs(x2 - x1);

        // If the line is steep, swap x and y
        if (steep)
        {
            (x1, y1) = (y1, x1);
            (x2, y2) = (y2, x2);
        }

        // Ensure we draw from left to right
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }

        int deltax = x2 - x1;
        int deltay = Mathf.Abs(y2 - y1);
        int error = deltax / 2;
        int ystep = (y1 < y2) ? 1 : -1;
        int y = y1;

        for (int x = x1; x <= x2; x++)
        {
            if (steep)
            {
                texture.SetPixel(y, x, c); // swap back x/y
            }
            else
            {
                texture.SetPixel(x, y, c);
            }

            error -= deltay;
            if (error < 0)
            {
                y += ystep;
                error += deltax;
            }
        }
    }

    public static void DrawCircle(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        float step = 1f / radius;

        for (float theta = 0; theta < 2* Mathf.PI; theta += step)
        {
            int x = Mathf.RoundToInt(centerX + radius * Mathf.Cos(theta));
            int y = Mathf.RoundToInt(centerY + radius * Mathf.Sin(theta));
               
            texture.SetPixel(x, y, c);
        }
    }

    // Mirror each quadrant to make the algorithm (potentially) 4x faster
    public static void DrawCircleOpt4(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        float step = 1f / radius;

        for (float theta = 0; theta < Mathf.PI / 2; theta += step)
        {
            int dx = Mathf.RoundToInt(radius * Mathf.Cos(theta));
            int dy = Mathf.RoundToInt(radius * Mathf.Sin(theta));

            texture.SetPixel(centerX + dx, centerY + dy, c); // Quadrant 1
            texture.SetPixel(centerX - dx, centerY + dy, c); // Quadrant 2
            texture.SetPixel(centerX - dx, centerY - dy, c); // Quadrant 3
            texture.SetPixel(centerX + dx, centerY - dy, c); // Quadrant 4
        }
    }


    // Mirror each quadrant to make the algorithm (potentially) 8x faster
    public static void DrawCircleOpt8(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        float step = 1f / radius;

        for (float theta = 0; theta < Mathf.PI / 4; theta += step)
        {
            int dx = Mathf.RoundToInt(radius * Mathf.Cos(theta));
            int dy = Mathf.RoundToInt(radius * Mathf.Sin(theta));

            texture.SetPixel(centerX + dx, centerY + dy, c);
            texture.SetPixel(centerX + dy, centerY + dx, c);
            texture.SetPixel(centerX - dx, centerY + dy, c);
            texture.SetPixel(centerX - dy, centerY + dx, c);
            texture.SetPixel(centerX - dx, centerY - dy, c);
            texture.SetPixel(centerX - dy, centerY - dx, c);
            texture.SetPixel(centerX + dx, centerY - dy, c);
            texture.SetPixel(centerX + dy, centerY - dx, c);
        }
    }

    // Drawing Circles with Trigonometry and Lines
    public static void DrawCircleWithLines(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        int segments = Mathf.CeilToInt(2 * radius * Mathf.PI / 1f);
        float angleStep = 2 * Mathf.PI / segments;

        float preX = centerX + radius * MathF.Cos(0);
        float preY = centerY + radius * MathF.Sin(0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = centerX + radius * Mathf.Cos(angle);
            float y = centerY + radius * Mathf.Sin(angle);

            texture.DrawBresLine(
                Mathf.RoundToInt(preX), Mathf.RoundToInt(preY),
                Mathf.RoundToInt(x), Mathf.RoundToInt(y), c
            );

            preX = x;
            preY = y;
        }
    }

    // Drawing Circles with Pythagoras
    public static void DrawCirclePythagoras(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        for (int x = 0; x <= radius; x++)
        {
            int y = Mathf.RoundToInt(Mathf.Sqrt(radius * radius - x * x));

            texture.SetPixel(centerX + x, centerY + y, c);
            texture.SetPixel(centerX + x, centerY - y, c);
            texture.SetPixel(centerX - x, centerY + y, c);
            texture.SetPixel(centerX - x, centerY - y, c);

            // // 8-way symmetry to make the distribution symmetric so circle to be more even
            // ChatGPT suggested this
            texture.SetPixel(centerX + y, centerY + x, c);
            texture.SetPixel(centerX - y, centerY + x, c);
            texture.SetPixel(centerX - y, centerY - x, c);
            texture.SetPixel(centerX + y, centerY - x, c);
        }
    }

    // Bresenham's Circle Algorithm
    public static void DrawCircleBresenham(this Texture2D texture, int centerX, int centerY, int radius, Color c)
    {
        int x = 0;
        int y = radius;
        int d = 3 - 2 * radius; // decision parameter

        while (x <= y)
        {
            // 8-way symmetry
            texture.SetPixel(centerX + x, centerY + y, c);
            texture.SetPixel(centerX - x, centerY + y, c);
            texture.SetPixel(centerX + x, centerY - y, c);
            texture.SetPixel(centerX - x, centerY - y, c);
            texture.SetPixel(centerX + y, centerY + x, c);
            texture.SetPixel(centerX - y, centerY + x, c);
            texture.SetPixel(centerX + y, centerY - x, c);
            texture.SetPixel(centerX - y, centerY - x, c);

            if (d < 0)
            {
                d = d + 4 * x + 6;
            }
            else
            {
                d = d + 4 * (x - y) + 10;
                y--;
            }
            x++;
        }
    }
}
