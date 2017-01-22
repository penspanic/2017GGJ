using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class DropItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    ItemManager itemMgr;

    void Awake()
    {
        itemMgr = GameObject.FindObjectOfType<ItemManager>();
    }

    public void OnDrop(PointerEventData data)
    {
        if (itemMgr.currentSelect == ItemType.Instigator)
        {
            if (itemMgr.currentRemainCash >= ItemManager.InstigatorItemCost)
            {
                if(GetComponent<Character>().MakeInstigater() == true)
                    itemMgr.UseItemCost(ItemType.Instigator);
            }
        }
        else if(itemMgr.currentSelect == ItemType.Kill)
        {
            if(itemMgr.currentRemainCash >= ItemManager.KillItemCost)
            {
                if (GetComponent<Character>().KillAndChange() == true)
                    itemMgr.UseItemCost(ItemType.Kill);
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
