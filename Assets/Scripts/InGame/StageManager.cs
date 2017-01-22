using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public GameObject gameClear;
    public GameObject gameFail;

    Character[] characters;
    MapManager mapMgr;
    ItemManager itemMgr;

    public bool IsGameEnd
    {
        get;
        private set;
    }

    public bool isDeliverUpdated;
    void Awake()
    {
        FadeFilter.instance.FadeIn(new Color(0, 0, 0, 0), 1f);
        mapMgr = GameObject.FindObjectOfType<MapManager>();
        itemMgr = GameObject.FindObjectOfType<ItemManager>();

        characters = mapMgr.CreateCharacters();
        itemMgr.SetCash(mapMgr.currentMapData.UsableCash);

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
            if(isDeliverUpdated == true)
            {
                isDeliverUpdated = false;

                yield return new WaitForSeconds(2f);

                if(isDeliverUpdated == false)
                {
                    CheckGameEnd();

                    if(IsGameEnd == true)
                    {
                        yield break;
                    }

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

    void CheckGameEnd()
    {
        // 게임 패배 체크
        if(itemMgr.CanBuyItem() == false && IsAllInfected() == false)
        {
            OnGameEnd(false);
            return;
        }
        //

        // 게임 승리 체크
        if(IsAllInfected() == true)
        {
            OnGameEnd(true);
        }
        //
    }

    bool IsAllInfected()
    {
        foreach (Character eachCharacter in characters)
        {
            if (eachCharacter.isProtester == false && eachCharacter.isInfected == false)
            {
                return false;
            }
        }

        return true;
    }

    void OnGameEnd(bool isClear)
    {
        Debug.Log("isClear : " + isClear.ToString());
        IsGameEnd = true;
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

    #region UI Handler
    bool isSceneChanging = false;
    public void OnPowerButtonDown()
    {
        if(isSceneChanging == true)
        {
            return;
        }
        isSceneChanging = true;

        FadeFilter.instance.FadeOut(Color.black, 1f);
        Invoke("GotoStageSelect", 1f);
    }

    void GotoStageSelect()
    {
        SceneManager.LoadScene("SelectStage");
    }

    #endregion
}