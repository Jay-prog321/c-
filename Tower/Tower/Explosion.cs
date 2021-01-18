using UnityEngine;

public class Explosion : WarEntity
{
    [SerializeField]
    AnimationCurve opacityCurve = default;
    [SerializeField]
    AnimationCurve scaleCurve = default;
    [SerializeField, Range(0f, 1f)]
    float duration = 0.5f;
    float age;
    static int colorPropertyID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock propertyBlock;
    float scale;
    MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion without renderer!");
    }
    public void Initialize(Vector3 postion,float blastRadius,float damage=0f)
    {
        if (damage > 0)
        {
            TargetPoint.FillBuffer(postion, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++)
            {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }
        transform.localPosition = postion;
        //transform.localScale = Vector3.one*(2f * blastRadius);
        scale = 2f * blastRadius;
    }
    public override bool GameUpdate()
    {
        age += Time.deltaTime;
        if (age >= duration)
        {
            OriginFactory.Reclaim(this);
            age = 0;
            return false;
        }
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
            float t = age / duration;
            Color c = Color.clear;
            c.a = opacityCurve.Evaluate(t);
            propertyBlock.SetColor(colorPropertyID, c);
            meshRenderer.SetPropertyBlock(propertyBlock);
            transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));
            return true;
    }
}
