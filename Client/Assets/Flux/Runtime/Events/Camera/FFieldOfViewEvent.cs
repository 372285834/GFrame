using UnityEngine;


// you don't have to put it inside the namespace, I just do that
// for all classes I create, instead just do #include Flux;
namespace Flux
{
	// in order to have tween events, just extend the class
	// from FTweenEvent, and then choose what you want to tween on
	// The FEvent attribute is to define how it will show up in the
	// add event menu
	[FEvent("Camera/Field Of View")]
	public class FFieldOfViewEvent : FTweenEvent<FTweenFloat> {

		// have variable to cache the camera so it is faster
		private Camera _camera = null;

		protected override void OnInit()
		{
			// cache the camera on init, so it does only once at
			// the start before the event is used
			_camera = Owner.GetComponent<Camera>();
		}

		protected override void ApplyProperty( float t )
		{
			// apply property gets a float from [0,1],
			// then we apply the tween to get the result
			_camera.fieldOfView = _tween.GetValue( t );
		}
	}
}
