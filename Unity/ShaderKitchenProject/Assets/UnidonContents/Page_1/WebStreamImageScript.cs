using UnityEngine;
using System.Collections;

public class WebStreamImageScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.LogError("このタイミングから先で、勝手にpngをロードし、ロードが終わったらアニメーションっていうのをやる。");
		/*
			まずは、AssetBundleにそういうのが含まれるようにしたい。
			・200枚くらいの画像をどうAssetBundleにするか。フォルダに入れてBundlizeルールにしちゃうか。
			・そいつをロードする。のか？AssetBundle化するためのトリガーをHierarchyに置ければいいんだよな。
		*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
