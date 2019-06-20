using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Random = UnityEngine.Random;

/// <summary>
/// Code by Aaron "Pyredrid" Bekker-Dulmage, licensed under WTFPL
/// </summary>
/// 
[CustomEditor(typeof(GradientTexture))]
public class GradientTextureEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		if (GUILayout.Button("Generate Texture") == true) {
			GradientTexture gradientTexture = target as GradientTexture;
			gradientTexture.CreateTexture();
		}
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background) {
		GradientTexture gradientTexture = target as GradientTexture;
		Texture2D texture = gradientTexture.GetTexture();
		if (texture != null) {
			Rect previewRect = new Rect(r);
			float textureAspectRatio = gradientTexture.resolution.x / gradientTexture.resolution.y;
			float previewAspectRatio = r.width / r.height;
			if (textureAspectRatio >= previewAspectRatio) {
				float scale = gradientTexture.resolution.x / r.width;
				float newHeight = gradientTexture.resolution.y / scale;
				previewRect.width = r.width;
				previewRect.height = newHeight;
				previewRect.y = (r.height / 2.0f) - (newHeight / 2.0f);
			} else {
				float scale = gradientTexture.resolution.y / r.height;
				float newWidth = gradientTexture.resolution.x / scale;
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
