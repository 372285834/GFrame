using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using GP;

namespace GPEditor
{
	[FEditor(typeof(FAnimationTrack))]
	public class FAnimationTrackEditor : FTrackEditor {

        protected override void OnEnable()
        {
            base.OnEnable();

            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        protected override void Init( FObject obj )
        {
            base.Init( obj );

			FAnimationTrack animTrack = (FAnimationTrack)obj;

			if( animTrack.Owner.GetComponent<Animator>() == null )
			{
				Animator animator = animTrack.Owner.gameObject.AddComponent<Animator>();
				Undo.RegisterCreatedObjectUndo( animator, string.Empty );
			}
        }

		public override void OnTrackChanged()
		{
//			Debug.LogWarning("FAnimationTrackEditor.OnTrackChanged");

			bool isPreviewing = _track.IsPreviewing;
			if( isPreviewing )
				_track.IsPreviewing = false;

			FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)_track );

			if( isPreviewing )
				_track.IsPreviewing = true;
		}

		public override void Render( int id, Rect rect, int headerWidth, FrameRange viewRange, float pixelsPerFrame )
		{
			base.Render( id, rect, headerWidth, viewRange, pixelsPerFrame );

			switch( Event.current.type )
			{
			case EventType.DragUpdated:
				if( rect.Contains(Event.current.mousePosition ) )
				{
					int numAnimationsDragged = FAnimationEventInspector.NumAnimationsDragAndDrop( _track.Sequence.FrameRate );
					int frame = SequenceEditor.GetFrameForX( Event.current.mousePosition.x );

					DragAndDrop.visualMode = numAnimationsDragged > 0 && _track.CanAddAt(frame) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
					Event.current.Use();
				}
				break;
			case EventType.DragPerform:
				if( rect.Contains(Event.current.mousePosition ) )
				{
					AnimationClip animClip = FAnimationEventInspector.GetAnimationClipDragAndDrop( _track.Sequence.FrameRate );

					if( animClip && Mathf.Approximately(animClip.frameRate, _track.Sequence.FrameRate) )
					{
						int frame = SequenceEditor.GetFrameForX( Event.current.mousePosition.x );
                        int maxLength;

						if( _track.CanAddAt( frame, out maxLength ) )
						{
							FPlayAnimationEvent animEvt = FEvent.Create<FPlayAnimationEvent>( new FrameRange( frame, frame + Mathf.Min(maxLength, Mathf.RoundToInt(animClip.length*animClip.frameRate))  ) );
                            _track.Add( animEvt );
                            FAnimationEventInspector.SetAnimationClip( animEvt, animClip );
							DragAndDrop.AcceptDrag();
						}
					}

					Event.current.Use();
				}
				break;
			}
		}

        void OnSceneGUI( SceneView sceneView )
		{
			if( _track == null )
				return;

			for( int i = 0; i != _eventEditors.Count; ++i )
			{
				FAnimationEventEditor animEvtEditor = (FAnimationEventEditor)_eventEditors[i];
				FPlayAnimationEvent animEvt = (FPlayAnimationEvent)_eventEditors[i]._evt;
				if( animEvt._animationClip != null && animEvt.IsAnimationEditable() && _track.IsPreviewing )
				{
					animEvtEditor.RenderTransformCurves( animEvt.Sequence.FrameRate );
				}
			}
		}

		private void PreviewAnimationEvent( FAnimationEventEditor animEvtEditor, int frame )
		{
			FPlayAnimationEvent animEvt = (FPlayAnimationEvent)animEvtEditor._evt;

			if( animEvt._animationClip == null )
				return;

			bool isEditable = animEvt.IsAnimationEditable();

			// render path
			if( isEditable )
			{
				TransformCurves transformCurves = new TransformCurves(animEvt.Owner, animEvt._animationClip);

				RenderTransformPath( transformCurves, animEvt.LengthTime, 1f/animEvt.Sequence.FrameRate );

				float t = (float)(frame + animEvt._startOffset - animEvt.Start) / animEvt.Sequence.FrameRate;

				if( animEvt.FrameRange.Contains( frame ) )
				{
//					float t = (float)(frame + animEvt._startOffset - animEvt.Start) / animEvt.Sequence.FrameRate;
					RenderTransformAnimation( transformCurves, t );
				}

//				AnimationClipCurveData[] allCurves = AnimationUtility.GetAllCurves( animEvt._animationClip, true );
//				foreach( AnimationClipCurveData curve in allCurves )
//				{
//
//				}
			}
			else if( animEvt.FrameRange.Contains( frame ) )
			{
				float t = (float)(frame + animEvt._startOffset - animEvt.Start) / animEvt.Sequence.FrameRate;

				bool wasInAnimationMode = AnimationMode.InAnimationMode();

				if( !AnimationMode.InAnimationMode() )
				{
					AnimationMode.StartAnimationMode();
				}
				AnimationMode.BeginSampling();
				AnimationMode.SampleAnimationClip( animEvt.Owner.gameObject, animEvt._animationClip, t );
				AnimationMode.EndSampling();

				if( !wasInAnimationMode )
					AnimationMode.StopAnimationMode();
			}
		}


		private void RenderTransformPath( TransformCurves transformCurves, float length, float samplingDelta )
		{
			float t = 0; 

			int numberSamples = Mathf.RoundToInt(length/samplingDelta)+1;

			float delta = length / numberSamples;

			Vector3[] pts = new Vector3[numberSamples];

			int index = 0;

			while( index < numberSamples )
			{
				pts[index++] = transformCurves.GetPosition( t );
				t += delta;
			}

			if( index != pts.Length )
				Debug.LogError("Number of samples doesn't match: " + (index+1) + " instead of " + pts.Length);

			Handles.DrawPolyLine( pts );
		}

		private void RenderTransformAnimation( TransformCurves transformCurves, float time )
        {
	        Vector3 pos = transformCurves.GetPosition(time);//new Vector3( xPos.Evaluate(t), yPos.Evaluate(t), zPos.Evaluate(t) );
	        Quaternion rot = transformCurves.GetRotation(time);
			Vector3 scale = transformCurves.GetScale(time);

			transformCurves.bone.localScale = scale;
			transformCurves.bone.localRotation = rot;
			transformCurves.bone.localPosition = pos;

	        Handles.RectangleCap( 0, pos, rot, 0.1f );
	        Handles.RectangleCap( 0, pos + rot*Vector3.forward, rot, 0.4f );
		}
    }




	[System.Serializable]
	public class GameObjectSnapshot
	{
		[SerializeField]
		private float _time;
		public float GetTime(){ return _time; }

		[SerializeField]
		private List<TransformSnapshot> _snapshotList;
		public List<TransformSnapshot> GetSnapshotList() { return _snapshotList; }

		[SerializeField]
		private Transform _root;
		public Transform GetRoot() { return _root; }

		public GameObjectSnapshot( GameObject go )
		{
			_time = -1; // hasn't taken it yet
			_root = go.transform;
			_snapshotList = new List<TransformSnapshot>();
		}

		public void Take( float time )
		{
			_time = time;

			TakeHierarchySnapshot( _root );
		}
		
		public void Apply()
		{
			if( _time < 0 )
				return;

            foreach( TransformSnapshot snapshot in _snapshotList )
            {
                snapshot.Apply();
            }
		}
		
		private void TakeHierarchySnapshot( Transform transform )
		{
			TransformSnapshot snapshot = new TransformSnapshot( transform );

			_snapshotList.Add( snapshot );
			
			for( int i = 0; i != transform.childCount; ++i )
			{
				TakeHierarchySnapshot( transform.GetChild(i) );
			}
		}

	}

	[System.Serializable]
	public class TransformSnapshot
	{
		[SerializeField]
		private Transform _transform;
		public Transform GetTransform() { return _transform; }

		[SerializeField]
		private Vector3 _localPosition;
		public Vector3 GetLocalPosition() { return _localPosition; }

		[SerializeField]
		private Quaternion _localRotation;
		public Quaternion GetLocalRotation() { return _localRotation; }

		public TransformSnapshot( Transform transform )
		{
			_transform = transform;
			_localPosition = _transform.localPosition;
			_localRotation = _transform.localRotation;
		}

        public void Apply()
        {
            _transform.localRotation = _localRotation;
            _transform.localPosition = _localPosition;
        }
	}
}
