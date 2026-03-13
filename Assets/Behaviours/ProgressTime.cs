using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ProgressTime")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ProgressTime", message: "Progress [Time]", category: "Events", id: "db34fbbcd158e0cb63bfde06c4f6195a")]
public sealed partial class ProgressTime : EventChannel<int> { }

