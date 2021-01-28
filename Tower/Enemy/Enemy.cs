using UnityEngine;
public enum EnemyType
{
    Small,Medium,Large
}
public class Enemy : GameBehavior
{
    [SerializeField]
    public EnemyAnimationConfig animationConfig = default;
    EnemyFactory originFactory;
    GameTile tileFrom,tileTo;
    Vector3 postionFrom, positonTo;
    float progress,progressFactor;
    Direction direction;
    DirectionChange directionChange;
    float directionAngleFrom, directionAngleTo;
    float pathOffset;
    float speed;
    public EnemyAnimator animator;
    Collider targetPointCollider;
    public Collider TargetPointCollider {
        set {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }
    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;
    float Health { get; set; }
    public float Scale { get; private set; }
    private void OnEnable()
    {
        game = FindObjectOfType<Game>();
        if (!model.GetChild(0).GetComponent<Animator>())
        {
            animator.Configure(model.GetChild(0).gameObject.AddComponent<Animator>(), animationConfig);
        }
        else
        {
            animator.Configure(model.GetChild(0).gameObject.GetComponent<Animator>(), animationConfig);
            if (!(model.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController))
            {
                RuntimeAnimatorController anim = (RuntimeAnimatorController)Resources.Load("Animation/Cube");
                model.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = anim;
            }
        }
    }

    public void Initialize(float scale,float speed,float pathOffset,float health)
    {
        Scale = scale;
        Health = health;
        model.localScale = new Vector3(scale, scale, scale);
        this.speed = speed*0.7f;
        this.pathOffset = pathOffset;
        animator.PlayIntro();
        targetPointCollider.enabled = false;
    }
    public EnemyFactory OriginFactory {
        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory");
            originFactory = value;
        }
    }
    public void SpawnOn(GameTile tile) {
        Debug.Assert(tile.NextTileOnPath!=null,"Nowhere to go!",this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        progress = 0f;
        PrepareIntro();
    }
    public void PrepareIntro() {
        postionFrom = tileFrom.transform.localPosition;
        transform.localPosition = postionFrom;
        positonTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f* speed;
    }
    public override bool GameUpdate() {
#if UNITY_EDITOR
        if (!animator.IsValid) {
            animator.RestoreAfterHotReload(
                model.GetChild(0).GetComponent<Animator>(),
                animationConfig,
                animationConfig.MoveAnimationSpeed * speed / Scale);
        }
#endif
        animator.GameUpdate();
        if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
            if (!animator.IsDone) {
                return true;
            }
            animator.PlayMove(animationConfig.MoveAnimationSpeed*speed / Scale);
            targetPointCollider.enabled = true;
        }
        else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro) {
            if (animator.IsDone) {
                Recycle();
                return false;
            }
            return true;
        }
        if (Health <= 0f)
        {
            animator.PlayDying();
            targetPointCollider.enabled = false;
            return true;
        }
        progress += Time.deltaTime*progressFactor;
        while (progress >= 1f) {
            if (tileTo == null) {
                Game.EnemyReachedDestination();
                animator.PlayOutro();
                targetPointCollider.enabled = false;
                return true;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }
        if (directionChange == DirectionChange.None) {
            transform.localPosition = Vector3.LerpUnclamped(postionFrom, positonTo, progress);
        }
        else{
            float angle = Mathf.LerpUnclamped(
                directionAngleFrom, directionAngleTo, progress
                );
            transform.localRotation = Quaternion.Euler(0f,angle,0f);
        }
        return true;
    }
    void PrepareNextState() {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        postionFrom = positonTo;
        if (tileTo == null) {
            PrepareOutro();
            return;
        }
        positonTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;
        switch (directionChange) {
            case DirectionChange.None:PrepareForward();break;
            case DirectionChange.TurnRight:PrepareTurnRight();break;
            case DirectionChange.TurnLeft:PrepareTurnLeft();break;
            default:PrepareTurnAround();break;
         }
    }
    [SerializeField]
    Transform model = default;
    void PrepareForward() {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        progressFactor = speed;
    }
    void PrepareTurnRight() {
        directionAngleTo = directionAngleFrom + 90f;
        model.localPosition = new Vector3(pathOffset-0.5f,0f);
        transform.localPosition = postionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f*(0.5f-pathOffset));
    }
    void PrepareTurnLeft() {
        directionAngleTo = directionAngleFrom - 90f;
        model.localPosition = new Vector3(pathOffset+0.5f,0f);
        transform.localPosition = postionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f*(0.5f+pathOffset));
    }
    void PrepareTurnAround() {
        directionAngleTo = directionAngleFrom + (pathOffset<0f?180f:-180f);
        model.localPosition = new Vector3(pathOffset,0f);
        transform.localPosition = postionFrom;
        progressFactor = speed / Mathf.PI*Mathf.Max(Mathf.Abs(pathOffset),0.2f);
    }
    void PrepareOutro() {
        positonTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f* speed;
    }

    public void ApplyDamage(float damage) {
        Debug.Assert(damage>=0f,"Negative damage applied.");
        Health -= damage;
    }
    Game game;
    public override void Recycle()
    {
        animator.Stop();
        OriginFactory.Reclaim(this);
        game.board.PlayerCoin += 10 * Scale;
        
    }
    private void OnDestroy()
    {
        animator.Destroy();
    }
}
