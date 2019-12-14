#pragma warning disable 0649
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableAudio
{
	[CreateAssetMenu(menuName="Audio Events/Simple")]
	public class SimpleAudioEvent : AudioEvent
	{
		[SerializeField] private AudioClip[] clips;

		[SerializeField] private RangedFloat volume;
		[SerializeField, MinMaxRange(0f, 2f)] private RangedFloat pitch;

		public override void Play(AudioSource source)
		{
			if (clips.Length == 0)
				return;
			
			source.clip = clips[Random.Range(0, clips.Length)];
			source.volume = Random.Range(volume.minValue, volume.maxValue);
			source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
			source.Play();
		}
	}
}
