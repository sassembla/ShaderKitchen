using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShaderKitchen.WebUI {
	
	[CustomEditor(typeof(WebStreamImageScript))] public class WebStreamImageInspector : Editor {
		public static string sourceFolderPath;
		
		public override void OnInspectorGUI () {
			var animations = ((WebStreamImageScript)target).animations;
			EditorGUILayout.LabelField("animations.Count:", "" + animations.Count);
			
			if (sourceFolderPath == null) sourceFolderPath = string.Empty;
			using (var hor = new GUILayout.HorizontalScope()) {
				GUILayout.Label("Load From Folder Path:", GUILayout.Width(130)); 
				sourceFolderPath = GUILayout.TextField(sourceFolderPath);
				
				var result = GUILayout.Button("Reload", GUILayout.Width(60));
				
				if (string.IsNullOrEmpty(sourceFolderPath)) result = false;
				if (!Directory.Exists(sourceFolderPath)) result = false;
				
				if (result) {
					animations.Clear();
					var filePaths = Directory.GetFiles(sourceFolderPath)
						.Where(path => !path.EndsWith(".meta"))
						.OrderBy(path => GetFileNumber(path))
						.ToArray();
					
					/*
						load images from paths.
					*/
					foreach (var filePath in filePaths) {
						var res = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)) as Texture2D;
						animations.Add(res);
						
						var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
						importer.mipmapEnabled = false;
						importer.textureType = TextureImporterType.Sprite;
					}
					AssetDatabase.Refresh();
				}
			}
			
			if (string.IsNullOrEmpty(sourceFolderPath)) {
				EditorGUILayout.HelpBox("folder path is empty.", MessageType.Error);
				return;
			}
			if (!Directory.Exists(sourceFolderPath)) {
				EditorGUILayout.HelpBox("folder does not exists:" + sourceFolderPath, MessageType.Error);
				return;
			}
		}
		
		private static int GetFileNumber (string path) {
			var numberSource = Path.GetFileNameWithoutExtension(path);
			var result = -1;
			var success = int.TryParse(numberSource, out result);
			
			if (success) return result;
			throw new Exception("error: filePath:" + path + " should be NUMBER.png");
		}
	}
}