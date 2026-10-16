using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal on-screen controls for M1: a floating virtual joystick on the left
/// half of the screen (drives movement) and an Action button on the right.
/// Built entirely in code with UGUI.
/// </summary>
public class Hud : MonoBehaviour
{
    public Vector2 Move { get; private set; }

    RectTransform canvasRect;
    RectTransform joyBg;
    RectTransform joyHandle;
    CanvasGroup joyGroup;
    readonly float radius = 140f;

    int activeId = -1; // -1 none, -2 mouse, >=0 touch finger id

    void Awake()
    {
        var cgo = new GameObject("Canvas");
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = canvas.GetComponent<RectTransform>();

        var circle = CircleSprite();

        joyBg = MakeImage(canvasRect, "JoyBg", new Color(1f, 1f, 1f, 0.18f), circle);
        joyBg.sizeDelta = new Vector2(radius * 2f, radius * 2f);
        joyBg.anchorMin = joyBg.anchorMax = new Vector2(0.5f, 0.5f);
        joyBg.pivot = new Vector2(0.5f, 0.5f);
        joyBg.anchoredPosition = Vector2.zero;
        joyGroup = joyBg.gameObject.AddComponent<CanvasGroup>();
        joyGroup.alpha = 0f;

        joyHandle = MakeImage(joyBg, "JoyHandle", new Color(1f, 1f, 1f, 0.55f), circle);
        joyHandle.sizeDelta = new Vector2(130f, 130f);
        joyHandle.anchoredPosition = Vector2.zero;

        var action = MakeImage(canvasRect, "Action", new Color(0.2f, 0.65f, 0.35f, 0.85f), circle);
        action.sizeDelta = new Vector2(190f, 190f);
        action.anchorMin = action.anchorMax = new Vector2(1f, 0f);
        action.pivot = new Vector2(0.5f, 0.5f);
        action.anchoredPosition = new Vector2(-190f, 190f);

        var font = BuiltinFont();
        if (font != null)
        {
            var status = MakeText(canvasRect, font, 34, Color.white);
            status.text = "Left half: move    Right half: buttons";
            var srt = status.rectTransform;
            srt.anchorMin = new Vector2(0f, 1f);
            srt.anchorMax = new Vector2(0f, 1f);
            srt.pivot = new Vector2(0f, 1f);
            srt.anchoredPosition = new Vector2(30f, -24f);
            srt.sizeDelta = new Vector2(1400f, 60f);

            var actLabel = MakeText(action, font, 34, Color.white);
            actLabel.text = "Action";
            actLabel.alignment = TextAnchor.MiddleCenter;
            var art = actLabel.rectTransform;
            art.anchorMin = Vector2.zero;
            art.anchorMax = Vector2.one;
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && activeId < 0 && t.position.x < Screen.width * 0.5f)
                {
                    activeId = t.fingerId;
                    Begin(t.position);
                }
                else if (t.fingerId == activeId)
                {
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) End();
                    else Drag(t.position);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && activeId < 0 && Input.mousePosition.x < Screen.width * 0.5f)
            {
                activeId = -2;
                Begin(Input.mousePosition);
            }
            else if (activeId == -2)
            {
                if (Input.GetMouseButton(0)) Drag(Input.mousePosition);
                else End();
            }
        }
    }

    void Begin(Vector2 screenPos)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var local))
            joyBg.anchoredPosition = local;
        joyHandle.anchoredPosition = Vector2.zero;
        Move = Vector2.zero;
        joyGroup.alpha = 1f;
    }

    void Drag(Vector2 screenPos)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var local))
            return;
        Vector2 d = local - joyBg.anchoredPosition;
        if (d.magnitude > radius) d = d.normalized * radius;
        Move = d / radius;
        joyHandle.anchoredPosition = d;
    }

    void End()
    {
        activeId = -1;
        Move = Vector2.zero;
        joyHandle.anchoredPosition = Vector2.zero;
        joyGroup.alpha = 0f;
    }

    static RectTransform MakeImage(Transform parent, string name, Color color, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.raycastTarget = false;
        return img.rectTransform;
    }

    static Text MakeText(Transform parent, Font font, int size, Color color)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static Font BuiltinFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static Sprite CircleSprite()
    {
        int s = 128;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        float r = s * 0.5f;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f));
    }
}
