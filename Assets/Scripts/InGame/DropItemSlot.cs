using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class DropItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]int _itemeffectRange=4;
    ItemManager itemMgr;
    Character _myChar;

    void Awake()
    {
        _myChar = GetComponent<Character>();
        itemMgr = GameObject.FindObjectOfType<ItemManager>();
    }

    public void OnDrop(PointerEventData data)
    {
        if (itemMgr.currentSelect == ItemType.Instigator)
        {
            GetComponent<CItemEffectDoumi>().OnpointerExitAction(_itemeffectRange);
            if (itemMgr.currentRemainCash >= ItemManager.InstigatorItemCost)
            {
                if(GetComponent<Character>().MakeInstigater() == true)
                    itemMgr.UseItemCost(ItemType.Instigator, transform.position);
            }
        }
        else if(itemMgr.currentSelect == ItemType.Kill)
        {
            if(itemMgr.currentRemainCash >= ItemManager.KillItemCost)
            {
                if (GetComponent<Character>().KillAndChange() == true)
                    itemMgr.UseItemCost(ItemType.Kill, transform.position);
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (itemMgr.currentSelect == ItemType.Instigator&&!_myChar.isProtester)
        {
            GetComponent<CItemEffectDoumi>().OnpointerEnterAction(_itemeffectRange);
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (itemMgr.currentSelect == ItemType.Instigator&&!_myChar.isProtester)
        {
            GetComponent<CItemEffectDoumi>().OnpointerExitAction(_itemeffectRange);
        }
    }
}
