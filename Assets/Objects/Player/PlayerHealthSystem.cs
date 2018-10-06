using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour {

	public GameObject graphicalUserInterface;
	private IGUI gui;

	public bool isInvulnerable;

	public float maxHealth;
	private float health;

	public float fallDamageMinHeight;	//0 damage below
	private float fallDamageMinVel;
	public float fallDamageMaxHeight;	//100 damage
	private float fallDamageMaxVel;

	public float normalGravity;

	public float maxArmor;
	private float armor;

	public float percentageOfDamageReceivedDoneToArmor;
	public float percentageOfDamageReceivedWhileArmored;

	void Start () {
		health = 100f;	//TODO load these values from playerprefs later on
		armor = 0f;
		fallDamageMinVel = Mathf.Sqrt(2 * normalGravity * fallDamageMinHeight);
		fallDamageMaxVel = Mathf.Sqrt(2 * normalGravity * fallDamageMaxHeight);
		gui = graphicalUserInterface.GetComponent<IGUI>();
		gui.SetMaxHealthValue(maxHealth);
		gui.SetMaxArmorValue(maxArmor);
		gui.UpdatePlayerHealthDisplay(health);
		gui.UpdatePlayerArmorDisplay(armor);
	}
	
	void Update () {
		
	}

	public void NotifyOfLanding(Vector3 lastVelocity, Vector3 currentVelocity){
		if(lastVelocity.y < 0f){
			Vector3 deltaV = currentVelocity - lastVelocity;
			float deltaY = ((currentVelocity.y < 0f) ? (deltaV.y) : (-lastVelocity.y));
			if(deltaY > fallDamageMinVel){
				float damageFactor = (deltaY - fallDamageMinVel) / (fallDamageMaxVel - fallDamageMinVel);
				float fallDamage = damageFactor * 100f;
				damageDirect(fallDamage);
			}
		}
	}

	private void damage(float dmg){
		if(!isInvulnerable){
			if(armor <= 0f){
				health -= dmg;
			}else{
				health -= dmg * percentageOfDamageReceivedWhileArmored;
				armor -= dmg * percentageOfDamageReceivedDoneToArmor;
				if(armor < 0f) armor = 0f;
			}
		}
		gui.UpdatePlayerHealthDisplay(health);
	}

	private void damageDirect(float dmg){
		if(!isInvulnerable){
			health -= dmg;
		}
		gui.UpdatePlayerHealthDisplay(health);
	}
}
