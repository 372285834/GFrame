using UnityEngine;

namespace Flux
{
	[FEvent("Sequence/Play Sequence", typeof(FSequenceTrack))]
	public class FPlaySequenceEvent : FEvent 
	{
		private FSequence _sequence = null;

		[SerializeField]
		private int _startOffset = 0;
		public int StartOffset { get { return _startOffset; } }

		protected override void OnInit()
		{
			_sequence = Owner.GetComponent<FSequence>();
		}

		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			if( Sequence.IsPlaying && Application.isPlaying )
			{
				_sequence.Play( _startOffset + framesSinceTrigger );
			}
		}

		protected override void OnUpdateEventEditor( int framesSinceTrigger, float timeSinceTrigger )
		{
//			_sequence.SetCurrentFrameEditor( _startOffset + framesSinceTrigger );
//			_sequence.SetCurrentFrame( _startOffset + framesSinceTrigger );
		}

		protected override void OnStop()
		{
			_sequence.Stop( true );
		}

		protected override void OnFinish()
		{
			_sequence.Pause();
		}

		protected override void OnPause ()
		{
			_sequence.Pause();
		}

		protected override void OnResume()
		{
//			_sequence.Play(_sequence.GetCurrentFrame());
			_sequence.Resume();
		}
	}
}
