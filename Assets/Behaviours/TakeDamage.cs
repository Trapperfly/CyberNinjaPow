using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TakeDamage")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TakeDamage", message: "Take [damage]", category: "Events", id: "7bc1eccefdb2c1a9aa27c0083baf49ad")]
public sealed partial class TakeDamage : EventChannel<int> { }

