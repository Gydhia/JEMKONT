using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class AudioFeedbackData : EventData<AudioFeedbackData>
    {
        public AK.Wwise.Event AudioRef;
        public GameObject SoundHolder;

        public AudioFeedbackData(AK.Wwise.Event sound, GameObject holder)
        : base()
        {
            this.AudioRef = sound;
            this.SoundHolder = holder;
        }
    }
}