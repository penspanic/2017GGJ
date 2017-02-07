using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CItemEffectDoumi : MonoBehaviour
{

    Character _myChar;
    StageManager _thisStage;
    public GameObject[] neighbors = new GameObject[4];
    public SpriteRenderer _black;
    Vector3 _originPos;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _myChar = GetComponent<Character>();
        _originPos = transform.position;
    }
    // Use this for initialization
    void Start()
    {
        _thisStage = GameObject.Find("Manager").GetComponent<StageManager>();
		neighbors = _thisStage.AllChar().Where(go => (SideCheck(go.transform) == true)&&!(this.Equals(go))).Select(go => go).OrderBy(go=>Vector3.Distance(transform.position,go.transform.position)).ToArray();
    }

    public void OnpointerEnterAction(int count)
    {
//        Debug.Log(count);
        if (!_black.enabled&&!_myChar.isProtester)
        {
            _black.enabled = true;
        }
        if (count == 0) return;
        foreach (GameObject item in neighbors)
        {
            item.SendMessage("OnpointerEnterAction", count - 1, SendMessageOptions.DontRequireReceiver);
        }
    }
    public void OnpointerExitAction(int count)
    {
        if (!_myChar.isInfected&&!_myChar.isProtester)
        {
            _black.enabled = false;
        }
        if (count == 0) return;
        foreach (GameObject item in neighbors)
        {
            item.SendMessage("OnpointerExitAction", count - 1, SendMessageOptions.DontRequireReceiver);
        }
    }

    bool SideCheck(Transform pos)
    {
        float charDist = Mathf.Abs(transform.position.x - pos.position.x) + Mathf.Abs(transform.position.y - pos.position.y);
        return charDist < 1.2f ? true : false;
    }
}
