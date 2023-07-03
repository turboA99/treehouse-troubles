using UnityEngine;
using UnityEngine.Events;

public class InGameEvent : MonoBehaviour
{
    [SerializeField] private bool allowMultipleActivations;
    [SerializeField] private UnityEvent onTriggered;
    private bool activated = false;
    private void OnTriggerEnter(Collider other)
    {
        Trigger();
    }
    public void Trigger()
    {
        if (!activated || allowMultipleActivations)
        {
            onTriggered.Invoke();
            activated = true;
        }
    }
}
