using UnityEngine;
using System.Collections.Generic;

public class SpiderWebMaker : MonoBehaviour
{
    [Header("Web Settings")]
    public int ringCount = 14;
    public int sidesPerRing = 8;
    public float ringSpacing = 0.25f;
    public float threadRadius = 0.005f;

    [Header("Mesh Settings")]
    public int cylinderDivisions = 10;
    public Material webMaterial;

    // Store all nodes and threads
    [HideInInspector] public List<List<GameObject>> nodeObjects = new List<List<GameObject>>();
    [HideInInspector] public List<Thread> threads = new List<Thread>();

    // Cache a single cylinder mesh to reuse for all threads
    private static Mesh cachedCylinderMesh;
    private static int cachedDivisions = -1;
    private static float cachedRadius = -1f;

    private bool webGenerated = false;

    void Start()
    {
        if (webGenerated) return;
        webGenerated = true;

        // set position
        // transform.position = new Vector3(-0.25f, 3.7f, 5);
        // transform.rotation = Quaternion.Euler(-75f, 0, 0);

        GenerateWeb();
    }

    void Update()
    {
        // Update all threads dynamically to match moving nodes
        foreach (Thread t in threads)
        {
            if (t != null)
                t.UpdateThread();
        }
    }

    void GenerateWeb()
    {
        nodeObjects.Clear();
        threads.Clear();

        // Create cached cylinder mesh once for all threads (Claude debugged)
        if (cachedCylinderMesh == null || cachedDivisions != cylinderDivisions || cachedRadius != threadRadius)
        {
            if (cachedCylinderMesh != null)
                Destroy(cachedCylinderMesh);
            
            cachedCylinderMesh = MeshUtilities.Cylinder(cylinderDivisions, threadRadius, 0.5f);
            cachedDivisions = cylinderDivisions;
            cachedRadius = threadRadius;
        }

        GameObject webParent = this.gameObject;
        

        // --- Create Node Objects ---
        for (int i = 0; i <= ringCount; i++)
        {
            List<GameObject> ringNodes = new List<GameObject>();
            float radius = i * ringSpacing;

            for (int j = 0; j < sidesPerRing; j++)
            {
                float angle = (2 * Mathf.PI * j) / sidesPerRing;
                Vector3 pos = new Vector3(radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle));

                GameObject node = new GameObject($"Node_{i}_{j}");
                node.transform.SetParent(webParent.transform, false);
                node.transform.localPosition = pos;

                ringNodes.Add(node);
            }

            nodeObjects.Add(ringNodes);
        }

        // --- Create Threads between Nodes ---
        for (int i = 0; i <= ringCount; i++)
        {
            for (int j = 0; j < sidesPerRing; j++)
            {
                // Connect around the ring
                if (i > 0)
                {
                    GameObject start = nodeObjects[i][j];
                    GameObject end = nodeObjects[i][(j + 1) % sidesPerRing];
                    CreateThread(start, end, webParent);
                }

                // Connect to previous ring (spokes)
                if (i > 0)
                {
                    GameObject start = nodeObjects[i - 1][j];
                    GameObject end = nodeObjects[i][j];
                    CreateThread(start, end, webParent);
                }
            }
        }

        // Connect center spokes (optional)
        for (int j = 0; j < sidesPerRing; j++)
        {
            CreateThread(nodeObjects[0][0], nodeObjects[1][j], webParent);
        }
    }

    void CreateThread(GameObject startNode, GameObject endNode, GameObject parent)
    {
        GameObject threadObj = new GameObject("Thread");
        threadObj.transform.SetParent(parent.transform, false);

        MeshFilter mf = threadObj.AddComponent<MeshFilter>();
        MeshRenderer mr = threadObj.AddComponent<MeshRenderer>();
        mr.material = webMaterial;

        // Use cached mesh instead of creating new ones
        mf.sharedMesh = cachedCylinderMesh;

        Thread t = new Thread(startNode, endNode, threadObj.transform);
        threads.Add(t);
        t.UpdateThread(); // Initialize
    }

    void OnDestroy()
    {
        // Clean up cached mesh when destroyed
        if (cachedCylinderMesh != null)
        {
            Destroy(cachedCylinderMesh);
            cachedCylinderMesh = null;
        }
    }

    // --- Inner class: Thread ---
    public class Thread
    {
        GameObject startNode;
        GameObject endNode;
        Transform threadTransform;

        public Thread(GameObject start, GameObject end, Transform transform)
        {
            startNode = start;
            endNode = end;
            threadTransform = transform;
        }

        public void UpdateThread()
        {
            Vector3 start = startNode.transform.position;
            Vector3 end = endNode.transform.position;
            Vector3 mid = (start + end) / 2f;
            Vector3 dir = (end - start);
            float length = dir.magnitude;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir.normalized);

            threadTransform.position = mid;
            threadTransform.rotation = rot;
            
            // Scale the cached cylinder to match the thread length
            // MeshUtilities.Cylinder uses height as half-extents, so scale by length
            threadTransform.localScale = new Vector3(1f, length, 1f);
        }
    }
}