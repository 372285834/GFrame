using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Flux;

namespace GPEditor
{
	[FEditor(typeof(FSequenceTrack))]
	public class FSequenceTrackEditor : FTrackEditor
	{

		private FSequenceEditor _sequenceEditor = null;

		protected override void Init(FObject obj)
		{
			base.Init(obj);

			if( _sequenceEditor == null )
			{
				_sequenceEditor = FSequenceEditor.CreateInstance<FSequenceEditor>();
				_sequenceEditor.Init( (Editor)null/*SequenceEditor*/ );
				_sequenceEditor.OpenSequence( _track.Owner.GetComponent<FSequence>() );
			}
		}

		public override void UpdateEventsEditor( int frame, float time )
		{
			base.UpdateEventsEditor( frame, time );

			FEvent[] evts = new FEvent[2];

			int numEvents = _track.GetEventsAt( frame, ref evts );

			if( numEvents > 0 )
			{
				int startOffset = ((FPlaySequenceEvent)evts[0]).StartOffset;
				_sequenceEditor.SetCurrentFrame( startOffset + frame - evts[0].Start ); /// @TODO handle offset

				if( numEvents > 1 )
				{
					startOffset = ((FPlaySequenceEvent)evts[1]).StartOffset;
					_sequenceEditor.SetCurrentFrame( startOffset + frame - evts[1].Start );
				}
			}
		}
	}
}
