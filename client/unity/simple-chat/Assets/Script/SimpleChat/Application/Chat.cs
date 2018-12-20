using System.Threading;
using SimpleChat.Domain.BusinessModel;
using SimpleChat.Domain.Model;
using SimpleChat.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleChat.Application
{
    public class Chat : MonoBehaviour
    {
        [SerializeField]
        private InputField inputField;

        private InputFieldView inputFieldView;

        private SynchronizationContext context;

        private WebSocketClient webSocketClient;

        [SerializeField]
        private uint userId;

        [SerializeField]
        private string userName;

        [SerializeField]
        private string userThumbnailUrl;

        private User user;

        private void Awake()
        {
            inputFieldView = inputField.GetComponent<InputFieldView>();

            // メインスレッドを表す context
            context = SynchronizationContext.Current;
            CreateUser();

            webSocketClient = WebSocketClient.Instance;
            webSocketClient.SetSendUser(user);
            webSocketClient.ReceiveMessageCallback = ReceiveMessageCallback;
            webSocketClient.TryConnect();

            inputFieldView.SetSender(user);
            inputFieldView.InputMessageCallback = SendToMessage;
        }

        private void CreateUser()
        {
            if (user == null) {
                user = new User(userId, userName, userThumbnailUrl); 
            }
        }

        private void SendToMessage(string message) {
            webSocketClient.Send(message);
        }

        public void ReceiveMessageCallback(Message message)
        {
             // メインスレッドに処理を戻す
            context.Post(__ => {
                if (user.id == message.user.id) {
                    return;
                }
                inputFieldView.ReceiveMessage(message);
            }, null);
        }

        public void Destroy()
        {
            webSocketClient.Destroy();
        }
    }
}
