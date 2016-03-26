using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WebTextScript : MonoBehaviour {
	private string html;
	private Rect textRect;
	
	// Use this for initialization
	void Start () {
		var rectTrans = GetComponent<Text>();
		
		// load text data from .md
		var bindata = Resources.Load("Page_1") as TextAsset;
		html = WebViewFunction.MarkdownToHTML(bindata.text);
		Debug.LogError("html:" + html);
		rectTrans.text = html;
	}
	
	void OnGUI () {
		// うーーん、、、これ使いたくないなあ。。
		// var rectTrans = GetComponent<RectTransform>();
		// textRect = new Rect(rectTrans.rect.x + 500, rectTrans.rect.y + 100, rectTrans.rect.width, rectTrans.rect.height);
		
		// WebViewFunction.DrawHTML(html, textRect);
	}
}
