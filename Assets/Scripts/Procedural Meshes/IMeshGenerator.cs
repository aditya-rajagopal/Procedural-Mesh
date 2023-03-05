using UnityEngine;

namespace ProceduralMeshes {

    public interface IMeshGenerator {
        int VertexCount { get; }
		
		int IndexCount { get; }
        int JobLength { get; }
        Bounds Bounds { get; }

        // We want our generator to be able to a grid of RxR squares.
        int Resolution {get; set;}

        // We want to generate a mesh and we can define an execute function taht is run by the job.
        // This means it needs to be Execute and needs an index parameter. The second parameter is the stream
        // type used for storage of data.
        void Execute<S> (int i, S streams) where S: struct, IMeshStreams;
    }

}