using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections;

public class TilePercentageCalculator : MonoBehaviour
{
    [Header("瓦片地图引用")]
    public Tilemap targetTilemap; // 要计算百分比的瓦片地图

    [Header("UI组件")]
    public Slider percentageSlider;     // 百分比滑动条
    public TextMeshProUGUI percentageText; // 使用TextMeshPro显示百分比

    [Header("设置")]
    public int totalTiles = 2840; // 总瓦片数
    public float updateInterval = 0.5f; // 更新间隔（秒）

    private int tileCount = 0;
    private float percentage = 0f;
    private float timer = 0f;

    void Start()
    {
        // 初始计算一次
        CalculateTilePercentage();
        UpdateUI();

        // 确保滑动条初始值正确
        if (percentageSlider != null)
        {
            percentageSlider.minValue = 0f;
            percentageSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        // 定时更新百分比
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            CalculateTilePercentage();
            UpdateUI();
            timer = 0f;
        }
    }

    // 计算瓦片百分比
    void CalculateTilePercentage()
    {
        // 重置计数
        tileCount = 0;

        // 获取瓦片地图的边界
        BoundsInt bounds = targetTilemap.cellBounds;

        // 计算瓦片数量
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (targetTilemap.HasTile(pos))
            {
                tileCount++;
            }
        }

        // 计算百分比
        percentage = (float)tileCount / totalTiles;

        // 确保百分比在0-1之间
        percentage = Mathf.Clamp01(percentage);
    }

    // 更新UI显示
    void UpdateUI()
    {
        // 更新滑动条
        if (percentageSlider != null)
        {
            percentageSlider.value = percentage;
        }

        // 更新百分比文本
        if (percentageText != null)
        {
            percentageText.text = $"{percentage:P1} ({tileCount}/{totalTiles})";
        }
    }

    // 公共方法：手动触发更新（可以从其他脚本调用）
    public void ForceUpdate()
    {
        CalculateTilePercentage();
        UpdateUI();
    }

    // 获取当前百分比（供其他脚本使用）
    public float GetPercentage()
    {
        return percentage;
    }

    // 获取当前瓦片数量（供其他脚本使用）
    public int GetTileCount()
    {
        return tileCount;
    }

    // 调试信息
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, Screen.height - 30, 400, 30),
                 $"瓦片数量: {tileCount} / {totalTiles} ({percentage:P1})", style);
    }
}