using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    float shotPerSecond = 1f;
    [SerializeField]
    Transform mortar = default;
    float launchSpeed;
    float launchProgress;
    [SerializeField, Range(0.5f, 3f)]
    float shellBlastRadius = 1f;
    [SerializeField, Range(1f, 100f)]
    float shellDamage = 10f;
    private void OnEnable()
    {
        OnValidate();
    }
    private void OnValidate()
    {
        float x = targetingRange + 0.25001f;//0.25为敌人模型半径
        float y = -mortar.position.y;
        launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)))+0.2f;
    }
    public override TowerType TowerType => TowerType.Mortar;
    public override void GameUpdate()
    {
        launchProgress += shotPerSecond * Time.deltaTime;
        while (launchProgress >= 1f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                launchProgress -= 1f;
            }
            else
            {
                launchProgress = 0.999f;
            }
        }
    }
    public void Launch(TargetPoint target) {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;
        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;
        float g = 9.81f;//重力加速度
        float s = launchSpeed;//发射速度
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;
        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x,tanTheta,dir.y));
        Game.SpawnShell().Initialize(
            launchPoint, targetPoint, new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),shellBlastRadius,shellDamage
            );
        
    }
}
