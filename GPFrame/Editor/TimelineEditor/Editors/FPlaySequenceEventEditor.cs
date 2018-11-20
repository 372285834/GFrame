using UnityEngine;
using UnityEditor;

using Flux;

namespace GPEditor
{
	[FEditor(typeof(FPlaySequenceEvent))]
	public class FPlaySequenceEventEditor : FEventEditor {

		private FSequenceEditor _sequenceEditor = null;

		protected override void Init (FObject obj)
		{
			base.Init(obj);

			if( _sequenceEditor == null )
			{
				_sequenceEditor = FSequenceEditor.CreateInstance<FSequenceEditor>();
				_sequenceEditor.Init( (EditorWindow)null ); // doesn't have a window
				_sequenceEditor.OpenSequence( _evt.Owner.GetComponent<FSequence>() );
			}
		}

	}
}
