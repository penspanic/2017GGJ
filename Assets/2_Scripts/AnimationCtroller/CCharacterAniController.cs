using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterAniController : MonoBehaviour
{
    protected Animator _ani;
    protected List<string> _idles = new List<string>();
    protected Task _idleMachine;
    protected Ballon ballon;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        ballon = GetComponentInChildren<Ballon>();
        _ani = GetComponent<Animator>();
    }
    // Use this for initialization
    void Start()
    {
        _idles.Add("Idle1");
        _idles.Add("Idle2");
        _idles.Add("Idle3");
        _idles.Add("Idle4");
        StartMachine();
    }

    void StartMachine()
    {
        _idleMachine = new Task(IdleMachine_Co());
    }
    void StopMachine()
    {
        if (_idleMachine != null && _idleMachine.Running)
        {
            _idleMachine.Stop();
        }
    }

    void PauseMachine()
    {
        if (_idleMachine != null && _idleMachine.Running)
        {
            _idleMachine.Pause();
        }
    }
    void UnPauseMachine()
    {
        if (_idleMachine != null && _idleMachine.Paused)
        {
            _idleMachine.Unpause();
        }
    }

    protected virtual IEnumerator IdleMachine_Co()
    {
        float delayTime = Random.Range(0f, 10f);
        yield return new WaitForSeconds(delayTime);
        int randomIdleString = Random.Range(0, _idles.Count);
        _ani.SetTrigger(_idles[randomIdleString]);
        while (true)
        {
            delayTime = Random.Range(10f, 15f);
            yield return new WaitForSeconds(delayTime);
            randomIdleString = Random.Range(0, _idles.Count);
            _ani.SetTrigger(_idles[randomIdleString]);
        }
    }
    public void TurnIdle()
    {
        _ani.SetBool("noAct", true);
        _ani.SetBool("Move", false);
        UnPauseMachine();
    }

    public void TurnMove()
    {
        PauseMachine();
        _ani.SetBool("noAct", false);
        _ani.SetBool("Move", true);
    }

}
