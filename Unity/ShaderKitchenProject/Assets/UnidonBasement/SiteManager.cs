using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unidon {
    public class SiteManager {
		private readonly GameObject siteManagerObj;
		private BootViewController cont;
		
		private readonly string basePath;
		private readonly string targetAssetPathBase;
		
		public static SiteManager sManager;
		
		private List<string> loadingUrls = new List<string>();
		
		private Dictionary<string, AssetBundle> assetCache = new Dictionary<string, AssetBundle>();
		
		public SiteManager (GameObject siteManagerObj, string indexUrl) {
			this.siteManagerObj = siteManagerObj;
			this.cont = siteManagerObj.GetComponent<BootViewController>() as BootViewController;
			this.basePath = indexUrl.Replace(UnidonSettings.BOOT_HTML_NAME, string.Empty);
			this.targetAssetPathBase = Path.Combine(basePath, "AssetBundles");
			
			Caching.CleanCache();
			sManager = this;
		}
		
		public void BackToIndex () {
			LoadIndexView();
		}
		
		public void LoadIndexView () {
			var sceneName = "index";
			var targetSceneAssetPath = Path.Combine(targetAssetPathBase, sceneName);
			OpenSceneFromURL(sceneName, targetSceneAssetPath);
		}
		
		public void OpenUrl (string url) {
			if (url.StartsWith("http")) {
				Debug.Log("外部サイトへ、別タブで開くとか。");
				return;
			}
			
			if (url.StartsWith(UnidonSettings.PATH_DELIMITER)) {
				Debug.LogError("/Aとかで来てる。どうすっかな〜〜 url:" + url);
				return;
			}
			
			var sceneName = url;
			var targetSceneAssetPath = Path.Combine(targetAssetPathBase, sceneName);
			OpenSceneFromURL(sceneName, targetSceneAssetPath);
		}
		
		public void OpenScene (string sceneName) {
			var targetSceneAssetPath = Path.Combine(targetAssetPathBase, sceneName);
			OpenSceneFromURL(sceneName, targetSceneAssetPath);
		}
		
		public void OpenSceneFromURL (string sceneName, string url) {
			// already cached, load from cache.
			if (Caching.IsVersionCached(url, 0)) {
				var sceneAsset = assetCache[url];
				OpenSceneAsset(sceneAsset.GetAllScenePaths()[0]);
				return;
			}
			
			/*
				start loading page data from AssetBundle.
			*/
			cont.StartCoroutine(
				// load resource first.(no mean yet..).
				CacheReadyThenOpenScene(
					url, 
					(AssetBundle asset) => {
						OpenSceneAsset(asset.GetAllScenePaths()[0]);
					}
				)
			);
		}
		
		private IEnumerator CacheReadyThenOpenScene (string sceneUrl, Action<AssetBundle> Finally) {
			while (!Caching.ready) {
				yield return null;
			}
			
			if (loadingUrls.Contains(sceneUrl)) {
				Debug.Log("url:" + sceneUrl + " is now loading.");
				yield break;
			}
			
			Debug.Log("ロード開始、 sceneUrl:" + sceneUrl);
			
			loadingUrls.Add(sceneUrl);
			
			cont.StartCoroutine(
				OpenScene(
					sceneUrl,
					(AssetBundle asset) => {
						assetCache[sceneUrl] = asset;
						loadingUrls.Remove(sceneUrl);
						Finally(asset);
					}
				)
			);
		}
		
		private IEnumerator OpenScene (string sceneUrl, Action<AssetBundle> Finally) {
			Debug.Log("start loading asset from:" + sceneUrl);
			
			AssetBundle asset;
			using (var www = WWW.LoadFromCacheOrDownload(sceneUrl, 0)) {
				while (!www.isDone) {
					yield return null;
				}
				
				if (string.IsNullOrEmpty(www.error)) {
					// no error.
				} else {
					Debug.LogError("failed to download asset of url:" + sceneUrl + " error:" + www.error);
					yield break;
				}
				
				asset = www.assetBundle;
			}
			
			Finally(asset);
		}
		
		private void OpenSceneAsset (string sceneNameSourceStr) {
			var bundledSceneName = ToSceneName(sceneNameSourceStr);
			
			// load scene syncronously.
			SceneManager.LoadScene(bundledSceneName, LoadSceneMode.Single);
			
			var destinationScene = SceneManager.GetSceneByName(bundledSceneName);
		}
		
		private string ToSceneName(string path) {
			string[] parts = path.Split('/');
			string scene = parts[parts.Length - 1];

			return scene.Remove(scene.Length - 6);
		}
		
	}
}