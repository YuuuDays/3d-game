using UnityEngine;

public class FlipperController : MonoBehaviour
{
    public enum FlipperSide { Left, Right }
    public FlipperSide side;

    public float restAngle = 0f;
    public float activeAngle = -45f;
    public float speed = 15f;

    private float targetAngle;
    private KeyCode inputKey;
    private Vector3 baseEuler; // 追加：配置時のX,Y角度を記憶しておく

    void Start()
    {
        targetAngle = restAngle;
        inputKey = (side == FlipperSide.Left) ? KeyCode.LeftArrow : KeyCode.RightArrow;
        baseEuler = transform.localEulerAngles; // 配置時点のX,Y,Zを保存
    }

    void Update()
    {
        if (Input.GetKey(inputKey))
        {
            targetAngle = (side == FlipperSide.Left) ? activeAngle : -activeAngle;
        }
        else
        {
            targetAngle = restAngle;
        }

        // X, Yは配置時のまま維持し、Zだけスクリプトで動かす
        Quaternion targetRotation = Quaternion.Euler(baseEuler.x, baseEuler.y, targetAngle);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, speed * 200f * Time.deltaTime);
    }
}