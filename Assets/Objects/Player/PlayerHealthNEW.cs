using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthNEW : MonoBehaviour {

	[Header("Health parameters")]
	[SerializeField] float maxHealth = 100f;
	[SerializeField] float healthRegenPerSecond = 10f;
	[SerializeField] float healthRegenLimit = 15f;
	[SerializeField] float healthRegenDelay = 2f;

	[Header("Armor parameters")]
	[SerializeField] float maxArmor = 200f;

	[Header("Fall damage parameters")]
	[SerializeField] float fallDamageMinHeight = 10f;	//0 dmg below
	[SerializeField] float fallDamageMaxHeight = 50f;	//100 dmg above (yes, 100, so varying health doesnt mean varying fall damage)
	[SerializeField] float normalGravity = 29.43f;

	Rigidbody rb;
	float health;
	float armor;

	public float Health {
		get { return health; }
	}

	public float Armor {
		get { return armor; }
	}

	public float Health01 {
		get { return (health / maxHealth); }
	}

	public float Armor01 {
		get { return (armor / maxArmor); }
	}

	float lastDamageTime;

	public void Initialize (Rigidbody rb) {
		this.rb = rb;
		health = maxHealth;
		armor = maxArmor;
	}

	void Update () {
		ManageHealthRegen();	//TODO if alive...
	}

	void ManageHealthRegen () {
		if(health < healthRegenLimit){	
			if(Time.time > (lastDamageTime + healthRegenDelay)){
				float deltaHealth = healthRegenLimit - health;
				float healValue = healthRegenPerSecond * Time.deltaTime;
				if(healValue > deltaHealth){
					healValue = deltaHealth;
				}
				health += healValue;
			}
		}
	}

	public void NotifyOfLanding (Rigidbody otherRB, Vector3 lastVelocity, Vector3 currentVelocity) {
		//TODO check for "bounce" tag or whatever i decide to do
		if(otherRB != null){
			lastVelocity -= otherRB.velocity;
			currentVelocity -= otherRB.velocity;
		}
		//fall damage is always relative to feet... not just deltaV
		Vector3 localCurrentV = rb.transform.InverseTransformDirection(currentVelocity);
		Vector3 localLastV = rb.transform.InverseTransformDirection(lastVelocity);
		float deltaY;
		if(localCurrentV.y > 0f){
			deltaY = -localLastV.y;
		}else{
			deltaY = localCurrentV.y - localLastV.y;
		}
		float fallDamage = CalculateFallDamageFromDeltaV(deltaY);
		if(fallDamage > 0f){
			Debug.LogWarning(fallDamage + " damage from falling");
			DamageDirect(fallDamage);
		}
	}

	void DamageDirect (float damage) {
		health -= damage;
		if(health <= 0f){
			//TODO decide whether to gib or whatever
			health = 0f;
		}
		lastDamageTime = Time.time;
	}

	public float CalculateFallDamageFromHeight (float height) {
		float lerpFactor = (height - fallDamageMinHeight) / (fallDamageMaxHeight - fallDamageMinHeight);
		if(lerpFactor < 0f) lerpFactor = 0f;
		return lerpFactor * 100f;
	}

	public float CalculateFallDamageFromDeltaV (float deltaV) {
		float height = (deltaV * deltaV) / (2f * normalGravity);
		return CalculateFallDamageFromHeight(height);
	}

}
