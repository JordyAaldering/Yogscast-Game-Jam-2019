using UnityEngine;

namespace ScriptableAudio
{
	public abstract class AudioEvent : ScriptableObject
	{
		public abstract void Play(AudioSource source);
	}
}
