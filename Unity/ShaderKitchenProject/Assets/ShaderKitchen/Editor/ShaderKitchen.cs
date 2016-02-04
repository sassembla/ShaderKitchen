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
			var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
			texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
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

	public class FileController {
		public static void RemakeDirectory (string localFolderPath) {
			if (Directory.Exists(localFolderPath)) Directory.Delete(localFolderPath, true);
			Directory.CreateDirectory(localFolderPath);
		}
		
		/**
			default is "overwrite same path" by filepath.
		*/
		public static void CopyFileFromGlobalToLocal (string absoluteSourceFilePath, string localTargetFilePath) {
			var parentDirectoryPath = Path.GetDirectoryName(localTargetFilePath);
			Directory.CreateDirectory(parentDirectoryPath);
			File.Copy(absoluteSourceFilePath, localTargetFilePath, true);
		}

		public static void DeleteFileThenDeleteFolderIfEmpty (string localTargetFilePath) {
			
			File.Delete(localTargetFilePath);
			File.Delete(localTargetFilePath + ShaderKitchenSettings.UNITY_METAFILE_EXTENSION);
			var directoryPath = Directory.GetParent(localTargetFilePath).FullName;
			var restFiles = FilePathsInFolder(directoryPath);
			if (!restFiles.Any()) {
				Directory.Delete(directoryPath, true);
				File.Delete(directoryPath + ShaderKitchenSettings.UNITY_METAFILE_EXTENSION);
			}
		}

		public static List<string> FilePathsOfFile (string filePath) {
			var folderPath = Path.GetDirectoryName(filePath);
			var results = FilePathsInFolder(folderPath);
			return results;
		}

		public static List<string> FilePathsInFolder (string localFolderPath) {
			var filePaths = new List<string>();
			
			if (string.IsNullOrEmpty(localFolderPath)) return filePaths;
			if (!Directory.Exists(localFolderPath)) return filePaths;

			GetFilePathsRecursive(localFolderPath, filePaths);
			
			return filePaths;
		}

		private static void GetFilePathsRecursive (string localFolderPath, List<string> filePaths) {
			var folders = Directory.GetDirectories(localFolderPath);
			
			foreach (var folder in folders) {
				GetFilePathsRecursive(folder, filePaths);
			}

			var files = FilePathsInFolderOnly1Level(localFolderPath);
			filePaths.AddRange(files);
		}

		public static List<string> FolderPathsInFolder (string path) {
			// change platform-depends folder delimiter -> '/'
			return ConvertSeparater(Directory.GetDirectories(path).ToList());
		}

		/**
			returns file paths which are located in the folder.

			this method is main point for supporting path format of cross platform.

			Usually Unity Editor uses '/' as folder delimter.

			e.g.
				Application.dataPath returns 
					C:/somewhere/projectPath/Assets @ Windows.
						or
					/somewhere/projectPath/Assets @ Mac, Linux.


			but "Directory.GetFiles(localFolderPath + "/")" method returns different formatted path by platform.

			@ Windows:
				localFolderPath + / + somewhere\folder\file.extention

			@ Mac/Linux:
				localFolderPath + / + somewhere/folder/file.extention

			the problem is, "Directory.GetFiles" returns mixed format path of files @ Windows.
			this is the point of failure.

			this method replaces folder delimiters to '/'.
		*/
		public static List<string> FilePathsInFolderOnly1Level (string localFolderPath) {
			// change platform-depends folder delimiter -> '/'
			var filePaths = ConvertSeparater(Directory.GetFiles(localFolderPath)
					.Where(path => !(Path.GetFileName(path).StartsWith(ShaderKitchenSettings.DOTSTART_HIDDEN_FILE_HEADSTRING)))
					.ToList());

			if (ShaderKitchenSettings.IGNORE_META) filePaths = filePaths.Where(path => !IsMetaFile(path)).ToList();

			return filePaths;
		}

		public static List<string> ConvertSeparater (List<string> source) {
			return source.Select(filePath => filePath.Replace(Path.DirectorySeparatorChar.ToString(), ShaderKitchenSettings.UNITY_FOLDER_SEPARATOR.ToString())).ToList();
		}

		/**
			create combination of path.

			delimiter is always '/'.
		*/
		public static string PathCombine (params string[] paths) {
			if (paths.Length < 2) throw new Exception("failed to combine paths: only 1 path.");

			var combinedPath = _PathCombine(paths[0], paths[1]);
			var restPaths = new string[paths.Length-2];

			Array.Copy(paths, 2, restPaths, 0, restPaths.Length);
			foreach (var path in restPaths) combinedPath = _PathCombine(combinedPath, path);

			return combinedPath;
		}

		private static string _PathCombine (string head, string tail) {
			if (!head.EndsWith(ShaderKitchenSettings.UNITY_FOLDER_SEPARATOR.ToString())) head = head + ShaderKitchenSettings.UNITY_FOLDER_SEPARATOR;
			
			if (string.IsNullOrEmpty(tail)) return head;
			if (tail.StartsWith(ShaderKitchenSettings.UNITY_FOLDER_SEPARATOR.ToString())) tail = tail.Substring(1);

			return Path.Combine(head, tail);
		}

		public static bool IsMetaFile (string filePath) {
			if (filePath.EndsWith(ShaderKitchenSettings.UNITY_METAFILE_EXTENSION)) return true;
			return false;
		}
	}
}