using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractInfoScript : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	public Text whiteText;
	public Text whiteTextDuplicate;
	public Text blackText;

	public Color whiteTextColor;
	public Color blackTextColor;

	private string key_interact;

	void Start(){
		AddSelfToPlayerPrefObserverList();
		key_interact = PlayerPrefManager.GetString("key_interact");
		whiteTextColor = Color.white;
		blackTextColor = Color.black;
	}

	public void AddSelfToPlayerPrefObserverList(){
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged(){
		key_interact = PlayerPrefManager.GetString("key_interact");
	}

	public void EnableInteractDisplay(){
		whiteText.color = whiteTextColor;
		whiteTextDuplicate.color = whiteTextColor;
		blackText.color = blackTextColor;
	}

	public void DisableInteractDisplay(){
		whiteText.color = Color.clear;
		whiteTextDuplicate.color = Color.clear;
		blackText.color = Color.clear;
	}

	public void SetInteractDisplayMessage(string message){
		if(message == null){
			UseErrorColors();
			SetText("NULLREFERENCE, PROBABLY NO <IInteractable> ON THE OBJECT!");
		}else if(message.Equals("")){
			SetText("");
		}else{
			UseDefaultColors();
			SetText("[" + key_interact.ToString() + "] " + message);
		}
	}

	private void SetText(string text){
		whiteText.text = text;
		whiteTextDuplicate.text = text;
		blackText.text = text;
	}

	private void UseDefaultColors(){
		whiteTextColor = Color.white;
		blackTextColor = Color.black;
	}

	private void UseWarningColors(){
		whiteTextColor = Color.yellow;
		blackTextColor = Color.black;
	}

	private void UseErrorColors(){
		whiteTextColor = Color.red;
		blackTextColor = Color.black;
	}

	private void UseTransparentColours(){
		whiteTextColor = Color.clear;
		blackTextColor = Color.clear;
	}

}
