using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public enum CellType
{
    Normal,
    Protester,
}

public struct MapData
{
    public int StageNum;
    public int MaxDeliverCount;
    public int UsableCash;
    public int[,] CellData;
}

public class MapManager : MonoBehaviour
{
    public TextAsset mapDataFile;

    List<MapData> stageMapDataList = new List<MapData>();
    public MapData currentMapData;
    void Awake()
    {

    }

    void LoadMaps()
    {
        JsonData dataObject = JsonMapper.ToObject(mapDataFile.text);

        int stageCount = dataObject["Stage"].Count;
        for(int i = 0; i < stageCount; ++i)
        {
            MapData newData = new MapData();
            newData.StageNum = int.Parse(dataObject["Stage"][i]["StageNum"].ToString());
            newData.MaxDeliverCount = int.Parse(dataObject["Stage"][i]["MaxDeliverCount"].ToString());
            newData.UsableCash = int.Parse(dataObject["Stage"][i]["UsableCash"].ToString());

            JsonData mapDataObject = dataObject["Stage"][i]["MapData"];

            int rowCount = 11;
            int colCount = 10;

            newData.CellData = new int[colCount, rowCount];

            for (int y = 0; y < rowCount; ++y)
            {
                for(int x = 0; x < colCount; ++x)
                {
                    int data = int.Parse(mapDataObject[y]["Row"][x].ToString());
                    newData.CellData[x, y] = data;
                }
            }

            stageMapDataList.Add(newData);
        }
    }

    public Character[] CreateCharacters()
    {
        LoadMaps();

        currentMapData = stageMapDataList[GameManager.instance.selectedStageNum - 1];
        GameObject characterPrefab = Resources.Load<GameObject>("Prefab/Character");
        GameObject protesterPrefab = Resources.Load<GameObject>("prefab/Protester");

        List<Character> characterList = new List<Character>();
        Transform characterParent = GameObject.Find("Characters").transform;
        for (int x = 0; x < 10; ++x)
        {
            for (int y = 0; y < 11; ++y)
            {
                CellType type = (CellType)currentMapData.CellData[x, y];

                Character character = null;
                if (type == CellType.Normal)
                    character = Instantiate(characterPrefab).GetComponent<Character>();
                else if (type == CellType.Protester)
                    character = Instantiate(protesterPrefab).GetComponent<Character>();

                character.transform.SetParent(characterParent);

                Vector3 createPos = new Vector3(-3.16f + 0.7f * x, 5f - 0.9f * y, 0);
                character.transform.position = createPos;

                character.SetCharacterType(type);
                character.GetComponent<CharacterVarietyController>().Set(type);
                characterList.Add(character);
            }
        }

        return characterList.ToArray();
    }
}
