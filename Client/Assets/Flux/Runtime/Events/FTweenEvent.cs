using UnityEngine;

namespace Flux
{
	public abstract class FTweenEvent<T> : FEvent where T : FTweenBase {

		[SerializeField]
		protected T _tween = default(T);

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			OnUpdateEvent( framesSinceTrigger, timeSinceTrigger );
		}

		protected override void OnUpdateEvent( int framesSinceTrigger, float timeSinceTrigger )
		{
			float t = timeSinceTrigger / LengthTime;

			ApplyProperty( t );
		}

		protected override void OnFinish()
		{
			ApplyProperty( 1f );
		}

		protected abstract void ApplyProperty( float t );
	}
}
