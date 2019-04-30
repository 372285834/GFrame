using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	public class FSequenceTrack : FTrack
	{
		public override void Stop()
		{
			base.Stop();
		}

		public override bool CanTogglePreview()
		{
			return true;
		}

		public override void CreatePreview()
		{
			if( HasPreview )
				return;

			FSequence sequence = Owner.GetComponent<FSequence>();

			List<FTimeline> timelines = sequence.GetTimelines();

			for( int i = 0; i != timelines.Count; ++i )
			{
				List<FTrack> tracks = timelines[i].GetTracks();

				for( int j = 0; j != tracks.Count; ++j )
				{
					if( tracks[j].CanTogglePreview() )
						tracks[j].IsPreviewing = true;
				}
			}

			HasPreview = true;
		}

		public override void ClearPreview()
		{
			if( !HasPreview )
				return;
			
			FSequence sequence = Owner.GetComponent<FSequence>();
			
			List<FTimeline> timelines = sequence.GetTimelines();
			
			for( int i = 0; i != timelines.Count; ++i )
			{
				List<FTrack> tracks = timelines[i].GetTracks();
				
				for( int j = 0; j != tracks.Count; ++j )
				{
					tracks[j].IsPreviewing = false;
				}
			}
			
			HasPreview = false;
		}
	}
}
