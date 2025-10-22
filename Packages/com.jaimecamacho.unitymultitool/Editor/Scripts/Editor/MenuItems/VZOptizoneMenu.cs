using UnityEditor;
using UnityEngine;

public static class VZOptizoneMenu
{
    [MenuItem("VZ Optizone/Crear Estructura de Objetos")]
    private static void CrearEstructuraDeObjetos()
    {
        // Crear el objeto ra�z "Enviroment"
        GameObject enviroment = new GameObject("Enviroment");
        // Crear hijos de "Enviroment"
        new GameObject("Static").transform.parent = enviroment.transform;
        new GameObject("Dynamic").transform.parent = enviroment.transform;
        new GameObject("InteractiveObjects").transform.parent = enviroment.transform;

        // Crear el objeto ra�z "Characters"
        GameObject characters = new GameObject("Characters");
        // Crear hijos de "Characters"
        new GameObject("NPCs").transform.parent = characters.transform;
        new GameObject("Animals").transform.parent = characters.transform;

        // Crear el objeto ra�z "UI"
        new GameObject("UI");

        // Crear el objeto ra�z "DELETE"
        new GameObject("DELETE");

        // Crear el objeto ra�z "OtherProgramingStuff"
        new GameObject("OtherProgramingStuff");

        // Seleccionar el primer objeto creado en la jerarqu�a
        Selection.activeGameObject = enviroment;
    }
}
