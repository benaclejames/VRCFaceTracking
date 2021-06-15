using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCFaceTracking
{
    public class LimitFloat
    {
        public float Value
        {
            get => _currValue;
            set { if (DateTime.Now - _lastUpdated >= _updateInterval) UpdateCache(value); }
        }

        private float _currValue;

        private readonly TimeSpan _updateInterval;
        private DateTime _lastUpdated;

        private readonly List<float> _cachedValues = new List<float>();
        private int _currListIndex;
        private readonly int _maxCachedValues;

        public LimitFloat(TimeSpan updateInterval, int maxCachedValues)
        {
            _updateInterval = updateInterval;
            _maxCachedValues = maxCachedValues;
        }

        private void UpdateCache(float newValue)
        {
            if (_currListIndex > _cachedValues.Count-1) _cachedValues.Add(newValue);
            else _cachedValues[_currListIndex] = newValue;
            
            _currListIndex = _currListIndex + 1 >= _maxCachedValues ? 0 : _currListIndex + 1;   // If adding 1 to the index would exceed the max cache value, set it to 0 instead

            _currValue = _cachedValues.Average();
            _lastUpdated = DateTime.Now;
        }
    }
}