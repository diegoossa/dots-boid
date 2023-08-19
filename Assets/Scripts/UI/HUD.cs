using UnityEngine;
using UnityEngine.UIElements;

namespace SpiritBoids.UI
{
    public class HUD : MonoBehaviour
    {
        private SliderInt _spawnCountSlider;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _spawnCountSlider = root.Q<SliderInt>("spawn-count-slider");
            _spawnCountSlider.RegisterValueChangedCallback(OnSpawnCountSliderValueChanged);
        }

        private void OnSpawnCountSliderValueChanged(ChangeEvent<int> evt)
        {
            _spawnCountSlider.label = evt.newValue.ToString();
        }
    }
}
