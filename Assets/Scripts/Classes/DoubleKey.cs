using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleKey{

	public DoubleKey(KeyCode firstKey, KeyCode secondKey){
		this.primaryKeyCode = firstKey;
		this.secondaryKeyCode = secondKey;
	}

	public KeyCode primaryKeyCode;
	public KeyCode secondaryKeyCode;

	public static DoubleKey FromPlayerPrefs(string name){
		string temp = PlayerPrefManager.GetString(name);
		KeyCode primary = (KeyCode)System.Enum.Parse(typeof(KeyCode), temp);
		temp = PlayerPrefManager.GetString(name + "_alt");
		KeyCode secondary = (KeyCode)System.Enum.Parse(typeof(KeyCode), temp);
		return new DoubleKey(primary, secondary);
	}

	public bool GetKeyDown(){
		return Input.GetKeyDown(primaryKeyCode) || Input.GetKeyDown(secondaryKeyCode);
	}

	public bool GetKeyUp(){
		return Input.GetKeyUp(primaryKeyCode) || Input.GetKeyUp(secondaryKeyCode);
	}

	public bool GetKey(){
		return Input.GetKey(primaryKeyCode) || Input.GetKey(secondaryKeyCode);
	}

}
