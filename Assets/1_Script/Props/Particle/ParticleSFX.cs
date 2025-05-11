using UnityEngine;

namespace Garage.Props.Particle
{
	[RequireComponent(typeof(ParticleSystem))]
	[RequireComponent(typeof(AudioSource))]
	public class ParticleSFX : MonoBehaviour
	{

		private ParticleSystem particle;
		private AudioSource audioSource;

		[SerializeField] private AudioClip sfxClip;

		private void Awake()
		{
			particle = GetComponent<ParticleSystem>();
			audioSource = GetComponent<AudioSource>();

			audioSource.clip = sfxClip;
		}

		void Update()
		{
			if (!particle.isEmitting)
			{
				if (audioSource.isPlaying)
				{
					audioSource.Stop();
				}
			}
			else if (particle.isPlaying)
			{
				if (!audioSource.isPlaying)
				{
					audioSource.loop = true;
					audioSource.Play();
				}
			}

		}
	}
}