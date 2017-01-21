using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    #region Singleton

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return _instance;
        }
    }

    #endregion
    public int selectedStageNum = 1;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
