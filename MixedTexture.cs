using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

/// <summary>
/// Code by Aaron "Pyredrid" Bekker-Dulmage, licensed under WTFPL
/// </summary>

[CreateAssetMenu(menuName = "Procedural Textures/Mix", fileName = "MixedTexture")]
public class MixedTexture : ScriptableObject {
	[Header("Texture Info")]
	public Vector2Int resolution = new Vector2Int(256, 256);
	public TextureWrapMode wrapModeU = TextureWrapMode.Repeat;
	public TextureWrapMode wrapModeV = TextureWrapMode.Repeat;
	[Header("Mixing Info")]
	public MixingType mixType = MixingType.RGBA_MULTIPLY;
	public Texture2D leftTexture;
	public Texture2D rightTexture;

	[SerializeField]
	[HideInInspector]
	private Texture2D texture;

	void Awake() {
		//This causes errors...??
		//Apparently AssetDatabase.AddObjectToAsset(texture, this) will cause problems on Awake
		//because texture won't be serialized yet if its made through the CreateAssetMenu?
		//Unity, why do you do this...
		//CreateTexture();
	}
	
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
		Debug.Assert(texture != null);

		if (leftTexture == null) {
			Debug.Log("Left Texture is required");
			return;
		}
		if (rightTexture == null) {
			Debug.Log("Right Texture is required");
			return;
		}

		texture.Resize(resolution.x, resolution.y);
		texture.wrapModeU = wrapModeU;
		texture.wrapModeV = wrapModeV;
		texture.alphaIsTransparency = true;
		texture.name = "Mixed_" + name;

		Color32[] colorArray = new Color32[resolution.x * resolution.y];
		int i = 0;
		for (int y = 0; y < resolution.y; y++) {
			for (int x = 0; x < resolution.x; x++) {
				float u = (float)x / resolution.x;
				float v = (float)y / resolution.y;
				Color32 leftColor = leftTexture.GetPixelBilinear(u, v);
				Color32 rightColor = rightTexture.GetPixelBilinear(u, v);
				colorArray[i] = Mix(leftColor, rightColor);
				i++;
			}
		}
		texture.SetPixels32(0, 0, texture.width, texture.height, colorArray);
		texture.Apply();
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
		EditorUtility.SetDirty(this);
	}

	private Color Mix(Color left, Color right) {
		Color result = new Color();
		switch (mixType) {
			case MixingType.RGBA_MULTIPLY:
				result.r = left.r * right.r;
				result.g = left.g * right.g;
				result.b = left.b * right.b;
				result.a = left.a * right.a;
				break;
			case MixingType.RGBA_ADDITION:
				result.r = left.r + right.r;
				result.g = left.g + right.g;
				result.b = left.b + right.b;
				result.a = left.a + right.a;
				break;
			case MixingType.RGBA_SUBTRACTION:
				result.r = left.r - right.r;
				result.g = left.g - right.g;
				result.b = left.b - right.b;
				result.a = left.a - right.a;
				break;
			case MixingType.LEFT_RGB_AND_RIGHT_ALPHA:
				result.r = left.r;
				result.g = left.g;
				result.b = left.b;
				result.a = right.a;
				break;
			case MixingType.LEFT_RGB_AND_RIGHT_RED:
				result.r = right.r;
				result.g = left.g;
				result.b = left.b;
				result.a = left.a;
				break;
			case MixingType.LEFT_RGB_AND_RIGHT_GREEN:
				result.r = left.r;
				result.g = right.g;
				result.b = left.b;
				result.a = left.a;
				break;
			case MixingType.LEFT_RGB_AND_RIGHT_BLUE:
				result.r = left.r;
				result.g = left.g;
				result.b = right.b;
				result.a = left.a;
				break;
			case MixingType.MIX_KUBELKA_MUNK:
				result = MixKubelkaMunk(left, right);
				break;
			default:
				break;
		}
		return result;
	}

	/// <summary>
	/// Using the Kubelka-Munk model of diffuse
	/// reflection, mix two colors.
	/// 
	/// The order of colors should not matter, this is 
	/// more like mixed paints.
	/// 
	/// Alpha will always return as full opaque.
	/// </summary>
	/// <param name="left">One of the colors to mix</param>
	/// <param name="right">Another of the colors to mix</param>
	/// <returns>The mixed color</returns>
	private Color MixKubelkaMunk(Color left, Color right) {
		Vector3 leftAbsorbance = new Vector3();
		leftAbsorbance.x = Mathf.Pow((1.0f - left.r), 2.0f) / (2.0f * left.r);
		leftAbsorbance.y = Mathf.Pow((1.0f - left.g), 2.0f) / (2.0f * left.g);
		leftAbsorbance.z = Mathf.Pow((1.0f - left.b), 2.0f) / (2.0f * left.b);
		Vector3 rightAbsorbance = new Vector3();
		rightAbsorbance.x = Mathf.Pow((1.0f - right.r), 2.0f) / (2.0f * right.r);
		rightAbsorbance.y = Mathf.Pow((1.0f - right.g), 2.0f) / (2.0f * right.g);
		rightAbsorbance.z = Mathf.Pow((1.0f - right.b), 2.0f) / (2.0f * right.b);
		Vector3 mixedAbsorbance = new Vector3();
		mixedAbsorbance.x = (leftAbsorbance.x / 3.0f) + (rightAbsorbance.x / 3.0f);
		mixedAbsorbance.x = (leftAbsorbance.y / 3.0f) + (rightAbsorbance.y / 3.0f);
		mixedAbsorbance.x = (leftAbsorbance.z / 3.0f) + (rightAbsorbance.z / 3.0f);
		Color mixedColor = new Color();
		mixedColor.r = 1.0f + mixedAbsorbance.x - Mathf.Sqrt(Mathf.Pow(mixedAbsorbance.x, 2.0f) + (2.0f * mixedAbsorbance.x));
		mixedColor.g = 1.0f + mixedAbsorbance.y - Mathf.Sqrt(Mathf.Pow(mixedAbsorbance.y, 2.0f) + (2.0f * mixedAbsorbance.y));
		mixedColor.b = 1.0f + mixedAbsorbance.z - Mathf.Sqrt(Mathf.Pow(mixedAbsorbance.z, 2.0f) + (2.0f * mixedAbsorbance.z));
		mixedColor.a = 1.0f;
		return mixedColor;
	}

#if UNITY_EDITOR
	//Used by MixedTextureEditor for preview display
	public Texture2D GetTexture() {
		return texture;
	}
#endif

	[Serializable]
	public enum MixingType {
		//Basic component-wise mixing, mostly for technical stuff
		RGBA_MULTIPLY = 0,
		RGBA_ADDITION = 1,
		RGBA_SUBTRACTION = 2,

		//These ones might seem weird, but are useful for embedding
		// heightmaps and such in the alpha channel for shaders
		LEFT_RGB_AND_RIGHT_ALPHA = 50,
		LEFT_RGB_AND_RIGHT_RED = 51,
		LEFT_RGB_AND_RIGHT_GREEN = 52,
		LEFT_RGB_AND_RIGHT_BLUE = 53,

		//More creative or accurate color mixing techniques
		MIX_KUBELKA_MUNK = 100,
	}
}
