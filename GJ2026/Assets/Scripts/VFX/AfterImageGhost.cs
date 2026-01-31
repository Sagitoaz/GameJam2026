using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AfterImageGhost : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.18f; // thời gian tồn tại
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 0f),    // vừa spawn: alpha 0
        new Keyframe(0.12f, 1f), // 12% lifetime: lên 1
        new Keyframe(1f, 0f)     // cuối: về 0
    );

    [SerializeField] private Sprite overrideSprite; // sprite blur nếu muốn

    private SpriteRenderer sr;
    private float t;
    private Color baseColor;
    private System.Action<AfterImageGhost> release;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SpawnFrom(SpriteRenderer source, Vector3 position, Vector3 scale,
                          System.Action<AfterImageGhost> releaseToPool,
                          int orderOffset = -1, Color? tintOverride = null)
    {
        sr.sprite = (overrideSprite != null) ? overrideSprite : source.sprite;
        release = releaseToPool;

        transform.position = position;
        transform.localScale = scale;

        // copy sprite state
        sr.sprite = source.sprite;
        sr.flipX = source.flipX;
        sr.flipY = source.flipY;

        // copy sorting
        sr.sortingLayerID = source.sortingLayerID;
        sr.sortingOrder = source.sortingOrder + orderOffset;

        // color
        baseColor = tintOverride ?? source.color;
        baseColor.a = 1f;
        sr.color = baseColor;

        t = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / lifeTime);

        Color c = baseColor;
        c.a = alphaCurve.Evaluate(p);
        sr.color = c;

        if (p >= 1f)
        {
            gameObject.SetActive(false);
            release?.Invoke(this);
        }
    }
}
