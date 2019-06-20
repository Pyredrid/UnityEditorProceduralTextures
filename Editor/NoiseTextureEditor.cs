using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Random = UnityEngine.Random;

/// <summary>
/// Code by Aaron "Pyredrid" Bekker-Dulmage, licensed under WTFPL
/// </summary>

[CustomEditor(typeof(NoiseTexture))]
public class NoiseTextureEditor : Editor {
	private SerializedProperty frequency;
	private SerializedProperty lacunarity;
	private SerializedProperty persistence;
	private SerializedProperty displacement;
	private SerializedProperty octaves;
	private SerializedProperty distance;

	void OnEnable() {
		frequency = serializedObject.FindProperty("frequency");
		lacunarity = serializedObject.FindProperty("lacunarity");
		persistence = serializedObject.FindProperty("persistence");
		displacement = serializedObject.FindProperty("displacement");
		octaves = serializedObject.FindProperty("octaves");
		distance = serializedObject.FindProperty("distance");
	}

	public override void OnInspectorGUI() {
		NoiseTexture noiseTexture = target as NoiseTexture;
		DrawDefaultInspector();
		serializedObject.Update();
		EditorGUILayout.PropertyField(frequency);
		if (noiseTexture.noiseType == NoiseTexture.NoiseType.Perlin
			|| noiseTexture.noiseType == NoiseTexture.NoiseType.Billow
			|| noiseTexture.noiseType == NoiseTexture.NoiseType.RidgedMultifractal) {
			EditorGUILayout.PropertyField(lacunarity);
		}
		if (noiseTexture.noiseType == NoiseTexture.NoiseType.Perlin
			|| noiseTexture.noiseType == NoiseTexture.NoiseType.Billow) {
			EditorGUILayout.PropertyField(persistence);
		}
		if (noiseTexture.noiseType == NoiseTexture.NoiseType.Voronoi) {
			EditorGUILayout.PropertyField(displacement);
		}
		if (noiseTexture.noiseType == NoiseTexture.NoiseType.Perlin
			|| noiseTexture.noiseType == NoiseTexture.NoiseType.Billow
			|| noiseTexture.noiseType == NoiseTexture.NoiseType.RidgedMultifractal) {
			//Large octave values can lag or crash Unity, so be careful...
			EditorGUILayout.IntSlider(octaves, 1, 16);
		}
		if (noiseTexture.noiseType == NoiseTexture.NoiseType.Voronoi) {
			EditorGUILayout.PropertyField(distance);
		}
		if (GUILayout.Button("Generate Texture") == true) {
			noiseTexture.CreateTexture();
		}
		serializedObject.ApplyModifiedProperties();
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background) {
		NoiseTexture noiseTexture = target as NoiseTexture;
		Texture2D texture = noiseTexture.GetTexture();
		if (texture != null) {
			Rect previewRect = new Rect(r);
			float textureAspectRatio = noiseTexture.resolution.x / noiseTexture.resolution.y;
			float previewAspectRatio = r.width / r.height;
			if (textureAspectRatio >= previewAspectRatio) {
				float scale = noiseTexture.resolution.x / r.width;
				float newHeight = noiseTexture.resolution.y / scale;
				previewRect.width = r.width;
				previewRect.height = newHeight;
				previewRect.y = (r.height / 2.0f) - (newHeight / 2.0f);
			} else {
				float scale = noiseTexture.resolution.y / r.height;
				float newWidth = noiseTexture.resolution.x / scale;
				previewRect.width = newWidth;
				previewRect.height = r.height;
				previewRect.x = (r.width / 2.0f) - (newWidth / 2.0f);
			}

			EditorGUI.DrawPreviewTexture(previewRect, texture);
		}
	}

	public override bool HasPreviewGUI() {
		return true;
	}
}
