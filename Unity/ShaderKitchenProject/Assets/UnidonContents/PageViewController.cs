using UnityEngine;
using System.Collections;
using Unidon;

public class PageViewController : MonoBehaviour {
	
	public void BackToIndex () {
		SiteManager.sManager.BackToIndex();
	}
	
	public void DownloadPackage (int index) {
		Debug.LogError("not yet implemented");
	}
}
