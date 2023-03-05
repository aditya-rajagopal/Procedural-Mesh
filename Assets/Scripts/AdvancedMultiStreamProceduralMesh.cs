using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

using static Unity.Mathematics.math;

// In the simple API we assign data to th mesh via the simple API
// unity has to copy and convert everything to the mesh's native memory at some point
// The advanced API allows us to work directly in the native memory format.
// The memory is split into regions. The two regions to focus on are the vertex region
// and the index region. The vertex region consists of one or more data streams which are
// sequential blocks of vertex data of the same format. 
// We can have up to 4 different vertex data streams per mesh.
// Since we have vertex position, normals, tangets and UV we can use a seperate stream for each
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedMultiStreamProceduralMesh : MonoBehaviour
{
    private void OnEnable() {
        // We need to allocate the native mesh data we want to write. There is a static method fo rthis.
        // It returns a Mesh.MeshDataArray strtuct that acts like an array of native mesh data which we can write to.
        // The paramter this method takes is the number of meshes we wnat to draw which in this case is 1.
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);

        // To fill in the data we have to do it by element. Each element of the meshDataArray is of type Mehs.MeshData
        Mesh.MeshData meshData = meshDataArray[0];

        // Each vertex in our mesh has 4 attributes position, normal, tangent, and UV. we can describe this by allocating
        // a temporary native array with VertexAttributeDescriptor elements.
        int vertexAttributeCount = 4; // how many types we have 
        // we can set the initialization to uninitialized because we always override it anyway and this is a small optimization step
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // We next set up the attributes
        // We use the VertexAttributeDescriptor consturctor. This has 4 optional parameters attribute, format, dimension, and stream
        // By default attribute is VertexAttribute.Position and the format is VertexAttributeFormat.Float32, dimension is 3 and stream is 0
        // These default values work accurately for our position attribute but we need to call the constructor with atleast 1 argument else it will 
        // throw an error so lets explicitly set the dimension to 3
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        // we can do the seam for the other 3 attributes
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);

        // since our mesh is so simple we can reduce the precision of the tangenta nd UV values This will help reduce the size of our vertex
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, dimension: 4, stream: 2);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, dimension: 2, stream: 3); // remember we have 8 possible texCoord(UV)

        // How many vertices we want
        int vertexCount = 4;
        // Teh vertex streams of the mesh are allocated by invoking the SetVertexBufferParams on th emesh data. with vertex count
        // and attribute definitions as the arguments
        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        // Once we have set the params we dont need the vertexAttributes anymroe and can dispose of it
        vertexAttributes.Dispose();

        // After setting the params we can set get the vertex streams by invoking the GetVertexData function. It returns a pointer
        // to the relavant section of the mesh data. It acts as a proxy and there is no seperate array. This will allow a job to directly write
        // to the mesh dataa skipping any intermediate copy steps from native array to mesh data.
        // The method retuns the first stream by default. it has float3 elements. We will use float3 instead of vector3
        NativeArray<float3> positions = meshData.GetVertexData<float3>();
        // we can access the values an set them to what we want
        positions[0] = 0f;
        positions[1] = right();
        positions[2] = up();
        positions[3] = float3(1f, 1f, 0f); // we set vertexCount to 4

        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        normals[0] = normals[1] = normals[2] = normals[3] = back();

        half h0 = half(0f), h1 = half(1f);

        NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
        tangents[0] = tangents[1] = tangents[2] = tangents[3] = half4(h1, h0, h0, half(-1f));

        NativeArray<half2> uv = meshData.GetVertexData<half2>(3);
        uv[0] = h0;
        uv[1] = half2(h1, h0);
        uv[2] = half2(h0, h1);
        uv[3] = half2(h1, h1);

        // Finally we also have to set the index buffer for the triangles. Which is set by using SetIndexBufferParams
        // First argument is the count of triangle indices and teh second which describes the index format. We can initiallyu use IndexFormat.UInt32
        // if we use UInt32 we will use twice the size we will need so instead we can use UInt16 which in mathematics is ushort
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
        // Next we need to specify which part of the indexbuffer the particular subemsh needs to use.
        // There are 2 parameters the submesh index (0 only since wew have only 1 submesh) and pass it a submeshdescriptor which takes in the start position
        // in the index buffer and how many indices to take

        // An important point to consider is that unity does not automatically calcualte the bounds of a mesh. It does calcualte the bouds of a submesh.
        // This woudl require checking all the vertices. We can avoid all that work by manually providng  the boudns ourself. We can also set the vertex property count.
        // we provide this to SubMeshDescriptor
        // and in th submesh parameters we can explcitly ask Unity to not calculate these values itself byu passing
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount) {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh{
            bounds = bounds, // use the same bounds for the mesh
            name = "Procedural Mesh"
        };

        // finish by invoking another method with the array and the mesh as the arguments.  This applys the mesh data to the mesh.
        // This direct applicatyion of the array to the mesh works because it only has a single element
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        // After this step we can no longer access the meshData unless we retreave it again via Mesh.AcquireReadOnlyMeshData.

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
