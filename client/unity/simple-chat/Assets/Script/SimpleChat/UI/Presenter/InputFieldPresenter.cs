using System;
using System.Collections;
using SimpleChat.Domain.Service;
using SimpleChat.Domain.Model;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleChat.UI.Presenter
{
    /// <summary>
    /// InputField に付与されるクラス。
    /// 入力を受けて ScrollView にメッセージを出力する。
    /// 外部からの入力も受け付けて ScrollView にメッセージとして表示できる。
    /// NOTE: Presenter に限り UnityEngine.UI のクラスと区別するため
    ///       意図的に Presenter の接尾詞をつける。
    /// </summary>
    public class InputFieldPresenter : MonoBehaviour
    {
        [SerializeField]
        private RectTransform otherMessageView;
        [SerializeField]
        private RectTransform myMessageView;
        [SerializeField]
        private RectTransform chatLogContent;
        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private uint MaxByteInOneLine;

        private RectTransform clonedMessageRectTransform;
        private InputField inputField;
        private Text messageText;
        private User user;
        private List<CachedTexture> cachedTextures = new List<CachedTexture>();

        public Action<string> InputMessageCallback = null;

        #region LifeCycleMethods

        public void Awake()
        {
            inputField = GetComponent<InputField>();
        }

        public void Update()
        {
            // LineType: Multi Line NewLine を採用したため以下のような delegate だと
            // onEndEdit がフックされないのでやむなく Update メソッドでフックする
            //
            // [x] void Awake() {
            //       i = GetComponent<InputField>()
            //       i.onEndEdit.AddListener(delegate { OnEndEdit(i.text); });
            //     }
            //
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnEndEdit(inputField.text);
            }
        }

        #endregion

        /// <summary>
        /// 入力が完了する(Enter)と、発火する
        /// </summary>
        /// <param name="message">入力された文字</param>
        private void OnEndEdit(string message)
        {
            // 全角だろうが空白のみは許容しない
            if (message.Trim().Equals(string.Empty))
            {
                return;
            }

            // 末尾に改行コードが入る場合は削除する
            message = Regex.Replace(message, "\n$", "");


            AddMessageView(myMessageView, new Message(user, message));

            if (InputMessageCallback != null)
            {
                InputMessageCallback(message);
            }
        }

        /// <summary>
        /// 送信者を設定する
        /// </summary>
        /// <param name="user">User.</param>
        public void SetSender(User user)
        {
            if (this.user != null) { return; }
            this.user = user;
        }

        /// <summary>
        /// メッセージを SrollView に追加する。
        /// 主に、外部からの入力を受け付けるために利用する。
        /// </summary>
        /// <param name="message">Message.</param>
        public void ReceiveMessage(Message message)
        {
            AddMessageView(otherMessageView, message);
        }

        /// <summary>
        /// メッセージを ScrollView に追加する
        /// </summary>
        /// <param name="messageView">Message view.</param>
        /// <param name="message">Message.</param>
        private void AddMessageView(RectTransform messageView, Message message)
        {
            RectTransform clonedMessageView = Instantiate<RectTransform>(messageView);

            // SerializeField を使ってキャッシュしたかったが、 clone しているので instanceId が異なるので使えない
            // 同様の理由で FindWithTag による参照もできないのでやむなし
            SetTextInContent(clonedMessageView, message);
            AdjustViewLayout(clonedMessageView);
            SetLayoutOrder(clonedMessageView);
            BeVisibleView(clonedMessageView);
            ResetInputField();
            ScrollBottom();
        }

        /// <summary>
        /// 1フレーム後にスクロール位置を一番下に指定する
        /// NOTE: View を追加したタイミングだと ScrollBar の正規化した位置が
        ///       正しく反映されていないため 1 フレームずらしている
        /// </summary>
        private void ScrollBottom()
        {
            StartCoroutine(DelayMethod(1, () =>
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }));
        }

        /// <summary>
        /// 渡された処理を指定時間後に実行する
        /// </summary>
        /// <param name="delayFrameCount"></param>
        /// <param name="action">実行したい処理</param>
        /// <returns></returns>
        private IEnumerator DelayMethod(int delayFrameCount, Action action)
        {
            for (var i = 0; i < delayFrameCount; i++)
            {
                yield return null;
            }
            action();
        }

        /// <summary>
        /// message として受け取った文字列を Text として gameObject にセットする
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        /// <param name="message">Message.</param>
        private void SetTextInContent(RectTransform clonedMessageView, Message message)
        {
            messageText = clonedMessageView.GetChild(2).GetComponent<Text>();
            messageText.text = AutoInsertNewLine(message.value);
            clonedMessageView.GetChild(1).GetComponent<Text>().text = message.user.name;

            StartCoroutine(SetTexture(message.user.id, message.user.thumbnailUrl, clonedMessageView.GetChild(0).GetComponent<RawImage>()));

            clonedMessageView.GetChild(2).GetChild(1).GetComponent<Text>().text = CurrentTime();
            // 第二引数を true にすると、 scale が resize されてしまうので false に。
            // see: https://docs.unity3d.com/ScriptReference/Transform.SetParent.html
            clonedMessageView.SetParent(chatLogContent, false);

        }

        /// <summary>
        /// 引数に渡した RawImage に url から fetch した画像を Texture として貼り付ける。
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="rawImage">Raw image.</param>
        private IEnumerator SetTexture(uint id, string url, RawImage rawImage)
        {
            var cachedTexture = cachedTextures.Find(i => i.Identifier == id);
            if (cachedTexture == null)
            {
                cachedTexture = new CachedTexture(id);
                // Fetch メソッドは内部で非同期処理を行うためその同期処理を待つ意味で IEnumerator なメソッドとして実装した。
                yield return StartCoroutine(cachedTexture.Fetch(url));
                cachedTexture.AdaptTo(rawImage);
                cachedTextures.Add(cachedTexture);
            } else {
                cachedTexture.AdaptTo(rawImage);
            }
        }

        /// <summary>
        /// レイアウトを整える
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        private void AdjustViewLayout(RectTransform clonedMessageView)
        {
            // 名前
            float h1 = clonedMessageView.GetChild(1).GetComponent<Text>().preferredHeight / 2;
            // テキスト（縦）
            float h2 = clonedMessageView.GetChild(2).GetComponent<Text>().preferredHeight;
            // テキスト（横）
            float w = clonedMessageView.sizeDelta.x;
            // 吹き出しのパディング
            float padding = clonedMessageView.GetChild(2).GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            float margin = Math.Abs(clonedMessageView.GetChild(0).GetComponent<RectTransform>().localPosition.x);

            clonedMessageView.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h1 + h2 + padding + margin * 2);
        }

        /// <summary>
        /// 新しいメッセージの挿入位置を決める
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        private void SetLayoutOrder(RectTransform clonedMessageView)
        {
            // 最新のコメントが一番下にくるように順序づけ
            clonedMessageView.SetAsLastSibling();
        }

        /// <summary>
        /// gameObject が非表示なら可視化する
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        private void BeVisibleView(RectTransform clonedMessageView)
        {
            clonedMessageView.gameObject.SetActive(true);
        }

        /// <summary>
        /// InputField を空の状態に戻す
        /// </summary>
        private void ResetInputField()
        {
            inputField.text = string.Empty;
        }

        /// <summary>
        /// 現在時刻を[HH:mm]のフォーマットで返す 
        /// </summary>
        /// <returns>The date.</returns>
        private string CurrentTime()
        {
            return DateTime.Now.ToString("HH:mm");
        }

        /// <summary>
        /// 自動で改行を施す
        /// </summary>
        /// <returns>The insert new line.</returns>
        /// <param name="message">Message.</param>
        private string AutoInsertNewLine(string message)
        {
            string result = "";
            char[] words = message.ToCharArray();
            int currentLineByte = 0;

            if(message.Contains("\n")) {
                messageText.alignment = TextAnchor.UpperLeft;
            }
            for (int i = 0; i < words.Length; i++)
            {
                int wordByte = System.Text.Encoding.GetEncoding("euc-jp").GetBytes(words[i].ToString()).Length;
                currentLineByte += wordByte;
                if (currentLineByte > MaxByteInOneLine)
                {
                    result += "\n";
                    currentLineByte = 0;
                    messageText.alignment = TextAnchor.UpperLeft;
                }
                result += words[i].ToString();
            }

            return result;
        }
    }
}