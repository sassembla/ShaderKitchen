using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unidon {
	
	public class FileController {
		private const char UNITY_FOLDER_SEPARATOR = '/';
		
		private const string UNITY_METAFILE_EXTENSION = ".meta";
		
		private const string DOTSTART_HIDDEN_FILE_HEADSTRING = ".";
		
		private const bool IGNORE_META = true;
		
		// private const 
		
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
			File.Delete(localTargetFilePath + UNITY_METAFILE_EXTENSION);
			var directoryPath = Directory.GetParent(localTargetFilePath).FullName;
			var restFiles = FilePathsInFolder(directoryPath);
			if (!restFiles.Any()) {
				Directory.Delete(directoryPath, true);
				File.Delete(directoryPath + UNITY_METAFILE_EXTENSION);
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
					.Where(path => !(Path.GetFileName(path).StartsWith(DOTSTART_HIDDEN_FILE_HEADSTRING)))
					.ToList());

			if (IGNORE_META) filePaths = filePaths.Where(path => !IsMetaFile(path)).ToList();

			return filePaths;
		}

		public static List<string> ConvertSeparater (List<string> source) {
			return source.Select(filePath => filePath.Replace(Path.DirectorySeparatorChar.ToString(), UNITY_FOLDER_SEPARATOR.ToString())).ToList();
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
			if (!head.EndsWith(UNITY_FOLDER_SEPARATOR.ToString())) head = head + UNITY_FOLDER_SEPARATOR;
			
			if (string.IsNullOrEmpty(tail)) return head;
			if (tail.StartsWith(UNITY_FOLDER_SEPARATOR.ToString())) tail = tail.Substring(1);

			return Path.Combine(head, tail);
		}

		public static bool IsMetaFile (string filePath) {
			if (filePath.EndsWith(UNITY_METAFILE_EXTENSION)) return true;
			return false;
		}
	}
}