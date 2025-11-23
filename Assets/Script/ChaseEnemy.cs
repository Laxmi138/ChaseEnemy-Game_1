using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseEnemy : MonoBehaviour
{
    [Header("Characters")]
    public Transform bird;
    public Transform pig;

    [Header("Parallax Layers")]
    public Transform frontLayer;   // Bushes in front
    public Transform middleLayer;  // Ground
    public Transform backLayer;    // Rear bush

    [Header("Parallax Speeds")]
    public float frontSpeed = 2f;
    public float middleSpeed = 1.2f;
    public float backSpeed = 0.6f;

    [Header("Chase Settings")]
    public int tapsToMeet = 5; // configurable in Inspector
    private int currentTap = 0;

    [Header("Bounce Settings")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 3f;

    private Vector3 birdStartPos;
    private Vector3 pigStartPos;
    private Vector3 birdEndPos;
    private Vector3 pigEndPos;

    private float groundOffset = 0;

    void Start()
    {
        // Save starting positions
        birdStartPos = bird.position;
        pigStartPos = pig.position;

        // Center meeting position
        float centerX = 0f;
        birdEndPos = new Vector3(centerX - 0.5f, bird.position.y, bird.position.z);
        pigEndPos = new Vector3(centerX + 0.5f, pig.position.y, pig.position.z);
    }

    void Update()
    {
        HandleParallax();
        HandleBounce();

        if (Input.GetMouseButtonDown(0))
        {
            HandleTap();
        }
    }

    void HandleParallax()
    {
        groundOffset += Time.deltaTime;
        frontLayer.position += Vector3.left * frontSpeed * Time.deltaTime;
        middleLayer.position += Vector3.left * middleSpeed * Time.deltaTime;
        backLayer.position += Vector3.left * backSpeed * Time.deltaTime;

        // Looping effect (optional)
        if (frontLayer.position.x < -10f) frontLayer.position += new Vector3(20f, 0, 0);
        if (middleLayer.position.x < -10f) middleLayer.position += new Vector3(20f, 0, 0);
        if (backLayer.position.x < -10f) backLayer.position += new Vector3(20f, 0, 0);
    }

    void HandleBounce()
    {
        float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        bird.position = new Vector3(bird.position.x, birdStartPos.y + newY, bird.position.z);
        pig.position = new Vector3(pig.position.x, pigStartPos.y + newY, pig.position.z);
    }

    void HandleTap()
    {
        if (currentTap >= tapsToMeet) return;
        currentTap++;

        float birdStep = (birdEndPos.x - birdStartPos.x) / tapsToMeet;
        float pigStep = (pigEndPos.x - pigStartPos.x) / tapsToMeet;

        bird.position += new Vector3(birdStep, 0, 0);
        pig.position += new Vector3(pigStep, 0, 0);
    }

}
