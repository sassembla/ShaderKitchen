using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour {

	int frame = 0;
	
	void Update () {
		Debug.LogWarning("late frame:" + frame);
		frame++;
	}
	
	void OnGUI () {
		GUI.Button(new Rect(0,0,100,100), "frame:" + frame);
	}
	
}
