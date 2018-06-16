using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounterScript : MonoBehaviour {

	public Text text;

	public int timespan;
	float[] memory;
	int index;
	float sum;

	void Start () {
		memory = new float[timespan];
		for(int i=0; i<timespan; i++){
			memory[i] = 0f;
		}
		index = 0;
		sum = 0f;
	}
	
	void Update () {
		sum -= memory[index];
		sum += Time.unscaledDeltaTime;
		memory[index] = Time.unscaledDeltaTime;
		index = (index + 1)%timespan;
		int fps = round(1f/(sum/timespan));
		string output = ((fps > 9999) ? "many" : fps.ToString());
		text.text = output;
	}

	private int round(float value){
		return (int)(Mathf.Floor(value + 0.5f));
	}
}
