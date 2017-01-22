using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour
{

    public void OnEffectEnd()
    {
        GameObject.FindObjectOfType<EffectManager>().OnEffectEnd(this.gameObject);
    }
}
