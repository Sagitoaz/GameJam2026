using System.Collections;
using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public GameObject target;
    public float interval = 3f;
    public bool useSetActive = false; // if true, toggle GameObject.SetActive; otherwise toggle Renderers
    public bool startHidden = false;

    Renderer[] renderers;

    void Awake()
    {
        if (target == null) target = gameObject;
        if (!useSetActive) renderers = target.GetComponentsInChildren<Renderer>(true);
        if (startHidden)
        {
            if (useSetActive) target.SetActive(false);
            else
            {
                foreach (var r in renderers) r.enabled = false;
            }
        }
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (useSetActive)
            {
                target.SetActive(!target.activeSelf);
            }
            else
            {
                bool currentlyEnabled = (renderers != null && renderers.Length > 0) ? renderers[0].enabled : true;
                foreach (var r in renderers) r.enabled = !currentlyEnabled;
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
