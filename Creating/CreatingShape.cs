using UnityEngine;

public class CreatingShape : PersistableObject
{
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }
    public int ColorCount { get {
            return colors.Length;
        } }
    int shapedID = int.MinValue;
    static int colorPropertyID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;
    [SerializeField]
    MeshRenderer[] meshRenderers;
    Color[] colors;
    ShapeFactory originalFactory;
    public ShapeFactory OriginalFactory
    {
        get
        {
            return originalFactory;
        }
        set
        {
            if (originalFactory == null)
            {
                originalFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change origin factory");
            }
        }
    }
    private void Awake()
    {
        colors = new Color[meshRenderers.Length];
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
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = material;
        }
        MaterialID = materialID;
    }
    Color color;
    public void SetColor(Color color)
    {
        //this.color = color;
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyID, color);
        for (int i = 0; i <meshRenderers.Length ; i++)
        {
            colors[i] = color;
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }
    public void SetColor(Color color,int index) {
        //this.color = color;
        if (sharedPropertyBlock == null) {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyID, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
    }
    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(colors.Length);
        for (int i = 0; i < colors.Length; i++)
        {
            writer.Write(colors[i]);
        }
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }
    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if (reader.Version >= 5)
        {
            LoadColors(reader);
        }
        else {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        AngularVelocity =
            reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }
    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity*Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }
    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for (; i < max; i++)
        {
            SetColor(reader.ReadColor(),i);
        }
        if (count > colors.Length)
        {
            for (; i < count; i++)
            {
                reader.ReadColor();
            }
        }
        else if (count < colors.Length) {
            for (; i < colors.Length; i++)
            {
                SetColor(Color.white,i);
            }
        }
    }
    public void Recycle()
    {
        OriginalFactory.Reclaim(this);
    }
}
