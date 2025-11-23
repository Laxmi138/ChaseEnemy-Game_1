using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class CharacterChase : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("References")]
    public Transform bird;   
    public Transform pig;    
    public ProgressBarController progressBar;
    [Header("Positions")]
    public float centerX = 0f; // meeting center X
    public float meetOffset = 0.5f; // spacing when they meet (bird left of center, pig right)

    [Header("Taps")]
    [Tooltip("Number of taps required to meet (configurable)")]
    public int tapsToMeet = 5;
    private int currentTaps = 0;

    [Header("Hop Settings")]
    public float hopHeight = 0.3f;
    public float hopDuration = 0.18f;
    public float idleBounceHeight = 0.08f;
    public float idleBounceSpeed = 3f;

    [Header("Movement smoothing")]
    public float positionLerpSpeed = 12f;

    [Header("Sound (optional)")]
    public AudioClip tapSfx;
    public AudioClip catchSfx;
    public AudioClip birdHappyLoop;

    public UnityEvent OnCaught; // hook GameManager to show Reward

    // internal
    private Vector3 birdStart;
    private Vector3 pigStart;
    private Vector3 birdTarget;
    private Vector3 pigTarget;
    private bool isCaught = false;
    private Coroutine birdHopCoroutine;
    private AudioSource sfxSource;
    private AudioSource birdLoopSource;

    void Start()
    {
        if (!bird || !pig) Debug.LogError("Assign bird and pig Transforms in CharacterChase");
        birdStart = bird.position;
        pigStart = pig.position;

        birdTarget = new Vector3(centerX - meetOffset, bird.position.y, bird.position.z);
        pigTarget = new Vector3(centerX + meetOffset, pig.position.y, pig.position.z);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        birdLoopSource = gameObject.AddComponent<AudioSource>();
        birdLoopSource.loop = true;
        birdLoopSource.playOnAwake = false;
        birdLoopSource.clip = birdHappyLoop;
    }

  public  void Update()
    {
        if (isCaught) // keep idle animation on pig head
        {
            // bird will be parented; keep idle bounce via local position if needed
            return;
        }

        // Idle bounce (visual) for both characters using Mathf.Sin
        float bob = Mathf.Sin(Time.time * idleBounceSpeed) * idleBounceHeight;
        Vector3 birdIdle = new Vector3(bird.position.x, birdStart.y + bob, bird.position.z);
        Vector3 pigIdle = new Vector3(pig.position.x, pigStart.y + bob, pig.position.z);

        // Lerp to keep positions smooth (bird/pig x controlled by taps)
        bird.position = Vector3.Lerp(bird.position, birdIdle, Time.deltaTime * positionLerpSpeed);
        pig.position = Vector3.Lerp(pig.position, pigIdle, Time.deltaTime * positionLerpSpeed);

        // Input: mouse or touch
        if (Input.GetMouseButtonDown(0)) HandleTap();

    }

    public void HandleTap()
    {
        if (isCaught) return;
        if (currentTaps >= tapsToMeet) return;

        currentTaps++;
        //Debug.Log("Tap detected! Progress: " + progress);//by lax me
        // Play tap SFX
        if (tapSfx) sfxSource.PlayOneShot(tapSfx);

        // Move their X positions by computed step such that after tapsToMeet taps they meet
        float birdStep = (birdTarget.x - birdStart.x) / (float)tapsToMeet;
        float pigStep = (pigTarget.x - pigStart.x) / (float)tapsToMeet;

        Vector3 newBirdPos = new Vector3(bird.position.x + birdStep, bird.position.y, bird.position.z);
        Vector3 newPigPos = new Vector3(pig.position.x + pigStep, pig.position.y, pig.position.z);

        // stop any previous hop coroutine and start a quick hop for bird and pig
        if (birdHopCoroutine != null) StopCoroutine(birdHopCoroutine);
        birdHopCoroutine = StartCoroutine(DoTapHop(bird, newBirdPos));
        StartCoroutine(DoTapHop(pig, newPigPos, false));

        // Check caught
        if (currentTaps >= tapsToMeet)
        {
            StartCoroutine(DoCaughtSequence());
        }
    }

    IEnumerator DoTapHop(Transform t, Vector3 targetPos, bool scaleHop = true)
    {
        // horizontal set instantly (target), vertical and scale animate as hop
        float startX = t.position.x;
        float endX = targetPos.x;
        float elapsed = 0f;

        Vector3 startPos = t.position;
        Vector3 endPos = new Vector3(endX, t.position.y, t.position.z);

        // scale hop (squash-stretch)
        Vector3 baseScale = t.localScale;
        Vector3 hopScale = baseScale * 1.08f;
        Vector3 squatScale = baseScale * 0.92f;

        while (elapsed < hopDuration)
        {
            float p = elapsed / hopDuration;
            // horizontal lerp
            float x = Mathf.Lerp(startX, endX, p);
            // vertical parabola for hop
            float yOff = 4f * hopHeight * p * (1 - p); // parabola peak at p=0.5
            t.position = new Vector3(x, startPos.y + yOff, startPos.z);

            // scale squash/stretch
            if (scaleHop)
            {
                if (p < 0.5f) t.localScale = Vector3.Lerp(baseScale, hopScale, p * 2f);
                else t.localScale = Vector3.Lerp(hopScale, baseScale, (p - 0.5f) * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // final
        t.position = endPos;
        t.localScale = baseScale;
    }

    IEnumerator DoCaughtSequence()
    {
        // mark caught so Update stops normal movement
        isCaught = true;

        if (catchSfx) sfxSource.PlayOneShot(catchSfx);

        // Make bird hop onto pig head (single hop), then parent bird to pig and set local pos
        Vector3 pigHeadWorld = pig.position + Vector3.up * 0.9f; // tweak per art
        // hop from current bird position to pigHeadWorld
        float duration = 0.35f;
        Vector3 start = bird.position;
        float elapsed = 0f;
        Vector3 baseScale = bird.localScale;
        while (elapsed < duration)
        {
            float p = elapsed / duration;
            // lerp pos
            bird.position = Vector3.Lerp(start, pigHeadWorld, p);
            // small scale bounce
            bird.localScale = baseScale * (1f + Mathf.Sin(p * Mathf.PI) * 0.08f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bird.position = pigHeadWorld;
        bird.localScale = baseScale;

        // Parent bird to pig and set local position (so it moves with pig/effects)
        bird.SetParent(pig, true);
        bird.localPosition = new Vector3(0f, 0.9f, 0f); // adjust to sit on head
        // start looping happy SFX on bird
        if (birdLoopSource.clip)
        {
            birdLoopSource.Play();
        }

        // pig defeated: squish scale and color change
        SpriteRenderer pigSr = pig.GetComponent<SpriteRenderer>();
        Vector3 pigBaseScale = pig.localScale;
        float squishDuration = 0.25f;
        elapsed = 0f;
        Color baseColor = pigSr ? pigSr.color : Color.white;
        Color squishColor = new Color(0.7f, 0.3f, 0.3f); // slightly reddish/pained

        while (elapsed < squishDuration)
        {
            float p = elapsed / squishDuration;
            // squish X slightly smaller, Y smaller too (but visually looks squished)
            pig.localScale = Vector3.Lerp(pigBaseScale, new Vector3(pigBaseScale.x * 0.85f, pigBaseScale.y * 0.7f, pigBaseScale.z), p);
            if (pigSr) pigSr.color = Color.Lerp(baseColor, squishColor, p);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pigSr) pigSr.color = squishColor;

        
        OnCaught?.Invoke();
    }

    // expose progress (0..1) for UI to query
    public float GetProgress01()
    {
        return Mathf.Clamp01(currentTaps / (float)tapsToMeet);
    }

  
    public void ResetChase()
    {
        StopAllCoroutines();
        currentTaps = 0;
        isCaught = false;
        birdLoopSource.Stop();

        // unparent bird and restore start positions/scales/colors
        bird.SetParent(null, true);
        bird.position = birdStart;
        pig.position = pigStart;
        bird.localScale = Vector3.one;
        pig.localScale = Vector3.one;
        SpriteRenderer pigSr = pig.GetComponent<SpriteRenderer>();
        if (pigSr) pigSr.color = Color.white;
    }
}
