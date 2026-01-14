using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

class PlayerMergeController : MonoBehaviour
{
    [SerializeField] private Player _player1;
    [SerializeField] private Player _player2;
    [SerializeField] private Player _player3;
    [SerializeField] private float _mergeSpeed;

    private BoxCollider2D col1;
    private Rigidbody2D rb1, rb2;
    private float halfWidth1;

    private void Awake()
    {
        col1 = _player1.GetComponent<BoxCollider2D>();
        rb1 = _player1.GetComponent<Rigidbody2D>();
        rb2 = _player2.GetComponent<Rigidbody2D>();
        halfWidth1 = col1.size.x * 0.5f;
    }
    private bool _isMerge;
    public void OnMerge(InputAction.CallbackContext context)
    {
        if (!context.performed || _isMerge) return;

        _isMerge = true;
        StartCoroutine(MergeRoutine());
    }

    IEnumerator MergeRoutine()
    {
        Vector3 midpoint = (_player1.transform.position + _player2.transform.position) / 2f;
        rb1.gravityScale = 0;
        rb2.gravityScale = 0;
        while (Vector2.Distance(_player1.transform.position, midpoint) > halfWidth1)
        {
            _player1.transform.position = Vector3.Lerp(_player1.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            _player2.transform.position = Vector3.Lerp(_player2.transform.position, midpoint, _mergeSpeed * Time.deltaTime);
            yield return null;
        }
        _player2.SetMerged();
        _player1.SetMerged();
        _player3.gameObject.SetActive(true);
        _player3.transform.position = midpoint;
    }
}