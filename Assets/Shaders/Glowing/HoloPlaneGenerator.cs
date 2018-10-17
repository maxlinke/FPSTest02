using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloPlaneGenerator : MonoBehaviour {

	public MeshFilter meshFilter;

	[Range(1,1024)] [SerializeField] int xResolution;
	[Range(1,1024)] [SerializeField] int yResolution;
	[Range(0f,1f)] [SerializeField] float subQuadScale;
	[SerializeField] bool generateOnStart;
		
	void Start () {
		if(generateOnStart) Generate();
	}

	public void Generate () {
		Vector2 quadSize = new Vector2(subQuadScale / (float)xResolution, subQuadScale / (float)yResolution);
		quadSize *= 0.5f;
		Vector3 normal = new Vector3(0,0,-1);

		int numberOfQuads = xResolution * yResolution;
		int numberOfTris = 2 * numberOfQuads;
		int numberOfVertices = 4 * numberOfQuads;

		Vector3[] vertices = new Vector3[numberOfVertices];
		Vector3[] normals = new Vector3[numberOfVertices];
		Vector2[] texcoords = new Vector2[numberOfVertices];
		int[] indices = new int[3 * numberOfTris];

		int vertexIndex = 0;

		for(int y=0; y<yResolution; y++){
			float yPos = ((float)y / (float)yResolution) - 0.5f + (0.5f / (float)yResolution);

			for(int x=0; x<xResolution; x++){
				float xPos = ((float)x / (float)xResolution) - 0.5f + (0.5f / (float)xResolution);

				Vector2 texcoord = new Vector2(xPos + 0.5f, yPos + 0.5f);

				vertices[vertexIndex] = new Vector3(xPos - quadSize.x , yPos - quadSize.y, 0f);
				normals[vertexIndex] = normal;
				texcoords[vertexIndex] = texcoord;
				vertexIndex++;

				vertices[vertexIndex] = new Vector3(xPos + quadSize.x , yPos - quadSize.y, 0f);
				normals[vertexIndex] = normal;
				texcoords[vertexIndex] = texcoord;
				vertexIndex++;

				vertices[vertexIndex] = new Vector3(xPos - quadSize.x , yPos + quadSize.y, 0f);
				normals[vertexIndex] = normal;
				texcoords[vertexIndex] = texcoord;
				vertexIndex++;

				vertices[vertexIndex] = new Vector3(xPos + quadSize.x , yPos + quadSize.y, 0f);
				normals[vertexIndex] = normal;
				texcoords[vertexIndex] = texcoord;
				vertexIndex++;

				//bottom left, right, top left, right
			}
		}

		for(int i=0; i<numberOfQuads; i++){
			int i4 = i*4;
			int i6 = i*6;

			indices[i6+0] = i4+0;
			indices[i6+1] = i4+3;
			indices[i6+2] = i4+1;

			indices[i6+3] = i4+0;
			indices[i6+4] = i4+2;
			indices[i6+5] = i4+3;

		}

		Mesh mesh = new Mesh();
		mesh.name = "Custom Plane";
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = texcoords;
		mesh.triangles = indices;
		meshFilter.sharedMesh = mesh;
	}

}
