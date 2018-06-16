using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DisplayMenuScript : MonoBehaviour {

	public GameObject textureQualityGroup;
	public GameObject anisotropicTexturesGroup;
	public GameObject antiAliasingGroup;
	public GameObject shadowGroup;
	public GameObject shadowResolutionGroup;
	public GameObject shadowCascadesGroup;
	public GameObject shadowProjectionGroup;
	public GameObject shadowDistanceGroup;
	public GameObject pixelLightCountGroup;

	public GameObject resolutionScrollView;
	public GameObject fullscreenObject;
	public GameObject vsyncObject;
	public GameObject targetFramerateGroup;

	private Slider slid_texQuality;
	private Slider slid_aniso;
	private Slider slid_antialias;
	private Slider slid_shadows;
	private Slider slid_shadowRes;
	private Slider slid_shadowCasc;
	private Slider slid_shadowProj;
	private Text plac_shadowDist;
	private Text plac_pixelLights;
	private Toggle tog_fullScreen;
	private Toggle tog_vSync;
	private Text plac_targetFPS;

	private Text desc_texQuality;
	private Text desc_aniso;
	private Text desc_antialias;
	private Text desc_shadows;
	private Text desc_shadowRes;
	private Text desc_shadowCasc;
	private Text desc_shadowProj;

	private GameObject resolutionScrollerContent;

	[Header("Prefab")]
	public GameObject resolutionButton;

	void Start () {
		LoadAllElements();
		LoadAllValues();
		InitializeResolutionScroller();
	}

	//the method names were copied and pasted from the previous version and i forgot to change them to uppercase
	//plz forgiv :<

	public void setFullScreen(bool value){
		Screen.fullScreen = value;
	}

	public void setResolution(Resolution resolution){
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	public void setVSync(bool value){
		QualitySettings.vSyncCount = (value ? 1 : 0);
	}

	public void setTextureQuality(float value){
		int number = 3-(int)value;
		QualitySettings.masterTextureLimit = number;
		if(number == 0) desc_texQuality.text = "Full Resolution";
		else if(number == 1) desc_texQuality.text = "Half Resolution";
		else if(number == 2) desc_texQuality.text = "Quarter Resolution";
		else if(number == 3) desc_texQuality.text = "Eighth Resolution";
	}

	public void setAnisoLevel(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
		else if(number == 1) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
		else if(number == 2) QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
		desc_aniso.text = QualitySettings.anisotropicFiltering.ToString();
	}

	public void setAntiAliasingLevel(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.antiAliasing = 0;
		else if(number == 1) QualitySettings.antiAliasing = 2;
		else if(number == 2) QualitySettings.antiAliasing = 4;
		else if(number == 3) QualitySettings.antiAliasing = 8;
		int aaValue = QualitySettings.antiAliasing;
		if(aaValue > 0) desc_antialias.text = aaValue.ToString() + "x MSAA";
		else desc_antialias.text = "Disabled";
	}

	public void setShadowQuality(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.shadows = ShadowQuality.Disable;
		else if(number == 1) QualitySettings.shadows = ShadowQuality.HardOnly;
		else if(number == 2) QualitySettings.shadows = ShadowQuality.All;
		desc_shadows.text = QualitySettings.shadows.ToString();
	}

	public void setShadowResolution(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.shadowResolution = ShadowResolution.Low;
		else if(number == 1) QualitySettings.shadowResolution = ShadowResolution.Medium;
		else if(number == 2) QualitySettings.shadowResolution = ShadowResolution.High;
		else if(number == 3) QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
		desc_shadowRes.text = QualitySettings.shadowResolution.ToString();
	}

	public void setShadowProjection(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.shadowProjection = ShadowProjection.CloseFit;
		else if(number == 1) QualitySettings.shadowProjection = ShadowProjection.StableFit;
		desc_shadowProj.text = QualitySettings.shadowProjection.ToString();
	}

	public void setShadowCascades(float value){
		int number = (int)value;
		if(number == 0) QualitySettings.shadowCascades = 1;
		else if(number == 1) QualitySettings.shadowCascades = 2;
		else if(number == 2) QualitySettings.shadowCascades = 4;
		int shadowCasc = QualitySettings.shadowCascades;
		if(shadowCasc > 1) desc_shadowCasc.text = shadowCasc.ToString() + " Cascades";
		else desc_shadowCasc.text = "None";
	}

	public void setShadowDistance(string stringValue){
		float result;
		if(float.TryParse(stringValue, out result)){
			QualitySettings.shadowDistance = result;
		}
	}

	public void setPixelLightCount(string stringValue){
		int result;
		if(int.TryParse(stringValue, out result)){
			QualitySettings.pixelLightCount = result;
		}
	}

	public void setTargetFPS(string stringValue){
		int result;
		if(int.TryParse(stringValue, out result)){
			Application.targetFrameRate = result;
		}
	}

	private Slider GetSlider(GameObject parentGroup){
		return parentGroup.GetComponentInChildren<Slider>();
	}

	private Text GetValueTextField(GameObject parentGroup){
		int childCount = parentGroup.transform.childCount;
		Text textField;
		for(int i=0; i<childCount; i++){
			textField = parentGroup.transform.GetChild(i).GetComponent<Text>();
			if(textField != null) return textField;
		}
		throw new MissingComponentException("no value text field as child found");
	}

	private Text GetInputFieldPlaceholder(GameObject parentGroup){
		int childCount = parentGroup.transform.GetChild(0).childCount;
		for(int i=0; i<childCount; i++){
			if(parentGroup.transform.GetChild(0).GetChild(i).name.Equals("Placeholder")){
				return parentGroup.transform.GetChild(0).GetChild(i).GetComponent<Text>();
			}
		}
		throw new ArgumentException("no child named placeholder in children");
	}

	private GameObject GetScrollerContent(GameObject parentGroup){
		if(parentGroup.transform.GetChild(0).GetChild(0).name == "Content"){
			return parentGroup.transform.GetChild(0).GetChild(0).gameObject;
		}else{
			throw new ArgumentException("the second child isnt the content");
		}
	}

	private void LoadAllElements(){
		slid_texQuality = GetSlider(textureQualityGroup);
		slid_aniso = GetSlider(anisotropicTexturesGroup);
		slid_antialias = GetSlider(antiAliasingGroup);
		slid_shadows = GetSlider(shadowGroup);
		slid_shadowRes = GetSlider(shadowResolutionGroup);
		slid_shadowCasc = GetSlider(shadowCascadesGroup);
		slid_shadowProj = GetSlider(shadowProjectionGroup);

		desc_texQuality = GetValueTextField(textureQualityGroup);
		desc_aniso = GetValueTextField(anisotropicTexturesGroup);
		desc_antialias = GetValueTextField(antiAliasingGroup);
		desc_shadows = GetValueTextField(shadowGroup);
		desc_shadowRes = GetValueTextField(shadowResolutionGroup);
		desc_shadowCasc = GetValueTextField(shadowCascadesGroup);
		desc_shadowProj = GetValueTextField(shadowProjectionGroup);

		tog_fullScreen = fullscreenObject.GetComponent<Toggle>();
		tog_vSync = vsyncObject.GetComponent<Toggle>();

		plac_pixelLights = GetInputFieldPlaceholder(pixelLightCountGroup);
		plac_shadowDist = GetInputFieldPlaceholder(shadowDistanceGroup);
		plac_targetFPS = GetInputFieldPlaceholder(targetFramerateGroup);

		resolutionScrollerContent = GetScrollerContent(resolutionScrollView);
	}

	private void LoadAllValues(){
		tog_fullScreen.isOn = Screen.fullScreen;
		tog_vSync.isOn = (QualitySettings.vSyncCount > 0);

		plac_targetFPS.text = Application.targetFrameRate.ToString();

		slid_texQuality.value = slid_texQuality.maxValue - QualitySettings.masterTextureLimit;

		AnisotropicFiltering aniso = QualitySettings.anisotropicFiltering;
		if(aniso.Equals(AnisotropicFiltering.Disable)) slid_aniso.value = slid_aniso.minValue;
		else if(aniso.Equals(AnisotropicFiltering.Enable)) slid_aniso.value = 1f;
		else if(aniso.Equals(AnisotropicFiltering.ForceEnable)) slid_aniso.value = slid_aniso.maxValue;

		int aaValue = QualitySettings.antiAliasing;
		if(aaValue == 0) slid_antialias.value = slid_antialias.minValue;
		else if(aaValue == 2) slid_antialias.value = 1f;
		else if(aaValue == 4) slid_antialias.value = 2f;
		else if(aaValue == 8) slid_antialias.value = slid_antialias.maxValue;

		ShadowQuality shadowQual = QualitySettings.shadows;
		if(shadowQual.Equals(ShadowQuality.Disable)) slid_shadows.value = slid_shadows.minValue;
		else if(shadowQual.Equals(ShadowQuality.HardOnly)) slid_shadows.value = 1f;
		else if(shadowQual.Equals(ShadowQuality.All)) slid_shadows.value = slid_shadows.maxValue;

		ShadowResolution shadowRes = QualitySettings.shadowResolution;
		if(shadowRes.Equals(ShadowResolution.Low)) slid_shadowRes.value = slid_shadowRes.minValue;
		else if(shadowRes.Equals(ShadowResolution.Medium)) slid_shadowRes.value = 1f;
		else if(shadowRes.Equals(ShadowResolution.High)) slid_shadowRes.value = 2f;
		else if(shadowRes.Equals(ShadowResolution.VeryHigh)) slid_shadowRes.value = slid_shadowRes.maxValue;

		ShadowProjection shadowProj = QualitySettings.shadowProjection;
		if(shadowProj.Equals(ShadowProjection.CloseFit)) slid_shadowProj.value = slid_shadowProj.minValue;
		else if(shadowProj.Equals(ShadowProjection.StableFit)) slid_shadowProj.value = slid_shadowProj.maxValue;

		int shadowCasc = QualitySettings.shadowCascades;
		if(shadowCasc == 1) slid_shadowCasc.value = slid_shadowCasc.minValue;
		else if(shadowCasc == 2) slid_shadowCasc.value = 1f;
		else if(shadowCasc == 4) slid_shadowCasc.value = slid_shadowCasc.maxValue;

		plac_shadowDist.text = ((int)QualitySettings.shadowDistance).ToString();
		plac_pixelLights.text = QualitySettings.pixelLightCount.ToString();
	}

	private void InitializeResolutionScroller(){
		Resolution[] resolutions = Screen.resolutions;
		List<Resolution> validResolutionList = new List<Resolution>();
		int numberOfValidResolutions = 0;
		for(int i=0; i<resolutions.Length; i++){
			bool duplicate = false;
			for(int j=i+1; j<resolutions.Length; j++){
				if(resolutions[i].Equals(resolutions[j])){
					duplicate = true;
					break;
				}
			}
			if(!duplicate){
				validResolutionList.Add(resolutions[i]);
				numberOfValidResolutions++;
			}
		}
		float containerHeight = ((RectTransform)resolutionButton.transform).rect.height*numberOfValidResolutions;
		RectTransform contentRectTransform = (RectTransform)resolutionScrollerContent.transform;
		contentRectTransform.offsetMax = Vector2.zero;
		contentRectTransform.offsetMin = new Vector2(0f, -containerHeight);

		Resolution[] validResolutions = validResolutionList.ToArray();

		if(validResolutions.Length != numberOfValidResolutions) throw new Exception("this shouldnt happen");
		for(int i=0; i<numberOfValidResolutions; i++){
			GameObject newBtn = Instantiate(resolutionButton) as GameObject;
			RectTransform newBtnRect = (RectTransform)newBtn.transform;
			newBtnRect.SetParent(resolutionScrollerContent.transform, false);
			newBtnRect.anchoredPosition = new Vector2(-5f, containerHeight/2 - (0.5f * newBtnRect.rect.height) - (i*newBtnRect.rect.height));		//x = -5f gilt nur für diesen scroller.
			DisplayMenuResolutionButtonScript btnScript = newBtn.GetComponent<DisplayMenuResolutionButtonScript>();
			int j = numberOfValidResolutions-i-1;
			btnScript.resolution = validResolutions[j];
			btnScript.displayMenu = this;
			btnScript.label.text = validResolutions[j].width + " x " + validResolutions[j].height + " " + validResolutions[j].refreshRate + "Hz";
		}
	}

}
