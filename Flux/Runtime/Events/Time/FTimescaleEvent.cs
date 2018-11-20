using UnityEngine;


namespace Flux
{
	[FEvent("Time/Timescale")]
	public class FTimescaleEvent : FEvent {

		[SerializeField]
		private AnimationCurve _curve;
		public AnimationCurve Curve { get { return _curve; } set { _curve = value; } }

		protected override void SetDefaultValues ()
		{
			_curve = new AnimationCurve( new Keyframe[]{ new Keyframe(0, 1) } );
		}

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			
		}

		protected override void OnUpdateEvent( int framesSinceTrigger, float timeSinceTrigger )
		{
			Time.timeScale = Mathf.Clamp( _curve.Evaluate( timeSinceTrigger ), 0, 100); // unity "breaks" if it is outside this range
		}

		protected override void OnStop()
		{
			Time.timeScale = 1;
		}
	}
}