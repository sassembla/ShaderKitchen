using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unidon {
	public class UnidonSettings {
		public const string BOOT_HTML_NAME = "index.html";
		public const string PATH_DELIMITER = "/";
		
		public const string CONTENTS_PATH = "UnidonContents";
		public const string CONTENTS_TARGET_EXTENSION = ".unity";
	}
	
	public class BootViewController : MonoBehaviour {
		
		private static SiteManager siteManager;
		
		/*
			this method will running between Awake and Start.
		*/
		[RuntimeInitializeOnLoadMethod] static void OnRuntimeMethodLoad () {
			var dataPath = Application.dataPath;
			var url = string.Empty;
			
			// set url for resources.
			/*
				local
			*/
			if (string.IsNullOrEmpty(Application.absoluteURL)) {
				var localAssetPathBase = Directory.GetParent(dataPath).ToString();
				var localAssetPath = Path.Combine(localAssetPathBase, "UnidonWeb/" + UnidonSettings.BOOT_HTML_NAME);
				
				url = "file://" + localAssetPath;
			}
			/*
				web
			*/
			else {
				url = Application.absoluteURL;
			}
			
			
			var usingMemory = Profiler.GetTotalAllocatedMemory();
			var reservedMemory = Profiler.GetTotalReservedMemory();
			
			Debug.Log("load done, show everything. url:" + url + " dataPath:" + dataPath + " usingMemory:" + usingMemory + " reservedMemory:" + reservedMemory);
			
			// url:file:///Users/tomggg/Desktop/UniCMS/UniCMSWeb/index.html 
			// dataPath:file:///Users/tomggg/Desktop/UniCMS/UniCMSWeb 
			// usingMemory:		1,143,025 
			// reservedMemory:	1,585,093
			
			switch (SceneManager.GetActiveScene().name) {
				case "Boot": {
					/*
						起動はすべてBootから行われるが、URLがBoot用(index.htmlとか)でない場合、対象のシーンから開く、とかを実行したい。
						なので、「起動シーンはBootだが、Indexではなく特定のページを表示する」とかはあり、ここのコードを拡張することで得られそう。
					*/
					// loaded from Boot scene. start loading index scene.
					var siteManagerObj = GameObject.Find("SiteManagerObject");
					siteManager = new SiteManager(siteManagerObj, url);
					
					var lastPathComponent = Path.GetFileName(url);
					switch (lastPathComponent) {
						case "index.html": {
							siteManager.LoadIndexView();
							break;
						}
						default: {
							var pageCount = 0; 
							break;
						}
					}
					break;
				}
				default: {
					/*
						boot以外からの起動で、まあ、エディタからしかここにこれないと思う。
					*/
					var existSiteManagerObj = GameObject.Find("SiteManagerObject");
					if (existSiteManagerObj != null) return;
					
					// create instance from prefab.
					var prefab = Resources.Load("SiteManagerObject") as GameObject;
					var siteManagerObj = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
					
					siteManager = new SiteManager(siteManagerObj, url);
					break;
				}
			}
		}
	}
}