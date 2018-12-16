using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleChat.UI.View
{
    /// <summary>
    /// InputField に付与されるクラス。
    /// 入力を受けて ScrollView にメッセージを出力する。
    /// 外部からの入力も受け付けて ScrollView にメッセージとして表示できる。
    /// </summary>
    public class InputFieldView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform otherMessageView;
        [SerializeField]
        private RectTransform myMessageView;
        [SerializeField]
        private RectTransform chatLogContent;

        private RectTransform clonedMessageRectTransform;
        private InputField inputField;

        public Action<string> InputMessageCallback = null;

        #region LifeCycleMethods

        public void Awake()
        {
            inputField = GetComponent<InputField>();

            // delegate を使って inputField を渡さなければ日本語に対応するための textComponent を参照できないため
            // [x] inputField.onEndEdit.AddListener(OnEndEdit)
            //     void OnEndEdit(string message)
            // see: https://docs.unity3d.com/ja/2017.4/ScriptReference/UI.InputField-onEndEdit.html
            inputField.onEndEdit.AddListener(delegate { OnEndEdit(inputField); });
        }

        #endregion

        /// <summary>
        /// 入力が完了する(Enter)と、発火する
        /// </summary>
        /// <param name="input">Input.</param>
        private void OnEndEdit(InputField input)
        {
            // 日本語に対応するには textComponent から参照しなければならない
            string message = input.textComponent.text
                                  ;
            // 全角だろうが空白のみは許容しない
            if (message.Trim().Equals(string.Empty)) {
                return;
            }

            AddMessageView(myMessageView, message);
            if (InputMessageCallback != null)
            {
                InputMessageCallback(message);
            }
        }

        /// <summary>
        /// メッセージを SrollView に追加する。
        /// 主に、外部からの入力を受け付けるために利用する。
        /// </summary>
        /// <param name="message">Message.</param>
        public void ReceiveMessage(string message)
        {
            AddMessageView(otherMessageView, message);
        }

        /// <summary>
        /// メッセージを ScrollView に追加する
        /// </summary>
        /// <param name="messageView">Message view.</param>
        /// <param name="message">Message.</param>
        private void AddMessageView(RectTransform messageView, string message)
        {
            RectTransform clonedMessageView = Instantiate<RectTransform>(messageView);

            SetTextInContent(clonedMessageView, message);
            AdjustViewLayout(clonedMessageView);
            SetLayoutOrder(clonedMessageView);
            BeVisibleView(clonedMessageView);
            ResetInputField();
        }

        /// <summary>
        /// message として受け取った文字列を Text として gameObject にセットする
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        /// <param name="message">Message.</param>
        private void SetTextInContent(RectTransform clonedMessageView, string message)
        {
            // SerializeField を使ってキャッシュしたかったが、 clone しているので instanceId が異なるので使えない
            // 同様の理由で FindWithTag による参照もできないのでやむなし
            clonedMessageView.GetChild(2).GetComponent<Text>().text = message;
            clonedMessageView.GetChild(2).GetChild(1).GetComponent<Text>().text = CurrentTime();
            // 第二引数を true にすると、 scale が resize されてしまうので false に。
            // see: https://docs.unity3d.com/ScriptReference/Transform.SetParent.html
            clonedMessageView.SetParent(chatLogContent, false);

        }

        /// <summary>
        /// レイアウトを整える
        /// </summary>
        /// <param name="clonedMessageView">Cloned message view.</param>
        private void AdjustViewLayout(RectTransform clonedMessageView)
        {
            float h = clonedMessageView.GetChild(2).GetComponent<Text>().preferredHeight;
            float w = clonedMessageView.sizeDelta.x;
            float padding = clonedMessageView.GetChild(2).GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            clonedMessageView.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h + padding);
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
    }
}