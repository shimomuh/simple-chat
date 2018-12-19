using System;
using SimpleChat.Domain.BusinessModel;
using SimpleChat.Domain.Model;
using UnityEngine;
using WebSocketSharp;

namespace SimpleChat.Application
{
    /// <summary>
    /// [Singleton] Chat をするための HttpClient
    /// Note: 二重送信を防ぐためシングルトン化
    /// </summary>
    public class HttpClient
    {
        #region singleton

        private static HttpClient instance;
        public static HttpClient Instance
        {
            get {
                if (instance == null) {
                    instance = new HttpClient();
                }
                return instance;
            }
        }

        #endregion

        private WebSocket ws;
        private User sendUser;
        public Action<Message> ReceiveMessageCallback;

        private HttpClient()
        {
            // TODO: プロトコルを wss にしたい see: https://github.com/sta/websocket-sharp
            // TODO: 定数化。外だし
            ws = new WebSocket("ws://localhost:3000/");
            ws.OnOpen += OnOpen;
            ws.OnMessage += OnMessage;
            ws.OnError += OnError;
            ws.OnClose += OnClose;
        }

        public void SetSendUser(User user)
        {
            this.sendUser = user;
        }

        public void TryConnect()
        {
            ws.Connect();
        }

        public void OnOpen(object sender, EventArgs e)
        {
            Debug.Log("WebSocket Open");
        }

        public void OnMessage(object sender, MessageEventArgs e)
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(e.RawData);
            Message message = JsonUtility.FromJson<Message>(jsonString);
            ReceiveMessageCallback(message);
        }

        public void OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        }

        public void OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log("WebSocket Close");
        }

        public void Send(string value)
        {
            Message message = new Message(sendUser, value);
            string jsonData = message.ToJson();
            ws.Send(jsonData);
        }

        public void Destroy()
        {
            ws.Close();
            ws = null;
        }
    }
}