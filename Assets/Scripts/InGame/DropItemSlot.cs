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
        if(ItemManager.Instance.currentSelect == ItemType.Instigator)
        {
            if (ItemManager.Instance.currentRemainCash >= ItemManager.InstigatorItemCost)
            {
                if(GetComponent<Character>().MakeInstigater() == true)
                    ItemManager.Instance.UseItemCost(ItemType.Instigator);
            }
        }
        else if(ItemManager.Instance.currentSelect == ItemType.Kill)
        {
            if(ItemManager.Instance.currentRemainCash >= ItemManager.KillItemCost)
            {
                if (GetComponent<Character>().KillAndChange() == true)
                    ItemManager.Instance.UseItemCost(ItemType.Kill);
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {

    }

    public void OnPointerExit(PointerEventData data)
    {

    }
}
