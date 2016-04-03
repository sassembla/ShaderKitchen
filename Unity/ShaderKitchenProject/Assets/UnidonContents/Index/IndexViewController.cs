using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using Unidon;

public class IndexViewController : MonoBehaviour {
	
	public void GoToIndex (int index) {
		switch (index) {
			default: {
				var sceneName = "page_" + (index);
				SiteManager.sManager.OpenScene(sceneName);
				break;
			}
		}
	}
	
	public void BackToIndex () {
		SiteManager.sManager.BackToIndex();
	}
	
}