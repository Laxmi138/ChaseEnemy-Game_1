using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Image fillImage;    // Image type = Filled (left->right)
    public Transform segmentsParent; // optional: parent containing tick images
    public GameObject segmentPrefab; // optional prefab to spawn ticks
    public int segments = 5; // number of segments to show ticks; set to same as tapsToMeet

    public CharacterChase chaseController;

    void Start()
    {
        if (segmentsParent != null && segmentPrefab != null)
        {
            // spawn ticks at equal spacing
            for (int i = 0; i < segments; i++)
            {
                GameObject g = Instantiate(segmentPrefab, segmentsParent);
                float x = (i + 1) / (float)segments; // place ticks along parent rect (requires anchoring)
                RectTransform rt = g.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(x, 0.5f);
                rt.anchorMax = new Vector2(x, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }

  public  void Update()
    {
        if (chaseController != null && fillImage != null)
        {
            float p = chaseController.GetProgress01();
            fillImage.fillAmount = p;
        }
    }

    // helper to sync segments count in editor/runtime
    public void SetSegments(int n)
    {
        segments = Mathf.Max(1, n);
    }
}
