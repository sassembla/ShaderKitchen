var WebUtil = {
	CopyToClipboard : function (textSource) {
		console.log("特定の文字列をcopy textSource:" + textSource);
		// document.execCommand("Copy");
	},
	DownloadFile : function (targetUrl) {
		var urlString = Pointer_stringify(targetUrl);
		console.log("targetUrlのファイルを、DLしてもらう。 urlString:" + urlString);
		var link = document.createElement("a");
		// link.download = name;
		link.href = urlString;
		link.click();
	}
};

mergeInto(LibraryManager.library, WebUtil);