using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGUI{

	void enableGUI();

	void disableGUI();

	void toggleGUI();

	void setGUIScale(float scale);

	void setCrosshairScale(float scale);


	void EnableInteractDisplay();

	void DisableInteractDisplay();

	void SetInteractDisplayMessage(string message);



	void UpdatePlayerHealthDisplay(float healthValue);

	void SetMaxHealthValue(float maxHealthValue);

	void UpdatePlayerArmorDisplay(float armorValue);

	void SetMaxArmorValue(float maxArmorValue);



	void UpdateAmmoDisplay(float ammoValue);

	void SetMaxAmmoValue(float maxAmmoValue);
}
