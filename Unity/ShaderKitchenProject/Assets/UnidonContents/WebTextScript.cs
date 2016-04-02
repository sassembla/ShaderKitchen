using UnityEngine;
using UnityEngine.UI;
using Unidon;
using UnityEngine.SceneManagement;

namespace ShaderKitchen.WebUI
{
    public class WebTextScript : MonoBehaviour {
		public TextAsset textData;
		
		// Use this for initialization
		void Start () {
			if (textData == null) {
				Debug.LogError("WebText data is null:" + SceneManager.GetActiveScene().name);
				return;
			}
			
			var rectTrans = GetComponent<InputField>();// このコンポーネント、特殊なGUIパーツにして、ユーザーが置かないでも最初から組み込めないかな。できるような。
			rectTrans.text = WebViewFunction.MarkdownToRichText(textData.text);
		}
		
	}
}