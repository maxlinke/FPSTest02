using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloPlaneAnimator : MonoBehaviour {

	[SerializeField] Texture2D[] textures;
	[SerializeField] Material material;
	[SerializeField] string texArrayPropertyName;
	[SerializeField] string numberPropertyName;
	[SerializeField] bool executeOnStart;

	void Start () {
		if(executeOnStart) ApplyTextureToMaterial();
	}
	
	public void ApplyTextureToMaterial () {
		CheckForDuplicates();
		Texture2DArray texArray = CreateTextureArray();
		material.SetTexture(texArrayPropertyName, texArray);
		material.SetFloat(numberPropertyName, textures.Length);
	}

	void CheckForDuplicates () {
		HoloPlaneAnimator[] others = GameObject.FindObjectsOfType<HoloPlaneAnimator>();
		List<HoloPlaneAnimator> conflicts = new List<HoloPlaneAnimator>();
		foreach(HoloPlaneAnimator other in others){
			if((other.material == this.material) && (other != this)){
				conflicts.Add(other);
			}
		}
		if(conflicts.Count > 0){
			Debug.DrawRay(this.transform.position, Vector3.up, Color.red, Mathf.Infinity, false);
			foreach(HoloPlaneAnimator conflict in conflicts){
				Debug.DrawRay(conflict.transform.position, Vector3.up * 1000f, Color.red, Mathf.Infinity, false);
			}
			throw new UnityException("There are multiple " + this.GetType().ToString() + "s for the same material! Drawing rays for easy identification...");
		}
	}

	Texture2DArray CreateTextureArray () {
		Texture2DArray texArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, true);
		for(int i=0; i<textures.Length; i++){
			texArray.SetPixels32(textures[i].GetPixels32(), i);
		}
		texArray.Apply();
		return texArray;
	}
}
