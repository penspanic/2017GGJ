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
        for(int x = 0; x < 36; ++x)
        {
            for(int y = 0; y< 64; ++y)
            {
                GameObject character = Instantiate(characterPrefab);

                Vector3 createPos = new Vector3(-3.6f + 0.2f * x, -6.4f + 0.2f * y, 0);

                character.transform.position = createPos;

                charcterList.Add(character.GetComponent<Character>());
            }
        }

        characters = charcterList.ToArray();
    }

    public void OnCharacterTouch(Character target)
    {
        foreach(Character eachCharacter in characters)
        {
            if (eachCharacter == target)
                return;
            if (eachCharacter.isDelivered == true)
                return;

            if (eachCharacter.IsInDeliverRange(target) == true)
            {
                eachCharacter.Talk();
            }
        }
    }
}
