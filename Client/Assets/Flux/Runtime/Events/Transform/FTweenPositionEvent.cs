using UnityEngine;

namespace Flux
{
	[FEvent("Transform/Tween Position")]
	public class FTweenPositionEvent : FTransformEvent 
	{
		private Vector3 _startPosition;

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			_startPosition = Owner.localPosition;
			base.OnTrigger( framesSinceTrigger, timeSinceTrigger );
		}

		protected override void OnStop()
		{
			base.OnStop();
			Owner.localPosition = _startPosition;
		}

		protected override void SetDefaultValues()
		{
//			_tween = new FTweenVector3( Owner.localPosition, Owner.localPosition );
		}

		protected override void ApplyProperty( float t )
		{
			Owner.localPosition = _tween.GetValue( t );
		}
	}
}
