using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections;

public class TilePercentageCalculator : MonoBehaviour
{
    [Header("��Ƭ��ͼ����")]
    public Tilemap targetTilemap; // Ҫ����ٷֱȵ���Ƭ��ͼ

    [Header("UI���")]
    public Slider percentageSlider;     // �ٷֱȻ�����
    public TextMeshProUGUI percentageText; // ʹ��TextMeshPro��ʾ�ٷֱ�

    [Header("����")]
    public int totalTiles = 2840; // ����Ƭ��
    public float updateInterval = 0.5f; // ���¼�����룩

    private int tileCount = 0;
    private float percentage = 0f;
    private float timer = 0f;

    void Start()
    {
        // ��ʼ����һ��
        CalculateTilePercentage();
        UpdateUI();

        // ȷ����������ʼֵ��ȷ
        if (percentageSlider != null)
        {
            percentageSlider.minValue = 0f;
            percentageSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        // ��ʱ���°ٷֱ�
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            CalculateTilePercentage();
            UpdateUI();
            timer = 0f;
        }
    }

    // ������Ƭ�ٷֱ�
    void CalculateTilePercentage()
    {
        // ���ü���
        tileCount = 0;

        // ��ȡ��Ƭ��ͼ�ı߽�
        BoundsInt bounds = targetTilemap.cellBounds;

        // ������Ƭ����
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (targetTilemap.HasTile(pos))
            {
                tileCount++;
            }
        }

        // ����ٷֱ�
        percentage = (float)tileCount / totalTiles;

        // ȷ���ٷֱ���0-1֮��
        percentage = Mathf.Clamp01(percentage);
    }

    // ����UI��ʾ
    void UpdateUI()
    {
        // ���»�����
        if (percentageSlider != null)
        {
            percentageSlider.value = percentage;
        }

        // ���°ٷֱ��ı�
        if (percentageText != null)
        {
            percentageText.text = $"{percentage:P1} ({tileCount}/{totalTiles})";
        }
    }

    // �����������ֶ��������£����Դ������ű����ã�
    public void ForceUpdate()
    {
        CalculateTilePercentage();
        UpdateUI();
    }

    // ��ȡ��ǰ�ٷֱȣ��������ű�ʹ�ã�
    public float GetPercentage()
    {
        return percentage;
    }

    // ��ȡ��ǰ��Ƭ�������������ű�ʹ�ã�
    public int GetTileCount()
    {
        return tileCount;
    }

    // ������Ϣ
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, Screen.height - 30, 400, 30),
                 $"��Ƭ����: {tileCount} / {totalTiles} ({percentage:P1})", style);
    }
}