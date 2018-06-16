using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightButtonScript : MonoBehaviour, IInteractable {

	public GameObject sun;
	public string description;
	public float timeToSwitch;
	private Light sunLight;
	private Vector3 noonVector;
	private Vector3 sunUp;
	private Vector3 sunPos;
	private Color dayFogColor;

	private bool isNoon;
	private bool rotateToMidnight;
	private bool rotateToNoon;

	private float timer;

	public ReflectionProbe reflectionProbe;
	public LightProbeGroup lightProbes;

	void Start () {
		sunLight = sun.GetComponent<Light>();
		noonVector = sun.transform.forward;
		sunUp = sun.transform.up;
		sunPos = sun.transform.position;
		isNoon = true;
		rotateToMidnight = false;
		rotateToNoon = false;
		dayFogColor = RenderSettings.fogColor;
	}

	void Update(){
		if(rotateToNoon){
			sun.transform.RotateAround(sunPos, sunUp, 180f * (Time.deltaTime / timeToSwitch));
			float lerpFactor = Mathf.Clamp01(sun.transform.forward.y / noonVector.y);
			sunLight.intensity = lerpFactor;
			RenderSettings.fogColor = Color.Lerp(dayFogColor, Color.black, lerpFactor);
			reflectionProbe.RenderProbe();
			timer += Time.deltaTime;
			if(timer >= timeToSwitch){
				isNoon = true;
				rotateToNoon = false;
				sun.transform.forward = noonVector;
				RenderSettings.fogColor = dayFogColor;
			}
		}else if(rotateToMidnight){
			sun.transform.RotateAround(sunPos, sunUp, 180f * (Time.deltaTime / timeToSwitch));
			float lerpFactor = Mathf.Clamp01(sun.transform.forward.y / noonVector.y);
			sunLight.intensity = lerpFactor;
			RenderSettings.fogColor = Color.Lerp(Color.black, dayFogColor, lerpFactor);
			reflectionProbe.RenderProbe();
			timer += Time.deltaTime;
			if(timer >= timeToSwitch){
				isNoon = false;
				rotateToMidnight = false;
				RenderSettings.fogColor = Color.black;
			}
		}
	}

	public void Interact(GameObject other){
		if(!(rotateToMidnight || rotateToNoon)){
			timer = 0f;
			if(isNoon) rotateToMidnight = true;
			else rotateToNoon = true;
		}
	}

	public string GetDescription(){
		return description;
	}

}
