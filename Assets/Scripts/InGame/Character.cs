using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour, ITouchable
{
    public bool isDelivered
    {
        get;
        private set;
    }

    public int maxDeliverCount;
    public bool isAmplier;

    private const float deliverDistance = 1f;
    private const float minBallonDisappearTime = 0.7f;
    private const float maxBallonDisappearTime = 1.5f;
    static StageManager stageMgr;

    private SpriteRenderer ballonSprRenderer;
    void Awake()
    {
        if(stageMgr == null)
        {
            stageMgr = GameObject.FindObjectOfType<StageManager>();
        }

        ballonSprRenderer = transform.FindChild("Ballon").GetComponent<SpriteRenderer>();
        ballonSprRenderer.enabled = false;

        isDelivered = false;
    }

    public void Talk(int deliverCount)
    {
        if(isDelivered == true || this.gameObject.activeSelf == false)
        {
            return;
        }

        isDelivered = true;

        StartCoroutine(TalkProcess(deliverCount));
    }

    private IEnumerator TalkProcess(int deliverCount)
    {

        float time = Random.Range(minBallonDisappearTime, maxBallonDisappearTime);

        float elapsedTime = 0f;
        ballonSprRenderer.enabled = true;

        yield return new WaitForSeconds(time);

        time = 0.3f;
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            ballonSprRenderer.color = new Color(1, 1, 1, 1f - elapsedTime / time);

            yield return null;
        }

        stageMgr.OnCharacterTouch(this, isAmplier ? maxDeliverCount : deliverCount);
        gameObject.SetActive(false);
    }

    public bool IsInDeliverRange(Character target)
    {
        if ((target.transform.position - this.transform.position).magnitude < deliverDistance)
            return true;

        return false;
    }

    public void OnTouch()
    {
        if (isDelivered == true)
            return;

        stageMgr.OnCharacterTouch(this, maxDeliverCount);
    }
}