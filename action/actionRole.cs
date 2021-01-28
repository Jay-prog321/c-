using UnityEngine;

public class actionRole : MonoBehaviour
{
    [SerializeField]
    Transform model;
    [SerializeField]
    actionUI actionUI;
    [SerializeField, Range(0, 100f)]
    float runSpeed = 5f;
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    Animator anim;
    Vector3 moveDirection;
    Vector2 inputSpeed;
    CharacterController cc;
    public void Init(Vector2 InitPos)
    {
        transform.position = InitPos;
    }
    private void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = model.GetComponent<Animator>();
        Debug.Assert(model != null, "查找不到角色模型");
    }
    //private bool isRun => inputSpeed.x != 0;
    private bool isRun {
        get {
            if (inputSpeed.x < 0)
            {
                model.localRotation = Quaternion.Euler(0, 180, 0);
                return true;
            }
            else if (inputSpeed.x > 0) {
                model.localRotation = Quaternion.Euler(0, 0, 0);
                return true;
            }
            else return false;
        }    
    }
    private bool isJump { get {
            if (Input.GetKeyDown(KeyCode.Space)) { return true; }
            else return false;
        }
    }
    float gravity = 20.0F;
    private void Update()
    {
        anim.SetBool("run",isRun);
        if (cc.isGrounded) {
            inputSpeed.x = Input.GetAxis("Horizontal");
            inputSpeed.y = Input.GetAxis("Vertical");
            moveDirection = new Vector3(inputSpeed.x, inputSpeed.y, 0) * runSpeed;
            moveDirection = model.transform.TransformDirection(moveDirection);
            if (isJump) { moveDirection.y = runSpeed; }

        }
        if (!cc.isGrounded) {
            moveDirection.y -= gravity * Time.deltaTime;
        }       
        cc.SimpleMove(moveDirection);
        Debug.Log(cc.isGrounded);
    }
    void Jump()
    {
        if (isJump)
        {
            inputSpeed.y += 1f;
        }
        else if(!isJump&& model.localPosition.y>=0)
        {
            inputSpeed.y -= 1f;
        }
    }
}
