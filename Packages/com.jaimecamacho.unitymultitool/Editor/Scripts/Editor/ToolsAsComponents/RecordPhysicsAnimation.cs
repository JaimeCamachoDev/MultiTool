using UnityEngine;
using UnityEditor.Animations;

public class RecordPhysicsAnimation : MonoBehaviour
{
    public AnimationClip clip;  // El clip de animaci�n donde se guardar�n los datos
    private GameObjectRecorder recorder;

    void Start()
    {
        // Crear el grabador y enlazar todos los componentes Transform del GameObject y sus hijos
        recorder = new GameObjectRecorder(gameObject);

        // Enlazar los componentes Transform del GameObject y todos sus hijos
        recorder.BindComponentsOfType<Transform>(gameObject, true);
    }

    void LateUpdate()
    {
        // Si no hay un clip, no hacemos nada
        if (clip == null)
        {
            Debug.LogWarning("No animation clip assigned!");
            return;
        }

        // Capturar una instant�nea de las transformaciones en este fotograma
        recorder.TakeSnapshot(Time.deltaTime);
    }

    void OnDisable()
    {
        // Si no hay un clip, no hacemos nada
        if (clip == null)
            return;

        // Si el recorder est� grabando, guardamos el resultado en el clip de animaci�n
        if (recorder.isRecording)
        {
            recorder.SaveToClip(clip);
            Debug.Log("Animation saved to clip.");
        }
    }
}
