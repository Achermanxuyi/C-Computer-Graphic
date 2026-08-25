using UnityEngine;

public class WebAniController : MonoBehaviour
{
    private SpiderWebMaker web;
    public float amplitude = 0.2f;
    public float frequency = 3f;

    public bool isPlaying = false; // controlled by MovieManager

    void Awake()
    {
        // get the SpiderWebMaker component on this GameObject
        web = GetComponent<SpiderWebMaker>();
        
        if (web == null)
        {
            Debug.LogError("SpiderWebMaker script not found on the same GameObject as WebAniController!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaying) return;
        
        if (web == null) return;

        float globalOffset = Mathf.Sin(Time.time * frequency);

        for (int i = 0; i <= web.ringCount; i++)
        {
            float moveFactor = 1f - (float)i / web.ringCount;

            foreach (GameObject node in web.nodeObjects[i])
            {
                Vector3 pos = node.transform.localPosition;

                if (i < web.ringCount)
                {
                    pos.y = globalOffset * amplitude * moveFactor;
                    node.transform.localPosition = pos;
                }
                else
                {
                    pos.y = 0f;
                    node.transform.localPosition = pos;
                }
            }
        }

        // genetated by Claude, reduing computation resource and memory usage        
        // Update threads AFTER all nodes have moved
        foreach (var thread in web.threads)
        {
            if (thread != null)
                thread.UpdateThread();
        }
    }
}
