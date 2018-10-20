using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TagManager{

	public static bool CompareTag (string tag, GameObject obj) {
		if(obj.CompareTag(tag)){
			return true;
		}else if(obj.CompareTag("MultiTag")){
			MultiTag multiTag = obj.GetComponent<MultiTag>();
			if(multiTag == null){
				throw new UnityException("GameObject \"" + obj.name + "\" tagged MultiTag but is missing MultiTag-Component!");
			}else{
				string[] tags = multiTag.tags;
				bool result = false;
				for(int i=0; i<tags.Length; i++){
					result = (result || tags[i].Equals(tag));
				}
				return result;
			}
		}else{
			return false;
		}
	}

	public static GameObject[] FindWithTag (string tag) {
		GameObject[] multi = GameObject.FindGameObjectsWithTag("MultiTag");
		if(tag.Equals("MultiTag")){
			return multi;
		}else{
			GameObject[] regular = GameObject.FindGameObjectsWithTag(tag);
			List<GameObject> output = new List<GameObject>();
			output.AddRange(regular);
			for(int i=0; i<multi.Length; i++){
				if(CompareTag(tag, multi[i])){
					output.Add(multi[i]);
				}
			}
			return output.ToArray();
		}
	}

}
