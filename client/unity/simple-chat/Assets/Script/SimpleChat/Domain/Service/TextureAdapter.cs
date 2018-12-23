using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleChat.Domain.Service
{
    public class TextureAdapter
    {
        public uint Identifier { get; private set; }

        private RawImage rawImage;
        private Texture texture;

        public TextureAdapter(uint identifier, RawImage rawImage)
        {
            Identifier = identifier;
            SetRawImage(rawImage);
        }

        private void SetRawImage(RawImage r)
        {
            this.rawImage = r;
        }

        public IEnumerator Fetch(string url)
        {
            WWW www = new WWW(url);
            yield return www;
            CacheTexture(www);
            Adapt();
        }

        private void CacheTexture(WWW www) {
            texture = www.textureNonReadable;
        }

        private void Adapt()
        {
            if (rawImage == null) {
                throw new NullReferenceException();
            }
            rawImage.texture = texture;
        }

        public void Adapt(RawImage rawImage)
        {
            SetRawImage(rawImage);
            Adapt();
        }
    }
}