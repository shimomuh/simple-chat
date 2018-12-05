using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace SimpleChat
{
    public class ChatApplication : MonoBehaviour
    {
        private InputFieldView inputFieldView;
        [SerializeField]
        private InputField inputField;

        private WebSocket ws;

        void Awake()
        {
            // メインスレッドを表す context
            var context = SynchronizationContext.Current;
            inputFieldView = inputField.GetComponent<InputFieldView>();

            // TODO: プロトコルを wss にしたい see: https://github.com/sta/websocket-sharp
            ws = new WebSocket("ws://localhost:3000/");
            ws.OnOpen += (sender, e) => { Debug.Log("WebSocket Open"); };
            ws.OnMessage += (sender, e) =>
            {
                // メインスレッドに処理を戻す
                context.Post(__ => {
                    string message = System.Text.Encoding.UTF8.GetString(e.RawData);
                    inputFieldView.ReceiveMessage(message);
                }, null);
            };
            ws.OnError += (sender, e) => { Debug.Log("WebSocket Error Message: " + e.Message); };
            ws.OnClose += (sender, e) => { Debug.Log("WebSocket Close"); };

            ws.Connect();
            inputField.onEndEdit.AddListener(delegate { SendMessage(inputField); });
        }

        private void SendMessage(InputField input) {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(input.textComponent.text); 
            ws.Send(data); 
        }

        public void Destroy()
        {
            ws.Close();
            ws = null;
        }
    }
}
