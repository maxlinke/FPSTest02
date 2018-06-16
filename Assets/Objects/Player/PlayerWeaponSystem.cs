using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSystem : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	public GameObject graphicalUserIterface;
	private IGUI gui;

	public GameObject head;
	public GameObject flashLight;

	private DoubleKey keyPrimaryFire;
	private DoubleKey keySecondaryFire;
	private DoubleKey keyToggleFlashlight;

	public bool holsterLock;

	void Start () {
		LoadKeys();
		gui = graphicalUserIterface.GetComponent<IGUI>();
		gui.SetMaxAmmoValue(0);
		gui.UpdateAmmoDisplay(0);
		flashLight.SetActive(false);	//TODO load later on (from playerprefs or whatever)
		holsterLock = false;			//TODO also load
	}
	
	void Update () {

		if(keyPrimaryFire.GetKeyDown()){
			
		}
		if(keySecondaryFire.GetKeyDown()){

		}

		if(keyToggleFlashlight.GetKeyDown()){
			flashLight.SetActive(!flashLight.activeSelf);
		}

	}

	public void HolsterWeapon(){

	}

	public void UnholsterWeapon(){
		if(!holsterLock){

		}
	}

	public void AddSelfToPlayerPrefObserverList(){
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged(){
		LoadKeys();
	}

	private void LoadKeys(){
		keyPrimaryFire = DoubleKey.FromPlayerPrefs("key_primary_fire");
		keySecondaryFire = DoubleKey.FromPlayerPrefs("key_secondary_fire");
		keyToggleFlashlight = DoubleKey.FromPlayerPrefs("key_toggle_flashlight");
	}
}
