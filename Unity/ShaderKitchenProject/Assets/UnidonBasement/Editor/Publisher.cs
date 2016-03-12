using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Unidon {
    public class Publisher {
		
		
		[MenuItem ("Unidon/Build Contents")] static void BuildContentAssets () {
			// automatically set assetbundle mark to specific path.
			var bundleTargetBasePath = FileController.PathCombine(Application.dataPath, UnidonSettings.CONTENTS_PATH);
			var targetSceneFilePaths = FileController.FilePathsInFolder(bundleTargetBasePath)
				.Where(path => IsQualified(path))
				.Select(path => path.Replace(Application.dataPath, "Assets"))
				.ToList(); 
			
			foreach (var targetSceneFilePath in targetSceneFilePaths) {
				var assetName = Path.GetFileNameWithoutExtension(targetSceneFilePath).ToLower();
				var importer = AssetImporter.GetAtPath(targetSceneFilePath);
				importer.assetBundleName = assetName;
			}
			
			var assetTargetPath = FileController.PathCombine(Directory.GetParent(Application.dataPath).ToString(), "UnidonWeb/AssetBundles");
			FileController.RemakeDirectory(assetTargetPath);
			BuildPipeline.BuildAssetBundles(assetTargetPath, 0, BuildTarget.WebGL);
		}
		
		[MenuItem ("Unidon/Generate link.xml")] static void GenerateLinkXML () {
			var classIds = new HashSet<int>{
				1,
				4,
				20,
				21,
				28,
				48,
				81,
				89,
				90,
				91,
				92,
				104,
				108,
				114,
				114,
				114,
				114,
				114,
				114,
				114,
				114,
				115,
				124,
				128,
				157,
				213,
				222,
				223,
				224,
				258,
			};
			ClassIdCollector.ExportLinkXMLWithUsingClassIds(classIds);
			AssetDatabase.Refresh();
		}
		
		[MenuItem ("Unidon/Publish Site")] static void Publish () {
			BuildContentAssets();
			
			GenerateLinkXML();
			
			var targetPath = FileController.PathCombine(Directory.GetParent(Application.dataPath).ToString(), "UnidonWeb");
			BuildPipeline.BuildPlayer(new string[]{"Assets/UnidonBasement/Boot.unity"}, targetPath, BuildTarget.WebGL, 0);
		}
		
		
		private static bool IsQualified (string path) {
			if (!path.EndsWith(UnidonSettings.CONTENTS_TARGET_EXTENSION)) return false;
			
			var fileName = Path.GetFileNameWithoutExtension(path);
			var folderName = Path.GetFileName(Directory.GetParent(path).ToString());
			if (fileName == folderName) return true; 
			return false;
		}
	}
}