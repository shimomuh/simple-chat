using System;
using SimpleChat.Domain.Service;
using SimpleChat.Domain.Model;
using UnityEngine;
using WebSocketSharp;
using System.Collections;

namespace SimpleChat.Application
{
    /// <summary>
    /// [Singleton] Chat をするための WebSocketSecureClient
    /// Note: 二重送信を防ぐためシングルトン化
    /// </summary>
    public class WebSocketClient
    {
        #region singleton

        private static WebSocketClient instance;
        public static WebSocketClient Instance
        {
            get {
                if (instance == null) {
                    instance = new WebSocketClient();
                }
                return instance;
            }
        }

        #endregion

        private WebSocket ws;
        private User sendUser;
        public Action<Message> ReceiveMessageCallback;
        public Action ConnectFailureCallback;
        public Action ConnectSuccessCallback;
        public Action ConnectRetryCallback;
        public Action<ErrorEventArgs> ConnectErrorCallback;
        private string endpoint = "wss://localhost:8080/";
        private uint retry;
        public uint Retry { get { return retry; } }
        private bool isAbortTryConnect;
        public bool IsAbortTryConnect { get { return isAbortTryConnect; } }

        private WebSocketClient()
        {
            Debug.Log("Start");
            retry = 0;
            ws = new WebSocket(endpoint);
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
            retry++;
            ws.Connect();
        }

        public void ResetRetry()
        {
            retry = 0;
        }

        public void AbortTryConnect()
        {
            isAbortTryConnect = true;
            ResetRetry();
        }

        public void OnOpen(object sender, EventArgs e)
        {
            ResetRetry();
            ConnectSuccessCallback();
        }

        public void OnMessage(object sender, MessageEventArgs e)
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(e.RawData);
            Message message = JsonUtility.FromJson<Message>(jsonString);
            ReceiveMessageCallback(message);
        }

        public void OnError(object sender, ErrorEventArgs e)
        {
            ConnectErrorCallback(e);
        }

        public void OnClose(object sender, CloseEventArgs e)
        {
            ConnectFailureCallback();
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