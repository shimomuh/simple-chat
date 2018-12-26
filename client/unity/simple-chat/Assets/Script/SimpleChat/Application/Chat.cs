using System.Threading;
using SimpleChat.Domain.Service;
using SimpleChat.Domain.Model;
using SimpleChat.UI.Presenter;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WebSocketSharp;

namespace SimpleChat.Application
{
    /// <summary>
    /// SimpleChat のメインアプリケーション
    /// </summary>
    public class Chat : MonoBehaviour
    {
        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private Image loading;

        private InputFieldPresenter inputFieldPresenter;

        private SynchronizationContext context;

        private WebSocketClient webSocketClient;
        private static readonly uint retryThreshold = 3;

        [SerializeField]
        private uint userId;

        [SerializeField]
        private string userName;

        [SerializeField]
        private string userThumbnailUrl;

        private User user;

        private void Awake()
        {
            inputFieldPresenter = inputField.GetComponent<InputFieldPresenter>();

            // メインスレッドを表す context
            context = SynchronizationContext.Current;
            CreateUser();

            webSocketClient = WebSocketClient.Instance;
            webSocketClient.SetSendUser(user);
            webSocketClient.ReceiveMessageCallback = ReceiveMessageCallback;
            webSocketClient.ConnectSuccessCallback = ConnectSuccessCallback;
            webSocketClient.ConnectFailureCallback = ConnectFailureCallback;
            webSocketClient.ConnectErrorCallback = ConnectErrorCallback;
            webSocketClient.TryConnect();

            inputFieldPresenter.SetSender(user);
            inputFieldPresenter.InputMessageCallback = SendToMessage;
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
                inputFieldPresenter.ReceiveMessage(message);
            }, null);
        }

        public void ConnectSuccessCallback()
        {
            Debug.Log("WebSocket Connect");
            loading.gameObject.SetActive(false);
        }

        public void ConnectFailureCallback()
        {
            Debug.Log("WebSocket TryAgain");
            if (!webSocketClient.IsAbortTryConnect)
            {
                StartCoroutine(Retry());
            }
        }

        public void ConnectErrorCallback(ErrorEventArgs e)
        {

            Debug.Log("WebSocket Error Message: " + e.Message);
        }

        /// <summary>
        /// コルーチンで Retry を行う。
        /// 閾値を越えると入力を受け付けなくなる。
        /// </summary>
        /// <returns>The waiting.</returns>
        public IEnumerator Retry()
        {
            loading.gameObject.SetActive(true);
            if (webSocketClient.Retry > retryThreshold)
            {
                Debug.Log("WebSocket Disconnect");
                loading.gameObject.SetActive(false);
                inputField.transform.GetChild(0).GetComponent<Text>().text = "サーバに接続できませんでした";
                inputField.readOnly = true;
                webSocketClient.AbortTryConnect();
                StopCoroutine(Retry());
            }

            // アニメーションの1サイクルが2秒。
            yield return new WaitForSeconds(2);
            webSocketClient.TryConnect();
        }

        public void Destroy()
        {
            webSocketClient.Destroy();
        }
    }
}
