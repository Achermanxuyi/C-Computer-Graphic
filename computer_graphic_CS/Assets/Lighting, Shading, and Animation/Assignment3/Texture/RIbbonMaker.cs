using UnityEngine;

public class RibbonMaker : MonoBehaviour
{
    public Material mobiusMaterial;
    public float animationSpeed = 1.0f;
    
    private Material instanceMaterial;
    
    void Start()
    {
        Mesh mobiusMesh = CreateMobiusStrip(
            divisions: 80,          
            width: 1.0f,            
            radius: 3.0f,           
            twists: 0.5f            
        );
        
        GameObject mobiusObject = new GameObject("MobiusStrip");
        mobiusObject.transform.parent = transform;
        
        MeshFilter meshFilter = mobiusObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = mobiusObject.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = mobiusMesh;
        
        // Create material with animated texture
        if (mobiusMaterial != null)
        {
            instanceMaterial = new Material(mobiusMaterial);
            meshRenderer.material = instanceMaterial;
        }
        else
        {
            instanceMaterial = new Material(Shader.Find("Standard"));
            instanceMaterial.mainTexture = CreateFlowingTexture();
            meshRenderer.material = instanceMaterial;
        }
    }
    
    void Update()
    {
        // Animate the texture to show it flowing through the strip
        if (instanceMaterial != null)
        {
            float offset = Time.time * animationSpeed * 0.1f;
            instanceMaterial.mainTextureOffset = new Vector2(0, offset);
        }
    }
    
    public static Mesh CreateMobiusStrip(int divisions, float width, float radius, float twists)
    {
        // Profile: straight line segment (ribbon cross-section)
        int profileDivisions = 8; // More divisions for smoother width
        Vector3[] profile = new Vector3[profileDivisions];
        for (int i = 0; i < profileDivisions; i++)
        {
            float t = (float)i / (profileDivisions - 1);
            profile[i] = new Vector3(0, (t - 0.5f) * width, 0);
        }
        
        // Path: circle with half-twist for Möbius strip
        Matrix4x4[] path = new Matrix4x4[divisions + 1];
        for (int i = 0; i <= divisions; i++)
        {
            float t = (float)i / divisions; 
            float angle = t * Mathf.PI * 2; // one full circle
            float twist = t * twists * Mathf.PI * 2; // rotation for mobius effect
            
            // Position along circle
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            // Calculate tangent direction (perpendicular to radius)
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Vector3 up = Vector3.up;
            Vector3 normal = Vector3.Cross(tangent, up).normalized;
            
            // Apply twist rotation around the tangent direction
            Quaternion twistRotation = Quaternion.AngleAxis(twist * Mathf.Rad2Deg, tangent);
            Vector3 twistedNormal = twistRotation * normal;
            Vector3 twistedUp = twistRotation * up;
            
            // Create rotation matrix from tangent, normal, and up vectors
            Quaternion rotation = Quaternion.LookRotation(tangent, twistedUp);
            
            path[i] = Matrix4x4.TRS(position, rotation, Vector3.one);
        }
        
        // Generate mesh using Sweep (closed loop)
        Mesh mesh = MeshUtilities.Sweep(profile, path, true);
        
        // calculate uvs for flowing effect + 1 to close the loop
        int numVerts = (divisions + 1) * profileDivisions;
        Vector2[] uvs = new Vector2[numVerts];
        
        for (int i = 0; i <= divisions; i++)
        {
            for (int j = 0; j < profileDivisions; j++)
            {
                // U coordinate: position across the ribbon width
                float u = (float)j / (profileDivisions - 1);
                
                // V coordinate: position around the strip (repeats for animation)
                // Multiply by 4 so texture repeats 4 times around the strip
                float v = ((float)i / divisions) * 4.0f;
                
                uvs[i * profileDivisions + j] = new Vector2(u, v);
            }
        }
        
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    Texture2D CreateFlowingTexture()
    {
        int width = 512;
        int height = 512;
        Texture2D texture = new Texture2D(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create animated arrow pattern
                float stripePos = (y % 100) / 100f;
                
                Color color;
                
                // Arrow pattern pointing downward (direction of flow)
                if (stripePos < 0.3f)
                {
                    // Arrow head
                    float arrowWidth = stripePos / 0.3f; // 0 to 1
                    float distFromCenter = Mathf.Abs((x / (float)width) - 0.5f) * 2; // 0 at center, 1 at edges
                    
                    if (distFromCenter < arrowWidth)
                    {
                        color = new Color(1f, 0.3f, 0.3f); // Red arrow
                    }
                    else
                    {
                        color = new Color(0.9f, 0.9f, 1f); // Light blue background
                    }
                }
                else if (stripePos < 0.5f)
                {
                    // Arrow shaft
                    float distFromCenter = Mathf.Abs((x / (float)width) - 0.5f) * 2;
                    
                    if (distFromCenter < 0.3f)
                    {
                        color = new Color(1f, 0.3f, 0.3f); // Red shaft
                    }
                    else
                    {
                        color = new Color(0.9f, 0.9f, 1f); // Light blue background
                    }
                }
                else
                {
                    // Gap between arrows
                    color = new Color(0.9f, 0.9f, 1f); // Light blue background
                }
                
                // Add edge highlights to show strip width
                if (x < 30 || x > width - 30)
                {
                    color = new Color(0.2f, 0.2f, 0.8f); // Dark blue edges
                }
                
                texture.SetPixel(x, y, color);
            }
        }
        
        // Set texture wrap mode to repeat for animation
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        return texture;
    }
}