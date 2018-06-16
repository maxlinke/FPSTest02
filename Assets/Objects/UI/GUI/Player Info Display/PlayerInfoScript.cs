using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoScript : MonoBehaviour {

	public Text healthText;
	public Text armorText;

	private int maxHealth;
	private int maxArmor;

	public void SetHealthValue(float healthValue){
		healthText.text = "HEALTH : " + Mathf.Floor(healthValue + 0.5f) + " / " + maxHealth;
	}

	public void SetMaxHealthValue(float maxHealthValue){
		this.maxHealth = (int)Mathf.Floor(maxHealthValue + 0.5f);
	}

	public void SetArmorValue(float armorValue){
		armorText.text = "ARMOR : " + Mathf.Floor(armorValue + 0.5f) + " / " + maxArmor;
	}

	public void SetMaxArmorValue(float maxArmorValue){
		this.maxArmor = (int)Mathf.Floor(maxArmorValue + 0.5f);
	}

}
