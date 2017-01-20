using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour, ITouchable
{
    public bool isDelivered
    {
        get;
        private set;
    }
    private const float deliverDistance = 1f;
    static StageManager stageMgr;
    void Awake()
    {
        if(stageMgr == null)
        {
            stageMgr = GameObject.FindObjectOfType<StageManager>();
        }
    }

    public void Talk()
    {
        if(isDelivered == true)
        {
            return;
        }

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
        stageMgr.OnCharacterTouch(this);
    }
}