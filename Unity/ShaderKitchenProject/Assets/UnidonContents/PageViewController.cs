using UnityEngine;
using System.Collections;
using Unidon;

public class PageVIewController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void BackToIndex () {
		SiteManager.sManager.BackToIndex();
	}
	
	public void Download (string url) {
		WebViewFunction.DownloadFile(url);
	}
}
