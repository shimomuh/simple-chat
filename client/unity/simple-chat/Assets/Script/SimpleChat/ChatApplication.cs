using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleChat
{
    public class ChatApplication : MonoBehaviour
    {
        [SerializeField]
        private InputField inputField;

        private InputFieldView inputFieldView;

        private SynchronizationContext context;

        private HttpClient httpClient;

        void Awake()
        {

            inputFieldView = inputField.GetComponent<InputFieldView>();

            // メインスレッドを表す context
            context = SynchronizationContext.Current;

            httpClient = new HttpClient();
            httpClient.ReceiveMessageCallback = ReceiveMessageCallback;
            httpClient.TryConnect();

            inputFieldView.InputMessageCallback = SendToMessage;
        }

        private void SendToMessage(string message) {
            httpClient.Send(message);
        }

        public void ReceiveMessageCallback(string message)
        {
             // メインスレッドに処理を戻す
            context.Post(__ => {
                inputFieldView.ReceiveMessage(message);
            }, null);
        }

        public void Destroy()
        {
            httpClient.Destroy();
        }
    }
}
