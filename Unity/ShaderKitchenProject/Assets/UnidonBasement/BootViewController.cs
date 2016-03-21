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
		public static List<string> indexies;
		
		private static SiteManager siteManager;
		
		/*
			this method will running between Awake and Start.
		*/
		[RuntimeInitializeOnLoadMethod] static void OnRuntimeMethodLoad () {
			var dataPath = Application.dataPath;
			var url = string.Empty;
			
			// set url for resources.
			if (string.IsNullOrEmpty(Application.absoluteURL)) {
				var localAssetPathBase = Directory.GetParent(dataPath).ToString();
				var localAssetPath = Path.Combine(localAssetPathBase, "UnidonWeb/" + UnidonSettings.BOOT_HTML_NAME);
				
				url = "file://" + localAssetPath;
			} else {
				url = Application.absoluteURL;
			}
			
			if (url.StartsWith("file://")) {
				Debug.Log("running in local. attaching websocket debugguer. browser's log window is suck.");
				// var webSocketConsole = new WebSocketConsole();
				// Application.logMessageReceived += webSocketConsole.SendLog;
			} else {
				Debug.Log("running in production.");
			}
			
			var usingMemory = Profiler.GetTotalAllocatedMemory();
			var reservedMemory = Profiler.GetTotalReservedMemory();
			
			Debug.Log("load done, show everything. url:" + url + " dataPath:" + dataPath + " usingMemory:" + usingMemory + " reservedMemory:" + reservedMemory);
			
			// url:file:///Users/tomggg/Desktop/UniCMS/UniCMSWeb/index.html 
			// dataPath:file:///Users/tomggg/Desktop/UniCMS/UniCMSWeb 
			// usingMemory:		1,143,025 
			// reservedMemory:	1,585,093
			
			// use this for debugging, editing.
			if (SceneManager.GetActiveScene().name != "Boot") {
				Debug.Log("this is not Boot scene, current scene is:" + SceneManager.GetActiveScene().name);	
				return;
			}
			
			// loaded from Boot scene. start loading index scene.
			var siteManagerObj = GameObject.Find("SiteManagerObject");
			DontDestroyOnLoad(siteManagerObj);
			
			siteManager = new SiteManager(siteManagerObj, url);
			
			Debug.Log("urlによって呼び出すものを変えればいい。index.htmlからきてるなら、index。それ以外ならそれ以外のコンテンツを読み出す。 url:" + url);
			siteManager.LoadIndexView();
		}
	}
	
	public class WebSocketConsole {
		// connect to editor.
		public WebSocketConsole () {
			Debug.LogError("not yet implemented. EditorとWSで通信したいっすね。ログをどっかに吐きたい");
		}
		
		
		public void SendLog (string condition, string stackTrace, LogType type) {
			
		}
		
		
	}
}