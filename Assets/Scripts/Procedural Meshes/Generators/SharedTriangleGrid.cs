using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators {

    public struct SharedTriangleGrid : IMeshGenerator
    {
		// The bounds of this new mesh
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f + 0.5f / Resolution, 0f, sqrt(3f) / 2f));

		public int VertexCount => (Resolution + 1) * (Resolution + 1);

		public int IndexCount => 6 * Resolution * Resolution;

		public int JobLength => Resolution + 1;

		public int Resolution { get; set; }

        public void Execute<S> (int z, S streams) where S : struct, IMeshStreams {
			int iA = -Resolution - 2, iB = -Resolution - 1, iC = -1, iD = 0;
			// The ordering of the vertices differ for the even and odd rows
			var tA = int3(iA, iC, iD);
			var tB = int3(iA, iD, iB);

			// We want to draw equilateral triangles instead of right angle triangles
			// This will allow us to have smoother looking meshes under certain conditions
			int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);

			var vertex = new Vertex();
			vertex.normal.y = 1f;
			vertex.tangent.xw = float2(1f, -1f);

			// vertex.position.x = -0.5f;
			// The difference between the right angle triangle approach and this is that we will end up with rhombus shapes.
			// And each row of vertices are offset by half the base lenght of the triangle. So for the even rows we will
			// shift them left by 0.25 and the odd rows we will shift right by 0.25.
			float xOffset = -0.25f;
			// If we just do this the texture is also morphed. We need to adjust the u coordinate as well.
			float  uOffset = 0.0f;
			if((z & 1) == 1) {
				xOffset = 0.25f;
				// With this triangle construction one thing different is that the width of the entire mesh is now 
				// bigger by half a triangle. So we need to find the offset for the odd rows in this new space. s
				uOffset = 0.5f / (Resolution + 0.5f);

				// The ordering of the vertices differ for the even and odd rows
				tA = int3(iA, iC, iB);
				tB = int3(iB, iC, iD);
			}

			// we then scale that down and center it
			xOffset = xOffset / Resolution - 0.5f;
			
			vertex.position.x = xOffset;
			// without this new transformation of multiplying the z offset by sqrt3/2 the triangle was not equilateral
			vertex.position.z = ((float)z / Resolution - 0.5f) * sqrt(3f) / 2f;
			vertex.texCoord0.x = uOffset;
			vertex.texCoord0.y = vertex.position.z / (1f + 0.5f / Resolution) + 0.5f;
			streams.SetVertex(vi, vertex);
			vi += 1;

			for (int x = 1; x <= Resolution; x++, vi++, ti += 2) {
				vertex.position.x = (float)x / Resolution + xOffset;
				vertex.texCoord0.x = x / (Resolution + 0.5f) + uOffset;
				streams.SetVertex(vi, vertex);

				if (z > 0) {
					streams.SetTriangle(
						ti + 0, vi + tA
					);
					streams.SetTriangle(
						ti + 1, vi + tB
					);
				}
			}
        }
    }

}
