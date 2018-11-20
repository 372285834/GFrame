using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace GPEditor
{
	public class FInspectorWindow : EditorWindow {

		public static FInspectorWindow _instance = null;

		[MenuItem(FSequenceEditorWindow.MENU_PATH+FSequenceEditorWindow.PRODUCT_NAME+"/Open Inspector", false, 1)]
		public static void Open()
		{
			_instance = GetWindow<FInspectorWindow>();

			_instance.Show();
            _instance.titleContent = new GUIContent("Flux Inspector");
		}

		private Vector2 _scroll = Vector2.zero;
		
		private List<FEvent> _events = new List<FEvent>();
		
		private List<FTrack> _tracks = new List<FTrack>();
		
		[SerializeField]
		private Editor _eventInspector;
		
		[SerializeField]
		private Editor _trackInspector;
		
		private Rect _viewRect;
		
		void OnEnable()
		{
			_instance = this;

			hideFlags = HideFlags.DontSave;
			
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;

			
		}
		
		void OnDestroy()
		{
			EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;

		}
		
		private void OnPlaymodeChanged()
		{
//			Debug.Log( "Compiling: " + EditorApplication.isCompiling + "Updating: " + EditorApplication.isUpdating );
			List<FEvent> newEvents = new List<FEvent>();
			foreach( FEvent evt in _events )
			{
				if( evt != null )
				{
					newEvents.Add( evt );
				}
				else
				{
					if( !object.Equals( evt, null ) )
					{
						newEvents.Add( (FEvent)EditorUtility.InstanceIDToObject( evt.GetInstanceID() ) );
					}
				}
			}
			
			_events.Clear();
			_events.AddRange( newEvents );

			CreateEventInspector();

			List<FTrack> newTracks = new List<FTrack>();
			foreach( FTrack track in _tracks )
			{
				if( track != null )
				{
					newTracks.Add( track );
				}
				else
				{
					if( !object.Equals( track, null ) )
					{
						newTracks.Add( (FTrack)EditorUtility.InstanceIDToObject( track.GetInstanceID() ) );
					}
				}
			}

			_tracks.Clear();
			_tracks.AddRange( newTracks );

			CreateTrackInspector();
		}

		private void CreateEventInspector()
		{
			if( _eventInspector != null )
				DestroyImmediate( _eventInspector );

			if( _events.Count == 1 )
			   _eventInspector = Editor.CreateEditor( _events[0] );
		   	else
			   _eventInspector = Editor.CreateEditor( _events.ToArray(), typeof(FEventInspector) );
		}

		private void CreateTrackInspector()
		{
			if( _trackInspector != null )
				DestroyImmediate( _trackInspector );

			if( _tracks.Count == 1 )
				_trackInspector = Editor.CreateEditor( _tracks[0] );
			else
				_trackInspector = Editor.CreateEditor( _tracks.ToArray(), typeof(FTrackInspector) );

			if( _trackInspector != null )
				((FTrackInspector)_trackInspector).ShowEvents = false;
		}
        public static void SetEvents(List<FEventEditor> eventList)
        {
            if (_instance == null)
                Open();
            _instance.setEvents(eventList);
        }
		void setEvents( List<FEventEditor> eventList )
		{
			_events.Clear();
			
			if( eventList == null )
			{
				DestroyImmediate( _eventInspector );
				_eventInspector = null;
				return;
			}
			
			for( int i = 0; i != eventList.Count; ++i )
			{
				_events.Add( (FEvent)eventList[i].GetRuntimeObject() );
			}
			
			_eventInspector = Editor.CreateEditor( _events.ToArray() );
		}
		public static void SetTracks( List<FTrackEditor> trackList )
        {
            if (_instance == null)
                Open();
            _instance.setTracks(trackList);
        }
		private void setTracks( List<FTrackEditor> trackList )
		{
			_tracks.Clear();
			
			if( trackList == null )
			{
				DestroyImmediate( _trackInspector );
				_trackInspector = null;
				return;
			}
			
			for( int i = 0; i != trackList.Count; ++i )
			{
				_tracks.Add( (FTrack)trackList[i].GetRuntimeObject() );
			}
			
			CreateTrackInspector();
		}
		
		public void Render( Rect rect )
		{
			
			if( Event.current.type == EventType.Repaint )
			{
				bool abortGUI = false;

				if( _eventInspector != null && _eventInspector.target == null )
				{
					abortGUI = true;
					SetEvents( null );
				}

				if( _trackInspector != null && _trackInspector.target == null )
				{
					abortGUI = true;
					SetTracks( null );
				}

				if( abortGUI )
				{
					EditorGUIUtility.ExitGUI();
					return;
				}
			}

			float contentWidth = rect.width;
			
			if( rect.height < _viewRect.height ) 
			{
				contentWidth -= 20;
				//				_viewRect.xMax -= 20;
			}
			
			_scroll = GUI.BeginScrollView( rect, _scroll, _viewRect );
			
			GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			
			if( _eventInspector != null )
			{
				EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(contentWidth));
				EditorGUILayout.LabelField("Events:", EditorStyles.boldLabel);
				if( _eventInspector.target != null )
					_eventInspector.OnInspectorGUI();
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			
			if( _trackInspector != null )
			{
				EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(contentWidth));
				EditorGUILayout.LabelField("Tracks:", EditorStyles.boldLabel);
				if( _trackInspector.target != null )
					_trackInspector.OnInspectorGUI();
				EditorGUILayout.EndVertical();
			}
			
			if( Event.current.type == EventType.Repaint && (_eventInspector != null || _trackInspector != null) )
			{
				Rect lastElementRect = GUILayoutUtility.GetLastRect();
				
				_viewRect = rect;
				
				_viewRect.height = Mathf.Max( _viewRect.height, lastElementRect.y + lastElementRect.height );
			}
			
			GUI.EndScrollView();
		}

		void OnGUI()
		{
			Rect rect = position;
			rect.x = 0; rect.y = 0;

            Render( rect );
		}

		void Update()
		{
			Repaint();
//			if( inspector.selectedElements != null )
//			{
//				inspector.fcEvent.SetDirty( false );
//				Repaint();
//			}
		}

//		public void UpdateInspector( List<ISelectableElement> elements )
//		{
//			inspector.SetElements( elements );
//			Repaint();
//		}
	}
}