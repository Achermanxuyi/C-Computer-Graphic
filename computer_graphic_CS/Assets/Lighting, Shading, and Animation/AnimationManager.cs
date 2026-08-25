using System.Runtime.ExceptionServices;
using UnityEditor.UIElements;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Transform lamp;

    public AnimationCurve curve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0.0f, 0.0f));
        curve.AddKey(new Keyframe(5.0f, 4.0f));
        curve.AddKey(new Keyframe(6.0f, -4.0f));
    }

    // Update is called once per frame
    void Update()
    {
        lamp.localPosition = new Vector3(0, curve.Evaluate(Time.time), 0);

        // the lamp move from (0, 0, 0) to (0.5, 0, 0) in 10 seconds
        // lamp.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0),
        //     Time.time / 10.0f);

        // the lamp move from (0, 0, 0) to (1, 0, 0) in 5 seconds
        // lamp.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 0, 0),
        //     Time.time / 5.0f);

        // // the lamp move from (1, 0, 0) to (0, 1, 0) in 5 seconds
        // lamp.localPosition = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 1, 0),
        //     Time.time / 5.0f);

        // float time = Time.time % 10f;
        // float t;

        // if (time <= 5f)
        // {
        //     // first 5 sconds
        //     t = time / 5f;
        //     lamp.localPosition = Vector3.Lerp(
        //         new Vector3(0, 0, 0), new Vector3(1, 0, 0), t
        //     );

        // } else
        // {
        //     // next 5 seconds
        //     t = (time - 5f) / 5f;
        //     lamp.localPosition = Vector3.Lerp(
        //         new Vector3(1, 0, 0), new Vector3(0, 1, 0), t
        //     );
        // }
    }
}
