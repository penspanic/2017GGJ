using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNpcsAnimationController : CCharacterAniController
{
    [SerializeField]
    float _jumpDelay = 1.0f;
    Task _jumpMachine;
    bool _isStartJump = false;
  
    public void ChangeMachine()
    {
        if (_idleMachine != null)
        {
            _idleMachine.Stop();
            _idleMachine = null;
        }
        if (_isStartJump) return;
        _jumpMachine = new Task(JumpMachine_Co());
    }
    
    //점프코루틴 
    protected virtual IEnumerator JumpMachine_Co()
    {
        WaitForSeconds jumpDelay = new WaitForSeconds(_jumpDelay/2);
         float delayTime = Random.Range(0f, 1f);
         yield return new WaitForSeconds(delayTime);
         _ani.SetTrigger("Jump");
         while (true)
        {
            yield return jumpDelay;
            ShowBallon("^-^",0.5f);
            yield return jumpDelay;
            ShowBallon("^-^", 0.5f);
            _ani.SetTrigger("Jump");
        }
    }
    public void ShowBallon(string talk, float time)
    {
        ballon.Show(talk, time);
    }

}
