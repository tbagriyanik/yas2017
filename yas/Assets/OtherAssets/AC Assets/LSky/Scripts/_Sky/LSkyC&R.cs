
/////////////////////////////////
/// Resources and components. ///
/////////////////////////////////

using System;
using UnityEngine;


namespace AC.LSky
{

	public partial class LSky : MonoBehaviour 
	{

		public bool      applySkybox    = true;  // Send skybox material to Lighting window.
		public Material  skyboxMaterial = null;  // Skybox material.
		public Texture2D moonTexture    = null;  // Moon texture.
		public Cubemap   outerSpaceCube = null;  // RGB: Nebula, Alpha: Stars field.
		public Cubemap   starsNoiseCube = null;  // Stars noise texture.
		//------------------------------------------------------------------------------------

		[SerializeField] private Light m_SunLight = null;        // Sun light component.
		public Transform SunLightTransform{ get; private set; }  // Sun light transform component.

		[SerializeField] private Light m_MoonLight = null;       // Moon light component.
		public Transform MoonLightTransform{ get; private set; } // Moon light transform component.
		//------------------------------------------------------------------------------------

		// Cache necessary components.
		void CacheComponents()
		{
			if(m_SunLight  != null) 
				SunLightTransform  = m_SunLight.transform;
			else
				SunLightTransform  = null;

			if(m_MoonLight != null) 
				MoonLightTransform = m_MoonLight.transform;
			else
				MoonLightTransform = null;
		}
		//------------------------------------------------------------------------------------

		/// <summary>
		/// Set sun local rotation.
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetSunLightLocalRotation(Quaternion rot)
		{
			SunLightTransform.localRotation = rot;
		}

		/// <summary>
		/// Set sun rotation
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetSunLightRotation(Quaternion rot)
		{
			SunLightTransform.rotation = rot;
		}
		//------------------------------------------------------------------------------------


		/// <summary>
		/// Set sun local rotation.
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetMoonLightLocalRotation(Quaternion rot)
		{
			MoonLightTransform.localRotation = rot;
		}

		/// <summary>
		/// Set sun rotation
		/// </summary>
		/// <param name="rot">Rot.</param>
		public void SetMoonLightRotation(Quaternion rot)
		{
			MoonLightTransform.rotation = rot;
		}


		// Check components and resources.
		bool CheckResources()
		{

			if(m_SunLight == null)
				return false;

			if(SunLightTransform == null)
				return false;

			if(m_MoonLight == null)
				return false;

			if(MoonLightTransform == null)
				return false;

			if(moonTexture == null)
				return false;

			if(skyboxMaterial == null)
				return false;

			if(outerSpaceCube == null)
				return false;

			if(starsNoiseCube == null)
				return false;

			return true;
		}
		//------------------------------------------------------------------------------------

		public bool IsReady
		{
			get{ return CheckResources(); }
		}
		//------------------------------------------------------------------------------------
	}
}
