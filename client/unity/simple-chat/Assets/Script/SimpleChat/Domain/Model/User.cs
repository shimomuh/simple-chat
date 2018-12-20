using System;
using UnityEngine;

namespace SimpleChat.Domain.Model
{
    [Serializable]
    public class User
    {
        [HideInInspector]
        public uint id;
        [HideInInspector]
        public string name;
        [HideInInspector]
        public string thumbnailUrl;

        public User(uint id, string name, string thumbnailUrl)
        {
            this.id = id;
            this.name = name;
            this.thumbnailUrl = thumbnailUrl;
        }
    }
}
