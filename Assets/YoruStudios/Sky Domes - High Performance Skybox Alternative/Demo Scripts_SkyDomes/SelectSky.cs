using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSky : MonoBehaviour{
	[SerializeField]
	private Texture2D[] skyTextures;//Array of sky textures

	[SerializeField]
	private Renderer[] tintRenderers;//Sprites that are "tinted" by sky colors

	//Reference to Dome Material
	private Material domeMaterial;
	//Current selected sky on array (counter)
	private int selectedSky = 0;

	private void Start(){
		domeMaterial = GetComponent<MeshRenderer>().material;
		//Set First sky texture
		domeMaterial.mainTexture = skyTextures[selectedSky];
		UpdateColors();
	}

	private void Update(){
		//Go to previous Sky
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			if(selectedSky < skyTextures.Length-1){
				selectedSky++;
				domeMaterial.mainTexture = skyTextures[selectedSky];
				UpdateColors();
			}
		}
		//Go to Next Sky
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			if(selectedSky > 0){
				selectedSky--;
				domeMaterial.mainTexture = skyTextures[selectedSky];
				UpdateColors();
			}
		}
	}

	// Update the tint colors of the attached Renderers based on the current sky
	public void UpdateColors(){
		//Get Sky base color
		Color baseColor = GetAverageColor(0.1f,-0.1f);
        
		//Apply color on Renderers
		for(int z = 0; z<tintRenderers.Length; z++){
			tintRenderers[z].material.color = baseColor; // Changed from tintSprites[z].color to tintRenderers[z].material.color
		}
	}

	// Calculate the average color of the dome texture with adjustments
	private Color GetAverageColor(float saturationAdjustment, float brightnessAdjustment)
	{
		Texture2D domeTexture = domeMaterial.mainTexture as Texture2D;
		Color[] pixels = domeTexture.GetPixels();
		float r = 0;
		float g = 0;
		float b = 0;

		int offset = 2; // Sets the number of pixels to skip

		for (int i = 0; i < pixels.Length; i += offset)
		{
			r += pixels[i].r;
			g += pixels[i].g;
			b += pixels[i].b;
		}

		int totalPixels = pixels.Length / offset;

		r /= totalPixels;
		g /= totalPixels;
		b /= totalPixels;

		// Convert RGB color to HSL
		Color.RGBToHSV(new Color(r, g, b), out float H, out float S, out float V);

		// Adjust saturation
		S -= saturationAdjustment;

		// Adjust brightness
		V -= brightnessAdjustment;

		// Ensure saturation stays within the [0, 1] range
		S = Mathf.Clamp01(S);

		// Ensure brightness stays within the [0, 1] range
		V = Mathf.Clamp01(V);

		// Avoids the color from becoming completely white
		if(S==0){
			S = 0.2f;
		}

		// Convert the HSL color back to RGB
		Color adjustedColor = Color.HSVToRGB(H, S, V);

		return adjustedColor;
	}
}
