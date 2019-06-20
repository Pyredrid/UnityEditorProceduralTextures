using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

/// <summary>
/// Code by Aaron "Pyredrid" Bekker-Dulmage, licensed under WTFPL
/// </summary>

[CreateAssetMenu(menuName = "Procedural Textures/Gradient", fileName = "NewGradientTexture")]
public class GradientTexture : ScriptableObject {
	[OnValueChanged("UpdateTexture")]
	public TextureWrapMode wrapModeU = TextureWrapMode.Mirror;
	[OnValueChanged("UpdateTexture")]
	public TextureWrapMode wrapModeV = TextureWrapMode.Clamp;
	[OnValueChanged("UpdateTexture")]
	public Gradient gradient = new Gradient();

	[SerializeField]
	[HideInInspector]
	private Texture2D texture;
	[SerializeField]
	[HideInInspector]
	private bool isTextureCreated = false;
	[Button]
	private void CreateTexture() {
#if UNITY_EDITOR
		if (isTextureCreated == false) {
			isTextureCreated = true;
			texture = new Texture2D(256, 1);
			texture.filterMode = FilterMode.Bilinear;
			texture.alphaIsTransparency = true;
			UpdateTexture();
			AssetDatabase.Refresh();
			AssetDatabase.AddObjectToAsset(texture, this);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
			EditorUtility.SetDirty(this);
		}
#endif
	}

	private void UpdateTexture() {
		if (texture == null) {
			return;
		}

		texture.wrapModeU = wrapModeU;
		texture.wrapModeV = wrapModeV;
		texture.alphaIsTransparency = true;
		texture.name = "Gradient_" + name;

		Color32[] colorArray = new Color32[texture.width];
		for (int i = 0; i < texture.width; i++) {
			colorArray[i] = gradient.Evaluate((float)i / texture.width);
		}
		texture.SetPixels32(0, 0, texture.width, 1, colorArray);
		texture.Apply();

		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
		EditorUtility.SetDirty(this);
	}
}
