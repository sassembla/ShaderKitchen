using UnityEngine;
using System.Collections;

public class WebTextScript : MonoBehaviour {
	private string html;
	private Rect textRect;
	
	// Use this for initialization
	void Start () {
		// var rectTrans = GetComponent<RectTransform>();
		// textRect = rectTrans.rect;
		
		// load text data from .md
		var bindata = Resources.Load("Page_1") as TextAsset;
		html = WebViewFunction.MarkdownToHTML(bindata.text);
	}
	
	void OnGUI () {
		// うーーん、、、これ使いたくないなあ。。
		// var rectTrans = GetComponent<RectTransform>();
		// textRect = new Rect(rectTrans.rect.x + 500, rectTrans.rect.y + 100, rectTrans.rect.width, rectTrans.rect.height);
		
		WebViewFunction.DrawHTML(html, textRect);
	}
}
