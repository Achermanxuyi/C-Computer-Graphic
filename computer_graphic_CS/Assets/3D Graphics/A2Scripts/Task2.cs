using System.Collections.Generic;
using UnityEngine;

public class Task2 : MonoBehaviour
{
    GameObject body;

    void Start()
    {
        // Root setup 
        transform.localPosition = new Vector3(4f, 4f, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Create body 
        body = CreateBody();
        body.transform.SetParent(transform, false);
        body.transform.localPosition = Vector3.zero;

        // Create neck joint hierarchy (attached to root)
        GameObject neckJoint = new GameObject("NeckJoint");
        neckJoint.transform.SetParent(transform, false);
        neckJoint.transform.localPosition = new Vector3(0, 1.85f, 0);
        neckJoint.transform.localRotation = Quaternion.identity;

        GameObject neck = CreateNeck();
        neck.transform.SetParent(neckJoint.transform, false);
        neck.transform.localPosition = Vector3.zero;

        GameObject headJoint = new GameObject("HeadJoint");
        headJoint.transform.SetParent(neck.transform, false);
        headJoint.transform.localPosition = new Vector3(0, 0.4f, 0);

        GameObject head = CreateHead();
        head.transform.SetParent(headJoint.transform, false);

        // Attach hat 
        GameObject hat = GameObject.Find("BowlerHat");
        if (hat != null)
        {

            GameObject hatClone = Instantiate(hat);

            // Reset the clone 
            hatClone.transform.localPosition = Vector3.zero;
            hatClone.transform.localRotation = Quaternion.identity;
            hatClone.transform.localScale = Vector3.one;
            hatClone.transform.SetParent(head.transform, false);
            hatClone.transform.localPosition = new Vector3(0, 0.3f, 0);
            hatClone.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            Debug.LogWarning("BowlerHat not found in the scene!");
        }

        // Create limbs under root
        CreateArmPair(transform);
        CreateLegPair(transform);
    }

    GameObject CreateBody()
    {
        GameObject body = new GameObject("Body");
        var renderer = body.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        var filter = body.AddComponent<MeshFilter>();
        filter.mesh = MeshUtilities.Cube(1.0f);

        // scale affects only the cube
        body.transform.localScale = new Vector3(1.2f, 1.5f, 0.6f);
        return body;
    }

    GameObject CreateNeck()
    {
        GameObject neck = new GameObject("Neck");
        var renderer = neck.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        var filter = neck.AddComponent<MeshFilter>();
        filter.mesh = MeshUtilities.Cylinder(12, 0.45f, 0.4f);
        return neck;
    }

    GameObject CreateHead()
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        return head;
    }

    GameObject CreateJoint(float radius)
    {
        GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        joint.name = "joint";
        joint.transform.localScale = new Vector3(radius, radius, radius);
        return joint;
    }

    void CreateArmPair(Transform root)
    {
        // LEFT ARM
        GameObject leftShoulderJoint = new GameObject("LeftShoulderJoint");
        leftShoulderJoint.transform.SetParent(root, false);
        leftShoulderJoint.transform.localPosition = new Vector3(-1.5f, 0.85f, 0);
        leftShoulderJoint.transform.localRotation = Quaternion.Euler(30f, 0, 0);

        GameObject leftArm = CreateUpperArm("LeftUpperArm");
        leftArm.transform.SetParent(leftShoulderJoint.transform, false);

        GameObject leftElbowJoint = new GameObject("LeftElbowJoint");
        leftElbowJoint.transform.SetParent(leftArm.transform, false);
        leftElbowJoint.transform.localPosition = new Vector3(0, -0.6f, 0);
        leftElbowJoint.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

        GameObject leftJoint = CreateJoint(0.4f);
        leftJoint.transform.SetParent(leftElbowJoint.transform, false);
        leftJoint.transform.localPosition = new Vector3(0, 0, 0);

        GameObject leftForearm = CreateLowerArm("LeftForearm");
        leftForearm.transform.SetParent(leftElbowJoint.transform, false);
        leftForearm.transform.localPosition = new Vector3(0f, -0.6f, 0f);

        // RIGHT ARM
        GameObject rightShoulderJoint = new GameObject("RightShoulderJoint");
        rightShoulderJoint.transform.SetParent(root, false);
        rightShoulderJoint.transform.localPosition = new Vector3(1.5f, 0.85f, 0);

        GameObject rightArm = CreateUpperArm("RightUpperArm");
        rightArm.transform.SetParent(rightShoulderJoint.transform, false);

        GameObject rightElbowJoint = new GameObject("RightElbowJoint");
        rightElbowJoint.transform.SetParent(rightArm.transform, false);
        rightElbowJoint.transform.localPosition = new Vector3(0, -0.6f, 0);

        GameObject rightJoint = CreateJoint(0.4f);
        rightJoint.transform.SetParent(rightElbowJoint.transform, false);
        rightJoint.transform.localPosition = new Vector3(0, 0, 0);

        GameObject rightForearm = CreateLowerArm("RightForearm");
        rightForearm.transform.SetParent(rightElbowJoint.transform, false);
        rightForearm.transform.localPosition = new Vector3(0, -0.6f, 0);
    }

    void CreateLegPair(Transform root)
    {
        // LEFT LEG
        Vector3 leftHipPosition = new Vector3(-0.6f, -1.6f, 0);

        // Create static hip sphere (marker)
        GameObject leftHip = CreateJoint(1f);
        leftHip.name = "LeftHip";
        leftHip.transform.SetParent(root, false);
        leftHip.transform.localPosition = leftHipPosition;
        leftHip.transform.localRotation = Quaternion.identity; // stays static, no rotation

        // Create rotatable hip joint (pivot)
        GameObject leftHipJoint = new GameObject("LeftHipJoint");
        leftHipJoint.transform.SetParent(root, false);
        leftHipJoint.transform.localPosition = leftHipPosition;
        leftHipJoint.transform.localRotation = Quaternion.identity;

        // Create thigh under joint (it starts slightly below the pivot)
        GameObject leftThigh = CreateUpperLeg("LeftThigh");
        leftThigh.transform.SetParent(leftHipJoint.transform, false);
        leftThigh.transform.localPosition = new Vector3(0, -0.85f, 0); // attach below pivot

        // Add knee joint under thigh
        GameObject leftKneeJoint = new GameObject("LeftKneeJoint");
        leftKneeJoint.transform.SetParent(leftThigh.transform, false);
        leftKneeJoint.transform.localPosition = new Vector3(0, -0.85f, 0);


        // RIGHT LEG
        Vector3 rightHipPosition = new Vector3(0.6f, -1.6f, 0);

        // Create static hip sphere (marker)
        GameObject rightHip = CreateJoint(1f);
        rightHip.name = "RightHip";
        rightHip.transform.SetParent(root, false);
        rightHip.transform.localPosition = rightHipPosition;
        rightHip.transform.localRotation = Quaternion.identity; // static

        // Create rotatable hip joint (pivot)
        GameObject rightHipJoint = new GameObject("RightHipJoint");
        rightHipJoint.transform.SetParent(root, false);
        rightHipJoint.transform.localPosition = rightHipPosition;
        //rightHipJoint.transform.localRotation = Quaternion.identity;
        rightHipJoint.transform.localRotation = Quaternion.Euler(45, 0, 0);

        // Create thigh under joint
        GameObject rightThigh = CreateUpperLeg("RightThigh");
        rightThigh.transform.SetParent(rightHipJoint.transform, false);
        rightThigh.transform.localPosition = new Vector3(0, -0.85f, 0);

        // Add knee joint
        GameObject rightKneeJoint = new GameObject("RightKneeJoint");
        rightKneeJoint.transform.SetParent(rightThigh.transform, false);
        rightKneeJoint.transform.localPosition = new Vector3(0, -0.85f, 0);
    }



    // Chatgpt bug fix (align the shoulder with the joint)
    GameObject CreateUpperArm(string name)
    {
        GameObject arm = new GameObject(name);

        var renderer = arm.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));

        var filter = arm.AddComponent<MeshFilter>();

        // --- Parameters ---
        int divisions = 16;     // circle resolution
        float radius = 0.2f;    // pipe radius
        float straightLength = 0.6f; // vertical distance to elbow
        float curvedRadius = 0.3f;
        int curvedSegments = 16;
        int straightSegments = 8;

        // decide which side (Left -> curve toward +X, Right -> curve toward -X)
        int side = name.Contains("Left") ? 1 : -1;

        // --- Circular profile in XZ plane ---
        Vector3[] circleProfile = new Vector3[divisions];
        for (int i = 0; i < divisions; i++)
        {
            float angle = 2f * Mathf.PI * i / divisions;
            circleProfile[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // --- Path list (local coordinates, origin = shoulder) ---
        List<Matrix4x4> pathList = new List<Matrix4x4>();

        // --- Curved section first: quarter-circle that ends at (0,0,0) (the shoulder)
        // center is at (side * curvedRadius, 0)
        Vector3 center = new Vector3(side * curvedRadius, 0f, 0f);
        for (int i = 0; i <= curvedSegments; i++)
        {
            float t = i / (float)curvedSegments;
            // angle runs from PI/2 -> PI (for side=+1) or PI/2 -> 0 (for side=-1)
            float angle = Mathf.PI * 0.5f + side * (Mathf.PI * 0.5f * t);

            float x = center.x + curvedRadius * Mathf.Cos(angle);
            float y = center.y + curvedRadius * Mathf.Sin(angle);

            Vector3 pos = new Vector3(x, y, 0f);

            // derivative of circle param: (-sin(angle), cos(angle))
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f).normalized;

            Quaternion rot = Quaternion.LookRotation(Vector3.forward, tangent);
            pathList.Add(Matrix4x4.TRS(pos, rot, Vector3.one));
        }

        // --- Straight section: from (0,0,0) down to (0,-straightLength,0)
        // start at i=1 because previous loop ended at (0,0,0)
        for (int i = 1; i <= straightSegments; i++)
        {
            float t = i / (float)straightSegments;
            Vector3 pos = new Vector3(0f, -t * straightLength, 0f);
            Vector3 tangent = Vector3.down;
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, tangent);
            pathList.Add(Matrix4x4.TRS(pos, rot, Vector3.one));
        }

        // --- Create the swept pipe mesh ---
        Mesh mesh = MeshUtilities.Sweep(circleProfile, pathList.ToArray(), false);

        // no extra offset required — last path point is (0, -straightLength, 0),
        // which matches CreateArmPair's elbow localPosition = new Vector3(0, -0.6f, 0)

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Ensure normals face outward
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        mesh.normals = normals;

        // Fix triangle winding
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] tris = mesh.GetTriangles(i);
            System.Array.Reverse(tris);
            mesh.SetTriangles(tris, i);
        }

        filter.mesh = mesh;
        return arm;
    }

    GameObject CreateLowerArm(string name)
    {
        GameObject forearm = new GameObject(name);
        var renderer = forearm.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        var filter = forearm.AddComponent<MeshFilter>();
        filter.mesh = MeshUtilities.Cylinder(16, 0.2f, 0.6f);
        return forearm;
    }

    GameObject CreateUpperLeg(string name)
    {
        GameObject leg = new GameObject(name);

        // Create a child object for the visual mesh
        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.SetParent(leg.transform, false);

        var renderer = meshObj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));

        var filter = meshObj.AddComponent<MeshFilter>();
        filter.mesh = MeshUtilities.Cube(1f);

        // Scale the cube mesh
        meshObj.transform.localScale = new Vector3(0.55f, 1.7f, 0.6f);

        // Move cube down so the top aligns with pivot (pivot at hip)
        meshObj.transform.localPosition = new Vector3(0, -0.85f, 0);

        return leg;
    }



}