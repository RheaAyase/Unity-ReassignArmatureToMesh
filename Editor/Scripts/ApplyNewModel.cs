using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


public class ReassignArmature: EditorWindow
{
	private int MeshCount = 0;
	private List<SkinnedMeshRenderer> Meshes = new List<SkinnedMeshRenderer>();
	private Transform NewArmature;
	private bool IncludeInactiveBones = true;
	private string RootBoneName = "Hips";
	private string Status = "Waiting for input.";

	[MenuItem("Rhea/ReassignArmature")]
	static void Init()
	{
		ReassignArmature window = EditorWindow.GetWindow(typeof(ReassignArmature)) as ReassignArmature;
		window?.Show();
	}
	void OnGUI()
	{
		GUILayout.Label("Reassigns Mesh weights to the selected Armature based on object names. (Might want to create a proper backup!!)");

		this.MeshCount = EditorGUILayout.IntField("Mesh count: ", this.MeshCount);
		while( this.MeshCount > this.Meshes.Count )
			this.Meshes.Add(null);
		while( this.MeshCount < this.Meshes.Count )
			this.Meshes.RemoveAt(this.Meshes.Count - 1);

		for( int i = 0; i < this.MeshCount; i++ )
		{
			this.Meshes[i] = EditorGUILayout.ObjectField($"Mesh {i}", this.Meshes[i], typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
		}

		this.NewArmature = EditorGUILayout.ObjectField("New Armature that will be assigned to the models", this.NewArmature, typeof(Transform), true) as Transform;
		this.RootBoneName = EditorGUILayout.TextField("Root Bone Name", this.RootBoneName);
		this.IncludeInactiveBones = EditorGUILayout.Toggle("Include inactive bones", this.IncludeInactiveBones);

		if( GUILayout.Button("Execute") )
		{
			if( this.NewArmature != null && this.Meshes != null && !string.IsNullOrWhiteSpace(this.RootBoneName) )
			{
				this.Status = "OK";

				for( int i = 0; i < this.MeshCount; i++ )
					Reassign(this.Meshes[i]);

				if( this.Status == "OK" )
					this.Status = "Done.";
			}
			else
			{
				this.Status = "Missing arguments.";
			}
			NewArmature = null;
		}

		EditorGUILayout.Separator();
		EditorGUILayout.TextField("Status:", this.Status);
	}

	private void Reassign(SkinnedMeshRenderer mesh)
	{
		if( this.NewArmature.Find(this.RootBoneName) == null )
		{
			this.Status = "Root bone not found.";
			return;
		}

		Transform[] bones = mesh.bones;
		Transform[] children = this.NewArmature.GetComponentsInChildren<Transform>(this.IncludeInactiveBones);
		mesh.rootBone = this.NewArmature.Find(this.RootBoneName);

		for( int i = 0; i < bones.Length; i++ )
		{
			Transform foundBone = children.FirstOrDefault(c => c.name == bones[i].name);
			if( foundBone == null )
			{
				this.Status = $"Aborting - couldn't find a bone: {bones[i].name}";
				return;
			}

			bones[i] = foundBone;
		}

		mesh.bones = bones;
	}
}
