using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMeshes {

    // We can define an interface for mesh streams. All mesh generators need to setup the meshData
    // and also need functions that will sety the vertex data into the buffers and also set the data into index buffers.
	public interface IMeshStreams { 
        void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount);
        void SetVertex(int index, Vertex data);
        void SetTriangle(int index, int3 triangle);
    }
}