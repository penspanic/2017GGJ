using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    Character[] characters;
    MapManager mapMgr;

    public bool IsGameEnd
    {
        get;
        private set;
    }

    void Awake()
    {
        mapMgr = GameObject.FindObjectOfType<MapManager>();
        characters = mapMgr.CreateCharacters();
    }

    public void OnCharacterTouch(Character target, int deliverRemainCount)
    {
        if(deliverRemainCount == 0)
            return;
        if (target.isProtester == true)
            return;

        foreach(Character eachCharacter in characters)
        {
            if (eachCharacter == target)
                continue;
            if (eachCharacter.isDelivered == true)
                continue;

            target.gameObject.SetActive(false);

            if (eachCharacter.IsInDeliverRange(target) == true)
            {
                eachCharacter.Talk(deliverRemainCount - 1);
            }
        }
    }

    public float GetDeliveredRate()
    {
        int normalCharacterCount = 0;
        int deliveredCharacterCount = 0;
        foreach(Character eachCharacter in characters)
        {
            if(eachCharacter.isProtester == true)
            {
                continue;
            }
            ++normalCharacterCount;
            if(eachCharacter.isDelivered)
            {
                deliveredCharacterCount++;
            }
        }

        return (float)deliveredCharacterCount / (float)normalCharacterCount;
    }
}