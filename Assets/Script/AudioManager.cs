using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public float bgmFadeDuration = 1f;

    public static AudioManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PlayBgm(AudioClip clip, bool loop = true)
    {
        if (!bgmSource) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void FadeOutBgm()
    {
        if (bgmSource) StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < bgmFadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVol;
    }
}
