using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public bool isDelivered // 메세지 전달 시스템을 만들기 위해 존재
    {
        get;
        private set;
    }
    public bool isInfected // 빨간 티셔츠를 입었는가
    {
        get;
        private set;
    }
    public bool isProtester
    {
        get;
        private set;
    }

    public bool isInstigator;

    private const float deliverDistance = 1f;
    private const float minBallonDisappearTime = 0.6f;
    private const float maxBallonDisappearTime = 0.9f;
    static StageManager stageMgr;
    static MapManager mapMgr;

    private SpriteRenderer ballonSprRenderer;
    private Ballon ballon;
    private int lastDeliverRemainCount = 0; // 마지막으로 전달됬을 때 남은 전달 횟수
    void Awake()
    {
        if(stageMgr == null)
        {
            stageMgr = GameObject.FindObjectOfType<StageManager>();
            mapMgr = GameObject.FindObjectOfType<MapManager>();
        }

        ballon = transform.FindChild("Ballon").GetComponent<Ballon>();

        isDelivered = false;
    }

    public void Talk(int deliverCount)
    {
        if(isProtester == true)
        {
            return;
        }

        if(isDelivered == true && deliverCount < lastDeliverRemainCount)
        {
            return;
        }

        lastDeliverRemainCount = deliverCount;
        isDelivered = true;
        isInfected = true;

        StartCoroutine(TalkProcess(deliverCount));
    }

    private IEnumerator TalkProcess(int deliverCount)
    {
        stageMgr.isDeliverUpdated = true;

        float time = Random.Range(minBallonDisappearTime, maxBallonDisappearTime);

        ballon.Show("-----", time);

        yield return new WaitForSeconds(time);

        int maxDeliverCount = mapMgr.currentMapData.MaxDeliverCount;
        stageMgr.OnCharacterTouch(this, isInstigator ? maxDeliverCount : deliverCount);

        yield return new WaitForSeconds(0.5f);

        if(isInstigator == false)
            GetComponent<CharacterVarietyController>().ChangeToRedType(isInstigator);
    }

    public bool IsInDeliverRange(Character target)
    {
        if (this.gameObject.activeSelf == false)
            return false;

        if ((target.transform.position - originalPos).magnitude < deliverDistance)
            return true;

        return false;
    }

    Vector3 originalPos;

    void LateUpdate()
    {
        transform.FindChild("Black").position = originalPos;
    }

    public void SetCharacterType(CellType cell)
    {
        isProtester = cell == CellType.Protester;

        originalPos = transform.position;
    }

    public void SetDeliveredState(bool state)
    {
        isDelivered = state;

        if(isDelivered == false)
        {
            StopCoroutine(TalkProcess(-1));
        }
    }

    public bool MakeInstigater()
    {
        if(isProtester == true)
        {
            return false;
        }
        stageMgr.isDeliverUpdated = true;
        isInstigator = true;

        GetComponent<CharacterVarietyController>().ChangeToAgent();
        
        if(lastDeliverRemainCount > 0)
        {
            GetComponent<CharacterVarietyController>().ChangeToRedType(true);
        }
        return true;
    }

    public bool KillAndChange()
    {
        if (isInstigator == true || isProtester == false)
        {
            return false;
        }

        stageMgr.isDeliverUpdated = true;
        isInstigator = false;
        isProtester = false;

        GetComponent<CharacterVarietyController>().Set(CellType.Normal);

        return true;
    }
}