using System;
using SimpleChat.Domain.Model;
using UnityEngine;

namespace SimpleChat.Domain.BusinessModel
{
    [Serializable]
    public class Message
    {
        [HideInInspector]
        public User user;
        [HideInInspector]
        public string value;

        public Message(User user, string value)
        {
            this.user = user;
            this.value = value;
        }

        public static Message CreateFromJson(string jsonString)
        {
            return JsonUtility.FromJson<Message>(jsonString);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}