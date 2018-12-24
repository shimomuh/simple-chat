using System;
using System.Collections;
using UnityEngine;
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
        /// </summary>
        /// <returns>The fetch.</returns>
        /// <param name="url">URL.</param>
        public IEnumerator Fetch(string url)
        {
            WWW www = new WWW(url);
            yield return www;
            Cache(www);
            yield return null;
        }

        /// <summary>
        /// www クラスを通じてテクスチャをキャッシュする
        /// NOTE: バックグラウンドダウンロード完了済みのオブジェクトを利用すること
        /// </summary>
        /// <param name="www">Www.</param>
        private void Cache(WWW www) {
            texture = www.textureNonReadable;
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