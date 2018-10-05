using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

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

	void Start () {
		gui = guiObject.GetComponent<IGUI>();
		playerMovement.Initialize(rb, worldCollider, head, playerHealthSystem);
		playerView.Initialize(rb, head, cam, gui);
	}

	void Reset () {
		playerMovement = GetComponentInChildren<PlayerMovementNEW>();
		playerView = GetComponentInChildren<PlayerViewNEW>();
		playerHealthSystem = GetComponentInChildren<PlayerHealthSystem>();
		playerWeaponSystem = GetComponentInChildren<PlayerWeaponSystem>();
	}

	//TODO enabling/disabling components on demand

}
