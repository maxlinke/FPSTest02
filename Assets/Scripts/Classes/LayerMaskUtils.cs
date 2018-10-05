using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMaskUtils {

	public static int CreateMask (int layer) {
		string[] names = new string[32];
		for(int i=0; i<32; i++){
			if(!Physics.GetIgnoreLayerCollision(layer, i)){
				names[i] = LayerMask.LayerToName(i);
			}
		}
		return LayerMask.GetMask(names);
	}

	public static int Createmask (string layerName) {
		int layer = LayerMask.NameToLayer(layerName);
		return CreateMask(layer);
	}

}
