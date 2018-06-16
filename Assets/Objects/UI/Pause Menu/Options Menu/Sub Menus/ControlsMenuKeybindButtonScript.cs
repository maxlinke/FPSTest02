using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuKeybindButtonScript : MonoBehaviour {

	public KeybinderScript keybinder;
	public Text buttonTextField;
	public Text labelTextField;

	public void OpenKeybinder(){
		keybinder.gameObject.SetActive(true);
		keybinder.callingButtonTextField = buttonTextField;
		keybinder.propertyName = gameObject.name;
		keybinder.actionNameTextField.text = labelTextField.text;
	}
}
