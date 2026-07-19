using UnityEngine;
using UnityEditor;

/// <summary>
/// ピンボール台を自動生成するエディタ拡張スクリプト。
/// Unity上部メニューの「Pinball > Build Full Table」から実行する。
///
/// 【置き場所】
/// Assets/Editor/PinballTableBuilder.cs
/// (Editorという名前のフォルダの中に置くのが必須。それ以外の場所だと
///  ビルド時にエラーになったり、メニューに出てこなかったりする)
/// </summary>
public static class PinballTableBuilder
{
    // ==== ここの数値を調整すれば配置やサイズを変えられる ====
    private const float TableTiltDegrees = -7f;   // PinballAssemblyの傾き
    private const float TableWidth = 6f;
    private const float TableLength = 10f;
    private const float WallHeight = 0.5f;
    private const float WallThickness = 0.2f;

    [MenuItem("Pinball/Build Full Table")]
    public static void BuildFullTable()
    {
        // 既に PinballAssembly があれば一旦削除して作り直す（何度でも再実行できるように）
        GameObject existing = GameObject.Find("PinballAssembly");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        // ---- ルートオブジェクト：台全体を傾けるための親 ----
        GameObject root = new GameObject("PinballAssembly");
        Undo.RegisterCreatedObjectUndo(root, "Build Pinball Table");
        root.transform.rotation = Quaternion.Euler(TableTiltDegrees, 0f, 0f);

        // ---- 台本体（プレイフィールド） ----
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.SetParent(root.transform, false);
        table.transform.localPosition = Vector3.zero;
        table.transform.localScale = new Vector3(TableWidth, 0.2f, TableLength);

        // ---- 壁（4方向） ----
        CreateWall("Wall_Left", root.transform, new Vector3(-TableWidth / 2f, WallHeight / 2f, 0f), new Vector3(WallThickness, WallHeight, TableLength));
        CreateWall("Wall_Right", root.transform, new Vector3(TableWidth / 2f, WallHeight / 2f, 0f), new Vector3(WallThickness, WallHeight, TableLength));
        CreateWall("Wall_Top", root.transform, new Vector3(0f, WallHeight / 2f, TableLength / 2f), new Vector3(TableWidth, WallHeight, WallThickness));
        // 下側（Bottom）はボールが落ちる出口として空けておく想定なので作らない

        // ---- バウンサー用の物理マテリアル ----
        PhysicsMaterial bumperMat = new PhysicsMaterial("BumperPhysicMaterial")
        {
            bounciness = 0.8f,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        // ---- バンパー（3個、適当な配置。座標はあとで自由に調整してOK） ----
        CreateBumper("Bumper_1", root.transform, new Vector3(-1.5f, 0.3f, 2f), bumperMat);
        CreateBumper("Bumper_2", root.transform, new Vector3(1.5f, 0.3f, 2f), bumperMat);
        CreateBumper("Bumper_3", root.transform, new Vector3(0f, 0.3f, 3.5f), bumperMat);

        // ---- フリッパー（左右）：根元を軸にするためPivotを親にする構造 ----
        CreateFlipper("Flipper_Left", root.transform, new Vector3(-1.2f, 0.2f, -4f), isLeft: true);
        CreateFlipper("Flipper_Right", root.transform, new Vector3(1.2f, 0.2f, -4f), isLeft: false);

        // ---- ボール ----
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.SetParent(root.transform, false);
        ball.transform.localPosition = new Vector3(0f, 1f, -2f);
        ball.transform.localScale = Vector3.one * 0.3f;
        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.5f;

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log("ピンボール台を生成しました: PinballAssembly");
    }

    private static void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;
    }

    private static void CreateBumper(string name, Transform parent, Vector3 localPos, PhysicsMaterial mat)
    {
        GameObject bumper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bumper.name = name;
        bumper.transform.SetParent(parent, false);
        bumper.transform.localPosition = localPos;
        bumper.transform.localScale = Vector3.one * 0.6f;

        SphereCollider col = bumper.GetComponent<SphereCollider>();
        col.material = mat;
    }

    private static void CreateFlipper(string name, Transform parent, Vector3 pivotLocalPos, bool isLeft)
    {
        // 根元（Pivot）を親、見た目の板（Flipper本体）を子にすることで
        // 回転軸を根元に寄せる。これが以前の「回転軸が中心になってしまう」問題への対処。
        GameObject pivot = new GameObject(name + "_Pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotLocalPos;

        GameObject flipper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flipper.name = name;
        flipper.transform.SetParent(pivot.transform, false);

        // 板の中心を根元からオフセットして、Pivot側の端が軸になるようにする
        float offsetDirection = isLeft ? 1f : -1f;
        flipper.transform.localPosition = new Vector3(offsetDirection * 0.5f, 0f, 0f);
        flipper.transform.localScale = new Vector3(1f, 0.2f, 0.3f);

        // 既存のFlipperControllerスクリプトがプロジェクトにあれば自動でアタッチする。
        // 名前空間を使っている場合は "Namespace.FlipperController" のように書き換えてください。
        System.Type flipperControllerType = System.Type.GetType("FlipperController");
        if (flipperControllerType != null)
        {
            pivot.AddComponent(flipperControllerType);
        }
        else
        {
            Debug.LogWarning($"{name}: FlipperControllerスクリプトが見つかりませんでした。手動でアタッチしてください。");
        }
    }
}
