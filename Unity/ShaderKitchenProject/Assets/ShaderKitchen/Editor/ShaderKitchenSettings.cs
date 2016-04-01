namespace ShaderKitchen {
	public class ShaderKitchenSettings {
		/*
			このへんがrecordのセッティング。
			MAX_SCREENSHOT_FRAME はスクリーンショットの枚数、
			SCREENSHOT_SIZE_WIDTH, SCREENSHOT_SIZE_HEIGHT はスクリーンショットのサイズ指定。
			
			SCREENSHOT_SIZE_WIDTH, SCREENSHOT_SIZE_HEIGHT のどちらか、短い辺に合わせた状態でセンタリング + トリミングされる。
			
			たとえばScreenSizeが1000x400だった時、
			SCREENSHOT_SIZE_WIDTH = 200, SCREENSHOT_SIZE_HEIGHT = 800 だと、
			200 x 800 サイズに、1000x400の高さ400 -> 800へと拡大、横幅も2000になるが、そこから200(中心から左右に100pixel分)クリッピングした状態でスクリーンショットが作成される。
		*/
		public const string SCREENSHOT_FILE_EXTENSION = ".png";
		public const int MAX_SCREENSHOT_FRAME = 100;
		public const int SCREENSHOT_SIZE_WIDTH = 366;
		public const int SCREENSHOT_SIZE_HEIGHT = 266;
		
		public const string SETTING_FILE_PATH = "Assets/ShaderKitchen/Editor/Data/data.json";
		public const string SCREENSHOT_PATH = "Assets/ShaderKitchen/Editor/ScreenTemp";
		
		public const string GUI_MENU_RECORDING = "ShaderKitchen/Record";
		
		public const string GUI_MENU_EXPORTPACKAGE = "ShaderKitchen/Export package(test)";
		public const bool IGNORE_META = true;
		public const string UNITY_METAFILE_EXTENSION = ".meta";
		public const char UNITY_FOLDER_SEPARATOR = '/';
		public const string DOTSTART_HIDDEN_FILE_HEADSTRING = ".";
	}
}