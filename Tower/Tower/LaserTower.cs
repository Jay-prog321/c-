using UnityEngine;

public class LaserTower : Tower
{
    //Tower towerPrefab = default;
    TargetPoint target;
    //const int enemyLayerMask = 1 << 9;
    [SerializeField]
    Transform turret = default, laserBeam = default;
    Vector3 laserBeamScale;
    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;
    private void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }
    public override TowerType TowerType => TowerType.Laser;
    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
        {
            //Debug.Log("Locked on target!");
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
    }
    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;
        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
    //[SerializeField, Range(1.5f, 10.5f)]
    // float targetingRange = 1.5f;
    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Vector3 postion = transform.localPosition;
    //    postion.y += 0.01f;
    //    Gizmos.DrawWireSphere(postion, targetingRange);
    //    if (target != null)
    //    {
    //        Gizmos.DrawLine(postion, target.Position);
    //    }
    //}
    //static Collider[] targetsBuffer = new Collider[100];
    //bool AcquireTarget()
    //{
    //    Vector3 a = transform.localPosition;
    //    Vector3 b = a;
    //    b.y += 3f;
    //    int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, targetsBuffer, enemyLayerMask);
    //    if (hits > 0)
    //    {
    //        target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
    //        Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
    //        return true;
    //    }
    //    target = null;
    //    return false;
    //}

    //bool TrackTarget()
    //{
    //    if (target == null)
    //    {
    //        return false;
    //    }
    //    Vector3 a = transform.localPosition;
    //    Vector3 b = target.Position;
    //    float x = a.x - b.x;
    //    float z = a.z - b.z;
    //    float r = targetingRange + 0.125f * target.Enemy.Scale;//0.125f为target的半径(这个半径因为敌人大小改变)，防止切换目标时再次回到原先目标
    //    if (x * x + z * z > r * r)
    //    {
    //        target = null;
    //        return false;
    //    }
    //    return true;
    //}
}
