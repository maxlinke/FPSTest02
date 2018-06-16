using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIScript : MonoBehaviour, IGUI {

	public InteractInfoScript interactInfo;
	public PlayerInfoScript playerInfo;
	public GameObject crosshairGroup;

	private bool guiEnabled;

	void Start(){
		enableGUI();
		setGUIScale(PlayerPrefManager.GetFloat("gui_gui_scale"));
		setCrosshairScale(PlayerPrefManager.GetFloat("gui_crosshair_scale"));
	}

	public void enableGUI(){
		int childCount = transform.childCount;
		for(int i=0; i<childCount; i++){
			transform.GetChild(i).gameObject.SetActive(true);
		}
		guiEnabled = true;
	}

	public void disableGUI(){
		int childCount = transform.childCount;
		for(int i=0; i<childCount; i++){
			transform.GetChild(i).gameObject.SetActive(false);
		}
		guiEnabled = false;
	}

	public void toggleGUI(){
		int childCount = transform.childCount;
		for(int i=0; i<childCount; i++){
			transform.GetChild(i).gameObject.SetActive(!guiEnabled);
		}
		guiEnabled = !guiEnabled;
	}

	public void setGUIScale(float scale){
		int childCount = transform.childCount;
		for(int i=0; i<childCount; i++){
			GameObject child = transform.GetChild(i).gameObject;
			if(child != crosshairGroup) child.transform.localScale = Vector3.one * scale;
		}
	}

	public void setCrosshairScale(float scale){
		crosshairGroup.transform.localScale = Vector3.one * scale;
	}



	public void EnableInteractDisplay(){
		interactInfo.EnableInteractDisplay();
	}

	public void DisableInteractDisplay(){
		interactInfo.DisableInteractDisplay();
	}

	public void SetInteractDisplayMessage(string message){
		interactInfo.SetInteractDisplayMessage(message);
	}



	public void UpdatePlayerHealthDisplay(float healthValue){
		playerInfo.SetHealthValue(healthValue);
	}

	public void SetMaxHealthValue(float maxHealthValue){
		playerInfo.SetMaxHealthValue(maxHealthValue);
	}

	public void UpdatePlayerArmorDisplay(float armorValue){
		playerInfo.SetArmorValue(armorValue);
	}

	public void SetMaxArmorValue(float maxArmorValue){
		playerInfo.SetMaxArmorValue(maxArmorValue);
	}



	public void UpdateAmmoDisplay(float ammoValue){

	}

	public void SetMaxAmmoValue(float maxAmmoValue){

	}

}
