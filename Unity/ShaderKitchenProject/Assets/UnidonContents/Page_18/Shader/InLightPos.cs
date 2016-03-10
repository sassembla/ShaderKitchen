using UnityEngine;
using System.Collections;

public class InLightPos : MonoBehaviour {

	public GameObject LightObject;
	private Material Obj;

	// Use this for initialization
	void Start () {
		Obj = transform.GetComponent<MeshRenderer> ().material;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 forwordPos = LightObject.transform.position + LightObject.transform.TransformDirection(Vector3.forward);
		Obj.SetVector ("_LP",LightObject.transform.position);
		Obj.SetVector ("_LPDir",forwordPos);
	}
}
