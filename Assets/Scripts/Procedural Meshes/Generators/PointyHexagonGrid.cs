using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{

    // If we structure teh vertices as hexagons you can have 1 central vertex with 6 surrounding ones
    public struct PointyHexagonGrid : IMeshGenerator
    {
        public int VertexCount => 7 * Resolution * Resolution; // 7 vertices per hexagon

        public int IndexCount => 18 * Resolution * Resolution;  // 

        public int JobLength => Resolution;

        // The thing that is left is to calculate the bounds of the mesh. The generator
        // should be able to provide the bounds.
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(
            (Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f),
            0f,
            0.75f + 0.25f / Resolution
        ));

        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = 7 * Resolution * z, ti = 6 * Resolution * z;  // each hexagon has 6 triangles and 7 vertices

            float h = sqrt(3) / 4f; // We can define the height of the equilateral triangle. Because we want the size of the hexagon to be similar to the square
                                    // we halve the size of triangle.

            float2 centerOffset = 0f;

			if (Resolution > 1) {
				centerOffset.x = (((z & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h; // We want the alteranting rows half the triangle height either side of the mesh boundry and offset it by (r-1)*h left to cneter it around x
                centerOffset.y = -0.375f * (Resolution - 1); // each subsequent row needs to be 0.75 above and if we want to center it around z then it must be offset by an additional r-1 times 1/2 of 3/4th
			}

            for (int x = 0; x < Resolution; x++, vi += 7, ti += 6)
            {
                // Each hexagon has 7 triangle. We can index the vertices with 0 starting at the center 1 below it 
                // and then the other vertices clockwise from there.

                // the hexagon is sqrt(3) / 2 across. So we need to move each hexagon to the right by that amount.
                // Also each subsequent row must be shifted by half the height.

                var center = (float2(2f * h * x, 0.75f * z) + centerOffset) / Resolution;
                // if you think about a hexagon with the point at the bottom then there are 3 possible x coordinates
                // one along 2,3 then 1 along 4, 0, 1 and finally one around 5, 6. From 0s position the other two are +- sqrt(3)/2 i.e height of the equilateral triangle.
                // But to keep the size of hte hexagon consistent with the square we will halve the size of the triangle.
                var xCoordinates = center.x + float2(-h, h) / Resolution;
                // As for the z coordinates we have 4 more ones. -0.5, -0.25, 0.25, and 0.5
                var zCoordinates = center.y + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;

                

                var vertex = new Vertex();
                
                vertex.normal.y = 1f;
                vertex.tangent.xw = float2(1f, -1f);
                vertex.position.xz = center;
                vertex.texCoord0 = 0.5f; // centered at 0
                streams.SetVertex(vi + 0, vertex); // set the central vertex.

                vertex.position.z = zCoordinates.x; // 1 is below 0
                vertex.texCoord0.y = 0f; // 1 at the bottom is the lower limit of y
                streams.SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y; // 2 is clockwise left to 1
                vertex.texCoord0 = float2(0.5f - h, 0.25f);  // The second vertex only goes up to 0.5 - h so that the texture is not stretched
                streams.SetVertex(vi + 2, vertex);

                vertex.position.z = zCoordinates.z;
                vertex.texCoord0.y = 0.75f;
                streams.SetVertex(vi + 3, vertex);
                
                vertex.position.x = center.x;
                vertex.position.z = zCoordinates.w;
                vertex.texCoord0 = float2(0.5f, 1f);
                streams.SetVertex(vi + 4, vertex);
                
                vertex.position.x = xCoordinates.y;
                vertex.position.z = zCoordinates.z;
                vertex.texCoord0 = float2(0.5f + h, 0.75f);
                streams.SetVertex(vi + 5, vertex);                

                vertex.position.z = zCoordinates.y;
                vertex.texCoord0.y = 0.25f;
                streams.SetVertex(vi + 6, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));
            }
        }
    }

}
