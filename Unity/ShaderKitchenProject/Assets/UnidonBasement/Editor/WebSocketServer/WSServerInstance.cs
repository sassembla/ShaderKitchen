using UnityEngine;

using System;

using ARWebSocket;
using ARWebSocket.Server;

/**
	Receive log message from browser.
*/
public class WSServerInstance : WebSocketBehavior {

	protected override void OnMessage (MessageEventArgs e) {
		Debug.Log("e:" + e.Data);
	}

	protected override void OnOpen () {
		Debug.Log("client connected.");
	}

	protected override void OnClose (CloseEventArgs e) {
		Debug.Log("code:"+ e.Code + " reason:" + e.Reason);
	}
}