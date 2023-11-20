using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using System.IO;
using System;
using Bercetech.Games.Fleepas.BinarySerialization;
using SevenZip.Compression.LZMA;


namespace Bercetech.Games.Fleepas
{
    public class MeshBuilder : MonoBehaviour
    {
        [SerializeField]
        private GameObject _meshPrefab;
        // Function to serialize mesh data
        public IEnumerator DecomposeARMesh(GameObject arMesh, Action<byte[]> GetSerializedMesh)
        {

            Vector3[] mVertices = new Vector3[0];
            Vector3[] mNormals = new Vector3[0];
            Vector2[] mUV = new Vector2[0];
            Vector2[] mUV2 = new Vector2[0];
            int[] mTriangles = new int[0];
            int[] verticesCount = new int[arMesh.transform.childCount];
            int[] uvCount = new int[arMesh.transform.childCount];
            int[] uv2Count = new int[arMesh.transform.childCount];
            int[] trianglesCount = new int[arMesh.transform.childCount];
            int i = 0;
            // Getting vertices, normals, uv and triangles from each chunk of the armesh
            foreach (Transform child in arMesh.transform)
            {

                var childSharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
                Vector3[] mVerticesChunk = childSharedMesh.vertices;
                Vector3[] mNormalsChunk = childSharedMesh.normals;
                Vector2[] mUVChunk = childSharedMesh.uv;
                Vector2[] mUV2Chunk = childSharedMesh.uv2;
                int[] mTrianglesChunk = childSharedMesh.triangles;
                // To be able to rebuid it, need to count how many vert, normals, uv, triang has each chunk, because it varies
                verticesCount[i] = mVerticesChunk.Length; // Same value for normals
                uvCount[i] = mUVChunk.Length;
                uv2Count[i] = mUV2Chunk.Length;
                trianglesCount[i] = mTrianglesChunk.Length;

                // Creating a big array with all vert, normals, uv and triang data for all the chunks
                mVertices = mVertices.Concat(mVerticesChunk).ToArray();
                mNormals = mNormals.Concat(mNormalsChunk).ToArray();
                mUV = mUV.Concat(mUVChunk).ToArray();
                mUV2 = mUV2.Concat(mUV2Chunk).ToArray();
                mTriangles = mTriangles.Concat(mTrianglesChunk).ToArray();
                i++;
                // Yield return null/WaitForSeconds line is the point where execution pauses and resumes in the following frame/seconds.
                yield return null;
            }
            // Calling serialization function
            GetSerializedMesh(SerializeARMesh(mVertices, mNormals, mUV, mUV2, mTriangles, verticesCount, uvCount, uv2Count, trianglesCount));

        }


        public IEnumerator RebuildMesh(byte[] serializedMesh, GameObject arMesh, Action<bool> BuildSuccess)
        {

            Logging.Omigari("Instantiating Host Mesh");
            (Vector3[], Vector3[], Vector2[], Vector2[], int[], int[], int[], int[], int[]) mesh = new();
            try
            {
                mesh = DeserializeARMesh(serializedMesh);
            } catch
            {
                Logging.Omigari("Host Mesh couldn't be instantiated");
                BuildSuccess(false);
            }

            Vector3[] mVertices = mesh.Item1;
            Vector3[] mNormals = mesh.Item2;
            Vector2[] mUV = mesh.Item3;
            Vector2[] mUV2 = mesh.Item4;
            int[] mTriangles = mesh.Item5;
            int[] verticesCount = mesh.Item6;
            int[] uvCount = mesh.Item7;
            int[] uv2Count = mesh.Item8;
            int[] trianglesCount = mesh.Item9;

            int verticesCountSkip = 0;
            int uvCountSkip = 0;
            int uv2CountSkip = 0;
            int trianglesCountSkip = 0;
            // Recreating chunk by chunk
            for (int j = 0; j < verticesCount.Length; j++)
            {

                Mesh meshChunk = new Mesh();
                // Need to copy the meshPrefab instead of using it directly, to keep it empty of mesh data.
                // Otherwsie, if we use Application.Unload to get out of Unity,
                // after consecutive fleep loads, old meshes reappear inmediately when meshing is reenabled.
                // No idea why (some kind of memory leak?)
                var meshPrefabCopy = Instantiate(_meshPrefab);
                meshPrefabCopy.GetComponent<MeshFilter>().mesh = meshChunk;
                meshChunk.vertices = mVertices.Skip(verticesCountSkip).Take(verticesCount[j]).ToArray();
                meshChunk.normals= mNormals.Skip(verticesCountSkip).Take(verticesCount[j]).ToArray();
                meshChunk.uv = mUV.Skip(uvCountSkip).Take(uvCount[j]).ToArray();
                meshChunk.uv2 = mUV2.Skip(uv2CountSkip).Take(uv2Count[j]).ToArray();
                meshChunk.triangles = mTriangles.Skip(trianglesCountSkip).Take(trianglesCount[j]).ToArray();


                verticesCountSkip += verticesCount[j];
                uvCountSkip += uvCount[j];
                uv2CountSkip += uv2Count[j];
                trianglesCountSkip += trianglesCount[j];

                meshPrefabCopy.GetComponent<MeshCollider>().sharedMesh = meshChunk;
                // Instantiating the Chunks in ARmesh
                Instantiate(meshPrefabCopy, arMesh.transform);
                // Yield return null/WaitForSeconds line is the point where execution pauses and resumes in the following frame/seconds.
                yield return null;
                Destroy(meshPrefabCopy);
            }
            BuildSuccess(true);
        }

        private byte[] SerializeARMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uv, Vector2[] uv2, int[] triangles, int[] verticesCount, int[] uvCount, int[] uv2Count, int[] trianglesCount)
        {
            using (var stream = new MemoryStream())
            {
                using (var serializer = new BinarySerializer(stream))
                {

                    serializer.Serialize(verticesCount.Length); // Chunkscount
                    serializer.Serialize(vertices.Length);
                    serializer.Serialize(normals.Length);
                    serializer.Serialize(uv.Length);
                    serializer.Serialize(uv2.Length);
                    serializer.Serialize(triangles.Length);
                    foreach (var v3 in vertices.ToList())
                    {
                        serializer.Serialize(new Vector3((float)Math.Round(v3.x, 3), (float)Math.Round(v3.y, 3), (float)Math.Round(v3.z, 3))); // Rounding to mm to reduce file size
                    }
                    foreach (var v3 in normals.ToList())
                    {
                        serializer.Serialize(new Vector3((float)Math.Round(v3.x, 3), (float)Math.Round(v3.y, 3), (float)Math.Round(v3.z, 3))); // Rounding to mm to reduce file size
                    }
                    foreach (var v2 in uv.ToList())
                    {
                        serializer.Serialize(new Vector2((float)Math.Round(v2.x, 3), (float)Math.Round(v2.y, 3))); // Rounding to mm to reduce file size
                    }
                    foreach (var v2 in uv2.ToList())
                    {
                        serializer.Serialize(new Vector2((float)Math.Round(v2.x, 3), (float)Math.Round(v2.y, 3))); // Rounding to mm to reduce file size
                    }
                    foreach (var i in triangles.ToList())
                    {
                        serializer.Serialize(i);
                    }
                    foreach (var i in verticesCount.ToList())
                    {
                        serializer.Serialize(i);
                    }
                    foreach (var i in uvCount.ToList())
                    {
                        serializer.Serialize(i);
                    }
                    foreach (var i in uv2Count.ToList())
                    {
                        serializer.Serialize(i);
                    }
                    foreach (var i in trianglesCount.ToList())
                    {
                        serializer.Serialize(i);
                    }
                    return stream.ToArray();
                }
            }
        }


        private (Vector3[], Vector3[], Vector2[], Vector2[], int[], int[], int[], int[], int[]) DeserializeARMesh(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var deserializer = new BinaryDeserializer(stream))
                {
                    int chunksCount = (int)deserializer.Deserialize();
                    int verticesLength = (int)deserializer.Deserialize();
                    int normalsLength = (int)deserializer.Deserialize();
                    int uvLength = (int)deserializer.Deserialize();
                    int uv2Length = (int)deserializer.Deserialize();
                    int trianglesLength = (int)deserializer.Deserialize();

                    (Vector3[], Vector3[], Vector2[], Vector2[], int[], int[], int[], int[], int[]) result =
                        (new Vector3[verticesLength], new Vector3[normalsLength], new Vector2[uvLength], new Vector2[uv2Length], new int[trianglesLength], new int[chunksCount], new int[chunksCount], new int[chunksCount], new int[chunksCount]);
                    for (int i = 0; i < verticesLength; i++)
                        result.Item1[i] = (Vector3)deserializer.Deserialize(); // Vertices
                    for (int i = 0; i < normalsLength; i++)
                        result.Item2[i] = (Vector3)deserializer.Deserialize(); // Normals
                    for (int i = 0; i < uvLength; i++)
                        result.Item3[i] = (Vector2)deserializer.Deserialize(); // UV
                    for (int i = 0; i < uv2Length; i++)
                        result.Item4[i] = (Vector2)deserializer.Deserialize(); // UV2
                    for (int i = 0; i < trianglesLength; i++)
                        result.Item5[i] = (int)deserializer.Deserialize(); // Triangles
                    for (int i = 0; i < chunksCount; i++)
                        result.Item6[i] = (int)deserializer.Deserialize(); // vertices-normals count
                    for (int i = 0; i < chunksCount; i++)
                        result.Item7[i] = (int)deserializer.Deserialize(); // uv count
                    for (int i = 0; i < chunksCount; i++)
                        result.Item8[i] = (int)deserializer.Deserialize(); // uv2 count
                    for (int i = 0; i < chunksCount; i++)
                        result.Item9[i] = (int)deserializer.Deserialize(); // triangles count
                    return result;
                }
            }
        }

        public void RebuildMeshFromTxtFile(string filePath)
        {
            // The filte path starts from Assets\Resources. For instance Assets\Resources\mesh.txt
            // would have a filepath of simply "mesh", without the file extension
            var text = Resources.Load<TextAsset>(filePath);
            byte[] serializedMesh = text.bytes;
            byte[] deSerializedMesh = SevenZipHelper.Decompress(serializedMesh);
            // Rebuilding the mesh
            StartCoroutine(RebuildMesh(deSerializedMesh, this.gameObject, _ => { }));

        }


    }
}
