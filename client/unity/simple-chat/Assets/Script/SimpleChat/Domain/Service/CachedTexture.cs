using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SimpleChat.Domain.Service
{
    /// <summary>
    /// Fetch メソッドを使って指定 URL からテクスチャをキャッシュするクラス
    /// NOTE: 同時に ID を紐づけるため、キャッシュ済みのインスタンスを利用すれば
    ///       ネットワークへのアクセスを減らすことができる
    /// </summary>
    public class CachedTexture
    {
        public uint Identifier { get; private set; }

        private Texture texture;

        public CachedTexture(uint identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// [非同期] 指定の URL から画像をダウンロードしテクスチャとしてキャッシュする
        /// XXX: UnityWebRequest は Coroutine で動作するためサブスレッドとしてマルチスレッド処理することはできない
        ///      https://developers.cyberagent.co.jp/blog/archives/6649/
        /// </summary>
        /// <returns>The fetch.</returns>
        /// <param name="url">URL.</param>
        public IEnumerator Fetch(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                throw new UnityException(request.error);
            }
            else
            {
                Cache(request.downloadHandler);
            }
            yield return null;
        }

        /// <summary>
        /// downloadHandler クラスを通じてテクスチャをキャッシュする
        /// NOTE: バックグラウンドダウンロード完了済みのオブジェクトを利用すること
        /// </summary>
        /// <param name="downloadHandler">downloadHandler.</param>
        private void Cache(DownloadHandler downloadHandler) {
            texture = ((DownloadHandlerTexture)downloadHandler).texture;
        }

        /// <summary>
        /// テクスチャを貼り付けたい場合に利用する
        /// </summary>
        /// <param name="rawImage">Raw image.</param>
        public void AdaptTo(RawImage rawImage)
        {
            rawImage.texture = texture;
        }
    }
}