////////////////////////////////////////////////////////////////////////////////
//  
// @module IOS Deploy
// @author Stanislav Osipov (Stan's Assets) 
// @support support@stansassets.com
//
////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SA.IOSDeploy {

	[System.Serializable]
	public class Variable  {
		//Editor Use Only
		public bool IsOpen = true;
		public bool IsListOpen = true;

		public string Name = string.Empty;
		public PlistValueTypes Type         = PlistValueTypes.String;

		public string StringValue = string.Empty;
		public int IntegerValue = 0;
		public float FloatValue = 0;
		public bool BooleanValue = true;

		public List<string> ChildrensIds = new List<string> ();



		public void AddChild(Variable v){
			string key = SA.Common.Util.IdFactory.NextId.ToString();
			ISD_Settings.Instance.AddVariableToDictionary (key, v);
			ChildrensIds.Add(key);
		}
			
	}
}