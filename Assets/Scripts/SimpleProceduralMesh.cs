using UnityEngine;

// First implementing a simple form of mesh generation
// This requires that the game object this is a component of needs to have a 
// meshFilter and a meshRenderer component. we can do this with the RequireComponent attribute
// Doing this will make it so that when this script is added as a component the component types added in the parameters
// will be auto added to the object.
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SimpleProceduralMesh : MonoBehaviour
{
    private void OnEnable() {
        var mesh = new Mesh{
            name = "Procedural Mesh"
        };
        // At its core a mesh contains triangles. Triangles are described by 3 vertices and these 3 vertices are 
        // used to describe the triangle in the mesh. A vertex is just a position in 3D space at the simplest level.
        // A simple triangle we can create is using the origin as 1 of the vertices, the right vector(1, 0, 0)?, and the up vector (0, 1, 0)?

        // Mesh accepts an array of vectors
        mesh.vertices = new Vector3[] {
            Vector3.zero, Vector3.right, Vector3.up
        };
        // When I enter play mode with just setting the above mesh to the meshfilter it does not display anything but
        // I do see that the mesh that is generated has 1 triagnle taking up 36 bytes: 12 (bytes per vec3 3x4) x 3.

        // After defining the triangles we also have to describe how the triangles are to be drawn.
        // This is done by providng the mesh.triangles a list of integers the repreent locations in the mesh.vertices array
        // so {0, 1, 2} would draw the first, second and 3rd position vertices into a triangle

        // mesh.triangles = new int[] {
        //     0, 1, 2
        // };
        // Once we set the triangles the mesh shows 1 triagnle in submesh #0. These indicies are stored as uint16s 
        // setting the triangle however does draw the mesh on the screen. but it is only visable from one direction
        // by default triangles are only visable from the front side. You can use the left hand thumb rule by rotating your left
        // hand in the direction the triangle vertices are drawn and the left thumb shows the direction of the front face
        // You can get it to be seen from the other side by reordering the vertices from 0, 1, 2 to 0, 2, 1
        mesh.triangles = new int[] {
            0, 2, 1
        };

        // One thing I notice is the triangle is not lit correction. It behaves as if it is lit from the opposite side. 
        // This is because there is no normal defined for the mesh. Normal is the direction the surface is supposed to face and
        // tells the engine at what angle the light is supposed to be bouncing from the surface to calculate lighting.
        // Usually it is of unit length. If we were standing on our triangle defined as 0, 2, 1. we should expect the
        // normal direction to be vector3.back since the face that is shown is in the negative z direction. (left hand rule above)
        // By default unity (if no normal is provided) sets the normal to the Vector3.forward direciton.
        // However the normals are defined per vertex. and the points within the trinagle ahve theri normal vectors interpolated. 
        // This allows one to give the illusion of curvature within the triagnle surface by having different normals for the different vertices.
        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back
        };
        // Adding normals doubles our vertex data
        
        // The next step is applying details to our triangle. The easiest way to do this is applying a texture. A texture can be many things
        // but in its simplest form it is an image that tells us the colour of each position.
        // Considering the triangle is 2D and so is an image we can tell how to colour the trinagle with providing 2D coordinates.
        // We provde coordinates in a nromalized space 0, 1 where (0, 0) is always the bottom left corner of the texture. And it is usually called 'uv' coordinates.
        // xy respectively.
        // We can provide a uv coordinate to each of our vertices which tell the shader which points of the texture should be sampled for each of the vertices
        // The remaining points in teh triangle are interpolated between the vertices and sampled from teh texture.
        // In our example of 0, 2, 1 we will give it (0, 0), (0, 1) and (1, 0).
        // The texture itself is applied to the base map of the material for URP
        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector2.up
        };
        // We can set upto 8 sets of texture coordinates per vertex namely mesh.uv, mesh.uv2, mesh.uv3... mesh.uv8. 
        // it is also possible to set 1D, 3D or 4D uv coordiantes.
        // You can also provide colours for each vertex and can interpolate the colours. The default URP shader does not support vertex colours
        // So it can be done in a custom shder graph.
        

        // Another way of adding details to the surface is through normal maps. Normal mapping is usually done with a nromal map texture.
        // This is a texture that is once again an image. However in this image the (R, G, B) colours instead of representing the colour of
        // a pixel instead represent the direction of the normal at a given uv. This can allow us to cause light to bounce off our surface
        // from specific parts giving the illusion of a 3D surface even though it is a flat 2D mesh. Applying to the normal map of the mesh
        // material allows us to see the result. However this is an incorrect result. The heights are off. The square sthat should be popping out are pushed in.
        // This incorrect result is  because the normal vectors currently exist in the texture space. We need to convert them to the world space
        // for it work correctly with lighting. we need a transformation matrix that defines a 3D space relative to the surface. This is called
        // The tangent space (the sapace tangent to the surface) with 2 axes in the up and right directions and the forward axis.
        // The up axis should point away from our surface, for which the vertex normal vectors are used. We also need the right and forward axes.
        // The right axis is whatever we consider right which in our case is vector3.right this is called the tangent vector/axis because this
        // direction must always be tangent to the surface curvature. We define these per vertex again by assigning vectors to the tangent property.
        // The shader constructs teh 3rd axis itself by calculating the vector orthogonal to the normal and tanget direction (norm(nromal x tangent)).
        // This direction however could be eitehr forward or back. For this reason we set the tangents with vector4s where the last value describes
        // which direction the 3rd axis should take (+- 1).
        // The default tangent points right and have the 4th component be 1. Dew to how unity shaders construct tanget space this is incorrect and we have to use
        // -1
        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        // To transform the mesh into a quad we can draw 2 triangles.

        // we dont have to define 3 new points we can reuse existing points by just providin
        // a new set of triangle draws
        mesh.vertices = new Vector3[] {
            Vector3.zero, Vector3.right, Vector3.up, new Vector3(1f, 1f)
        };
        mesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };
        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector3.up, Vector2.one
        };
        mesh.normals = new Vector3[]{
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };
        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
