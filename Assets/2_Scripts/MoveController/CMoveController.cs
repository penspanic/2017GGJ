using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMoveController : MonoBehaviour {

	Action StartMove;
	Action EndMove;
	CNpcsAnimationController _ani;
	[SerializeField] float _moveRange = 0.3f;
	[SerializeField] float _moveSpeed = 0.5f;
	Vector3 _minLimitPosition;
	Vector3 _maxLimitPosition;
	Vector3 _myOriginPosition;
	Vector3 _targetPosition;
	Task _moveMachine;
    bool _isGameEnd=false;

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		_ani = GetComponent<CNpcsAnimationController>();
	}
	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		StartMove += _ani.TurnIdle;
		EndMove += _ani.TurnMove;
		_targetPosition = transform.position;
		_minLimitPosition = new Vector3(transform.position.x-_moveRange,transform.position.y-_moveRange);
		_maxLimitPosition = new Vector3(transform.position.x+_moveRange,transform.position.y+_moveRange);
		StartMoveMachine();
	}
	/// <summary>
	/// This function is called when the object becomes enabled and active.
	/// </summary>
	public void StartMoveMachine()
	{
		_moveMachine = new Task(MoveControll_Co());
	}

	public void StopMoveMachine()
	{
		if (_moveMachine != null && _moveMachine.Running)
		{
			_moveMachine.Stop();
		}
        EndSetPosition();
	}
	IEnumerator MoveControll_Co()
	{
		float delayTime = UnityEngine.Random.Range(0.5f, 2.8f);
        yield return new WaitForSeconds(delayTime);
		while (true)
		{
			 delayTime = UnityEngine.Random.Range(5f, 10f);
            yield return new WaitForSeconds(delayTime);
			StartMove();
			_targetPosition = LockOnToTarget();
//			Debug.Log(_targetPosition);
		}
	}
	public void EndSetPosition()
    {
        //자기칸 맨밑 오브젝트 찾기 
        _targetPosition = GameObject.FindGameObjectsWithTag("Character").
                            Where(x =>Mathf.Abs(x.transform.position.x-transform.position.x)<0.5f).
                            OrderBy(x => x.transform.position.y).
                            FirstOrDefault().transform.position;
        //        Debug.Log("target:"+_targetPosition+"my"+transform.position);
        _isGameEnd = true;
        StartMove();
    }
	// Update is called once per frame
	void Update () {
		float px = transform.position.x;
		float py = transform.position.y;
		
		float targetX = Mathf.Lerp(px,_targetPosition.x,_moveSpeed*Time.deltaTime);
		float targetY = Mathf.Lerp(py,_targetPosition.y,_moveSpeed*Time.deltaTime);
        if (!_isGameEnd)
        {
            targetX = Mathf.Clamp(targetX, _minLimitPosition.x, _maxLimitPosition.x);
            targetY = Mathf.Clamp(targetY, _minLimitPosition.y, _maxLimitPosition.y);
        }
		transform.position = new Vector3(targetX,targetY,0);
		if (Vector3.Distance(transform.position,_targetPosition)<0.005f)
		{
			EndMove();
		}
	}

	Vector3 LockOnToTarget()
	{
		float randomX = UnityEngine.Random.Range(_minLimitPosition.x,_maxLimitPosition.x);
		float randomY = UnityEngine.Random.Range(_minLimitPosition.y,_maxLimitPosition.y);

		float targetX = _myOriginPosition.x + randomX;
		float targetY = _myOriginPosition.y + randomY;

		return new Vector3(targetX,targetY);
	}
}
