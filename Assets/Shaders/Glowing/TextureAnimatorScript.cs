﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureAnimatorScript : MonoBehaviour {

	public Material mat;
	public Texture[] textures;
	public float delay;
	private int texIndex;

	void Start () {
		texIndex = 0;
		mat.mainTexture = textures[texIndex];
		StartCoroutine(TextureSwitcher(delay));
	}

	IEnumerator TextureSwitcher(float delay){
		while(true){
			yield return new WaitForSeconds(delay);
			texIndex = (texIndex + 1) % textures.Length;
			mat.mainTexture = textures[texIndex];
		}
	}
}
