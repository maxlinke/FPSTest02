using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IPauseObserver {

	[Header("Components")]
	[SerializeField] Rigidbody rb;
	[SerializeField] CapsuleCollider worldCollider;
	[SerializeField] GameObject head;
	[SerializeField] Camera cam;

	[Header("Script Components")]
	[SerializeField] PlayerMovementNEW playerMovement;
	[SerializeField] PlayerViewNEW playerView;
	[SerializeField] PlayerHealthSystem playerHealthSystem;
	[SerializeField] PlayerWeaponSystem playerWeaponSystem;

	[Header("Scene Objects")]
	[SerializeField] GameObject guiObject;

	IGUI gui;

	bool movementWasEnabled, viewWasEnabled, healthSystemWasEnabled, weaponSystemWasEnabled;

	void Start () {
		gui = guiObject.GetComponent<IGUI>();
		playerMovement.Initialize(rb, worldCollider, head, playerHealthSystem);
		playerView.Initialize(this, rb, head, cam, gui);
	}

	void Reset () {
		playerMovement = GetComponentInChildren<PlayerMovementNEW>();
		playerView = GetComponentInChildren<PlayerViewNEW>();
		playerHealthSystem = GetComponentInChildren<PlayerHealthSystem>();
		playerWeaponSystem = GetComponentInChildren<PlayerWeaponSystem>();
	}

	//TODO on start load what components to have enabled (for example dont be able to move etc)

	public void Pause () {
		movementWasEnabled = playerMovement.enabled;
		viewWasEnabled = playerView.enabled;
//		healthSystemWasEnabled = playerHealthSystem.enabled;
//		weaponSystemWasEnabled = playerWeaponSystem.enabled;
		SetAllFunctionsEnabled(false);
	}

	public void Unpause () {
		playerMovement.enabled = movementWasEnabled;
		playerView.enabled = viewWasEnabled;
//		playerHealthSystem.enabled = healthSystemWasEnabled;
//		playerWeaponSystem.enabled = weaponSystemWasEnabled;
	}

	public void SetAllFunctionsEnabled (bool value) {
		playerMovement.enabled = value;
		playerView.enabled = value;
//		playerHealthSystem.enabled = value;
//		playerWeaponSystem.enabled = value;
	}
		
}
