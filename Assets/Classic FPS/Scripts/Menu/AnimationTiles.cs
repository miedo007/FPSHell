using UnityEngine;
using UnityEditor;

namespace HellishBattle
{
	public class AnimationTiles : MonoBehaviour
	{
		public int materialIndex = 0;
		public MeshRenderer AnimatedMeshRenderer;
		public float TilesPerSecound = 32;
		[Suffix("Tiles in Material")] public int TilesInTextureY = 128;

		// Private
		Material MoveableMaterial;
		int frame = 0;
		float timer;

		// Get Animated Material and Set Scale
		public void Start()
		{
			MoveableMaterial = AnimatedMeshRenderer.materials[materialIndex];
			// Main Texture
			MoveableMaterial.SetTextureScale("_MainTex", new Vector2(1f, 1f / TilesInTextureY));
			// Normal Map
			MoveableMaterial.SetTextureScale("_BumpMap", new Vector2(1f, 1f / TilesInTextureY));
			// Occlusion
			MoveableMaterial.SetTextureScale("_OcclusionMap", new Vector2(1f, 1f / TilesInTextureY));
			// Emission
			MoveableMaterial.SetTextureScale("_EmissionMap", new Vector2(1f, 1f / TilesInTextureY));
		}

		// Change Material Texture to Another
		public void Update()
		{
			timer += Time.deltaTime;

			if (timer > 1 / TilesPerSecound)
			{
				frame++;
				MoveableMaterial.SetTextureOffset("_MainTex", new Vector2(1f, (1f / TilesInTextureY) * frame));
				MoveableMaterial.SetTextureOffset("_BumpMap", new Vector2(1f, (1f / TilesInTextureY) * frame));
				MoveableMaterial.SetTextureOffset("_OcclusionMap", new Vector2(1f, (1f / TilesInTextureY) * frame));
				MoveableMaterial.SetTextureOffset("_EmissionMap", new Vector2(1f, (1f / TilesInTextureY) * frame));
				timer = 0;
			}
		}
	}

	/// Custom Editor
#if UNITY_EDITOR

	[CustomEditor(typeof(AnimationTiles)), CanEditMultipleObjects]
	public class AnimationTilesEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorUtility.SetDirty(target);
			AnimationTiles tiles = (AnimationTiles)target;

			EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
			EditorGUILayout.LabelField("Animated Tiles Script", EditorStyles.boldLabel);
			GUI.enabled = false;
			EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((AnimationTiles)target), typeof(AnimationTiles), false);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical("box");

			TinyGUI.EditorTitle("Animated Tiles Setttings", "Create animated materials");

			TinyGUI.InfoBox($"Number Tiles: {tiles.TilesInTextureY} \nLoot Lenght: {tiles.TilesInTextureY / tiles.TilesPerSecound}s", "Material Icon");
			TinyGUI.InfoBox("The entered number of Tiles in Direction is Incorrect!\nMaterial may be stretched incorrectly", "console.erroricon");

			EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatedMeshRenderer"), new GUIContent("Mesh Renderer"));
			if (tiles.AnimatedMeshRenderer != null)
			{
				if (tiles.AnimatedMeshRenderer.materials.Length > 1) tiles.materialIndex = EditorGUILayout.IntSlider(new GUIContent("Material Index"), tiles.materialIndex, 0, tiles.AnimatedMeshRenderer.materials.Length - 1);
				else tiles.materialIndex = 0;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("TilesPerSecound"), new GUIContent("Tiles per Second"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("TilesInTextureY"), new GUIContent("Tiles in Direction"));

				// Button for Automatic
				if (TinyGUI.IconButton("AutoLightbakingOn", " Automatic Tiles in Direction", GUILayout.Height(38)))
				{
					tiles.TilesInTextureY = tiles.AnimatedMeshRenderer.materials[tiles.materialIndex].GetTexture("_MainTex").height / tiles.AnimatedMeshRenderer.materials[tiles.materialIndex].GetTexture("_MainTex").width;
				}
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatedMeshRenderer"), new GUIContent("Mesh Renderer"));
				EditorGUILayout.HelpBox("In order for the Script to work you need to assign a MeshRenderer script object in which you want to create the animated material", MessageType.Error);
			}

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}
	}

#endif
}
