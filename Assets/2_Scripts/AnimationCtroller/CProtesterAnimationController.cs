using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CProtesterAnimationController : CNpcsAnimationController {

	protected override IEnumerator IdleMachine_Co()
	{
		float delayTime = Random.Range(0f, 1.8f);
        yield return new WaitForSeconds(delayTime);
        int randomIdleString = Random.Range(0, _idles.Count);
        _ani.SetTrigger(_idles[randomIdleString]);
        while (true)
        {
            delayTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(delayTime);
            randomIdleString = Random.Range(0, _idles.Count);
            _ani.SetTrigger(_idles[randomIdleString]);
        }
	}
}
