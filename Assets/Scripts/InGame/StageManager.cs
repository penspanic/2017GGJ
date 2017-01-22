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

    public bool deliverUpdated;
    void Awake()
    {
        FadeFilter.instance.FadeIn(new Color(0, 0, 0, 0), 1f);
        mapMgr = GameObject.FindObjectOfType<MapManager>();
        characters = mapMgr.CreateCharacters();
        ItemManager.Instance.SetCash(mapMgr.currentMapData.UsableCash);

        StartCoroutine(StartProcess());
    }

    IEnumerator StartProcess()
    {
        yield return new WaitForSeconds(1f);

        characters[0].MakeInstigater();

        yield return new WaitForSeconds(1f);

        IsGameEnd = false;
        OnCharacterTouch(characters[0], mapMgr.currentMapData.MaxDeliverCount);

        StartCoroutine(DeliverProcess());
    }

    IEnumerator DeliverProcess()
    {
        while (true)
        {
            if(deliverUpdated == true)
            {
                deliverUpdated = false;

                yield return new WaitForSeconds(2f);

                if(IsGameEnd == true)
                {
                    yield break;
                }

                if(deliverUpdated == false)
                {
                    foreach (Character eachCharacter in characters)
                    {
                        eachCharacter.SetDeliveredState(false);
                    }

                    OnCharacterTouch(characters[0], mapMgr.currentMapData.MaxDeliverCount);
                }
            }
            yield return null;
        }

    }

    void OnGameEnd(bool isClear)
    {

    }

    public void OnCharacterTouch(Character target, int deliverRemainCount)
    {
        if(deliverRemainCount == -1)
            return;
        if (target.isProtester == true)
            return;

        foreach(Character eachCharacter in characters)
        {
            if (eachCharacter == target)
                continue;
            if (eachCharacter.isDelivered == true)
                continue;

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
            if(eachCharacter.isInfected == true)
            {
                deliveredCharacterCount++;
            }
        }

        return (float)deliveredCharacterCount / (float)normalCharacterCount;
    }
}