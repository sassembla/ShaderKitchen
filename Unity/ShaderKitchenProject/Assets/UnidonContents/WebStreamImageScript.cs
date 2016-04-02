using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ShaderKitchen.WebUI {
	public class WebStreamImageScript : MonoBehaviour {
		private Image imagePanel;
		private string sceneName;
		
		
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
	}
}