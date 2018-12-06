using System;
using UnityEngine;
using WebSocketSharp;

namespace SimpleChat
{
    // TODO: シングルトン化
    public class HttpClient
    {
        private WebSocket ws;
        public Action<string> ReceiveMessageCallback;

        public HttpClient()
        {
            // TODO: プロトコルを wss にしたい see: https://github.com/sta/websocket-sharp
            // TODO: 定数化。外だし
            ws = new WebSocket("ws://localhost:3000/");
            ws.OnOpen += OnOpen;
            ws.OnMessage += OnMessage;
            ws.OnError += OnError;
            ws.OnClose += OnClose;
        }

        public void TryConnect()
        {
            ws.Connect();
        }

        public void OnOpen(object sender, System.EventArgs e)
        {
            Debug.Log("WebSocket Open");
        }

        public void OnMessage(object sender, MessageEventArgs e)
        {
            string message = System.Text.Encoding.UTF8.GetString(e.RawData);
            ReceiveMessageCallback(message);
        }

        public void SetReceiveMessageCallback(Action<string> callback)
        {
            ReceiveMessageCallback = callback;
        }

        public void OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        }

        public void OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log("WebSocket Close");
        }

        public void Send(string message)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            ws.Send(data);
        }

        public void Destroy()
        {
            ws.Close();
            ws = null;
        }
    }
}