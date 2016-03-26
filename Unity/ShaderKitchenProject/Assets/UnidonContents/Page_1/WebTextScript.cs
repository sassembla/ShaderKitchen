using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WebTextScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
		// この部分がGUIから済むようになるといいな。設定、表示を含めて。
		var rectTrans = GetComponent<InputField>();
		
		// load text data from .md
		var bindata = Resources.Load("Page_1") as TextAsset;// ここ、AssetBundleにできるといいなあ。更新が楽。
		rectTrans.text = WebViewFunction.MarkdownToRichText(bindata.text);
	}
	
}
