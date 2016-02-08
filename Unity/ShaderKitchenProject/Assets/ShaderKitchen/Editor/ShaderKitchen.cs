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
	
	[InitializeOnLoad]
	public class ShaderKitchen {
		
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
			
			EditorApplication.update += Update;
			 
		}
		
		[MenuItem(ShaderKitchenSettings.GUI_MENU_RECORDING, false, 1)]
		public static void Recording () {
			Load();
						
			dataStruct.recording = true;
			
			Save();
			
			// remake screen shot destination folder.
			FileController.RemakeDirectory(ShaderKitchenSettings.SCREENSHOT_PATH);
			
			EditorApplication.isPlaying = true;
		}
		
		
		private static int frame;
		
		private static Texture2D CaptureScreenshot () {
			var texture = new Texture2D(ShaderKitchenSettings.SCREENSHOT_SIZE_WIDTH, ShaderKitchenSettings.SCREENSHOT_SIZE_HEIGHT, TextureFormat.RGB24, true);
			texture.ReadPixels(new Rect(0, 0, ShaderKitchenSettings.SCREENSHOT_SIZE_WIDTH, ShaderKitchenSettings.SCREENSHOT_SIZE_HEIGHT), 0, 0, false);
			texture.Apply();
			return texture;
		}
		
		private static Texture2D[] shots = new Texture2D[ShaderKitchenSettings.MAX_SCREENSHOT_FRAME];
		
		private static void Update () {
			if (ShaderKitchenSettings.MAX_SCREENSHOT_FRAME < frame) return;
			
			if (dataStruct.recording) {
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
				
				var t = CaptureScreenshot();
				shots[frame] = t;
				frame++;
			}
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