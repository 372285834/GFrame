using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using GP;
using GPEditor;

namespace GPEditor
{
	[CustomEditor(typeof(GTimelineEditor))]
	public class FSequenceInspector : Editor {

		private GTimelineEditor _sequence;

		private bool _advancedInspector = false;

		private const string CHANGE_FRAME_RATE_TITLE = "Change Frame Rate?";
		private const string CHANGE_FRAME_RATE_MSG = "Changing Frame Rate may cause the Sequence to " +
			"drop certain events that are Frame Rate dependent " +
			"(e.g. Animations). Are you sure you want to change Frame Rate from {0} to {1}?";
		private const string CHANGE_FRAME_RATE_OK = "Change";
		private const string CHANGE_FRAME_RATE_CANCEL = "Cancel";

		private SerializedProperty _timelineContainer = null;

		void OnEnable()
		{
			_sequence = (GTimelineEditor)target;

			_timelineContainer = serializedObject.FindProperty("_timelineContainer");
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			Rect r = GUILayoutUtility.GetRect( EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight );

			r.width -= EditorGUIUtility.labelWidth;

			EditorGUI.PrefixLabel( r, new GUIContent( "Frame Rate" ) );

			r.width += EditorGUIUtility.labelWidth;

			EditorGUI.BeginChangeCheck();
			r.xMin += EditorGUIUtility.labelWidth;

			int frameRate = FGUI.FrameRatePopup( r, _sequence.FrameRate );

			if( EditorGUI.EndChangeCheck() )
			{
				if( frameRate == -1 )
				{
					FChangeFrameRateWindow.Show( new Vector2( r.xMin-EditorGUIUtility.labelWidth, r.yMax ), _sequence, FSequenceInspector.Rescale );
					EditorGUIUtility.ExitGUI();
				}
				else
					Rescale( _sequence, frameRate, true );
			}

			EditorGUILayout.Space();

			if( GUILayout.Button( "Open In Flux Editor" ) )
			{
				FSequenceEditorWindow.Open( _sequence );
			}

			EditorGUILayout.Space();

			if( GUILayout.Button( _advancedInspector ? "Normal Inspector" : "Advanced Inspector") )
				_advancedInspector = !_advancedInspector;

			if( _advancedInspector )
			{
				EditorGUILayout.PropertyField( _timelineContainer );

				EditorGUI.BeginChangeCheck();
				bool showTimelines = EditorGUILayout.Toggle( "Show Timelines", (_timelineContainer.objectReferenceValue.hideFlags & HideFlags.HideInHierarchy) == 0 );
				if( EditorGUI.EndChangeCheck() )
				{
					if( showTimelines )
						_timelineContainer.objectReferenceValue.hideFlags &= ~HideFlags.HideInHierarchy;
					else
						_timelineContainer.objectReferenceValue.hideFlags |= HideFlags.HideInHierarchy;
				}
			}

//			if( GUILayout.Button("Play") )
//				_sequence.Play();
		}

		public static void Rescale( GTimelineEditor sequence, int frameRate, bool confirm )
		{
			if( sequence.FrameRate == frameRate )
				return;

			if( !confirm || sequence.IsEmpty() || EditorUtility.DisplayDialog( CHANGE_FRAME_RATE_TITLE, string.Format(CHANGE_FRAME_RATE_MSG, sequence.FrameRate, frameRate), CHANGE_FRAME_RATE_OK, CHANGE_FRAME_RATE_CANCEL ) )
			{
				Rescale( sequence, frameRate );
			}
		}

		public static void Rescale( GTimelineEditor sequence, int frameRate )
		{
			if( sequence.FrameRate == frameRate )
				return;

			float scaleFactor = (float)frameRate / sequence.FrameRate;

			Undo.RecordObject( sequence, "Change Frame Rate" );

			sequence.Length = Mathf.RoundToInt( sequence.Length * scaleFactor );
			sequence.FrameRate = frameRate;

			foreach( FTimeline timeline in sequence.GetTimelines() )
			{
				Rescale( timeline, scaleFactor );
			}

			EditorUtility.SetDirty( sequence );
		}

		private static void Rescale( FTimeline timeline, float scaleFactor )
		{
			List<FTrack> tracks = timeline.GetTracks();
			foreach( FTrack track in tracks )
				Rescale( track, scaleFactor );
		}

		private static void Rescale( FTrack track, float scaleFactor )
		{
			List<FEvent> events = track.GetEvents();
			foreach( FEvent evt in events )
				Rescale( evt, scaleFactor );
		}

		private static void Rescale( FEvent evt, float scaleFactor )
		{
			FrameRange newFrameRange = evt.FrameRange;
	        newFrameRange.Start = Mathf.RoundToInt( newFrameRange.Start * scaleFactor );
	        newFrameRange.End = Mathf.RoundToInt( newFrameRange.End * scaleFactor );

	        FUtility.Rescale( evt, newFrameRange );
		}

		public static bool IsMultipleOf( int a, int b )
		{
			return (b / a >= 1) && (b % a == 0);
		}
	}
}
