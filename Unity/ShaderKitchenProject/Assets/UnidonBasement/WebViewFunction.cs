using MarkdownSharp;
using UnityEngine;

public class WebViewFunction : MonoBehaviour {
	private static Markdown markdownSharp = new Markdown();
	
	private static string html = "";
		
	const string stringToEdit = "##Unlit\n一番シンプルな、テクスチャを表示するだけのシェーダーです<br>\nTextureのタイリングやオフセットをUnityではどう取り扱っているのか確認できます<br>\n\n```\nShader \"ShaderDic/SMP1_Unlit\"\n{\n	Properties\n	{\n		_MainTex (\"Texture\", 2D) = \"white\" {}\n	}\n	\n	SubShader\n	{\n		Tags { \"RenderType\"=\"Opaque\" }\n		LOD 100\n\n		Pass\n		{\n			CGPROGRAM\n			#pragma vertex vert\n			#pragma fragment frag\n			#include \"UnityCG.cginc\"\n			\n			struct appdata //UnityCG.cginc参照\n			{\n				float4 vertex : POSITION;\n				float2 uv : TEXCOORD0;\n			};\n			struct v2f\n			{\n				float2 uv : TEXCOORD0;\n				float4 vertex : SV_POSITION;\n			};\n			sampler2D _MainTex;\n			float4 _MainTex_ST;\n			v2f vert (appdata v)\n			{\n				v2f o;\n				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); \n		// モデルビュー行列（Object2World）x 射影行列 (v.vertex)\n		// ワールド座標系からプロジェクション座標系\n		//（レンダリングされる画面の座標系）へ変換します		\n				o.uv = TRANSFORM_TEX(v.uv, _MainTex);\n		//InspectorのTillingとOffsetを反映するための処理になります。 		//(UnityCg.cginc参照)\n		// #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)\n		//o.uv = v.uv;\n		// ↑こっちにしてみると、TillingとOffsetがどう変わるかわかります\n	\n				return o;\n			}\n\n			fixed4 frag (v2f i) : SV_Target\n			{\n				fixed4 col = tex2D(_MainTex, i.uv);\n				return col;\n			}\n			ENDCG\n		}\n	}\n}\n```";
	
	
	void Start () {
		html = markdownSharp.Transform(stringToEdit);
	}
	
	void OnGUI () {
		DrawHTML(html, new Rect(Screen.width / 2 + 10, 20, Screen.width - (10 + Screen.width / 2 + 10), Screen.height - 20));
	}
	
	
	private static void DrawHTML (string text, Rect showRect) {
		//backup color 
		Color backupColor = GUI.color;
		Color backupContentColor = GUI.contentColor;
		Color backupBackgroundColor = GUI.backgroundColor;

		//add textarea with transparent text
		GUI.contentColor = new Color(1f, 1f, 1f, 0f);
		GUIStyle style = new GUIStyle(GUI.skin.textArea);
		
		// このサイズでUIを出力すればよさげ！よし、ビューのProxyができるぞ。
		Rect bounds = showRect;
		text = GUI.TextArea(bounds, text);

		//get the texteditor of the textarea to control selection
		int controlID = GUIUtility.GetControlID(bounds.GetHashCode(), FocusType.Keyboard);    
		TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID -1);

		//set background of all textfield transparent
		GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);    

		//backup selection to remake it after process
		int backupPos = editor.cursorIndex;
		int backupSelPos = editor.selectIndex;

		//get last position in text
		editor.MoveTextEnd();
		int endpos = editor.cursorIndex;
		UnityEngine.Random.seed = 123;

		//draw textfield with color on top of text area
		editor.MoveTextStart();        
		while (editor.cursorIndex != endpos) {
			editor.SelectToStartOfNextWord();
			string wordtext = editor.SelectedText;    

			//set word color
			GUI.contentColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

			//draw each word with a random color
			Vector2 pixelselpos = style.GetCursorPixelPosition(editor.position, editor.content, editor.selectIndex);
			Vector2 pixelpos = style.GetCursorPixelPosition(editor.position, editor.content, editor.cursorIndex);
			GUI.TextField(new Rect(pixelselpos.x - style.border.left, pixelselpos.y - style.border.top, pixelpos.x, pixelpos.y), wordtext);

			editor.MoveToStartOfNextWord();
		}

		//Reposition selection
		Vector2 bkpixelselpos = style.GetCursorPixelPosition(editor.position, editor.content, backupSelPos);    
		editor.MoveCursorToPosition(bkpixelselpos);    

		//Remake selection
		Vector2 bkpixelpos = style.GetCursorPixelPosition(editor.position, editor.content, backupPos);    
		editor.SelectToPosition(bkpixelpos);    

		//Reset color
		GUI.color = backupColor;
		GUI.contentColor = backupContentColor;
		GUI.backgroundColor = backupBackgroundColor;
	}
}