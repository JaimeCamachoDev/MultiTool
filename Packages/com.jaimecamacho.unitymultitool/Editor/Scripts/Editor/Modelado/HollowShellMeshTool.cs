using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OptiZone
{
    public static class HollowShellMeshTool
    {
        private static List<GameObject> gameObjectsToModify = new List<GameObject>(); // Lista de GameObjects para modificar
        private static Transform clippingPlane; // El plano de recorte
        private enum ClipDirection { Below, Above } // Direcciones de recorte
        private static ClipDirection clipDirection = ClipDirection.Below; // Opci�n seleccionada del dropdown

        private static Dictionary<GameObject, Mesh> originalMeshes = new Dictionary<GameObject, Mesh>(); // Diccionario para almacenar las mallas originales antes de la vista previa

        public static void DrawTool()
        {
            GUILayout.Label("1. Drag objects with MeshRenderer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Drag the objects here to modify their meshes.", MessageType.Info);

            // Mostrar los GameObjects arrastrados
            for (int i = 0; i < gameObjectsToModify.Count; i++)
            {
                gameObjectsToModify[i] = (GameObject)EditorGUILayout.ObjectField($"Object {i + 1}", gameObjectsToModify[i], typeof(GameObject), true);
            }

            // Bot�n para agregar m�s objetos a la lista
            if (GUILayout.Button("Add object"))
            {
                gameObjectsToModify.Add(null);
            }

            // Bot�n para eliminar el �ltimo objeto de la lista
            if (GUILayout.Button("Remove last object"))
            {
                if (gameObjectsToModify.Count > 0)
                {
                    gameObjectsToModify.RemoveAt(gameObjectsToModify.Count - 1);
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("2. Drag the Clipping Plane", EditorStyles.boldLabel);
            clippingPlane = (Transform)EditorGUILayout.ObjectField("Clipping Plane", clippingPlane, typeof(Transform), true);

            GUILayout.Space(10);

            GUILayout.Label("3. Select clip direction", EditorStyles.boldLabel);
            clipDirection = (ClipDirection)EditorGUILayout.EnumPopup("Clip Direction", clipDirection);

            GUILayout.Space(20);

            // Bot�n para previsualizar el recorte
            if (GUILayout.Button("Preview clip"))
            {
                PreviewMeshModification();
            }

            // Bot�n para deshacer la vista previa y restaurar las mallas originales
            if (GUILayout.Button("Undo preview"))
            {
                RestoreOriginalMeshes();
            }

            GUILayout.Space(20);

            // Bot�n para guardar los cambios y reemplazar las mallas originales
            if (GUILayout.Button("Save changes"))
            {
                SaveModifiedMeshes();
            }
        }

        private static void PreviewMeshModification()
        {
            if (clippingPlane == null)
            {
                Debug.LogError("Clipping Plane not assigned.");
                return;
            }

            // Almacenamos las mallas originales solo la primera vez
            if (originalMeshes.Count == 0)
            {
                foreach (var obj in gameObjectsToModify)
                {
                    if (obj == null) continue;

                    var meshFilter = obj.GetComponent<MeshFilter>();
                    var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();

                    if (meshFilter != null && meshFilter.sharedMesh != null && !originalMeshes.ContainsKey(obj))
                    {
                        originalMeshes.Add(obj, meshFilter.sharedMesh);
                    }
                    else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && !originalMeshes.ContainsKey(obj))
                    {
                        originalMeshes.Add(obj, skinnedMeshRenderer.sharedMesh);
                    }
                }
            }

            // Aplicamos el recorte basado en la direcci�n seleccionada
            foreach (var obj in gameObjectsToModify)
            {
                if (obj == null) continue;

                var meshFilter = obj.GetComponent<MeshFilter>();
                var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();

                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Mesh modifiedMesh = CreateClippedMesh(meshFilter.sharedMesh, clippingPlane, clipDirection, obj.transform);
                    meshFilter.sharedMesh = modifiedMesh;
                }
                else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    Mesh modifiedMesh = CreateClippedMesh(skinnedMeshRenderer.sharedMesh, clippingPlane, clipDirection, obj.transform);
                    skinnedMeshRenderer.sharedMesh = modifiedMesh;
                }
            }
        }

        private static void RestoreOriginalMeshes()
        {
            // Restauramos las mallas originales desde el diccionario
            foreach (var entry in originalMeshes)
            {
                var obj = entry.Key;
                var meshFilter = obj.GetComponent<MeshFilter>();
                var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();

                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = entry.Value;
                }
                else if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.sharedMesh = entry.Value;
                }
            }
        }

        private static void SaveModifiedMeshes()
        {
            foreach (var obj in gameObjectsToModify)
            {
                if (obj == null) continue;

                var meshFilter = obj.GetComponent<MeshFilter>();
                var skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();

                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    SaveMesh(meshFilter.sharedMesh, obj);
                }
                else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    SaveMesh(skinnedMeshRenderer.sharedMesh, obj);
                }
            }
        }

        private static void SaveMesh(Mesh modifiedMesh, GameObject obj)
        {
            // Obtener la ruta de la malla original
            string originalMeshPath = AssetDatabase.GetAssetPath(originalMeshes[obj]);
            string directory = Path.GetDirectoryName(originalMeshPath);
            string newMeshName = modifiedMesh.name + "_HollowShell.asset";
            newMeshName = newMeshName.Replace("(Clone)", "");
            string newPath = Path.Combine(directory, newMeshName);

            // Guardar la nueva malla
            AssetDatabase.CreateAsset(modifiedMesh, newPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Hollow shell saved: {newPath}");
        }

        private static Mesh CreateClippedMesh(Mesh originalMesh, Transform plane, ClipDirection direction, Transform meshTransform)
        {
            // Creamos una copia de la malla original
            Mesh newMesh = Object.Instantiate(originalMesh);

            // Obtenemos los v�rtices y tri�ngulos originales
            Vector3[] vertices = newMesh.vertices;
            Vector2[] uvs = newMesh.uv;
            Vector3[] normals = newMesh.normals;
            Vector4[] tangents = newMesh.tangents;
            List<int> newTriangles = new List<int>(newMesh.triangles);

            // Obtener la posici�n y normal del plano
            Vector3 planePos = plane.position;
            Vector3 planeNormal = plane.up;

            // Transformaci�n inversa para llevar los v�rtices al espacio local del plano
            Matrix4x4 localToWorld = meshTransform.localToWorldMatrix;

            // Lista para almacenar los v�rtices que se mantienen
            HashSet<int> verticesToKeep = new HashSet<int>();

            // Filtramos los v�rtices y eliminamos los que est�n en la direcci�n seleccionada
            for (int i = newTriangles.Count - 1; i >= 0; i -= 3)
            {
                // Obtenemos los �ndices de los tres v�rtices del tri�ngulo actual
                int index0 = newTriangles[i];
                int index1 = newTriangles[i - 1];
                int index2 = newTriangles[i - 2];

                // Convertir v�rtices al espacio local del mundo
                Vector3 worldPos0 = localToWorld.MultiplyPoint3x4(vertices[index0]);
                Vector3 worldPos1 = localToWorld.MultiplyPoint3x4(vertices[index1]);
                Vector3 worldPos2 = localToWorld.MultiplyPoint3x4(vertices[index2]);

                // Chequeamos si los v�rtices deben eliminarse solo si los tres est�n por debajo del plano
                bool remove = ShouldRemoveVertex(worldPos0, planePos, planeNormal, direction) &&
                              ShouldRemoveVertex(worldPos1, planePos, planeNormal, direction) &&
                              ShouldRemoveVertex(worldPos2, planePos, planeNormal, direction);

                // Si todos los v�rtices est�n por debajo del plano, eliminamos el tri�ngulo
                if (remove)
                {
                    newTriangles.RemoveAt(i);
                    newTriangles.RemoveAt(i - 1);
                    newTriangles.RemoveAt(i - 2);
                }
                else
                {
                    // Si no se elimina, agregamos estos v�rtices a la lista de v�rtices que se mantienen
                    verticesToKeep.Add(index0);
                    verticesToKeep.Add(index1);
                    verticesToKeep.Add(index2);
                }
            }

            // Crear nuevas listas de v�rtices y datos asociados que solo contengan los v�rtices que se utilizan
            List<Vector3> optimizedVertices = new List<Vector3>();
            List<Vector2> optimizedUVs = new List<Vector2>();
            List<Vector3> optimizedNormals = new List<Vector3>();
            List<Vector4> optimizedTangents = new List<Vector4>();
            Dictionary<int, int> oldToNewIndexMap = new Dictionary<int, int>();

            // Crear nuevos �ndices de tri�ngulo ajustados a los nuevos v�rtices
            List<int> optimizedTriangles = new List<int>();

            int newIndex = 0;
            foreach (int oldIndex in verticesToKeep)
            {
                // Copiamos los datos de los v�rtices, uvs, normales y tangentes de los v�rtices que se mantienen
                optimizedVertices.Add(vertices[oldIndex]);
                if (uvs.Length > 0) optimizedUVs.Add(uvs[oldIndex]);
                if (normals.Length > 0) optimizedNormals.Add(normals[oldIndex]);
                if (tangents.Length > 0) optimizedTangents.Add(tangents[oldIndex]);

                // Mapear los �ndices antiguos a los nuevos
                oldToNewIndexMap[oldIndex] = newIndex;
                newIndex++;
            }

            // Rehacer los tri�ngulos utilizando el nuevo mapeo de �ndices
            for (int i = 0; i < newTriangles.Count; i++)
            {
                optimizedTriangles.Add(oldToNewIndexMap[newTriangles[i]]);
            }

            // Asignar los nuevos v�rtices, tri�ngulos, UVs, etc. a la nueva malla
            newMesh.Clear();
            newMesh.vertices = optimizedVertices.ToArray();
            newMesh.triangles = optimizedTriangles.ToArray();
            if (optimizedUVs.Count > 0) newMesh.uv = optimizedUVs.ToArray();
            if (optimizedNormals.Count > 0) newMesh.normals = optimizedNormals.ToArray();
            if (optimizedTangents.Count > 0) newMesh.tangents = optimizedTangents.ToArray();

            // Recalcular las propiedades finales de la malla
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            return newMesh;
        }

        private static bool ShouldRemoveVertex(Vector3 vertex, Vector3 planePos, Vector3 planeNormal, ClipDirection direction)
        {
            // Proyectamos la posici�n del v�rtice en el plano y determinamos si eliminarlo en funci�n de la direcci�n de recorte
            Vector3 relativePos = vertex - planePos;

            switch (direction)
            {
                case ClipDirection.Below:
                    return Vector3.Dot(relativePos, planeNormal) < 0;
                case ClipDirection.Above:
                    return Vector3.Dot(relativePos, planeNormal) > 0;
                default:
                    return false;
            }
        }
    }
}
