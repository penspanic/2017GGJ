using UnityEngine;
using System.Collections;
using Pattern;

public class PlayerPrefsManager : MonoBehaviour
{
    private static PlayerPrefsManager _instance;
    public static PlayerPrefsManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameObject("PlayerPrefsManager").AddComponent<PlayerPrefsManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Set(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public void Set(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public void Set(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public bool GetBool(string key)
    {
        return (PlayerPrefs.GetInt(key) == 1) ? true : false;
    }

    public string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public bool IsExist(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}