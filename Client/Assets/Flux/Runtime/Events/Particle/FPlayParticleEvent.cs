using UnityEngine;
using System.Collections;

namespace Flux
{
	[FEvent( "Particle System/Play Particle" )]
	public class FPlayParticleEvent : FEvent {

		[SerializeField]
		[Tooltip("True: ParticleSystem playback speed will be adjusted to match event length"
		         +"\nFalse: ParticleSystem plays at normal speed, i.e. doesn't scale based on event length")]
		private bool _normalizeToEventLength = false;

		private ParticleSystem _particleSystem = null;

		protected override void OnInit ()
		{
			_particleSystem = Owner.GetComponent<ParticleSystem>();
#if UNITY_EDITOR
			if( _particleSystem == null )
				Debug.LogError("FParticleEvent is attached to an object that doesn't have a ParticleSystem");
#endif
		}

		protected override void OnTrigger( int frameSinceTrigger, float timeSinceTrigger )
		{
			_particleSystem.playbackSpeed = _normalizeToEventLength ? _particleSystem.duration / LengthTime : 1;

			if( _particleSystem != null )
			{
				_particleSystem.Play( true );
			}
		}

		protected override void OnFinish()
		{
			if( _particleSystem != null )
				_particleSystem.Stop( true );

		}

		protected override void OnStop()
		{
			if( _particleSystem != null )
				_particleSystem.Clear( true );
		}

        protected override void OnUpdateEventEditor( int frameSinceTrigger, float timeSinceTrigger )
		{
//			float t = timeSinceTrigger / LengthTime;
			if( _particleSystem != null )
			{
				float t = timeSinceTrigger;
				if( _normalizeToEventLength )
					t *= _particleSystem.duration / LengthTime;

				_particleSystem.Simulate( t, true, true );
			}
		}

	}
}
