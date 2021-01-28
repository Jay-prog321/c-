using UnityEngine;

public class CreatingShape : PersistableObject
{
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }
    int shapedID = int.MinValue;
    MeshRenderer meshRenderer;
    static int colorPropertyID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public int ShapeID {
        get { return shapedID; }
        set {
            if (shapedID == int.MinValue && value != int.MinValue)
            {
                shapedID = value;
            }
            else {
                Debug.LogError("Not allowed to change shapeID");
            }
        }
    }
    public int MaterialID { get;private set; }
    public void SetMaterial(Material material,int materialID) {
        meshRenderer.material = material;
        MaterialID = materialID;
    }
    Color color;
    public void SetColor(Color color) {
        this.color = color;
        //meshRenderer.material.color = color;
        //var propertyBlock = new MaterialPropertyBlock();
        if (sharedPropertyBlock == null) {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyID, color);
        meshRenderer.SetPropertyBlock(sharedPropertyBlock);
    }
    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }
    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version>0?reader.ReadColor():Color.white);
        AngularVelocity =
            reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }
    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity*Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }
}
