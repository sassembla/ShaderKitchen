using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class WebStreamImageScript : MonoBehaviour {
	private Image imagePanel;
	private string sceneName;

	// Use this for initialization
	void Start () {
		Debug.LogWarning("このタイミングから先で、勝手にpngをロードし、ロードが終わったらアニメーションっていうのをやる。");
		/*
			まずは、AssetBundleにそういうのが含まれるようにしたい。
			・200枚くらいの画像をどうAssetBundleにするか。フォルダに入れてBundlizeルールにしちゃうか。
			・そいつをロードする。のか？AssetBundle化するためのトリガーをHierarchyに置ければいいんだよな。
		*/
		sceneName = SceneManager.GetActiveScene().name;
		imagePanel = GetComponent<Image>();
	}
	
	int frame = 0;
	
	// Update is called once per frame
	void Update () {
		// 適当にResourcesから画像をロードして表示しよう。
		var resName = "Animations/" + sceneName + "/" + frame;
		
		var s = Resources.Load(resName) as Texture2D;
		
		imagePanel.sprite = Sprite.Create(s, new Rect(0,0,s.width, s.height), Vector2.zero);
		
		
		frame++;
		if (100 == frame) frame = 0; 
	}
}
