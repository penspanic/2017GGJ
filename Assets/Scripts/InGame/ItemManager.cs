using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pattern;

public enum ItemType
{
    None,
    Instigator,
    Kill
}

public class ItemManager : MonoBehaviour
{
    public ItemType currentSelect
    {
        get { return _currentSelect; }
    }

    ItemType _currentSelect;

    public static readonly int InstigatorItemCost = 100;
    public static readonly int KillItemCost = 200;
    public int currentRemainCash
    {
        get;
        private set;
    }

    Text remainMoneyText;

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

        remainMoneyText = GameObject.Find("Money Text").GetComponent<Text>();
        remainMoneyText.text = currentRemainCash.ToString();
    }

    public void SetCash(int cash)
    {
        currentRemainCash = cash;

        if (remainMoneyText != null)
            remainMoneyText.text = cash.ToString();
    }

    public void UseItemCost(ItemType type)
    {
        if (type == ItemType.Instigator)
        {
            AppSound.instance.SE_ITEM_MONEY.Play();
            currentRemainCash -= InstigatorItemCost;
        }
        else if (type == ItemType.Kill)
        {
            AppSound.instance.SE_ITEM_KNIFE.Play();
            currentRemainCash -= KillItemCost;
        }

        if (currentRemainCash < 0)
            currentRemainCash = 0;

        remainMoneyText.text = currentRemainCash.ToString();
    }

    public bool CanBuyItem()
    {
        if (currentRemainCash < InstigatorItemCost)
            return false;

        return true;
    }
}
