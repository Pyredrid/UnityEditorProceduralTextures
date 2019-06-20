using LibNoise;
using LibNoise.Generator;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

/// <summary>
/// Code by Aaron "Pyredrid" Bekker-Dulmage, licensed under WTFPL
/// 
/// Noise generation library made by Ricardo J. Mendez, licensed under LGPL v3.0
/// Source code and library at:  https://github.com/ricardojmendez/LibNoise.Unity
/// </summary>

[CreateAssetMenu(menuName = "Procedural Textures/Noise", fileName = "NewNoiseTexture")]
public class NoiseTexture : ScriptableObject {
	[Header("Texture Info")]
	public Vector2Int resolution = new Vector2Int(256, 256);
	public TextureWrapMode wrapModeU = TextureWrapMode.Repeat;
	public TextureWrapMode wrapModeV = TextureWrapMode.Repeat;
	public Gradient colorGradient = new Gradient();
	[Header("Noise Info")]
	public NoiseType noiseType = NoiseType.Perlin;
	public Vector2 zoom = new Vector2(1.0f, 1.0f);
	public Vector2 offset = new Vector2(0.0f, 0.0f);
	public bool isSeamless = true;
	[Tooltip("The seed to use for RNG, 0 will be replaced by a random seed")]
	public int seed = 0;

	//Everything below is hidden in the inspector and handled by NoiseTextureEditor instead
	[HideInInspector]
	public double frequency = 1.0f;
	[HideInInspector]
	public double lacunarity = 1.0f;
	[HideInInspector]
	public double persistence = 1.0f;
	[HideInInspector]
	public double displacement = 1.0f;
	[HideInInspector]
	public int octaves = 4;
	[HideInInspector]
	public bool distance;

	[SerializeField]
	[HideInInspector]
	private Texture2D texture;
	
	public void CreateTexture() {
#if UNITY_EDITOR
		if (texture == null) {
			texture = new Texture2D(resolution.x, resolution.y);
			texture.filterMode = FilterMode.Bilinear;
			texture.alphaIsTransparency = true;
			UpdateTexture();
			AssetDatabase.Refresh();
			AssetDatabase.AddObjectToAsset(texture, this);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
			EditorUtility.SetDirty(this);
		} else {
			UpdateTexture();
		}
#endif
	}

	private void UpdateTexture() {
		texture.Resize(resolution.x, resolution.y);
		texture.wrapModeU = wrapModeU;
		texture.wrapModeV = wrapModeV;
		texture.alphaIsTransparency = true;
		texture.name = "Noise_" + name;

		if (seed == 0) {
			seed = Random.Range(int.MinValue, int.MaxValue);
		}
		
		ModuleBase noiseGenerator;
		switch (noiseType) {
			case NoiseType.Billow:
				Billow billow = new Billow(frequency, lacunarity, persistence, octaves, seed, QualityMode.High);
				noiseGenerator = billow;
				break;
			case NoiseType.RidgedMultifractal:
				RidgedMultifractal ridgedMultifractal = new RidgedMultifractal(frequency, lacunarity, octaves, seed, QualityMode.High);
				noiseGenerator = ridgedMultifractal;
				break;
			case NoiseType.Voronoi:
				Voronoi voronoi = new Voronoi(frequency, displacement, seed, distance);
				noiseGenerator = voronoi;
				break;
			default:
				//Default to perlin so the compiled doesn't complain
				Perlin perlin = new Perlin(frequency, lacunarity, persistence, octaves, seed, QualityMode.High);
				noiseGenerator = perlin;
				break;
		}

		Noise2D noiseMap = new Noise2D(resolution.x, resolution.y, noiseGenerator);
		noiseMap.GeneratePlanar(
			offset.x + -1 * 1 / zoom.x,
			offset.x + offset.x + 1 * 1 / zoom.x,
			offset.y + -1 * 1 / zoom.y,
			offset.y + 1 * 1 / zoom.y,
			isSeamless
		);
		Texture2D noiseTexture = noiseMap.GetTexture(colorGradient);
		Color32[] colorArray = noiseTexture.GetPixels32();
		texture.SetPixels32(0, 0, texture.width, texture.height, colorArray);
		texture.Apply();
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
		EditorUtility.SetDirty(this);
	}


#if UNITY_EDITOR
	//Used by NoiseTextureEditor for preview display
	public Texture2D GetTexture() {
		return texture;
	}
#endif

	[Serializable]
	public enum NoiseType {
		Perlin = 0,
		Billow = 1,
		RidgedMultifractal = 2,
		Voronoi = 3,
	}
}
