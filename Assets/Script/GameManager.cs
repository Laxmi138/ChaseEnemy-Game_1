using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject gameUI;
    public GameObject rewardScreen;
    public Button playButton;
    public Button backButton;

    public CharacterChase chaseController;
    public ProgressBarController progressBar;
    public AudioClip bgmClip;

    void Start()
    {
        ShowStart();
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (backButton != null) backButton.onClick.AddListener(BackToStart);
        if (chaseController != null) chaseController.OnCaught.AddListener(OnCaught);
        // start BGM (but paused until Play)
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBgm(bgmClip, true);
    }

    public void ShowStart()
    {
        startScreen.SetActive(true);
        gameUI.SetActive(false);
        rewardScreen.SetActive(false);
    }

    public void StartGame()
    {
        startScreen.SetActive(false);
        gameUI.SetActive(true);
        rewardScreen.SetActive(false);

        // sync segments to tapsToMeet
        if (progressBar != null)
        {
            progressBar.SetSegments(chaseController.tapsToMeet);
            progressBar.chaseController = chaseController;
        }

        // reset characters
        chaseController.ResetChase();

        // ensure BGM playing
        if (AudioManager.instance != null && AudioManager.instance.bgmSource != null && !AudioManager.instance.bgmSource.isPlaying)
            AudioManager.instance.bgmSource.Play();
    }

    public void BackToStart()
    {
        ShowStart();
        // stop bgm if you want or keep playing
    }

   public void OnCaught()
    {
        // Fade out BGM
        if (AudioManager.instance != null) AudioManager.instance.FadeOutBgm();

        // Show Reward Screen after small delay to allow catch animation
        Invoke(nameof(ShowReward), 0.6f);
    }

    void ShowReward()
    {
        gameUI.SetActive(false);
        rewardScreen.SetActive(true);
    }

    public void Replay()
    {
        StartGame();
    }
}

