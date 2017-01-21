using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pattern;

public enum ItemType
{
    None,
    Instigator,
}

public class ItemManager : Singleton<ItemManager>
{
    public ItemType currentSelect
    {
        get { return _currentSelect; }
    }

    ItemType _currentSelect;

    public static readonly int AmplierItemCost = 100;

    int currentRemainCash = 0;

    public void TurnItem(ItemType item)
    {
        _currentSelect = item;
    }

    public bool IsSelected()
    {
        // 갯수 처리
        // 떨궜을 때 NONE으로 변경
        return _currentSelect.Equals(ItemType.None) ? false : true;
    }

    void Awake()
    {
        _currentSelect = ItemType.None;
    }

    public void SetCash(int cash)
    {
        currentRemainCash = cash;
    }
}
