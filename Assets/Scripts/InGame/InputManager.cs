using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            //    return;

            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 레이캐스트 사용

            RaycastHit2D[] hitInfo = Physics2D.RaycastAll(touchPos, Vector2.zero);

            for (int i = 0; i < hitInfo.Length; ++i)
            {
                if (hitInfo[i].collider != null)
                {
                    GameObject hitObject = hitInfo[i].collider.gameObject;

                    if (hitObject.GetComponent<ITouchable>() != null)
                    {
                        hitObject.GetComponent<ITouchable>().OnTouch();
                    }
                }
            }
        }
    }
}
