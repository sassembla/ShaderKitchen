using UnityEngine;
using System.Collections;
using Unidon;

public class P1Controller : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnBackTapped () {
		SiteManager.sManager.BackToIndex();
	}
	
	public void GoToPage (int index) {
		SiteManager.sManager.OpenScene("page1");
	}
}
