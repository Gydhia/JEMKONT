﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
[InitializeOnLoad]
#endif
[RequireComponent(typeof(MeshFilter))]
public class MergeSmoothingGroup : MonoBehaviour
{
	static MergeSmoothingGroup()
	{
		#if UNITY_EDITOR
		EditorApplication.update += Update;
		#endif
	}

	static void Update ()
	{
		MergeSmoothingGroup[] msgs = Resources.FindObjectsOfTypeAll (typeof(MergeSmoothingGroup)) as MergeSmoothingGroup[];

		foreach (MergeSmoothingGroup msg in msgs)
		{
			msg.enabled = true;
		}
		#if UNITY_EDITOR
		EditorApplication.update -= Update;
		#endif
	}

	void Awake()
	{
		OnEnable ();
	}

	void OnEnable()
	{
		MeshFilter meshFilter =  GetComponent<MeshFilter>();
		if(meshFilter == null || meshFilter.sharedMesh == null)
		{
			return;
		}

		Mesh mesh = meshFilter.sharedMesh;

		mesh.colors = getAverageNormalsAsColors(mesh.normals, createGroups(mesh.vertices));

		#if UNITY_EDITOR
		EditorUtility.SetDirty(this);
		SceneView.RepaintAll();
		#endif

		enabled = false;
	}

	private List<List<int> > createGroups(Vector3[] vertices)
	{
		List<List<int>> verticesGroups = new List<List<int>>();
		for (int i = 0; i < vertices.Length; i++)
		{
			bool alone = true;
			foreach (List<int> verticesGroup in verticesGroups)
			{
				if (vertices[verticesGroup[0]] == vertices[i])
				{
					verticesGroup.Add(i);
					alone = false;
					break;
				}
			}

			if (alone)
			{
				List<int> list = new List<int>();
				list.Add(i);
				verticesGroups.Add(list);
			}
		}

		return verticesGroups;
	}

	private Color[] getAverageNormalsAsColors(Vector3[] normals, List<List<int> > groups)
	{
		Color[] colors = new Color[normals.Length];
		foreach (List<int> group in groups)
		{
			Vector3 average = Vector3.zero;
			foreach (int i in group)
			{
				average += normals[i];
			}
			average.Normalize();

			foreach (int i in group)
			{
				colors[i] = new Color(average.x, average.y, average.z);
			}
		}

		return colors;
	}


}