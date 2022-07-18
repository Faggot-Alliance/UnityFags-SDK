using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace UnityEditor {
	#if UNITY_EDITOR
	public static class AudioUtility {

		public static void PlayClip(AudioClip clip) {
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

			MethodInfo method = audioUtilClass.GetMethod(
				"PlayClip",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new System.Type[] {
				typeof(AudioClip), typeof(int), typeof(bool)
			},
			null
			);

			method.Invoke(
				null,
				new object[] {
				clip, 0, false
			}
			);
		}

		public static void StopAllClips () {
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			MethodInfo method = audioUtilClass.GetMethod(
				"StopAllClips",
				BindingFlags.Static | BindingFlags.Public
				);

			method.Invoke(
				null,
				null
				);
		}
		
		
		public static void SetClipSamplePosition(AudioClip clip , int iSamplePosition) {
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			MethodInfo method = audioUtilClass.GetMethod(
				"SetClipSamplePosition",
				BindingFlags.Static | BindingFlags.Public
				);
			
			method.Invoke(
				null,
				new object[] {
				clip,
				iSamplePosition
			}
			);
		}
	}
	
	#endif
}
