using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

    public enum GeneratorType {SquareGrid, SharedSquareGrid, SharedTriangleGrid, PointyHexagonGrid, FlatHexagonGrid};

    // static MeshJobScheduleDelegate[,] jobs = {
    //     {
    //         MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
    //         MeshJob<SquareGrid, MultiStream>.ScheduleParallel
    //     },
    //     {
    //         MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
    //         MeshJob<SharedSquareGrid, MultiStream>.ScheduleParallel
    //     }
    // };

    static MeshJobScheduleDelegate[] jobs = {
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
        MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel
	};

    [SerializeField, Range(1, 50)]
    int resolution = 1;

    [SerializeField]
    GeneratorType generatorType;

    Mesh mesh;

    private void Awake() {
        mesh = new Mesh {
            name = "Procedural Mesh"
        };

        // GenerateMesh();
        // We want to constantly generate the new mesh. In case we change the resolution
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void GenerateMesh() {
       Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1); // we only want 1 mesh
       Mesh.MeshData meshData = meshDataArray[0];
    //    int rowsOrHeight = jobs.GetLength(0);
    //     int colsOrWidth = jobs.GetLength(1);
    //     Debug.Log("ROws: " + rowsOrHeight);
    //     Debug.Log("Cols: " + colsOrWidth);
       jobs[(int)generatorType](mesh, meshData, resolution, default).Complete(); // default job handel and then when the job handel returns we wait for it to complete.

       Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    void OnValidate () => enabled = true;

	void Update () {
		GenerateMesh();
        // Debug.Log((int)generatorType);
		enabled = false;
	}

}