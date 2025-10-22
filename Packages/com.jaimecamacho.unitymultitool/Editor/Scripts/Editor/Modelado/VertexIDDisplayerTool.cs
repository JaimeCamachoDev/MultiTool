using UnityEngine;
using UnityEditor;

namespace VZ_Optizone
{
    public static class VertexIDDisplayerTool
    {
        // Variables para almacenar el objeto seleccionado, la malla, y la posici�n del v�rtice
        private static GameObject selectedObject;
        private static Mesh selectedMesh;
        private static Vector3[] vertices;
        private static int vertexID = -1;
        private static Vector3 vertexWorldPosition;

        // M�todo para dibujar la herramienta en el Editor
        public static void DrawTool()
        {
            GUILayout.Label("VZ Optizone - Vertex ID Displayer", EditorStyles.boldLabel);

            // Campo para seleccionar el GameObject
            selectedObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedObject, typeof(GameObject), true);

            // Si se selecciona un objeto, obtener el MeshFilter
            if (selectedObject != null)
            {
                MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    selectedMesh = meshFilter.sharedMesh;
                }
                else
                {
                    selectedMesh = null;
                }

                if (selectedMesh != null)
                {
                    vertices = selectedMesh.vertices;

                    // Campo para ingresar el ID del v�rtice
                    GUILayout.Label("Enter Vertex ID to Display:");
                    vertexID = EditorGUILayout.IntField(vertexID);

                    // Bot�n para mostrar el v�rtice seleccionado
                    if (GUILayout.Button("Display Vertex ID"))
                    {
                        DisplayVertexID();
                    }

                    GUILayout.Space(10);

                    // Mostrar las coordenadas del v�rtice en el mundo
                    if (vertexID >= 0 && vertexID < vertices.Length)
                    {
                        GUILayout.Label($"Vertex {vertexID} World Position:");
                        GUILayout.Label($"X: {vertexWorldPosition.x}");
                        GUILayout.Label($"Y: {vertexWorldPosition.y}");
                        GUILayout.Label($"Z: {vertexWorldPosition.z}");
                    }
                }
                else
                {
                    GUILayout.Label("Selected GameObject does not have a MeshFilter with a mesh.", EditorStyles.helpBox);
                }
            }
        }

        // M�todo para mostrar la informaci�n del v�rtice seleccionado
        private static void DisplayVertexID()
        {
            if (vertexID >= 0 && vertexID < vertices.Length)
            {
                Transform objectTransform = selectedObject.transform;
                vertexWorldPosition = objectTransform.TransformPoint(vertices[vertexID]);
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogError("Vertex ID out of range.");
            }
        }

        // M�todo para manejar la escena y dibujar el v�rtice
        public static void OnSceneGUI(SceneView sceneView)
        {
            if (selectedMesh == null || vertices == null || vertexID < 0 || vertexID >= vertices.Length)
            {
                return;
            }

            Handles.color = Color.green;

            // Dibujar el v�rtice especificado por el ID
            Handles.Label(vertexWorldPosition, vertexID.ToString());
            Handles.SphereHandleCap(0, vertexWorldPosition, Quaternion.identity, 0.05f, EventType.Repaint);
        }

        // M�todos para gestionar el evento de la escena
        public static void EnableSceneView()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public static void DisableSceneView()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}
