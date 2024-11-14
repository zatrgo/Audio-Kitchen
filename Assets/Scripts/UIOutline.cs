using UnityEngine;
using UnityEngine.UI;

public class Outline : MonoBehaviour {

    RectTransform rectTransform;
    public float width = 5;
    public Color color = Color.white;
    public RectTransform parent;
    RectTransform top;
    RectTransform bottom;
    RectTransform left;
    RectTransform right;
    void Awake() {rectTransform = GetComponent<RectTransform>();}
    void Start() {
         var findParent = transform.Find("Outlines");
        if (findParent != null) {
            parent = findParent.GetComponent<RectTransform>();
            top = parent.GetChild(0).GetComponent<RectTransform>();
            bottom = parent.GetChild(1).GetComponent<RectTransform>();
            left = parent.GetChild(2).GetComponent<RectTransform>();
            right = parent.GetChild(3).GetComponent<RectTransform>();
        } else {
            parent = new GameObject("Outlines", typeof(RectTransform)).GetComponent<RectTransform>();
            parent.SetParent(rectTransform);
            parent.anchorMin = Vector2.zero;
            parent.anchorMax = Vector2.one;
            parent.offsetMin = Vector2.zero;
            parent.offsetMax = Vector2.zero;
            top = new GameObject("Top", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            top.SetParent(parent);
            bottom = new GameObject("Bottom", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            bottom.SetParent(parent);
            left = new GameObject("Left", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            left.SetParent(parent);
            right = new GameObject("Right", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            right.SetParent(parent);
        }
    }

    // Update is called once per frame
    void Update() {
        if (!Application.isPlaying) Start();
        foreach (RectTransform rect in parent) {
            rect.GetComponent<Image>().color = color;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        }
        top.offsetMin = Vector2.zero;
        top.offsetMax = new Vector2(0, -(rectTransform.rect.height - width));
        bottom.offsetMin = new Vector2(0, rectTransform.rect.height - width);
        bottom.offsetMax = Vector2.zero;
        left.offsetMin = Vector2.zero;
        left.offsetMax = new Vector2(-(rectTransform.rect.width - width), 0);
        right.offsetMin = new Vector2(rectTransform.rect.width - width, 0);
        right.offsetMax = Vector2.zero;
    }

    void OnDestroy() {
        if (Application.isPlaying) return;
        if (top != null) DestroyImmediate(top.gameObject);
        if (bottom != null) DestroyImmediate(bottom.gameObject);
        if (left != null) DestroyImmediate(left.gameObject);
        if (right != null) DestroyImmediate(right.gameObject);
        if (parent != null) DestroyImmediate(parent.gameObject);
    }
}
