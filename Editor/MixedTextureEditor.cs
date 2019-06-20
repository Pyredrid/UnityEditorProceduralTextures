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
[CustomEditor(typeof(MixedTexture))]
public class MixedTextureEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		if (GUILayout.Button("Generate Texture") == true) {
			MixedTexture mixedTexture = target as MixedTexture;
			mixedTexture.CreateTexture();
		}
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background) {
		MixedTexture mixedTexture = target as MixedTexture;
		Texture2D texture = mixedTexture.GetTexture();
		if (texture != null) {
			Rect previewRect = new Rect(r);
			float textureAspectRatio = mixedTexture.resolution.x / mixedTexture.resolution.y;
			float previewAspectRatio = r.width / r.height;
			if (textureAspectRatio >= previewAspectRatio) {
				float scale = mixedTexture.resolution.x / r.width;
				float newHeight = mixedTexture.resolution.y / scale;
				previewRect.width = r.width;
				previewRect.height = newHeight;
				previewRect.y = (r.height / 2.0f) - (newHeight / 2.0f);
			} else {
				float scale = mixedTexture.resolution.y / r.height;
				float newWidth = mixedTexture.resolution.x / scale;
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
