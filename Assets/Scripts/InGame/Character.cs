using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour, ITouchable
{
    public bool isDelivered
    {
        get;
        private set;
    }
    public bool isProtester
    {
        get;
        private set;
    }

    public int maxDeliverCount;
    public bool isInstigator;

    private const float deliverDistance = 1f;
    private const float minBallonDisappearTime = 0.6f;
    private const float maxBallonDisappearTime = 0.9f;
    static StageManager stageMgr;

    private SpriteRenderer ballonSprRenderer;
    private Ballon ballon;
    void Awake()
    {
        if(stageMgr == null)
        {
            stageMgr = GameObject.FindObjectOfType<StageManager>();
        }

        ballon = transform.FindChild("Ballon").GetComponent<Ballon>();

        isDelivered = false;
    }

    public void Talk(int deliverCount)
    {
        if(isDelivered == true || isProtester == true)
        {
            return;
        }

        isDelivered = true;

        StartCoroutine(TalkProcess(deliverCount));
    }

    private IEnumerator TalkProcess(int deliverCount)
    {

        float time = Random.Range(minBallonDisappearTime, maxBallonDisappearTime);

        ballon.Show("dadss", time);

        yield return new WaitForSeconds(time);

        stageMgr.OnCharacterTouch(this, isInstigator ? maxDeliverCount : deliverCount);
    }

    public bool IsInDeliverRange(Character target)
    {
        if (this.gameObject.activeSelf == false)
            return false;

        if ((target.transform.position - this.transform.position).magnitude < deliverDistance)
            return true;

        return false;
    }

    public void OnTouch()
    {
        return;

        if (isDelivered == true || isProtester == true || stageMgr.IsGameEnd == true)
            return;

        stageMgr.OnCharacterTouch(this, maxDeliverCount);
    }

    public void SetCharacterType(CellType cell)
    {
        isProtester = cell == CellType.Protester;
    }

    public void SetDeliveredState(bool state)
    {
        isDelivered = state;

        if(isDelivered == false)
        {
            StopCoroutine(TalkProcess(-1));
        }
    }

    public void MakeInstigater()
    {
        if(isProtester == true)
        {
            return;
        }

        isInstigator = true;

        GetComponent<CharacterVarietyController>().ChangeToAgent();
    }
}