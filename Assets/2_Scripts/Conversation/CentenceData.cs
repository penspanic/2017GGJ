using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentenceData : MonoBehaviour {

	public int _countNumber;
	public string _centence;
	List<string> _centences = new List<string>();
	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		 string[] strs = _centence.Split("".ToCharArray(), System.StringSplitOptions.None);
        foreach (string str in strs)
        {
			_centences.Add(str);
        }
	}
	
	public List<string> GetCentences()
	{
		return _centences;
	}

}
