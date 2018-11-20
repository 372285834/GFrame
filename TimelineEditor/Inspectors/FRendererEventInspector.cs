﻿using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using System.Collections.Generic;

using GP;

namespace GPEditor
{
	public abstract class FRendererEventInspector : FEventInspector
	{
		protected List<string> _propertyNames = new List<string>();
		protected int _selectedProperty;
		
		protected string _customPropertyName = "_Alpha";
		
		protected SerializedProperty _propertyName;

		protected AnimBool _showPropertyName = new AnimBool();
		
		protected abstract bool IsValidProperty( ShaderUtil.ShaderPropertyType shaderPropertyType );

		protected override void OnEnable ()
		{
			base.OnEnable ();

			_propertyName = serializedObject.FindProperty("_propertyName");

			FEvent evt = (FEvent)target;
			
			Renderer renderer = evt.Owner.GetComponent<Renderer>();
			
			if( renderer == null )
				return;
			
			Shader shader = renderer.sharedMaterial.shader;
			
			int numProperties = ShaderUtil.GetPropertyCount( shader );
			
			_selectedProperty = -1;
			
			for( int i = 0; i != numProperties; ++i )
			{
				if( IsValidProperty( ShaderUtil.GetPropertyType( shader, i ) ) )
				{
					string propertyName = ShaderUtil.GetPropertyName( shader, i );
					if( propertyName == _propertyName.stringValue )
						_selectedProperty = _propertyNames.Count;
					
					_propertyNames.Add( propertyName );
				}
			}
			
			if( _selectedProperty == -1 ) // didn't find any? Use last, aka 'Custom'
			{
				_selectedProperty = _propertyNames.Count;
				_showPropertyName.target = true;
			}
			else
			{
				_showPropertyName.target = false; // don't show custom field
			}
			
			_propertyNames.Add( "Custom...");
			
#if UNITY_4_6
			_showPropertyName.valueChanged.AddListener( Repaint );
#endif
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			_selectedProperty = EditorGUILayout.Popup( ObjectNames.NicifyVariableName(_propertyName.name), _selectedProperty, _propertyNames.ToArray() );
			if( EditorGUI.EndChangeCheck() )
			{
				_showPropertyName.target = _selectedProperty == _propertyNames.Count-1;
				
				if( _selectedProperty != _propertyNames.Count-1 )
					_propertyName.stringValue = _propertyNames[_selectedProperty];
			}
			
			if( EditorGUILayout.BeginFadeGroup( _showPropertyName.faded ) )
			{
				EditorGUILayout.PropertyField( _propertyName, new GUIContent(" ") );
				EditorGUILayout.EndFadeGroup();
			}

			serializedObject.ApplyModifiedProperties();

#if UNITY_4_5
			if( _showPropertyName.isAnimating )
				Repaint();
#endif
		}
	}
}