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

        private void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string message)
        {
            // early return when only white-space
            if (message.Trim().Equals(string.Empty)) {
                return;
            }

            AddMessageView(myMessageView, message);
        }

        private void AddMessageView(RectTransform messageView, string message)
        {
            var clonedMessageView = Instantiate<RectTransform>(messageView);
            clonedMessageRectTransform = clonedMessageView.GetComponent<RectTransform>();
            clonedMessageRectTransform.GetChild(1).GetComponent<Text>().text = message;
            // TODO: had better find component method
            clonedMessageRectTransform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = DateTime.Now.ToString("HH:mm");
            clonedMessageView.SetParent(chatLogContent);

            // when SetParent execution, RectTransform is to be scaled automatically (0.5, 0.5, 0.5)
            clonedMessageRectTransform.localScale = initialLocalScale;
            // sort order for new message
            clonedMessageView.SetAsFirstSibling();
            clonedMessageView.gameObject.SetActive(true);
            GetComponent<InputField>().text = "";
        }
    }
}