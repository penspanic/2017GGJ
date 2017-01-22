using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour
{

    public GameObject itemUseEffectPrefab;

    void Awake()
    {

    }

    public void ShowEffect(Vector2 pos)
    {
        GameObject newEffect = Instantiate(itemUseEffectPrefab);
        newEffect.transform.position = pos;
    }

    public void OnEffectEnd(GameObject effect)
    {
        Destroy(effect);
    }
}