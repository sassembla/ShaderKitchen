using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unidon;

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
		
		// load text data from .md
		var bindata = Resources.Load("Page_1") as TextAsset;// ここ、AssetBundleにできるといいなあ。更新が楽。
		rectTrans.text = WebViewFunction.MarkdownToRichText(bindata.text);
	}
	
}
