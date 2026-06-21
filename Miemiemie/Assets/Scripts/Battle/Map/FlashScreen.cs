using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageVignette : MonoBehaviour
{
    [Header("暗角设置")]
    [SerializeField] private Color cornerColor = new Color(1f, 0f, 0f, 0.8f);//暗角颜色
    [SerializeField][Range(0f, 1f)] private float cornerSize = 0.3f;    // 三角形直角边长度
    [SerializeField] private float fadeInSpeed = 15f;
    [SerializeField] private float fadeOutSpeed = 3f;

    private RawImage rawImage;              // 用于显示暗角贴图的 UI 组件
    private Texture2D vignetteTexture;      // 程序生成的暗角贴图（四个角有红色三角形）
    private float currentAlpha = 0f;        // 当前透明度（0=完全透明，1=完全不透明）
    private float targetAlpha = 0f;         // 目标透明度（Lerp 的终点）

    private static DamageVignette instance;
    public static DamageVignette Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
            rawImage = gameObject.AddComponent<RawImage>();

        // --- 生成暗角贴图并赋值 ---
        GenerateVignetteTexture();
        rawImage.texture = vignetteTexture;

        // --- 初始完全透明 ---
        SetAlpha(0f);
    }

    void Update()
    {
        // 如果当前透明度和目标透明度差距很小，就不动
        if (Mathf.Abs(currentAlpha - targetAlpha) > 0.001f)
        {
            // 选择速度：淡入用 fadeInSpeed，淡出用 fadeOutSpeed
            float speed = targetAlpha > currentAlpha ? fadeInSpeed : fadeOutSpeed;

            // Lerp 平滑过渡：currentAlpha 会逐渐靠近 targetAlpha
            // 公式：从 currentAlpha 往 targetAlpha 走 speed * Time.deltaTime 这一步
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, speed * Time.deltaTime);

            // 把当前的透明度应用到 RawImage 上
            SetAlpha(currentAlpha);
        }
    }

    private void SetAlpha(float alpha)
    {
        rawImage.color = new Color(1f, 1f, 1f, alpha);
    }

    /// 玩家受伤时调用，屏幕四角闪一下红色暗角
    public void TriggerDamageVignette()
    {
        // 停掉之前可能还在跑的协程（防止多次受伤叠加）
        StopAllCoroutines();
        StartCoroutine(VignettePulse());
    }

    /// 受伤暗角的闪烁节奏：
    private IEnumerator VignettePulse()
    {
        targetAlpha = 1f;
        yield return new WaitForSeconds(0.05f);  // 等一下让暗角显示出来
        yield return new WaitForSeconds(0.15f);  // 暗角保持可见一小会儿
        targetAlpha = 0f;
    }

    // 程序生成暗角贴图
    // ============================================

    /// <summary>
    /// 生成一张 512x512 的贴图
    /// 四个角画上红色的三角形，模拟"视野边缘变红"的效果
    /// 三角形斜边按 16:9 比例计算，适配宽屏
    /// </summary>
    private void GenerateVignetteTexture()
    {
        int size = 512;  // 贴图分辨率

        // 创建一张新的 Texture2D，RGBA32 格式，不开启 mipmap
        vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        vignetteTexture.wrapMode = TextureWrapMode.Clamp;  // 边缘不重复

        // --- 先把整张贴图填充为完全透明 ---
        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];  // 像素数组，长度 = 512 * 512
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transparent;              // 全部初始化为透明

        // cornerSize 是 0~1，乘上贴图尺寸得到三角形直角边的像素长度
        int cornerPixels = Mathf.RoundToInt(size * cornerSize);

        // --- 在四个角画红色三角形 ---
        // 参数说明：像素数组, 贴图尺寸, 角点的X坐标, 角点的Y坐标, 三角形直角边长
        // 坐标系：左下角是 (0, 0)，右上角是 (size-1, size-1)

        DrawTriangle(pixels, size, 0, size - 1, cornerPixels);           // 左上角（X=0，Y=511）
        DrawTriangle(pixels, size, size - 1, size - 1, cornerPixels);    // 右上角（X=511，Y=511）
        DrawTriangle(pixels, size, 0, 0, cornerPixels);                  // 左下角（X=0，Y=0）
        DrawTriangle(pixels, size, size - 1, 0, cornerPixels);           // 右下角（X=511，Y=0）

        // 把像素数组写回贴图
        vignetteTexture.SetPixels(pixels);
        vignetteTexture.Apply();  // 应用修改（必须调用才会生效）
    }

    private void DrawTriangle(Color[] pixels, int size, int cornerX, int cornerY, int triSize)
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - cornerX);
                int dy = Mathf.Abs(y - cornerY);

                // dx/16 + dy/9 < triSize  斜边贴合16:9屏幕
                if (dx / 16f + dy / 9f < triSize)
                {
                    // 离角落越近 alpha 越高
                    float dist = dx / 16f + dy / 9f;
                    float alpha = 1f - (dist / triSize);

                    int index = y * size + x;
                    float finalAlpha = Mathf.Max(pixels[index].a, alpha * cornerColor.a);

                    pixels[index] = new Color(
                        cornerColor.r,
                        cornerColor.g,
                        cornerColor.b,
                        finalAlpha
                    );
                }
            }
        }
    }
}