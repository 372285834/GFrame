using UnityEngine;
using UnityEditor;

using GP;

namespace GPEditor
{
	[FEditor(typeof(FPlaySequenceEvent))]
	public class FPlaySequenceEventEditor : FEventEditor {

		private GTimelineEditor _sequenceEditor = null;

		protected override void Init (FObject obj)
		{
			base.Init(obj);

			if( _sequenceEditor == null )
			{
				_sequenceEditor = GTimelineEditor.CreateInstance<GTimelineEditor>();
				_sequenceEditor.Init( (EditorWindow)null ); // doesn't have a window
				_sequenceEditor.OpenSequence( _evt.Owner.GetComponent<GTimeline>() );
			}
		}

	}
}
