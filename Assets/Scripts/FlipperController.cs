using UnityEngine;

// MonoBehaviour: Unityのゲームオブジェクトにアタッチできるスクリプトの基底クラス（Unity独自）
public class FlipperController : MonoBehaviour
{
    public enum FlipperSide { Left, Right }
    public FlipperSide side;

    public float restAngle = 0f;
    public float activeAngle = -45f;
    public float speed = 15f;

    private float targetAngle;
    // KeyCode: Unityのキーボード入力を表す列挙型（Unity独自）
    private KeyCode inputKey;

    // Start(): ゲーム開始時に1度だけ呼ばれるUnity独自のライフサイクル関数
    void Start()
    {
        targetAngle = restAngle;
        inputKey = (side == FlipperSide.Left) ? KeyCode.LeftArrow : KeyCode.RightArrow;
    }

    // Update(): 毎フレーム呼ばれるUnity独自のライフサイクル関数
    void Update()
    {
        // Input.GetKey(): 指定キーが押されている間ずっとtrueを返すUnity独自の入力関数　
        if (Input.GetKey(inputKey))
        {
            targetAngle = (side == FlipperSide.Left) ? activeAngle : -activeAngle;
        }
        else
        {
            targetAngle = restAngle;
        }

        // Quaternion: Unityで回転を表すデータ型。オイラー角（度数）より計算が安定する（Unity独自）
        // Quaternion.Euler(): X,Y,Z の角度（度数）をQuaternionに変換するUnity独自の関数
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);


        // transform.localRotation: 親オブジェクト基準の回転。transformはUnityが自動で用意するコンポーネント（Unity独自）
        // Quaternion.RotateTowards(): 現在の回転を目標の回転へ一定速度で近づけるUnity独自の関数
        // Time.deltaTime: 前フレームからの経過秒数。掛けることでFPSに依存しない速度になる（Unity独自）
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, speed * 200f * Time.deltaTime);
    }
}