using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unidon;
using UnityEngine.SceneManagement;

public class WebTextScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
		/*
			やってることは、
			・特定のパスのPage_1.mdを読み込む(これはビルド時にバイナリにできると良いので、機構もろともAssetBundle化される前提が好ましい。)
			・読み込んだデータをrichTextに変形させる
			ってとこなので、それがGUI上でセットできると嬉しい。
		*/
		
		var rectTrans = GetComponent<InputField>();// このコンポーネント、最初から組み込めないかな。できるような。
		
		// get scene name then use it for loading text from Resources/SCENE_NAME.txt
		var currentSceneName = SceneManager.GetActiveScene().name;
		
		// load text data from .md
		var bindata = Resources.Load(currentSceneName) as TextAsset;// ここ、AssetBundleにできるといいなあ。更新が楽。
		rectTrans.text = WebViewFunction.MarkdownToRichText(bindata.text);
	}
	
}
