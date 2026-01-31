using System.Collections;
using UnityEngine;

public class PlayerAfterImageVFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer sourceSprite; // SpriteRenderer ở VisualRoot
    [SerializeField] private AfterImagePool pool;
    [SerializeField] private float minTimeBetween = 0.02f; // 20ms
    [SerializeField] private float minDistanceX = 0.03f;   // lọc theo X nếu bạn hay nhảy đứng yên
    [SerializeField] private float minDistanceY = 0.06f;   // lọc theo Y khi nhảy

    [Header("When to emit")]
    [SerializeField] private bool emitOnJump = true;
    [SerializeField] private bool emitOnDash = false; // để sau cũng được

    [Header("Burst settings")]
    [SerializeField] private int burstCount = 7;          // số bóng
    [SerializeField] private float burstDuration = 0.12f; // tổng thời gian bắn bóng
    [SerializeField] private float minDistance = 0.04f;   // tránh spam khi đứng yên

    [Header("Look")]
    [SerializeField] private int sortingOrderOffset = -1; // bóng phía sau
    [SerializeField] private Color tint = new Color(0.95f, 0.98f, 1f, 1f); // hơi lạnh
    [SerializeField] private bool overrideTint = false;

    private Vector3 lastPos;
    private Coroutine co;

    private void Reset()
    {
        sourceSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void PlayJumpAfterImage()
    {
        if (!emitOnJump) return;
        PlayBurst();
    }

    public void PlayDashAfterImage()
    {
        if (!emitOnDash) return;
        PlayBurst();
    }

    private void PlayBurst()
    {
        if (sourceSprite == null || pool == null) return;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(EmitBurst());
    }

    private IEnumerator EmitBurst()
    {
        lastPos = transform.position;

        int count = Mathf.Max(1, burstCount);
        float dt = (count <= 1) ? 0f : (burstDuration / (count - 1));

        float timeAcc = 0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = transform.position;

            // lọc theo trục: nhảy lên thì Y thay đổi nhiều, đi ngang thì X thay đổi nhiều
            float dx = Mathf.Abs(p.x - lastPos.x);
            float dy = Mathf.Abs(p.y - lastPos.y);

            timeAcc += dt;

            bool movedEnough = (dx >= minDistanceX) || (dy >= minDistanceY);
            bool timeEnough  = (timeAcc >= minTimeBetween);

            if (i == 0 || (movedEnough && timeEnough))
            {
                lastPos = p;
                timeAcc = 0f;

                var ghost = pool.Get();
                ghost.SpawnFrom(
                    sourceSprite,
                    p,
                    sourceSprite.transform.lossyScale,
                    pool.Release,
                    sortingOrderOffset,
                    overrideTint ? tint : (Color?)null
                );
            }

            if (dt > 0f) yield return new WaitForSeconds(dt);
        }

        co = null;
    }

}
