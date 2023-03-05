using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralMeshes.Streams {

    public struct SingleStream : IMeshStreams {

        // To store the vertex data we introduce a new stream0 struct that sotres the different attributes
        // sequentially. We can use the Vertex struct from Vertex.cs but this allows us to modify Vertex without
        // having to change how SingleStream works. The burst compiler will take care of the conversion from Vertex
        // to Stream0.
        [StructLayout(LayoutKind.Sequential)]
		struct Stream0 {
			public float3 position, normal;
			public float4 tangent;
			public float2 texCoord0;
		}
		
        // By default if we try to generate a mesh as is without the attribute NativeDisableContainerSafetyRestriction then we will end up with
        // an error from unity that the stream0 and triangles native arrays might be representing overlapping data. Mesh data is usually a single
        // unmanaged block. Our job trries to access two subsections of this data at the same time. This is not allowed in Unity by default.
        // While this is generally a bad thing we know for sure that the vertex and index data are never overlapping. We can disable this safety.
        // importing Unity.Collections.LowLevel.Unsafe
        [NativeDisableContainerSafetyRestriction]
		NativeArray<Stream0> stream0;
        // [NativeDisableContainerSafetyRestriction]
        // NativeArray<int3> triangles;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
            // Here we implement the setup of the single stream variant of generating the mesh.
            // The onlyu change here is we use full size arrays instead of the reduced size we used in the first example.
            var discriptor = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            discriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            discriptor[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
            discriptor[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4);
            discriptor[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);

            meshData.SetVertexBufferParams(vertexCount, discriptor);
            discriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;

            // If we try to run the job to generate a square grid as is this step will fail. This is because the job has not run yet and there 
            // are no vertices yet to calculate bounds and the index buffer has aribtrary data. We thus set both flags DontRecalculateBounds and DontValidateIndices
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) {
                bounds = bounds,
                vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }

        // This set vertex function is trivial so it will mostlikely b e conmverted to an inline command on call by the compiler.
        // However if we want do something more complex like convert from Float32 to Float16 in here there is a chance that 
        // the burst compiler will not convert it to an inline command. In general it is better to have calls become inline commands.
        // Especially since we will call the setVertex 4 times per quad. To solve this we can instruct the compiler to agressively inline
        // This function. System.Runtime.CompilerServices has the required type.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex data) => stream0[index] = new Stream0 {
          position = data.position,
          normal = data.normal,
          tangent = data.tangent,
          texCoord0 = data.texCoord0  
        };

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

    }


    public struct MultiStream : IMeshStreams {

       
        [NativeDisableContainerSafetyRestriction]
		NativeArray<float3> stream0, stream1; // position and normal streams
        [NativeDisableContainerSafetyRestriction]
		NativeArray<float4> stream2; // tanget stream
        [NativeDisableContainerSafetyRestriction]
		NativeArray<float2> stream3; // uv stream

        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
            // Here we implement the setup of the single stream variant of generating the mesh.
            // The onlyu change here is we use full size arrays instead of the reduced size we used in the first example.
            var discriptor = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            discriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            discriptor[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);
            discriptor[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4, stream: 2);
            discriptor[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3);

            meshData.SetVertexBufferParams(vertexCount, discriptor);
            discriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;

            // If we try to run the job to generate a square grid as is this step will fail. This is because the job has not run yet and there 
            // are no vertices yet to calculate bounds and the index buffer has aribtrary data. We thus set both flags DontRecalculateBounds and DontValidateIndices
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) {
                bounds = bounds,
                vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            stream0 = meshData.GetVertexData<float3>();
            stream1 = meshData.GetVertexData<float3>(1);
            stream2 = meshData.GetVertexData<float4>(2);
            stream3 = meshData.GetVertexData<float2>(3);
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }

        // This set vertex function is trivial so it will mostlikely b e conmverted to an inline command on call by the compiler.
        // However if we want do something more complex like convert from Float32 to Float16 in here there is a chance that 
        // the burst compiler will not convert it to an inline command. In general it is better to have calls become inline commands.
        // Especially since we will call the setVertex 4 times per quad. To solve this we can instruct the compiler to agressively inline
        // This function. System.Runtime.CompilerServices has the required type.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex data) {
            stream0[index] = data.position;
            stream1[index] = data.normal;
            stream2[index] = data.tangent;
            stream3[index] = data.texCoord0;
        }

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

    }
}