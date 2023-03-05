using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

    public enum GeneratorType {SquareGrid, SharedSquareGrid, SharedTriangleGrid, PointyHexagonGrid, FlatHexagonGrid};

    [System.Flags]  // by setting system.Flags we ask the following enum to be interpreted as boolean flags. Then we use 1 bit for each flag
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangets = 0b100}

    [SerializeField]
    GizmoMode gizmos;

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

    Vector3[] vertices, normals;

    Vector4[] tangents;

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

       jobs[(int)generatorType](mesh, meshData, resolution, default).Complete(); // default job handel and then when the job handel returns we wait for it to complete.

       Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    void OnValidate () => enabled = true;

	void Update () {
		GenerateMesh();
        // Debug.Log((int)generatorType);
		enabled = false;

        vertices = null;
        normals = null;
        tangents = null;
	}

    void OnDrawGizmos()
    {
        if (gizmos == GizmoMode.Nothing || mesh == null){
            return;
        }

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangets = (gizmos & GizmoMode.Tangets) != 0;

        if (vertices == null) {
            vertices = mesh.vertices;
        }

        if (drawNormals && normals == null) {
            normals = mesh.normals;
        }

        if (drawTangets && tangents == null) {
            tangents = mesh.tangents;
        }

        Transform t = transform;
        for (int i =0; i < vertices.Length; i++) {
            Vector3 position = t.TransformPoint(vertices[i]);

            if (drawVertices) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.05f / resolution);
            }
            
            if (drawNormals) {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformDirection(normals[i]) * 0.5f / resolution);
            }

            if (drawTangets) {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformDirection(tangents[i]) * 0.5f / resolution);
            }
        }
    }

}