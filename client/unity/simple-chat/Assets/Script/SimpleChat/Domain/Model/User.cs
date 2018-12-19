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
            id = this.id;
            name = this.name;
            thumbnailUrl = this.thumbnailUrl;
        }
    }
}
