using UnityEngine;
using System.Collections.Generic;

namespace Flux
{	
	public class FRendererTrack : FTrack {

		private static Dictionary<int,MaterialPropertyBlockInfo> _materialPropertyBlocks = null;

		private static MaterialPropertyBlockInfo GetMaterialPropertyBlockInfo( int objInstanceId )
		{
			MaterialPropertyBlockInfo matPropertyBlockInfo = null;
			
			if( _materialPropertyBlocks.TryGetValue( objInstanceId, out matPropertyBlockInfo ) )
				return matPropertyBlockInfo;
			
			matPropertyBlockInfo = new MaterialPropertyBlockInfo();
			
			_materialPropertyBlocks.Add( objInstanceId, matPropertyBlockInfo );
			
			return matPropertyBlockInfo;
		}

		private class MaterialPropertyBlockInfo
		{
			public MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();
			public int _frameGotCleared = 0;

			public void Clear( int frame )
			{
				_materialPropertyBlock.Clear();
				_frameGotCleared = frame;
			}
		}

		private MaterialPropertyBlockInfo _matPropertyBlockInfo = null;
		public MaterialPropertyBlock GetMaterialPropertyBlock() { return _matPropertyBlockInfo._materialPropertyBlock; }

		private Renderer _renderer = null;
		public Renderer GetRenderer(){ return _renderer; }

		public override void Init()
		{
			_renderer = Owner.GetComponent<Renderer>();

			if( _materialPropertyBlocks == null )
				_materialPropertyBlocks = new Dictionary<int, MaterialPropertyBlockInfo>();

			_matPropertyBlockInfo = GetMaterialPropertyBlockInfo( Owner.GetInstanceID() );

			base.Init();
		}



		public override void UpdateEvents (int frame, float time)
		{
			if( _matPropertyBlockInfo._frameGotCleared != frame )
				_matPropertyBlockInfo.Clear( frame );
			base.UpdateEvents(frame, time);
			_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
		}

		public override void UpdateEventsEditor (int currentFrame, float currentTime)
		{
			if( _matPropertyBlockInfo._frameGotCleared != currentFrame )
				_matPropertyBlockInfo.Clear( currentFrame );
			base.UpdateEventsEditor (currentFrame, currentTime);
			_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
		}

		public override void Stop ()
		{
			base.Stop();
			if( _matPropertyBlockInfo == null )
				Init();
			_matPropertyBlockInfo.Clear( _matPropertyBlockInfo._frameGotCleared );
			_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
		}
	}
}
