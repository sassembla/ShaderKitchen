using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using Unidon;

public class IndexViewController : MonoBehaviour {
	
	
	public void Awake () {}
	
	public void ShowContext () {
		// 右クリックを検知できるのかな。そもそもなんかキーバインド持って行かれてるきがするな。
		// 
	}
	
	public void GoToIndex (int index) {
		switch (index) {
			default: {
				Debug.Log("here comes!" +index);
				var sceneName = "page_" + (index);
				SiteManager.sManager.OpenScene(sceneName);
				break;
			}
		}
		
	}
	
    [DllImport("__Internal")] private static extern void Connect();

    
    [DllImport("__Internal")] private static extern void SendLog(string message);
		
	
	[DllImport("__Internal")] private static extern void CopyToClipboard (string text);
	
	// この発展系でDLもいけるはず。
}