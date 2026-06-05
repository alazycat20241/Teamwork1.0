using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageVignette : MonoBehaviour
{
    [Header("暗角设置")]
    [SerializeField] private Color cornerColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField][Range(0f, 1f)] private float cornerSize = 0.3f;    // 三角形直角边长度
    [SerializeField] private float fadeInSpeed = 15f;
    [SerializeField] private float fadeOutSpeed = 3f;

    private RawImage rawImage;
    private Texture2D vignetteTexture;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private static DamageVignette instance;
    public static DamageVignette Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
            rawImage = gameObject.AddComponent<RawImage>();

        GenerateVignetteTexture();
        rawImage.texture = vignetteTexture;
        SetAlpha(0f);
    }

    void Update()
    {
        if (Mathf.Abs(currentAlpha - targetAlpha) > 0.001f)
        {
            float speed = targetAlpha > currentAlpha ? fadeInSpeed : fadeOutSpeed;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, speed * Time.deltaTime);
            SetAlpha(currentAlpha);
        }
    }

    private void SetAlpha(float alpha)
    {
        rawImage.color = new Color(1f, 1f, 1f, alpha);
    }

    public void TriggerDamageVignette()
    {
        StopAllCoroutines();
        StartCoroutine(VignettePulse());
    }

    private IEnumerator VignettePulse()
    {
        targetAlpha = 1f;
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(0.15f);
        targetAlpha = 0f;
    }

    private void GenerateVignetteTexture()
    {
        int size = 512;
        vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        vignetteTexture.wrapMode = TextureWrapMode.Clamp;

        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transparent;

        int cornerPixels = Mathf.RoundToInt(size * cornerSize);

        // 四个角
        DrawTriangle(pixels, size, 0, size - 1, cornerPixels);      // 左上
        DrawTriangle(pixels, size, size - 1, size - 1, cornerPixels); // 右上
        DrawTriangle(pixels, size, 0, 0, cornerPixels);             // 左下
        DrawTriangle(pixels, size, size - 1, 0, cornerPixels);      // 右下

        vignetteTexture.SetPixels(pixels);
        vignetteTexture.Apply();
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