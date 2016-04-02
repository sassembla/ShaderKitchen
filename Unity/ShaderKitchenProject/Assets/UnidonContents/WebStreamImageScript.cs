using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

namespace ShaderKitchen.WebUI {
	public class WebStreamImageScript : MonoBehaviour {
		private Image imagePanel;
		private string sceneName;
		
		public static string sourceFolderPath;
		
		public List<Texture2D> animations;

		// Use this for initialization
		void Start () {
			sceneName = SceneManager.GetActiveScene().name;
			imagePanel = GetComponent<Image>();
			
			var tex0 = animations[0];
			imagePanel.sprite = Sprite.Create(tex0, new Rect(0, 0, tex0.width, tex0.height), Vector2.zero);
		}
		
		int frame = 0;
		
		// Update is called once per frame
		void Update () {
			var tex = animations[frame];
			imagePanel.sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), Vector2.zero);
			
			frame++;
			if (animations.Count == frame) frame = 0; 
		}
		
		[CustomEditor(typeof(WebStreamImageScript))] public class WebStreamImageInspector : Editor {

			public override void OnInspectorGUI () {
				var animations = ((WebStreamImageScript)target).animations;
				EditorGUILayout.LabelField("animations.Count:", "" + animations.Count);
				
				if (sourceFolderPath == null) sourceFolderPath = string.Empty;
				using (var hor = new GUILayout.HorizontalScope()) {
					GUILayout.Label("Load From Folder Path:", GUILayout.Width(130)); 
					sourceFolderPath = GUILayout.TextField(sourceFolderPath);
					
					var result = GUILayout.Button("Reload", GUILayout.Width(60));
					
					if (string.IsNullOrEmpty(sourceFolderPath)) result = false;
					if (!Directory.Exists(sourceFolderPath)) result = false;
					
					if (result) {
						animations.Clear();
						var filePaths = Directory.GetFiles(sourceFolderPath)
							.Where(path => !path.EndsWith(".meta"))
							// ナンバリングでのオーダーが結局必要。
							.ToArray();
							
						foreach (var filePath in filePaths) {
							Debug.LogError("filePath:" + filePath);
							var res = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)) as Texture2D;
							animations.Add(res);
						}
					}
				}
				
				if (string.IsNullOrEmpty(sourceFolderPath)) {
					EditorGUILayout.HelpBox("folder path is empty.", MessageType.Error);
					return;
				}
				if (!Directory.Exists(sourceFolderPath)) {
					EditorGUILayout.HelpBox("folder does not exists:" + sourceFolderPath, MessageType.Error);
					return;
				}
			}
		}
	}
}