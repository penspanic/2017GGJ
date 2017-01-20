using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pattern;
using System.Reflection;

public class InputHelper : Singleton<InputHelper>
{

    public Camera inputCamera;
    private GameObject[] _selectObject = new GameObject[5];
    private bool[] _isClick = new bool[] { false, false, false, false, false };

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {

    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        string returnTag = "Untagged";
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            CheckPress(Input.mousePosition, ref returnTag, 0);
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_isClick[0])
            {
                CheckDoubleClick(Input.mousePosition, ref returnTag, 0);
            }
            else
            {
                CheckClick(Input.mousePosition, ref returnTag, 0);
            }
        }
#endif
#if UNITY_ANDROID
	//touch
#endif
    }
    public bool CheckSelectObject(string tag)
    {
        bool returnbool = false;
        GameObject getObj = GetObject(tag);

        if (getObj != null && getObj.tag.Equals(tag))
        {
            return true;
        }
        return returnbool;
    }
    public GameObject GetObject(string tag)
    {
        GameObject returnobj = null;
        LayerMask lay = LayerMask.NameToLayer(tag);
        returnobj = ShootToTheRayToVoid(GetPoint());
        if (returnobj != null)
        {
            Debug.Log(returnobj.ToString());
        }
        return returnobj;
    }
    Vector3 GetPoint()
    {
#if UNITY_EDITOR
        return Input.mousePosition;
#endif
#if UNITY_ANDROID

		return Input.GetTouch(0).position;
#endif
    }
    Vector3 GetScreenPosition(Vector3 point)
    {
        return inputCamera.ScreenToWorldPoint(point);
    }
    GameObject ShootToTheRayToVoid(Vector3 rayPoint)
    {
        Vector3 touchPosWorld = inputCamera.ScreenToWorldPoint(rayPoint);

        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);

        RaycastHit2D hit = Physics2D.Raycast(touchPosWorld2D, inputCamera.transform.forward);
        Debug.DrawRay(touchPosWorld2D, inputCamera.transform.forward, Color.blue, 1.0f);
        if (hit.collider != null)   //마우스 근처에 오브젝트가 있는지 확인
        {
            //있으면 오브젝트를 저장한다.
            return hit.collider.gameObject;
        }
        return null;
    }
    void Clear(int indexNum)
    {
        _selectObject[indexNum] = null;
        _isClick[indexNum] = false;
    }

    void SendMessageToObject(string method, int indexNum = 0)
    {
        _selectObject[indexNum].SendMessage(method, SendMessageOptions.DontRequireReceiver);
    }

    void CheckDoubleClick(Vector3 point, ref string rTag, int indexNum)
    {
        if (null != _selectObject[indexNum].GetType().GetMethod("OnDoubleClick"))
        {
            CheckClick(point, ref rTag, indexNum);
        }
        else
        {
            if (ClickCheckMothod(point, _selectObject[indexNum], "OnDoubleClick"))
            {
                Debug.Log("DoubleClick");
            }
            else
            {
            }
        }
        Clear(indexNum);
    }
    void CheckClick(Vector3 point, ref string rTag, int indexNum)
    {

        if (ClickCheckMothod(point, _selectObject[indexNum], "OnClick"))
        {
            _isClick[indexNum] = true;
            //Debug.Log("Click");
        }
        else
        {
            _isClick[indexNum] = false;
        }

    }
    void CheckPress(Vector3 point, ref string rTag, int indexNum)
    {
        _selectObject[indexNum] = ShootToTheRayToVoid(point);
        if (null != _selectObject[indexNum])
        {
            //           rTag = _selectObject[indexNum].transform.tag;
            SendMessageToObject("OnPress");
        }
        else
        {
            Clear(indexNum);
        }
    }
    bool ClickCheckMothod(Vector3 point, GameObject go, string method)
    {
        GameObject TakenObject = ShootToTheRayToVoid(point);
        if (null != TakenObject)
        {
            if (TakenObject.Equals(go))
            {
                SendMessageToObject(method);
                return true;
            }
            else
            {
				SendMessageToObject("OnVoidClick");
                return false;
            }
        }
        else
            return false;
    }
}
