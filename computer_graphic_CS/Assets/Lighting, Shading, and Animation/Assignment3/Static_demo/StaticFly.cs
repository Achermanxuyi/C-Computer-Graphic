using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticFly : MonoBehaviour
{
    [Header("Fly Appearance")]
    public float mouseLength = 0.3f;
    public float bodyRadius = 0.5f;
    public float wingLength = 1.0f;
    public float wingWidth = 0.5f;
    public float eyeRadius = 0.4f;
    public float tailLength = 0.7f;
    public int legCount = 6;
    public float upperLegLength = 0.4f;
    public float lowerLegLength = 0.4f;
    public float legThickness = 0.05f;
    public float footLength = 0.1f;
    public float legSpreadAngle = -45f;
    public Material mouseMaterial;
    public Material eyeMaterial;
    public Material bodyMaterial;
    public Material wingMaterial;

    GameObject bodyObj;
    GameObject headObj;

    void Start()
    {
        // transform.localPosition = new Vector3(0, 1, 0);
        // transform.localScale = Vector3.one * 0.3f;
        // transform.localRotation = Quaternion.identity;

        // Create head with embedded eyes
        headObj = CreateHead(transform);
        headObj.transform.SetParent(transform, false);
        headObj.transform.localPosition = Vector3.zero;
        headObj.transform.localRotation = Quaternion.Euler(0, 180f, 0);

        // Attach eyes to the head
        GameObject eyes = CreateEyes(headObj.transform);
        eyes.transform.SetParent(headObj.transform, false);
        eyes.transform.localPosition = new Vector3(0, 0.1f, 0.45f);

        // Create mouth (proboscis)
        GameObject mouth = CreateMouth(headObj.transform);
        mouth.transform.SetParent(headObj.transform, false);
        mouth.transform.localPosition = new Vector3(0, -0.05f, 0.42f);

        // Create body
        bodyObj = CreateBody(transform);
        bodyObj.transform.SetParent(transform, false);
        bodyObj.transform.localPosition = new Vector3(0, 0, 0.5f);

        // Create wings
        GameObject wings = CreateWings(bodyObj.transform);
        wings.transform.SetParent(bodyObj.transform, false);
        wings.transform.localPosition = new Vector3(0, 0.2f, -0.1f);

        // Create legs attached to body
        GameObject legs = CreateLegs(bodyObj.transform);
        legs.transform.SetParent(bodyObj.transform, false);
        legs.transform.localPosition = Vector3.zero;

        // Create tail
        GameObject tail = CreateTail(bodyObj.transform);
        tail.transform.SetParent(bodyObj.transform, false);
        tail.transform.localPosition = new Vector3(0, 0, 0.3f);
    }

    GameObject CreateHead(Transform root)
    {
        GameObject head = new GameObject("Head");

        MeshFilter mf = head.AddComponent<MeshFilter>();
        MeshRenderer mr = head.AddComponent<MeshRenderer>();
        mr.material = new Material(bodyMaterial);
        mr.material.renderQueue = 2000;

        // --- Define circular profile (cross-section) ---
        int profileSteps = 24;
        Vector3[] profile = new Vector3[profileSteps];
        float headRadius = eyeRadius * 1.2f;
        for (int i = 0; i < profileSteps; i++)
        {
            float angle = (i / (float)profileSteps) * 2f * Mathf.PI;
            profile[i] = new Vector3(Mathf.Cos(angle) * headRadius, Mathf.Sin(angle) * headRadius, 0);
        }

        // --- Define closed spherical path using latitude rings ---
        int pathSteps = 24;
        Matrix4x4[] path = new Matrix4x4[pathSteps];
        for (int i = 0; i < pathSteps; i++)
        {
            // t ranges from 0 to 2π to create full revolution
            float t = (i / (float)pathSteps) * 2f * Mathf.PI;
            // Use sin/cos to make a sphere-like shape along z-axis
            float z = Mathf.Cos(t) * headRadius;
            float scale = Mathf.Sin(t);
            scale = Mathf.Abs(scale); // ensure positive radius

            path[i] = Matrix4x4.TRS(
                new Vector3(0, 0, z),
                Quaternion.identity,
                new Vector3(scale, scale, 1)
            );
        }

        // --- Generate closed sweep (wrap path around) ---
        Mesh mesh = MeshUtilities.Sweep(profile, path, true);

        // --- Flip winding so normals point outward ---
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int tmp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = tmp;
        }
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
        return head;
    }


    GameObject CreateMouth(Transform root)
    {
        GameObject mouth = new GameObject("Mouth");
        
        MeshFilter mf = mouth.AddComponent<MeshFilter>();
        MeshRenderer mr = mouth.AddComponent<MeshRenderer>();
        mr.material = new Material(mouseMaterial);
        mr.material.renderQueue = 2000;

        int profileSteps = 8;
        Vector3[] profile = new Vector3[profileSteps];
        for (int i = 0; i < profileSteps; i++)
        {
            float angle = (i / (float)profileSteps) * 2f * Mathf.PI;
            profile[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        }

        int pathSteps = 16;
        Matrix4x4[] path = new Matrix4x4[pathSteps];
        for (int i = 0; i < pathSteps; i++)
        {
            float t = i / (float)(pathSteps - 1);
            float z = t * mouseLength * 0.8f; // slightly shorter
            float radius = Mathf.Lerp(0.025f, 0.07f, Mathf.Pow(t, 0.6f));
            float yOffset = -t * 0.08f;

            path[i] = Matrix4x4.TRS(
                new Vector3(0, yOffset, z),
                Quaternion.Euler(0, 180f, 0),
                new Vector3(radius, radius, 1)
            );
        }

        Mesh mesh = MeshUtilities.Sweep(profile, path, false);

        // flip winding
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int tmp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = tmp;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;

        return mouth;
    }


    GameObject CreateBody(Transform root)
    {
        GameObject body = new GameObject("Body");
        
        // Simple scaled sphere for body
        GameObject bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bodyMesh.name = "BodyMesh";
        bodyMesh.transform.SetParent(body.transform, false);
        bodyMesh.transform.localScale = new Vector3(bodyRadius * 1.2f, bodyRadius, bodyRadius * 1.8f);
        
        var mr = bodyMesh.GetComponent<MeshRenderer>();
        mr.material = bodyMaterial;

        return body;
    }

    GameObject CreateEyes(Transform root)
    {
        GameObject eyes = new GameObject("Eye");

        GameObject leftEye = CreateSingleEye(eyes.transform, "Left");
        leftEye.transform.SetParent(eyes.transform, false);
        leftEye.transform.localPosition = new Vector3(-eyeRadius * 0.5f, 0, 0);
        
        GameObject rightEye = CreateSingleEye(eyes.transform, "Right");
        rightEye.transform.SetParent(eyes.transform, false);
        rightEye.transform.localPosition = new Vector3(eyeRadius * 0.5f, 0, 0);

        return eyes;
    }

    GameObject CreateSingleEye(Transform root, String side)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = side + "Eye";
        eye.transform.SetParent(root, false);
        eye.transform.localPosition = Vector3.zero;
        eye.transform.localRotation = Quaternion.identity;
        
        // Flatten a bit and slightly smaller for better embedding
        eye.transform.localScale = new Vector3(eyeRadius * 0.9f, eyeRadius * 1f, eyeRadius * 0.6f);

        var mr = eye.GetComponent<MeshRenderer>();
        mr.sharedMaterial = eyeMaterial;
        Destroy(eye.GetComponent<Collider>());
        return eye;
    }

    GameObject CreateWings(Transform root)
    {
        GameObject wings = new GameObject("Wings");

        // Create left wingss
        GameObject leftWing = CreateSingleWing(wings.transform);
        leftWing.name = "LeftWing";
        leftWing.transform.SetParent(wings.transform, false);
        leftWing.transform.localPosition = new Vector3(-bodyRadius * 0.02f, 0.03f, 0);
        leftWing.transform.localRotation = Quaternion.Euler(180, 210, -30);

        // Create right wing
        GameObject rightWing = CreateSingleWing(wings.transform);
        rightWing.name = "RightWing";
        rightWing.transform.SetParent(wings.transform, false);
        rightWing.transform.localPosition = new Vector3(bodyRadius * 0.02f, 0.03f, 0);
        rightWing.transform.localRotation = Quaternion.Euler(0, -30, 30);

        return wings;
    }

    GameObject CreateSingleWing(Transform root)
    {
        GameObject wing = new GameObject("Wing");

        MeshFilter mf = wing.AddComponent<MeshFilter>();
        MeshRenderer mr = wing.AddComponent<MeshRenderer>();
        mr.material = wingMaterial;

        Mesh wingMesh = new Mesh();

        float thickness = 0.002f; // small Y-thickness to make both sides visible

        // ---------- Vertices ----------
        Vector3[] vertices = new Vector3[]
        {
            // Front face (y = 0)
            new Vector3(0, 0, 0),                                               // 0
            new Vector3(wingLength * 0.3f, 0, wingWidth * 0.5f),               // 1
            new Vector3(wingLength, 0, wingWidth * 0.3f),                       // 2
            new Vector3(wingLength * 0.8f, 0, -wingWidth * 0.2f),              // 3
            new Vector3(wingLength * 0.2f, 0, -wingWidth * 0.3f),              // 4

            // Back face (y = -thickness)
            new Vector3(0, -thickness, 0),                                      // 5
            new Vector3(wingLength * 0.3f, -thickness, wingWidth * 0.5f),      // 6
            new Vector3(wingLength, -thickness, wingWidth * 0.3f),              // 7
            new Vector3(wingLength * 0.8f, -thickness, -wingWidth * 0.2f),     // 8
            new Vector3(wingLength * 0.2f, -thickness, -wingWidth * 0.3f)      // 9
        };

        // ---------- Triangles ----------
        List<int> triangles = new List<int>();

        // Front face
        triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 });

        // Back face (reverse winding)
        triangles.AddRange(new int[] { 5, 7, 6, 5, 8, 7, 5, 9, 8 });

        // Side faces to close the wing edges
        AddQuad(triangles, 0, 5, 6, 1); // leading edge
        AddQuad(triangles, 1, 6, 7, 2);
        AddQuad(triangles, 2, 7, 8, 3);
        AddQuad(triangles, 3, 8, 9, 4);
        AddQuad(triangles, 4, 9, 5, 0); // trailing edge

        wingMesh.vertices = vertices;
        wingMesh.triangles = triangles.ToArray();
        wingMesh.RecalculateNormals();
        wingMesh.RecalculateBounds();

        mf.mesh = wingMesh;

        return wing;
    }

    void AddQuad(List<int> triList, int v00, int v10, int v11, int v01)
    {
        triList.Add(v00);
        triList.Add(v10);
        triList.Add(v11);

        triList.Add(v00);
        triList.Add(v11);
        triList.Add(v01);
    }



    GameObject CreateLegs(Transform root)
    {
        GameObject legs = new GameObject("Legs");
        
        float angleStep = 360f / legCount;
        
        for (int i = 0; i < legCount; i++)
        {
            float angle = i * angleStep + 60f;
            
            GameObject leg = CreateSingleLeg(legs.transform, "Leg_" + i, angle, i, out Transform baseJoint, out Transform kneeJoint);
            leg.transform.SetParent(legs.transform, false);
        }
        
        return legs;
    }
    
    GameObject CreateSingleLeg(Transform root, String legRef, float angle, int legIndex, out Transform baseJoint, out Transform kneeJoint)
    {
        GameObject singleLeg = new GameObject(legRef);
        
        // Position inside the body
        float attachRadius = bodyRadius * 0.6f;
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 attachPoint = new Vector3(
            Mathf.Cos(angleRad) * attachRadius,
            -bodyRadius * 0.3f,
            Mathf.Sin(angleRad) * attachRadius
        );
        
        singleLeg.transform.localPosition = attachPoint;
        
        GameObject baseJointObj = new GameObject("BaseJoint");
        baseJointObj.transform.SetParent(singleLeg.transform, false);
        baseJointObj.transform.localPosition = Vector3.zero;

        Quaternion outwardRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)), Vector3.up);
        Quaternion tilt = Quaternion.Euler(-legSpreadAngle, -30f, 0);
        baseJointObj.transform.localRotation = outwardRotation * tilt * Quaternion.Euler(0, 180f, 0);

        baseJoint = baseJointObj.transform;
        
        GameObject upperLeg = CreateLegSegment("UpperLeg", upperLegLength, legThickness);
        upperLeg.transform.SetParent(baseJointObj.transform, false);
        upperLeg.transform.localPosition = new Vector3(0, 0, -upperLegLength / 2f);
        upperLeg.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        
        // debugged by ChatGPT
        // --- KneeJoint pivot (no scale applied here) ---
        GameObject kneeJointObj = new GameObject("KneeJoint");
        kneeJointObj.transform.SetParent(baseJointObj.transform, false);
        kneeJointObj.transform.localPosition = new Vector3(0, 0, -upperLegLength);
        kneeJointObj.transform.localRotation = Quaternion.Euler(-100f, 0, 0);
        kneeJoint = kneeJointObj.transform;

        // --- Visual sphere for knee joint (isolated scale) ---
        GameObject kneeVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kneeVisual.name = "KneeVisual";
        kneeVisual.transform.SetParent(kneeJointObj.transform, false);
        kneeVisual.transform.localScale = Vector3.one * legThickness * 0.75f; // scale sphere only
        kneeVisual.transform.localPosition = Vector3.zero; // center sphere at pivot
        kneeVisual.GetComponent<MeshRenderer>().sharedMaterial = bodyMaterial;

        kneeJoint = kneeJointObj.transform;
        
        GameObject lowerLeg = CreateLegSegment("LowerLeg", lowerLegLength, legThickness * 0.8f);
        lowerLeg.transform.SetParent(kneeJointObj.transform, false);
        lowerLeg.transform.localPosition = new Vector3(0, 0, -lowerLegLength / 2f);
        lowerLeg.transform.localRotation = Quaternion.Euler(-90f, 0, 0);
        
        // --- FootJoint pivot (no scaling here) ---
        GameObject footJoint = new GameObject("FootJoint");
        footJoint.transform.SetParent(kneeJointObj.transform, false);
        footJoint.transform.localPosition = new Vector3(0, 0, -lowerLegLength);
        footJoint.transform.localRotation = Quaternion.Euler(-120f, 0, 0);

        // --- Visual sphere for FootJoint ---
        GameObject footVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        footVisual.name = "FootVisual";
        footVisual.transform.SetParent(footJoint.transform, false);
        footVisual.transform.localScale = Vector3.one * legThickness * 0.75f;   // scale only this sphere
        footVisual.transform.localPosition = Vector3.zero;               // align with pivot
        footVisual.GetComponent<MeshRenderer>().sharedMaterial = bodyMaterial;
        Destroy(footVisual.GetComponent<Collider>());                    // optional

        // --- Foot mesh (child of pivot, unaffected by scale) ---
        GameObject foot = CreateLegSegment("Foot", footLength, legThickness * 0.6f);
        foot.transform.SetParent(footJoint.transform, false);
        foot.transform.localPosition = new Vector3(0, 0, footLength / 2f);
        foot.transform.localRotation = Quaternion.Euler(90f, 0, 0);

        
        return singleLeg;
    }
    
    GameObject CreateLegSegment(String name, float length, float thickness)
    {
        GameObject segment = new GameObject(name);
        
        MeshFilter mf = segment.AddComponent<MeshFilter>();
        MeshRenderer mr = segment.AddComponent<MeshRenderer>();
        mr.material = bodyMaterial;
        
        mf.mesh = MeshUtilities.Cylinder(8, thickness / 2f, length / 2f);
        
        return segment;
    }
    
    GameObject CreateTail(Transform root)
    {
        GameObject tail = new GameObject("Tail");
        
        MeshFilter mf = tail.AddComponent<MeshFilter>();
        MeshRenderer mr = tail.AddComponent<MeshRenderer>();
        mr.material = bodyMaterial;

        // Profile: circle for cross-section
        int profileSteps = 12;
        Vector3[] profile = new Vector3[profileSteps];
        for (int i = 0; i < profileSteps; i++)
        {
            float angle = (i / (float)profileSteps) * 2f * Mathf.PI;
            profile[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        }

        // Path: elliptical with pointed end
        int pathSteps = 20;
        Matrix4x4[] path = new Matrix4x4[pathSteps];
        for (int i = 0; i < pathSteps; i++)
        {
            float t = i / (float)(pathSteps - 1);
            float z = t * tailLength;
            
            // Taper to point at the end
            float radius = bodyRadius * 0.4f * (1f - Mathf.Pow(t, 1.5f));
            
            // Slight vertical scale for elliptical shape
            float verticalScale = 0.8f;
            
            Matrix4x4 m = Matrix4x4.TRS(
                new Vector3(0, 0, z),
                Quaternion.identity,
                new Vector3(radius, radius * verticalScale, 1)
            );
            path[i] = m;
        }

        mf.mesh = MeshUtilities.Sweep(profile, path, false);

        return tail;
    }
}