using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
namespace CameraMove{
public class CameraMoveEdGUI{

	#region gui style/skin
	public const string EditorResourcePath = "Assets/CameraMoveTool/Editor/";

	public static Texture2D Icon_body;
	public static Texture2D Icon_face_front;
	public static Texture2D Icon_face_left;
	public static Texture2D Icon_face_right;


	#endregion

	public static void UseSkin(){
		LoadSkinTextures ();
	}

	private static void LoadSkinTextures(){
		Icon_body = LoadEditorTexture (EditorResourcePath + "Icons/bodyIcon.png");
		Icon_face_front = LoadEditorTexture (EditorResourcePath + "Icons/faceIcon_front.png");
		Icon_face_left = LoadEditorTexture (EditorResourcePath + "Icons/faceIcon_left.png");
		Icon_face_right = LoadEditorTexture (EditorResourcePath + "Icons/faceIcon_right.png");
	}

	// ================================================================================================================
	#region resource loading

	public static Texture2D LoadEditorTexture(string fn)
	{
		Texture2D tx = AssetDatabase.LoadAssetAtPath(fn, typeof(Texture2D)) as Texture2D;
		if (tx == null) Debug.LogWarning("Failed to load texture: " + fn);
		else if (tx.wrapMode != TextureWrapMode.Clamp)
		{
			string path = AssetDatabase.GetAssetPath(tx);
			TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			tImporter.textureType = TextureImporterType.GUI;
			tImporter.npotScale = TextureImporterNPOTScale.None;
			tImporter.filterMode = FilterMode.Point;
			tImporter.wrapMode = TextureWrapMode.Clamp;
			tImporter.maxTextureSize = 64;
			tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			AssetDatabase.SaveAssets();
		}
		return tx;
	}

	#endregion
	// ================================================================================================================

	#region GUI Controls

	public static void FaceIconButtonFront(UnityAction onClick, params GUILayoutOption[] options){
		if (GUILayout.Button (Icon_face_front, options)) {
			onClick ();
		}
	}

	public static void FaceIconButtonLeft(UnityAction onClick, params GUILayoutOption[] options){
		if (GUILayout.Button (Icon_face_left, options)) {
			onClick ();
		}
	}

	public static void FaceIconButtonRight(UnityAction onClick, params GUILayoutOption[] options){
		if (GUILayout.Button (Icon_face_right, options)) {
			onClick ();
		}
	}

	public static void BodyIconButton(UnityAction onClick, params GUILayoutOption[] options){
		if (GUILayout.Button (Icon_body, options)) {
			onClick ();
		}
	}
	#endregion
}
}
