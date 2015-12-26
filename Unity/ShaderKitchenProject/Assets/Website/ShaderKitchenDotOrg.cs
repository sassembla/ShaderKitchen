using System.Collections.Generic;
using UnityEngine;

/**
	head page drawer for website.
	あくまでフロントエンドとして、Websiteの皮を被って動けばいい。
	ユーザーの手元のUnityと連携させるようなことはあんまり考えてない(必要ができればやる)
	たぶんブラウザのほうが圧倒的に便利なので、
	・ビューを作る
	・packageをDLさせる
	以上のことはしなそう。

	ログインとか？ まあ必要ができてから考えよう。
*/
public class ShaderKitchenDotOrg : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("DBどうすっかな〜");
	}
    
	
	// Update is called once per frame
	void Update () {
		
	}
    
    /**
        なんかIDとかを基準に、packageをDLする。
    */
    public void DownloadPackage () {
        Debug.Log("DownloadPackage");
        // ここでJSを起動、ってできればそれでよさそうなんだよな。単純な、同じoriginから素材をDLするやつ。
        // Unityの世界からDLする必要はなくて、単純に
    }
    
}
