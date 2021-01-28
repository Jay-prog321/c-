using UnityEngine;
public enum TowerType { 
    Laser,Mortar
}
public abstract class Tower : GameTileContent
{
    public abstract TowerType TowerType { get; }
    [SerializeField, Range(1.5f, 10.5f)]
    protected float targetingRange = 1.5f;
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 postion = transform.localPosition;
        postion.y += 0.01f;
        Gizmos.DrawWireSphere(postion, targetingRange);
    }
    protected bool AcquireTarget(out TargetPoint target) {
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            target = TargetPoint.RandomBuffered;
            return true;
        }
        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target) {
        if (target == null||!target.Enemy.IsValidTarget) {
            return false;
        }
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;//0.125f为target的半径(这个半径因为敌人大小改变)，防止切换目标时再次回到原先目标
        if (x*x+z*z>r*r) {
            target = null;
            return false;
        }
        return true;
    }
}
