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

        private Vector3 renderPosition = new Vector3(0, 0, 0);
        private Vector3 initialLocalScale = new Vector3(1, 1, 1);

        private RectTransform clonedMessageRectTransform;
        private InputField inputField;

        private void Start()
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

        private void AddMessageView(RectTransform messageView, string message)
        {
            var clonedMessageView = Instantiate<RectTransform>(messageView);
            clonedMessageRectTransform = clonedMessageView.GetComponent<RectTransform>();
            // TODO: 絶対もっとうまい書き方がある
            clonedMessageRectTransform.GetChild(1).GetComponent<Text>().text = message;
            clonedMessageRectTransform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = DateTime.Now.ToString("HH:mm");
            clonedMessageView.SetParent(chatLogContent);

            // SetParent 実行後、なぜか localScale が (0.5, 0.5, 0.5) に resize されてしまうので。
            clonedMessageRectTransform.localScale = initialLocalScale;
            // 最新のコメントが一番上にくるように順序づけ
            clonedMessageView.SetAsFirstSibling();
            clonedMessageView.gameObject.SetActive(true);
            ResetInputField();
        }

        private void ResetInputField()
        {
            inputField.text = "";
        }
    }
}