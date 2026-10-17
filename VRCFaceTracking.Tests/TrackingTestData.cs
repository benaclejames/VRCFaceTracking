using Microsoft.Extensions.Logging.Abstractions;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Sandboxing.IPC;
using VRCFaceTracking.Core.Types;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace VRCFaceTracking.Tests;

internal static class TrackingTestData
{
    internal const float Unset = (float)uint.MaxValue;
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    internal static UnifiedTrackingData Frame(bool second = false)
    {
        var data = new UnifiedTrackingData();
        data.Eye.Left = new UnifiedSingleEyeData
        {
            Gaze = new Vector2(second ? .3f : .1f, second ? .4f : .2f),
            PupilDiameter_MM = second ? 5f : 3f,
            Openness = second ? .75f : .25f
        };
        data.Eye.Right = new UnifiedSingleEyeData
        {
            Gaze = new Vector2(second ? -.3f : -.1f, second ? -.4f : -.2f),
            PupilDiameter_MM = second ? 7f : 5f,
            Openness = second ? .5f : 1f
        };
        data.Eye._minDilation = second ? -2f : 0f;
        data.Eye._maxDilation = second ? 14f : 8f;
        data.Head = new UnifiedHeadData
        {
            HeadYaw = .1f, HeadPitch = .2f, HeadRoll = .3f,
            HeadPosX = .4f, HeadPosY = .5f, HeadPosZ = second ? .75f : .25f
        };
        for (var i = 0; i < data.Shapes.Length; i++)
            data.Shapes[i].Weight = second ? .75f : .25f;
        return data;
    }

    internal static UnifiedTrackingData UnsetFrame()
    {
        var data = new UnifiedTrackingData();
        data.Eye.Left = data.Eye.Right = new UnifiedSingleEyeData
        {
            Gaze = new Vector2(Unset, Unset), PupilDiameter_MM = Unset, Openness = Unset
        };
        data.Eye._maxDilation = data.Eye._minDilation = Unset;
        data.Head = new UnifiedHeadData
        {
            HeadYaw = Unset, HeadPitch = Unset, HeadRoll = Unset,
            HeadPosX = Unset, HeadPosY = Unset, HeadPosZ = Unset
        };
        for (var i = 0; i < data.Shapes.Length; i++)
            data.Shapes[i].Weight = Unset;
        return data;
    }

    internal static byte[] Serialize(UnifiedTrackingData data)
    {
        var snapshot = new TrackingDataSnapshot();
        snapshot.Publish(data);
        return new ReplyUpdatePacket(snapshot).GetBytes();
    }

    internal static ReplyUpdatePacket Packet(UnifiedTrackingData data)
    {
        var packet = new ReplyUpdatePacket();
        packet.Decode(Serialize(data));
        return packet;
    }

    internal static void AssertEyeFrame(UnifiedTrackingData data)
    {
        var second = data.Eye.Left.PupilDiameter_MM == 5f;
        Assert.Equal(second ? 5f : 3f, data.Eye.Left.PupilDiameter_MM);
        Assert.Equal(second ? 7f : 5f, data.Eye.Right.PupilDiameter_MM);
        Assert.Equal(second ? -2f : 0f, data.Eye._minDilation);
        Assert.Equal(second ? 14f : 8f, data.Eye._maxDilation);
        Assert.Equal(.5f, data.Eye.Combined().PupilDiameter_MM);
    }

    internal sealed class GlobalState : IDisposable
    {
        private readonly UnifiedTrackingData _data = UnifiedTracking.Data;
        private readonly UnifiedTrackingMutator _mutator = UnifiedTracking.Mutator;
        private readonly Action<UnifiedTrackingData> _callback = UnifiedTracking.OnUnifiedDataUpdated;
        internal readonly UnifiedTrackingMutator Mutator;

        internal GlobalState()
        {
            UnifiedTracking.Data = Frame();
            Mutator = new UnifiedTrackingMutator(NullLogger<UnifiedTrackingMutator>.Instance, new Settings());
            UnifiedTracking.OnUnifiedDataUpdated = _ => { };
        }

        public void Dispose()
        {
            UnifiedTracking.OnUnifiedDataUpdated = _callback;
            UnifiedTracking.Mutator = _mutator;
            UnifiedTracking.Data = _data;
        }
    }

    internal sealed class Settings : ILocalSettingsService
    {
        public Task<T> ReadSettingAsync<T>(string key, T? defaultValue = default, bool forceLocal = false)
            => Task.FromResult(defaultValue!);
        public Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false) => Task.CompletedTask;
        public Task Save(object target) => Task.CompletedTask;
        public Task Load(object target) => Task.CompletedTask;
    }
}
