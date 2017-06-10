using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace CameraMove{
public class Vector2UnityEvent : UnityEvent<Vector2>{
}

public class FloatUnityEvent : UnityEvent<float>{
}


public class CameraMoveWindow : EditorWindow {

	protected GameObject go;
	protected GameObject focusObj;
	// 選択した位置からのOffset
	protected Vector3 focusPos = Vector3.zero;
	public enum CameraPoint
	{
		Front,
		Right,
		Left,
		Body
	}

	protected bool lockSelection = false;
	protected float wheelSpeed = 0.4f;
	protected float moveSpeed = 0.4f;
	protected float rotateSpeed = 5f;


	protected Vector2UnityEvent OnMouseDragRight = new Vector2UnityEvent();
	protected Vector2UnityEvent OnMouseDragMiddle = new Vector2UnityEvent();
	protected FloatUnityEvent OnScrollWheel = new FloatUnityEvent();


	public enum MoveType{
		Focus,
		Local
	}

	protected MoveType currentMoveType;

	[MenuItem ("Custom/CameraMove")]
	static void Open ()
	{
		var window = GetWindow<CameraMoveWindow> ();
		window.minSize = new Vector2(430,200);
	}

	// Has a GameObject been selection?
	public void OnSelectionChange()
	{
		if (!lockSelection)
		{
			go = Selection.activeGameObject;
			Repaint();
		}
	}

	void OnEnable ()
	{
		Init ();
	}

	void Init(){
		OnMouseDragRight.AddListener (delta => MouseDragRight (delta));
		OnMouseDragMiddle.AddListener (delta => MouseDragMiddle (delta));
		OnScrollWheel.AddListener (deltaY => ScrollWheel (deltaY));
	}

	void EditorInput(){
		var e = Event.current;


		switch (e.type) {
		case EventType.MouseDrag:
			if (e.button == 0) {
				// トラックパッド対応
				if (e.alt && e.command) {
					OnMouseDragMiddle.Invoke (e.delta);
					e.Use ();
				}else if(e.alt){
					OnMouseDragRight.Invoke (e.delta);
					e.Use ();
				}
			}else if (e.button == 1) { // 右ドラッグ
				OnMouseDragRight.Invoke(e.delta);
				e.Use ();
			} else if (e.button == 2) { // ミドルドラッグ
				OnMouseDragMiddle.Invoke(e.delta);
				e.Use ();
			}
			break;
		case EventType.ScrollWheel:
			OnScrollWheel.Invoke (e.delta.y);
			e.Use ();
			break;
		}
	}

	void OnGUI ()
	{
		
		EditorGUILayout.LabelField("カメラを動かすツール");

		// Wait for user to select a GameObject
		if (go == null || !go.GetComponent<Camera>())
		{
			EditorGUILayout.HelpBox("Please select a Camera", MessageType.Info);
			return;
		}

		CameraMoveEdGUI.UseSkin ();
		using (new EditorGUILayout.HorizontalScope (GUI.skin.box, GUILayout.Height(85))) {
			using (new EditorGUILayout.VerticalScope (GUI.skin.box, GUILayout.MinWidth(270))) {
				using (new EditorGUILayout.HorizontalScope ()) {
					EditorGUILayout.LabelField ("MoveType", GUILayout.Width(80));
					currentMoveType = (MoveType)EditorGUILayout.EnumPopup (currentMoveType);
				}

				using (new EditorGUILayout.HorizontalScope ()) {
					GUILayout.FlexibleSpace ();
					lockSelection = GUILayout.Toggle (lockSelection, "Lock");
				}
				using (new EditorGUILayout.HorizontalScope ()) {
					EditorGUILayout.LabelField ("Camera", GUILayout.Width(80));
					go = EditorGUILayout.ObjectField (go, typeof(GameObject), true) as GameObject;
				}

				if (currentMoveType == MoveType.Focus) {
					using (new EditorGUILayout.HorizontalScope ()) {
						EditorGUILayout.LabelField ("FocusTarget", GUILayout.Width(80));
						focusObj = EditorGUILayout.ObjectField (focusObj, typeof(GameObject), true) as GameObject;
					}
				}
				GUILayout.FlexibleSpace ();
			}
			using (new EditorGUILayout.VerticalScope (GUI.skin.box)) {
				if (currentMoveType == MoveType.Focus) {
					FocusObjMode ();
				}
				using (new EditorGUILayout.HorizontalScope ()) {
					if (GUILayout.Button ("Z0", GUILayout.Height (30), GUILayout.Width (30))) {
						go.transform.rotation = Quaternion.Euler(
							go.transform.eulerAngles.x,
							go.transform.eulerAngles.y,
							0
						);
					}

					GUILayout.FlexibleSpace ();
				}

				GUILayout.FlexibleSpace ();
			}
		}
		//入力
		EditorInput ();


		using (new EditorGUILayout.HorizontalScope (GUI.skin.box)) {
			GUILayout.FlexibleSpace ();
			using (new EditorGUILayout.VerticalScope ()) {
				GUILayout.FlexibleSpace ();
				EditorGUILayout.LabelField ("ここで動かす");
				GUILayout.FlexibleSpace ();
			}
			GUILayout.FlexibleSpace ();
		}

	}


	void FocusObjMode(){
		using (new EditorGUILayout.HorizontalScope ()) {
			var iconButtonOptions = new[]{ GUILayout.Height (30), GUILayout.Width (30) };
			CameraMoveEdGUI.FaceIconButtonFront (
				()=>SetCameraPos(CameraPoint.Front),
				iconButtonOptions
			);
			CameraMoveEdGUI.FaceIconButtonLeft (()=>SetCameraPos(CameraPoint.Left), iconButtonOptions);
			CameraMoveEdGUI.FaceIconButtonRight (()=>SetCameraPos(CameraPoint.Right), iconButtonOptions);
			CameraMoveEdGUI.BodyIconButton (()=>SetCameraPos(CameraPoint.Body), iconButtonOptions);
		}
	}

	void SetCameraPos(CameraPoint point){
		var offset = Vector3.zero;

		if (focusObj) {
			var focusTran = focusObj.transform;
			switch (point) {
			case CameraPoint.Front:
				offset = focusTran.forward * 10;
				break;
			case CameraPoint.Left:
				offset = focusTran.right * (-10);
				break;
			case CameraPoint.Right:
				offset = focusTran.right * 10;
				break;
			case CameraPoint.Body:
				offset = focusTran.forward * 20;
				break;
			}
			go.transform.position = focusTran.position + offset;
			focusPos = focusTran.position;
		} else {
			focusPos = Vector3.zero;
			switch (point) {
			case CameraPoint.Front:
				offset = Vector3.forward * 10;
				break;
			case CameraPoint.Left:
				offset = Vector3.right * (-10);
				break;
			case CameraPoint.Right:
				offset = Vector3.right * 10;
				break;
			case CameraPoint.Body:
				offset = Vector3.forward * 20;
				break;
			}
			go.transform.position = offset;
		}
		go.transform.LookAt (focusPos);
	}



	void MouseDragRight(Vector2 delta){
		if(delta.magnitude < Vector3.kEpsilon)
			return;
		var angle = new Vector2 (delta.y, delta.x) * Time.deltaTime * rotateSpeed;
		Undo.RecordObject (go.transform, "Camera Rotate");
		if (currentMoveType == MoveType.Local) {
			CameraLocalMove.Rotate (go, angle);
		} else {
			CameraFocusMove.Rotate (go, focusPos, angle);
		}
	}

	void MouseDragMiddle(Vector2 delta){
		if(delta.magnitude < Vector3.kEpsilon)
			return;
		Undo.RecordObject (go.transform, "Camera Move");
		var val = new Vector2 (-delta.x, delta.y) * Time.deltaTime * moveSpeed;
		if (currentMoveType == MoveType.Local) {
			CameraLocalMove.HorizontalMove (go, val);
		} else {
			CameraFocusMove.HorizontalMove (go, ref focusPos, val); 
		}
	}


	void ScrollWheel(float deltaY){
		Undo.RecordObject (go.transform, "Camera Move");
		CameraLocalMove.ForwardMove (go, deltaY * wheelSpeed * Time.deltaTime);
	}
}

// カメラの注目点移動
public class CameraFocusMove{
	public static void HorizontalMove(GameObject camera, ref Vector3 focusPos, Vector2 val){
		Vector3 diff = Vector3.zero;
		var temp = camera.transform.position;
		camera.transform.Translate (val);
		diff = camera.transform.position - temp;
		focusPos += diff;
	}

	public static void Rotate(GameObject camera, Vector3 focusPos,  Vector2 angle){
		
		// 回転前のカメラの情報を保存する
		Vector3 preUpV, preAngle, prePos;
		var trans = camera.transform;
		preUpV = trans.up;
		preAngle = trans.localEulerAngles;
		prePos = trans.position;

		// カメラの回転
		// 横方向の回転はグローバル座標系のY軸で回転する
		trans.RotateAround(focusPos, Vector3.up, angle.y);
		// 縦方向の回転はカメラのローカル座標系のX軸で回転する
		trans.RotateAround(focusPos, trans.right, angle.x);

		// カメラを注視点に向ける
		trans.LookAt(focusPos);

		// ジンバルロック対策
		// カメラが真上や真下を向くとジンバルロックがおきる
		// ジンバルロックがおきるとカメラがぐるぐる回ってしまうので、一度に90度以上回るような計算結果になった場合は回転しないようにする(計算を元に戻す)
		Vector3 up = trans.up;
		if(Vector3.Angle(preUpV, up) > 90.0f)
		{
			trans.localEulerAngles = preAngle;
			trans.position = prePos;
		}

		return;
	}
}

// カメラのローカル移動
public class CameraLocalMove
{
	public static void HorizontalMove(GameObject camera, Vector2 val){
		camera.transform.Translate (val);
	}

	public static void ForwardMove(GameObject camera, float val){
		
		camera.transform.position -= camera.transform.forward  * val;
	}

	public static void Rotate(GameObject camera, Vector2 angle){
		
		camera.transform.Rotate (camera.transform.right * angle.x, Space.World);
		camera.transform.Rotate (Vector3.up * angle.y, Space.World);
	}
}
}