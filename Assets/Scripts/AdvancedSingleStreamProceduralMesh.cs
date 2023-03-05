using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System.Runtime.InteropServices;

using static Unity.Mathematics.math;

// We can also create the mesh using a single stream instead of multiple
// Although we could define the attributes in any order, Unity requires a fixed attribute order per stream: position, normal, tangent, color, texture coordinate sets from 0 up to 7, blend weights, and blend indices.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedSingleStreamProceduralMesh : MonoBehaviour
{
    // We will define a struct that contains all of our vertex information
    // We want to copy this over to the meshData directly so we want the data structure to be used
    // exactly how we describe it. This is not garunteed by default. The c# compiler might arrange it in a 
    // different order. We can enforce the exact order by attaching the structLayout attribute with Layoutkind.Sequential.
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex {
		public float3 position, normal;
		public half4 tangent;
		public half2 texCoord0;
	}

    private void OnEnable() {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        int vertexAttributeCount = 4; 
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // To put all the attributes into the first stream we can simply remove the strream argument
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2); // remember we have 8 possible texCoord(UV)

        // How many vertices we want
        int vertexCount = 4;
        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();
    
        // NativeArray<float3> positions = meshData.GetVertexData<float3>();
        // // we can access the values an set them to what we want
        // positions[0] = 0f;
        // positions[1] = right();
        // positions[2] = up();
        // positions[3] = float3(1f, 1f, 0f); // we set vertexCount to 4

        // NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        // normals[0] = normals[1] = normals[2] = normals[3] = back();

        // half h0 = half(0f), h1 = half(1f);

        // NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
        // tangents[0] = tangents[1] = tangents[2] = tangents[3] = half4(h1, h0, h0, half(-1f));

        // NativeArray<half2> uv = meshData.GetVertexData<half2>(3);
        // uv[0] = h0;
        // uv[1] = half2(h1, h0);
        // uv[2] = half2(h0, h1);
        // uv[3] = half2(h1, h1);
        // remove all code that sets the data to different streams

        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>(); // get the first stream and store it as vertex

        half h0 = half(0f), h1 = half(1f);

		var vertex = new Vertex {
			normal = back(),
			tangent = half4(h1, h0, h0, half(-1f))
		};

		vertex.position = 0f;
		vertex.texCoord0 = h0;
		vertices[0] = vertex;

		vertex.position = right();
		vertex.texCoord0 = half2(h1, h0);
		vertices[1] = vertex;

		vertex.position = up();
		vertex.texCoord0 = half2(h0, h1);
		vertices[2] = vertex;

		vertex.position = float3(1f, 1f, 0f);
		vertex.texCoord0 = h1;
		vertices[3] = vertex;

        int triangleIndexCount = 6;
        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;

        // Also we have to define the submeshes of the mesh. We do thsi after setting the indices by setting subMeshCount to 1 ( we are only drawing 1 mesh)
        meshData.subMeshCount = 1;
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount) {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh{
            bounds = bounds, // use the same bounds for the mesh
            name = "Procedural Mesh"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
