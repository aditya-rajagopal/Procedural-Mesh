using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMeshes {

    public delegate JobHandle MeshJobScheduleDelegate (
		Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency
	);

    // Lets create a generic for loop job that uses a generator and a stream
	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct MeshJob<G, S> : IJobFor
		where G : struct, IMeshGenerator
		where S : struct, IMeshStreams {
            G generator;

            [WriteOnly] // We dont read from the streams while generating the mesh so we can force any native arrays within streams to also be write only.
            S streams;

            public void Execute(int i) => generator.Execute(i, streams);

            public static JobHandle ScheduleParallel (
			Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency
            ) {
                var job = new MeshJob<G, S>();
                job.generator.Resolution = resolution;
                job.streams.Setup(
                    meshData,
                    mesh.bounds = job.generator.Bounds,
                    job.generator.VertexCount,
                    job.generator.IndexCount
                );
                return job.ScheduleParallel(
                    job.generator.JobLength, 1, dependency
                );
            }
        }
}