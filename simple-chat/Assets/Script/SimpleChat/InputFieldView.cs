using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleChat
{
    public class InputFieldView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform otherMessageView;
        [SerializeField]
        private RectTransform myMessageView;
        [SerializeField]
        private RectTransform chatLogContent;

        private Vector3 initialLocalScale = new Vector3(1, 1, 1);

        private RectTransform clonedMessageRectTransform;
        private InputField inputField;
        public InputField InputField { get { return inputField; }  }

        public void Awake()
        {
            inputField = GetComponent<InputField>();

            // delegate を使って inputField を渡さなければ日本語に対応するための textComponent を参照できないため
            // [x] inputField.onEndEdit.AddListener(OnEndEdit)
            //     void OnEndEdit(string message)
            // see: https://docs.unity3d.com/ja/2017.4/ScriptReference/UI.InputField-onEndEdit.html
            inputField.onEndEdit.AddListener(delegate { OnEndEdit(inputField); });
        }

        private void OnEndEdit(InputField input)
        {
            // 全角だろうが空白のみは許容しない
            if (input.textComponent.text.Trim().Equals(string.Empty)) {
                return;
            }

            // 日本語に対応するには textComponent から参照しなければならない
            AddMessageView(myMessageView, input.textComponent.text);
        }

        public void ReceiveMessage(string message)
        {
            AddMessageView(otherMessageView, message);
        }

        private void AddMessageView(RectTransform messageView, string message)
        {
            RectTransform clonedMessageView = Instantiate<RectTransform>(messageView);

            SetTextInContent(clonedMessageView, message);
            AdjustViewLayout(clonedMessageView);
            SetLayoutOrder(clonedMessageView);
            BeVisibleView(clonedMessageView);
            ResetInputField();
        }

        private void SetTextInContent(RectTransform clonedMessageView, string message)
        {
            // SerializeField を使ってキャッシュしたかったが、 clone しているので instanceId が異なるので使えない
            // 同様の理由で FindWithTag による参照もできないのでやむなし
            clonedMessageView.GetChild(1).GetComponent<Text>().text = message;
            clonedMessageView.GetChild(2).GetComponent<Text>().text = CurrentDate();
            clonedMessageView.SetParent(chatLogContent);

        }

        private void AdjustViewLayout(RectTransform clonedMessageView)
        {
            // SetParent 実行後、なぜか localScale が (0.5, 0.5, 0.5) に resize されてしまうので。
            clonedMessageView.localScale = initialLocalScale;
            float h = clonedMessageView.GetChild(1).GetComponent<Text>().preferredHeight;
            float w = clonedMessageView.sizeDelta.x;
            float padding = clonedMessageView.GetChild(1).GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            clonedMessageView.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h + padding);
        }

        private void SetLayoutOrder(RectTransform clonedMessageView)
        {
            // 最新のコメントが一番上にくるように順序づけ
            clonedMessageView.SetAsFirstSibling();
        }

        private void BeVisibleView(RectTransform clonedMessageView)
        {
            clonedMessageView.gameObject.SetActive(true);
        }

        private void ResetInputField()
        {
            inputField.text = string.Empty;
        }

        private string CurrentDate()
        {
            return DateTime.Now.ToString("HH:mm");
        }
    }
}