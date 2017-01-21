using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Ballon : MonoBehaviour
{
    Text wordText;
    SpriteRenderer sprRenderer;
    void Awake()
    {
        wordText = transform.FindChild("Canvas").FindChild("Text").GetComponent<Text>();
        sprRenderer = GetComponent<SpriteRenderer>();

        wordText.enabled = false;
        sprRenderer.enabled = false;
    }

    public void Show(string word, float disappearTime)
    {
        Hide();

        wordText.enabled = true;
        sprRenderer.enabled = true;

        wordText.text = word;

        StopCoroutine(ShowProcess(-1f));

        StartCoroutine(ShowProcess(disappearTime));
    }

    IEnumerator ShowProcess(float disappearTime)
    {

        yield return new WaitForSeconds(disappearTime);

        float elapsedTime = 0f;
        disappearTime = 0.5f;

        while(elapsedTime < disappearTime)
        {
            elapsedTime += Time.deltaTime;

            wordText.color = new Color(0f, 0f, 0f, 1f - elapsedTime / disappearTime);
            sprRenderer.color = new Color(1f, 1f, 1f, 1f - elapsedTime / disappearTime);

            yield return null;
        }
        Hide();
    }

    public void Hide()
    {
        wordText.enabled = false;
        sprRenderer.enabled = false;

        wordText.color = new Color(0f, 0f, 0f, 1f);
        sprRenderer.color = new Color(1f, 1f, 1f, 1f);
    }
}