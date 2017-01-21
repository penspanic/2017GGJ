using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    Character[] characters;

    public bool IsGameEnd
    {
        get;
        private set;
    }

    void Awake()
    {
        CreateCharacters();
    }

    void CreateCharacters()
    {
        GameObject characterPrefab = Resources.Load<GameObject>("Prefab/Character");
        List<Character> charcterList = new List<Character>();
        for(int x = 0; x < 10; ++x)
        {
            for(int y = 0; y < 12; ++y)
            {
                GameObject character = Instantiate(characterPrefab);

                Vector3 createPos = new Vector3(-3.16f + 0.7f * x, -4.08f + 0.9f * y, 0);

                character.transform.position = createPos;

                charcterList.Add(character.GetComponent<Character>());
            }
        }

        characters = charcterList.ToArray();
    }

    public void OnCharacterTouch(Character target, int deliverRemainCount)
    {
        if(deliverRemainCount == 0)
        {
            return;
        }

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
        return 0.5f;
    }
}
