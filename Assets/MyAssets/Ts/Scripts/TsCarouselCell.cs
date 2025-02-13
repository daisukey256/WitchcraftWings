using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FancyCarouselView.Runtime.Scripts;


public class TsCarouselCell : CarouselCell<TsData, TsCarouselCell>
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;

    private TsData _data;

    protected override void Refresh(TsData data)
    {
        _data = data;
        _image.sprite = data.SpriteResource;
        _text.text = data.Text;
    }

    protected override void OnVisibilityChanged(bool visibility)
    {
        if (visibility)
            _button.onClick.AddListener(OnClick);
        else
            _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        _data?.Clicked?.Invoke();
    }
}
