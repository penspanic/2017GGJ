using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class DropItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnEnable()
    {

    }

    public void OnDrop(PointerEventData data)
    {
        Debug.Log("ItemDroped" + " " + gameObject.name);
    }

    public void OnPointerEnter(PointerEventData data)
    {

    }

    public void OnPointerExit(PointerEventData data)
    {

    }
}
