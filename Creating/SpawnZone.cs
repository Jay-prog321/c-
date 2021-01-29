using UnityEngine;

public abstract class SpawnZone : PersistableObject
{
    [System.Serializable]
    public struct SpawnConfiguration {
        public enum MovementDirection
        {
            Forward,
            Upward,
            Outward,
            Random
        }
        public ShapeFactory[] factories;
        public MovementDirection movementDirection;
        public CreatingFloatRange speed;
        public CreatingFloatRange angularSpeed;
        public CreatingFloatRange scale;
        public ColorRangeHSV color;
        public bool uniformColor;
    }
    [SerializeField]
    SpawnConfiguration spawnConfig;
    int nextSequentialIndex;
    public abstract Vector3 SpawnPoint { get; }
    public override void Save(GameDataWriter writer)
    {
        writer.Write(nextSequentialIndex);
    }
    public override void Load(GameDataReader reader)
    {
        nextSequentialIndex = reader.ReadInt();
    }
    //public virtual void ConfigureSpawn(CreatingShape shape) {
    public virtual CreatingShape SpawnShape() {
        int factoryIndex = Random.Range(0,spawnConfig.factories.Length);
        CreatingShape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        if (spawnConfig.uniformColor)
        {
            shape.SetColor(spawnConfig.color.RandomInRange);
        }
        else {
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColor(spawnConfig.color.RandomInRange,i);
            }
        }
        shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
        Vector3 direction;
        switch (spawnConfig.movementDirection) { 
        case SpawnConfiguration.MovementDirection.Upward:
            direction = transform.up;
            break;
        case SpawnConfiguration.MovementDirection.Outward:
            direction = (t.localPosition - transform.position).normalized;
            break;
        case SpawnConfiguration.MovementDirection.Random:
            direction = Random.onUnitSphere;
            break;
        default:
            direction = transform.forward;
            break;
        }
        shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
        return shape;
    }
    //{
    //    get {
    //        //return Random.insideUnitSphere * 5f+transform.position;
    //        return transform.TransformPoint(
    //            surfaceOnly?Random.onUnitSphere:Random.insideUnitSphere);
    //    }
    //}
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.matrix = transform.localToWorldMatrix;
    //    Gizmos.DrawWireSphere(Vector3.zero,1f);
    //}
}
