using UnityEngine;

namespace Flux
{
	[FEvent("Sequence/Stop Sequence")]
	public class FStopSequenceEvent : FEvent 
	{
		private FSequence _sequence = null;
		
		protected override void OnInit()
		{
			_sequence = Owner.GetComponent<FSequence>();
		}
		
		protected override void OnTrigger( int framesSinceTrigger, float timeSinceTrigger )
		{
			_sequence.Stop();
		}
	}
}
