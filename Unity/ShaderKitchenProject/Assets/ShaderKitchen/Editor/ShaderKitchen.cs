using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace ShaderKitchen {

    [Serializable] public class DataStruct {
		[SerializeField] public bool recording;
	}
	
	
	[InitializeOnLoad] public class ShaderKitchen {
		
		private static DataStruct dataStruct;
		
		static ShaderKitchen () {
			dataStruct = new DataStruct();
			
			if (!File.Exists(ShaderKitchenSettings.SETTING_FILE_PATH)) return;
			 
			using (var sr = new StreamReader(ShaderKitchenSettings.SETTING_FILE_PATH)) {
				var settingFileString = sr.ReadToEnd();
				
				if (string.IsNullOrEmpty(settingFileString)) return;
				
				dataStruct = JsonUtility.FromJson<DataStruct>(settingFileString);
			}
			
			if (!dataStruct.recording) return;
			
			screenshotWidth = ShaderKitchenSettings.SCREENSHOT_SIZE_WIDTH;
			screenshotHeight = ShaderKitchenSettings.SCREENSHOT_SIZE_HEIGHT;
			EditorApplication.update += Update; 
		}
		
		
		[MenuItem(ShaderKitchenSettings.GUI_MENU_RECORDING, false, 1)] public static void Recording () {
			Load();
						
			dataStruct.recording = true;
			
			Save();
			
			// remake screen shot destination folder.
			FileController.RemakeDirectory(ShaderKitchenSettings.SCREENSHOT_PATH);
			
			EditorApplication.isPlaying = true;
		}
		
		
		private static int frame;
		private static int screenshotWidth;
		private static int screenshotHeight;
		
		private static Texture2D CaptureScreenshot (Camera currentCamera) {
			// ここでサイズを指定すると、そのサイズにセンタリングしたスクリーンショットが手に入る。
			
			// render to target texture.
			var texture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
			currentCamera.targetTexture = texture;
			currentCamera.Render();
			
			// then, set render result to screen.
			RenderTexture.active = currentCamera.targetTexture;
			
			// create new Texture2D for get screenshot.
			var newTexture = new Texture2D(currentCamera.pixelWidth, currentCamera.pixelHeight, TextureFormat.RGB24, true);
			newTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			newTexture.Apply();
			
			currentCamera.targetTexture = null;
			return newTexture;
		}
		
		private static Texture2D[] shots = new Texture2D[ShaderKitchenSettings.MAX_SCREENSHOT_FRAME];
		
		private static Camera camera;
		
		private static void Update () {
			if (ShaderKitchenSettings.MAX_SCREENSHOT_FRAME < frame) return;
			if (frame == 0) {
				var cameraObject = GameObject.Find("Main Camera");
				if (cameraObject == null) {
					CaptureAborted("no Main Camera found. please check camera GameObject name.");
					return;
				}
				 
				camera = cameraObject.GetComponent<Camera>();
				if (screenshotWidth == 0) screenshotWidth = camera.pixelWidth;
				if (screenshotHeight == 0) screenshotHeight = camera.pixelHeight;
			}
			
			if (frame == ShaderKitchenSettings.MAX_SCREENSHOT_FRAME) {
				frame++;
				
				foreach (var tex in shots.Select((v,i) => new {i, v})) {
					var bytes = tex.v.EncodeToPNG();
					var screenShotsPath = FileController.PathCombine(ShaderKitchenSettings.SCREENSHOT_PATH, tex.i + ShaderKitchenSettings.SCREENSHOT_FILE_EXTENSION);
					
					using (var file = File.Open(screenShotsPath, FileMode.Create)) {
						using (var binaryWriter = new BinaryWriter(file) ) {
							binaryWriter.Write(bytes);
						}
					}
				}
				
				EditorApplication.isPlaying = false;
				dataStruct.recording = false;
				Save();
				
				AssetDatabase.Refresh();
				return;
			}
			
			var t = CaptureScreenshot(camera);
			shots[frame] = t;
			frame++;
		}
		
		private static void CaptureAborted (string reason) {
			EditorApplication.update -= Update;
			dataStruct.recording = false;
			Save();
			Debug.LogError("recording aborted. reason:" + reason);
		}
		
		private static void Load () {
			var settingFileString = string.Empty;
			if (File.Exists(ShaderKitchenSettings.SETTING_FILE_PATH)) { 
				using (var sr = new StreamReader(ShaderKitchenSettings.SETTING_FILE_PATH)) {
					settingFileString = sr.ReadToEnd();
					var dataStruct = new DataStruct();
					if (!string.IsNullOrEmpty(settingFileString)) dataStruct = JsonUtility.FromJson<DataStruct>(settingFileString);
				}
			}
		}
		
		private static void Save () {
			using (var sw = new StreamWriter(ShaderKitchenSettings.SETTING_FILE_PATH)) {
				var newDataStruct = JsonUtility.ToJson(dataStruct);
				sw.Write(newDataStruct);
			}
		}
		
		
		
		[MenuItem(ShaderKitchenSettings.GUI_MENU_EXPORTPACKAGE, false, 1)]
		public static void ExportPackage () {
			var filePaths = FileController.FilePathsInFolder("Assets/ShaderKitchen");
			Debug.LogError("なんかできることでいうと、ここからUploadしたりする。まあ最初はメンテナンス用とかなんで、"+
				"手元にファイルを吐く" + 
				"それをUploadする経路がある　っていう程度か。" +
				"んーーーまず固められればそれでいいな、それをどっかに置いてDLできるようなWebGUIをでっちあげよう。一回転させる。");
	        AssetDatabase.ExportPackage(filePaths.ToArray(), "ShaderKitchen_0.unitypackage");
	    }
	}
}