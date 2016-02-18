using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Collections.Generic;

using ARWebSocket;
using ARWebSocket.Server;

[InitializeOnLoad]
public class ServerInitializer {

	private static WebSocketServer wsServer;
	private static Action<string> emitter;

	private static List<string> offlineLogBuffer = new List<string>();

	static ServerInitializer () {
		Initialize();
	}
	
	private static void Initialize () {
		wsServer = new WebSocketServer(
			WebConsoleDefinitions.SERVE_PROTOCOL + WebConsoleDefinitions.SERVE_ADDRESS + ":" + WebConsoleDefinitions.SERVE_PORT
		);
		wsServer.AddWebSocketService<WSServerInstance>("/");
		wsServer.Start();
	}

	// public static void Initialized (Action<string> emitterSource) {
	// 	// emitter = emitterSource;
	// }

	private static void EmitLogAsync (string message) {
		emitter(message);
	}

}