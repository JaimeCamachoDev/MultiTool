using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VZ_Optizone
{
    public static class UVAdjusterToolOpti
    {
        // Variables est�ticas para las filas y columnas
        private static int rows = 1;
        private static int columns = 1;

        // Variables est�ticas para la posici�n en la cuadr�cula
        private static int gridX = 0;
        private static int gridY = 0;

        // Lista est�tica para almacenar los Mesh Filters seleccionados
        private static List<MeshFilter> selectedMeshFilters = new List<MeshFilter>();

        // Diccionario para almacenar las UV originales antes de cualquier modificaci�n
        private static Dictionary<MeshFilter, Vector2[]> originalUVs = new Dictionary<MeshFilter, Vector2[]>();

        // M�todo que dibuja la herramienta en el editor
        public static void DrawTool()
        {
            GUILayout.Label("VZ Optizone UV Adjuster Tool", EditorStyles.boldLabel);

            // Input para las filas y columnas
            rows = EditorGUILayout.IntField("Rows", rows);
            columns = EditorGUILayout.IntField("Columns", columns);

            // Input para la posici�n en la cuadr�cula
            gridX = EditorGUILayout.IntField("Grid X", gridX);
            gridY = EditorGUILayout.IntField("Grid Y", gridY);

            // Mostrar lista de Mesh Filters seleccionados
            GUILayout.Label("Select Mesh Filters", EditorStyles.label);

            // Bot�n para agregar un nuevo Mesh Filter
            if (GUILayout.Button("Add Mesh Filter"))
            {
                selectedMeshFilters.Add(null);
            }

            // Mostrar todos los Mesh Filters con un bot�n de eliminar
            for (int i = 0; i < selectedMeshFilters.Count; i++)
            {
                GUILayout.BeginHorizontal();

                // Campo para seleccionar un Mesh Filter
                selectedMeshFilters[i] = (MeshFilter)EditorGUILayout.ObjectField(selectedMeshFilters[i], typeof(MeshFilter), true);

                // Bot�n para eliminar el Mesh Filter de la lista
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    selectedMeshFilters.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }

            // Bot�n para ajustar UVs
            if (GUILayout.Button("Adjust UVs"))
            {
                AdjustUVs();
            }

            // Bot�n para deshacer los cambios
            if (GUILayout.Button("Undo last change"))
            {
                UndoUVChanges();
            }
        }

        // M�todo para ajustar las UVs de las mallas seleccionadas
        private static void AdjustUVs()
        {
            if (selectedMeshFilters.Count == 0)
            {
                Debug.LogError("No Mesh Filters!");
                return;
            }

            if (rows <= 0 || columns <= 0)
            {
                Debug.LogError("Rows and colums must be greater than 0.");
                return;
            }

            foreach (var meshFilter in selectedMeshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                // Obtener la malla
                Mesh mesh = meshFilter.sharedMesh;
                Vector2[] uvs = mesh.uv;

                // Almacenar las UV originales antes de modificarlas
                if (!originalUVs.ContainsKey(meshFilter))
                {
                    originalUVs[meshFilter] = (Vector2[])uvs.Clone();
                }

                // Calcular el tama�o del cuadrado de UV
                float uvWidth = 1.0f / columns;
                float uvHeight = 1.0f / rows;

                // Calcular el offset basado en la posici�n de la cuadr�cula
                Vector2 offset = new Vector2(gridX * uvWidth, gridY * uvHeight);

                // Ajustar las UVs
                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = new Vector2(
                        uvs[i].x * uvWidth + offset.x,
                        uvs[i].y * uvHeight + offset.y
                    );
                }

                // Asignar las nuevas UVs a la malla
                mesh.uv = uvs;

                // Marcar la malla como modificada y guardar los cambios
                EditorUtility.SetDirty(mesh);
                string path = AssetDatabase.GetAssetPath(mesh);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("UVs adjusted and saved: " + mesh.name);
                }
                else
                {
                    Debug.LogError("Error saving.");
                }
            }
        }

        // M�todo para deshacer los cambios en las UVs
        private static void UndoUVChanges()
        {
            foreach (var meshFilter in selectedMeshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null || !originalUVs.ContainsKey(meshFilter))
                    continue;

                // Obtener la malla y las UV originales
                Mesh mesh = meshFilter.sharedMesh;
                mesh.uv = originalUVs[meshFilter];

                // Marcar la malla como modificada y guardar los cambios
                EditorUtility.SetDirty(mesh);
                string path = AssetDatabase.GetAssetPath(mesh);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("UVs restored: " + mesh.name);
                }
                else
                {
                    Debug.LogError("Error restoring.");
                }
            }
        }
    }
}
