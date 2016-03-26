using MarkdownSharp;
using UnityEngine;

public class WebViewFunction {
	private static Markdown markdownSharp = new Markdown();
	
	public static string MarkdownToHTML (string markdown) {
		return markdownSharp.Transform(markdown);
	}
	private static bool first = true;
	private static Color col;
	
	public static void DrawHTML (string text, Rect showRect) {
		// この関数の中身を、ブラウザで見ているときとそうでないときで入れ替えればいい。
		
		/*
			stack color, style, cursors.
		*/
		var backupColor = GUI.color;
		var backupContentColor = GUI.contentColor;
		var backupBackgroundColor = GUI.backgroundColor;

		//add textarea with transparent text
		GUI.contentColor = new Color(1f, 1f, 1f, 0f);
		GUIStyle style = new GUIStyle(GUI.skin.textArea);
		text = GUI.TextArea(showRect, text);

		
		int controlID = GUIUtility.GetControlID(showRect.GetHashCode(), FocusType.Keyboard);    
		TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID -1);

		//set background of all textfield transparent
		    

		//backup selection to remake it after process
		int backupPos = editor.cursorIndex;
		int backupSelPos = editor.selectIndex;
		
		
		//get last position in text
		editor.MoveTextEnd();
		int endpos = editor.cursorIndex;
		
		GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);

		//set word color
		if (first) {
			col = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
			first = false;
		}
		GUI.contentColor = col;

		//draw textfield with color on top of text area
		editor.MoveTextStart();        
		while (editor.cursorIndex != endpos) {
			editor.SelectToStartOfNextWord();
			string wordtext = editor.SelectedText;
			
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