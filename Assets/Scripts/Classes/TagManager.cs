using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager{

	public static bool CompareTag(string tag, GameObject obj){
		if(obj.CompareTag(tag)) return true;
		else if(obj.CompareTag("MultiTag")){
			MultiTag multitag = obj.GetComponent<MultiTag>();
			string[] tags = multitag.tags;
			for(int i=0; i<tags.Length; i++){
				if(tags[i].Equals(tag)) return true;
			}
		}
		return false;
	}

}
