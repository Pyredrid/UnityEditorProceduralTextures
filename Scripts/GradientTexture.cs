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
	public Vector2Int resolution = new Vector2Int(256, 256);
	public TextureWrapMode wrapModeU = TextureWrapMode.Mirror;
	public TextureWrapMode wrapModeV = TextureWrapMode.Mirror;
	public GradientShape shape = GradientShape.LEFT_TO_RIGHT;
	public Gradient gradient = new Gradient();

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
		} else {
			UpdateTexture();
		}
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
		EditorUtility.SetDirty(this);
#endif
	}

	private void UpdateTexture() {
		texture.Resize(resolution.x, resolution.y);
		texture.wrapModeU = wrapModeU;
		texture.wrapModeV = wrapModeV;
		texture.alphaIsTransparency = true;
		texture.name = "Gradient_" + name;
		
		Color32[] colorArray = new Color32[resolution.x * resolution.y];
		switch (shape) {
			case GradientShape.LEFT_TO_RIGHT:
				int ltrIndex = 0;
				for (int y = 0; y < resolution.y; y++) {
					for (int x = 0; x < resolution.x; x++) {
						Color c = gradient.Evaluate((float)x / resolution.x);
						colorArray[ltrIndex] = c;
						ltrIndex++;
					}
				}
				break;
			case GradientShape.RIGHT_TO_LEFT:
				int rtlIndex = 0;
				for (int y = 0; y < resolution.y; y++) {
					for (int x = resolution.x - 1; x >= 0; x--) {
						Color c = gradient.Evaluate((float)x / resolution.x);
						colorArray[rtlIndex] = c;
						rtlIndex++;
					}
				}
				break;
			case GradientShape.TOP_TO_BOTTOM:
				int ttbIndex = 0;
				for (int y = 0; y < resolution.y; y++) {
					Color c = gradient.Evaluate((float)y / resolution.y);
					for (int x = 0; x < resolution.x; x++) {
						colorArray[ttbIndex] = c;
						ttbIndex++;
					}
				}
				break;
			case GradientShape.BOTTOM_TO_TOP:
				int bttIndex = 0;
				for (int y = resolution.y - 1; y >= 0; y--) {
					Color c = gradient.Evaluate((float)y / resolution.y);
					for (int x = 0; x < resolution.x; x++) {
						colorArray[bttIndex] = c;
						bttIndex++;
					}
				}
				break;
			default:
				break;
		}
		texture.SetPixels32(0, 0, texture.width, texture.height, colorArray);
		texture.Apply();
	}
#if UNITY_EDITOR
	//Used by GradientTextureEditor for preview display
	public Texture2D GetTexture() {
		return texture;
	}
#endif

	public enum GradientShape {
		LEFT_TO_RIGHT = 0,
		RIGHT_TO_LEFT = 1,
		TOP_TO_BOTTOM = 2,
		BOTTOM_TO_TOP = 3,
	}
}
