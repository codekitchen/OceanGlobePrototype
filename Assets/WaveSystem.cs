using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveSystem {
  [System.Serializable]
  public struct Wave {
    public bool enabled;
    public float wavelength;
    public float amplitude;
    public float speed;
    [Range(0f, 360f)]
    public float angle;
    [Range(0f, 1f)]
    public float steepness;
    public Vector2 direction { get { return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)); } }
  }
  public List<Wave> waves = new List<Wave>();

  const float HalfPI = Mathf.PI * 0.5f;
  const float TwoPI = Mathf.PI * 2f;

  public Vector3 WaveOffset(Vector2 position) {
    Vector3 offset = Vector3.zero;
    foreach (Wave wave in waves) {
      if (!wave.enabled)
        continue;
      float freq = TwoPI / wave.wavelength;
      float phase = wave.speed * freq;
      float steepness = wave.steepness / (freq * wave.amplitude * waves.Count);
      Vector2 direction = wave.direction;
      offset.x += steepness * wave.amplitude * direction.x * Mathf.Cos(Vector2.Dot(position, freq * direction) + phase * Time.time);
      offset.z += steepness * wave.amplitude * direction.y * Mathf.Cos(Vector2.Dot(position, freq * direction) + phase * Time.time);
      offset.y += wave.amplitude * Mathf.Sin(Vector2.Dot(position, freq * direction) + phase * Time.time);
    }
    return offset;
  }
}