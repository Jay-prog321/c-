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
        public MovementDirection movementDirection;
        public CreatingFloatRange speed;
        public CreatingFloatRange angularSpeed;
        public CreatingFloatRange scale;
        public ColorRangeHSV color;
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
    public virtual void ConfigureSpawn(CreatingShape shape) {
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        //shape.SetColor(Random.ColorHSV(
        //    hueMin: 0f, hueMax: 1f,
        //    saturationMin: 0.5f, saturationMax: 1f,
        //    valueMin: 0.25f, valueMax: 1f,
        //    alphaMin: 1f, alphaMax: 1f
        //    ));
        shape.SetColor(spawnConfig.color.RandomInRange);
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
