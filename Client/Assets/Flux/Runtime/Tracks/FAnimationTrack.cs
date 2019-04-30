using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	 public class FAnimationTrack : FTrack {

		// what is the animation controller that will be used to 
		// build this track's animation state machine
		[SerializeField]
		[HideInInspector]
		private RuntimeAnimatorController _animatorController = null;
		public RuntimeAnimatorController GetAnimatorController() { return _animatorController; }

		[SerializeField]
		[HideInInspector]
		private Vector3 _startPos;
		[SerializeField]
		[HideInInspector]
		private Quaternion _startRot;
		[SerializeField]
		[HideInInspector]
		private Vector3 _startScale;

		public override void Init()
		{
			if( Owner.GetComponent<Animator>() == null )
				Owner.gameObject.AddComponent<Animator>();

			base.Init();
		}

		public override void Stop ()
		{
			base.Stop();

			if( IsPreviewing && HasPreview )
			{
				Animator animator = Owner.GetComponent<Animator>();
				animator.playbackTime = GetPreviewTime( 0 );
				animator.Update( 0f );
			}
		}

		public override void UpdateEventsEditor( int frame, float time )
		{
			if( IsPreviewing && HasPreview )
			{
				Animator animator = Owner.GetComponent<Animator>();
				animator.playbackTime = GetPreviewTime( frame );
				animator.Update( 0f );
			}
		}

		public override bool CanTogglePreview ()
		{
			return true;
		}

		public override void CreatePreview ()
		{
			if( HasPreview )
				return;
			
			if( !CanCreatePreview() )
			{
				IsPreviewing = false;
				Debug.LogWarning( "Couldn't create preview because it doesn't have an Animator Controller or at least one of the animation events doesn't have an Animation Clip.");
				return;
			}
			
//			Debug.LogWarning("Creating Preview");
			
			int currentFrame = Sequence.GetCurrentFrame();
			
			_startPos = Owner.localPosition;
			_startRot = Owner.localRotation;
			_startScale = Owner.localScale;

			if( !Sequence.IsStopped )
				Sequence.Stop();
				
			Sequence.Init();
			
			Animator animator = Owner.GetComponent<Animator>();
			
			RuntimeAnimatorController controller = animator.runtimeAnimatorController;
			animator.runtimeAnimatorController = null;
			animator.runtimeAnimatorController = controller;
			
			float delta = 1f / Sequence.FrameRate;
			
			animator.enabled = true;
			
			animator.StartRecording( -1 );
			
			animator.enabled = false;
			
			int frame = 0;
			
			while( frame <= Sequence.Length )
			{
				bool wasEnabled = animator.enabled;
				UpdateEvents( frame, frame * delta );
				if( wasEnabled )
				{
					if( animator.enabled )
					{
						animator.Update( delta );
					}
					else
					{
						animator.enabled = true;
						animator.Update( delta );
						animator.enabled = false;
					}
				}
				else if( animator.enabled )
				{
					animator.Update( 0 );
				}
				++frame;
			}
			
			animator.StopRecording();
			
			animator.StartPlayback();
			animator.playbackTime = 0;
			animator.Update( 0f );
			
			if( currentFrame < 0 )
				Sequence.Stop();
			else
				Sequence.SetCurrentFrame( currentFrame );
			
			float previewTime = GetPreviewTime( currentFrame );
			
			if( previewTime >= 0 )
			{
				animator.playbackTime = previewTime;
				animator.Update( 0f );
			}
			
			
			HasPreview = true;

		}

		public override void ClearPreview ()
		{
			if( !HasPreview )
				return;
			
//			Debug.LogWarning("Clearing Preview");
			
			Animator animator = Owner.GetComponent<Animator>();
			//			if( animator.recorderStopTime > 0 )
//			{
//				animator.playbackTime = 0;
//				animator.Update( 0f );
//			}
			animator.StopPlayback();
			
			Owner.localPosition = _startPos;
			Owner.localRotation = _startRot;
			Owner.localScale = _startScale;
			
			HasPreview = false;
		}

		public override bool CanCreatePreview ()
		{
			if( _animatorController == null )
				return false;
			
			List<FEvent> evts = GetEvents();
			for( int i = 0; i != evts.Count; ++i )
			{
				if( ((FPlayAnimationEvent)evts[i])._animationClip == null )
					return false;
			}
			
			return true;
		}

		private float GetPreviewTime( int frame )
		{
			if( frame < 0 )
				return 0f;
			
			int actualFrame = 0;
			for( int i = 0; i <= frame; ++i )
			{
				if( HasAnimationOnFrame(i) )
					++actualFrame;
			}
			
			if( actualFrame == 0 )
				return 0f; // haven't gotten there yet
			
			float t = (float)actualFrame / Sequence.FrameRate;
			
			Animator animator = Owner.GetComponent<Animator>();
			
			t = Mathf.Clamp( t, 0, animator.recorderStopTime - animator.recorderStartTime );
			return t;
		}

		private bool HasAnimationOnFrame( int frame )
		{
			FEvent[] evts = new FEvent[2];
			int numEvents = GetEventsAt( frame, ref evts );
			if( numEvents == 0 )
				return false;

			return (evts[0].Start != frame && ((FPlayAnimationEvent)evts[0])._animationClip != null) || (numEvents == 2 && evts[1].Start != frame && ((FPlayAnimationEvent)evts[1])._animationClip != null );
		}
	}
}