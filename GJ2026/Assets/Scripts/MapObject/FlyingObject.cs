using UnityEngine;

public class FlyingObject : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _anim.SetTrigger("Fly");
        }
    }
}
