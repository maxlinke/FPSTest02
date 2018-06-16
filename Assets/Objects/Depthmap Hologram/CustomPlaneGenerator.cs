using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlaneGenerator : MonoBehaviour {

	public MeshFilter meshFilter;

	[Range(1,1024)]
	public int xResolution;
	[Range(1,1024)]
	public int yResolution;

	void Start () {
		Generate_v1();
	}
	
	void Update () {
		
	}

	void OnDrawGizmos(){
		/*
		if(vertices != null){
			for(int i=0; i<vertices.Length; i++){
				float asd = (float)i/(float)(vertices.Length-1);
				Color col = new Color(asd, asd, asd);
				Gizmos.color = col;
				Gizmos.DrawCube(transform.position + vertices[i], Vector3.one * 0.05f);
			}
		}
		*/
	}
		
	void Generate_v1(){
		Vector3[] vertices = new Vector3[(xResolution + 1) * (yResolution + 1)];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] texcoords = new Vector2[vertices.Length];

		for(int i=0; i<(yResolution + 1); i++){
			float yPos = ((float)i / (float)yResolution) - 0.5f;
			for(int j=0; j<(xResolution + 1); j++){
				float xPos = ((float)j / (float)xResolution) - 0.5f;
				int vertIndex = i * (xResolution + 1) + j;
				vertices[vertIndex] = new Vector3(xPos, yPos, 0f);
				normals[vertIndex] = new Vector3(0,0,-1);
				texcoords[vertIndex] = new Vector2(((float)j / (float)xResolution), ((float)i / (float)yResolution));
			}
		}

		int numberOfQuads = xResolution * yResolution;
		int numberOfTris = 2 * numberOfQuads;
		int[] indices = new int[3 * numberOfTris];

		for(int i=0; i<numberOfQuads; i++){
			int i6 = i*6;
			int quadIndex = i;
			int rowIndex = quadIndex / xResolution;
			int bottomLeft = quadIndex + rowIndex;
			int bottomRight = bottomLeft + 1;
			int topLeft = bottomLeft + (xResolution + 1);
			int topRight = topLeft + 1;

			indices[i6] = bottomLeft;
			indices[i6+1] = topRight;
			indices[i6+2] = bottomRight;

			indices[i6+3] = bottomLeft;
			indices[i6+4] = topLeft;
			indices[i6+5] = topRight;
		}

		meshFilter.mesh = new Mesh();
		meshFilter.mesh.name = "Custom Plane";
		meshFilter.mesh.vertices = vertices;
		meshFilter.mesh.normals = normals;
		meshFilter.mesh.uv = texcoords;
		meshFilter.mesh.triangles = indices;
	}
}
