using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleChat.UI.View
{
    public class AutoInvisibleBarScrollRect : ScrollRect
    {
        private Image handle;
        private PointerEventData scrollEventData;

        protected override void Awake()
        {
            base.Awake();
            handle = GetComponent<RectTransform>().GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();
        }

        // Update メソッドを使わずにコルーチンや非同期メソッド、UniRx などを用いて
        // onValueChanged.AddListener(delegate { Func(); }); などで動作できるのが理想
        private void Update()
        {
            if (scrollEventData != null) {
                if (!scrollEventData.IsScrolling())
                {
                    FadeOutHandle();
                    scrollEventData = null;
                }
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            handle.gameObject.SetActive(true);
            FadeInHandle();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            FadeOutHandle();
        }

        public override void OnScroll(PointerEventData data)
        {
            base.OnScroll(data);
            scrollEventData = data;
            handle.gameObject.SetActive(true);
            FadeInHandle();
        }

        /// <summary>
        /// 0.25 秒でスクロールバーの色のアルファ値を 1 にしてフェードインする
        /// </summary>
        private void FadeInHandle()
        {
            handle.CrossFadeAlpha(1f, 0.25f, true);
        }

        /// <summary>
        /// 0.25 秒でスクロールバーの色のアルファ値を 0 にしてフェードアウトする
        /// </summary>
        private void FadeOutHandle()
        {
            handle.CrossFadeAlpha(0.0f, 0.25f, true);
        }
    }
}