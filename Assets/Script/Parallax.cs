using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Layers (assign Transforms that hold repeating sprites)")]
    public Transform frontLayer;   // front bushes (fast)
    public Transform middleLayer;  // ground (medium)
    public Transform backLayer;    // rear bush (slow)

    [Header("Speeds (units/sec)")]
    public float frontSpeed = 2f;
    public float middleSpeed = 1.2f;
    public float backSpeed = 0.6f;

    [Header("Looping")]
    public float loopThreshold = -20f; // when x < -threshold -> wrap
    public float loopOffset = 40f;     // amount to move to the right to loop

    void Update()
    {
        float dt = Time.deltaTime;
        if (frontLayer) frontLayer.position += Vector3.left * frontSpeed * dt;
        if (middleLayer) middleLayer.position += Vector3.left * middleSpeed * dt;
        if (backLayer) backLayer.position += Vector3.left * backSpeed * dt;

        // Simple wrap - assumes each layer's child sprites are tiled across a width around loopOffset
        if (frontLayer && frontLayer.position.x < loopThreshold) frontLayer.position += Vector3.right * loopOffset;
        if (middleLayer && middleLayer.position.x < loopThreshold) middleLayer.position += Vector3.right * loopOffset;
        if (backLayer && backLayer.position.x < loopThreshold) backLayer.position += Vector3.right * loopOffset;
    }
}
