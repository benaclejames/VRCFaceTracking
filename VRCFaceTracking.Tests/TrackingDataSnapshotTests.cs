using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Tests;

public class TrackingDataSnapshotTests
{
    [Fact]
    public void PublishedPacketPreservesTheExistingWireLayout()
    {
        using var state = new TrackingTestData.GlobalState();
        var source = TrackingTestData.Frame();
        UnifiedTracking.Data = source;
        var actual = TrackingTestData.Serialize(source);
        var expected = new byte[12 + 64 + source.Shapes.Length * sizeof(float)];
        new byte[] { 0xAF, 0xEC, 0, 0x8D }.CopyTo(expected, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(4), 302);
        BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(8), expected.Length - 12);
        float[] fields = [8f, 0f, .1f, .2f, 3f, .25f, -.1f, -.2f, 5f, 1f,
            .1f, .2f, .3f, .4f, .5f, .25f];
        for (var i = 0; i < fields.Length; i++)
            BinaryPrimitives.WriteSingleLittleEndian(expected.AsSpan(12 + i * sizeof(float)), fields[i]);
        for (var i = 0; i < source.Shapes.Length; i++)
            BinaryPrimitives.WriteSingleLittleEndian(expected.AsSpan(76 + i * sizeof(float)), .25f);

        Assert.Equal(expected, actual);
        Assert.Equal(expected, new ReplyUpdatePacket().GetBytes());
    }

    [Fact]
    public void SerializationPreservesAllFloatBitsIncludingUnsetAndNanPayloads()
    {
        using var state = new TrackingTestData.GlobalState();
        uint[] bits = [0x80000000, 0x7F800000, 0xFF800000, 0x7FC12345,
            0x7FA12345, 0xFFC54321, 0x4F800000, 0x00000001,
            0x80000001, 0x7F7FFFFF, 0xFF7FFFFF, 0x3F800000,
            0xBF800000, 0x00000000, 0x00800000, 0x807FFFFF];
        float Value(int index) => BitConverter.UInt32BitsToSingle(bits[index % bits.Length]);
        var source = new UnifiedTrackingData();
        source.Eye._maxDilation = Value(0);
        source.Eye._minDilation = Value(1);
        source.Eye.Left.Gaze.x = Value(2);
        source.Eye.Left.Gaze.y = Value(3);
        source.Eye.Left.PupilDiameter_MM = Value(4);
        source.Eye.Left.Openness = Value(5);
        source.Eye.Right.Gaze.x = Value(6);
        source.Eye.Right.Gaze.y = Value(7);
        source.Eye.Right.PupilDiameter_MM = Value(8);
        source.Eye.Right.Openness = Value(9);
        source.Head.HeadYaw = Value(10);
        source.Head.HeadPitch = Value(11);
        source.Head.HeadRoll = Value(12);
        source.Head.HeadPosX = Value(13);
        source.Head.HeadPosY = Value(14);
        source.Head.HeadPosZ = Value(15);
        for (var i = 0; i < source.Shapes.Length; i++)
            source.Shapes[i].Weight = Value(i + 16);
        var expected = new byte[12 + (16 + source.Shapes.Length) * sizeof(float)];
        Assert.Equal(expected.Length - 12, Marshal.SizeOf(typeof(ReplyUpdatePacket)
            .GetNestedType("UpdateDataContiguous", BindingFlags.NonPublic)!));
        new byte[] { 0xAF, 0xEC, 0, 0x8D }.CopyTo(expected, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(4), 302);
        BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(8), expected.Length - 12);
        for (var i = 0; i < 16 + source.Shapes.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12 + i * sizeof(float)), bits[i % bits.Length]);

        UnifiedTracking.Data = source;
        Assert.Equal(expected, new ReplyUpdatePacket().GetBytes());
        Assert.Equal(expected, TrackingTestData.Serialize(source));
    }

    [Fact]
    public void PublishDoesNotAdvanceAutomaticPupilCalibration()
    {
        var source = TrackingTestData.Frame();
        source.Eye._maxDilation = 0;
        source.Eye._minDilation = 999;
        var snapshot = new TrackingDataSnapshot();
        snapshot.Publish(source);
        var bytes = new ReplyUpdatePacket(snapshot).GetBytes();

        Assert.Equal(0f, source.Eye._maxDilation);
        Assert.Equal(999f, source.Eye._minDilation);
        Assert.Equal(0f, source.Eye._leftDiameter);
        Assert.Equal(0f, source.Eye._rightDiameter);
        Assert.Equal(0f, BitConverter.ToSingle(bytes, 12));
        Assert.Equal(999f, BitConverter.ToSingle(bytes, 16));
    }

    [Fact]
    public void UnpublishedChangesDoNotAlterTheCompletedFrame()
    {
        var source = TrackingTestData.Frame();
        var snapshot = new TrackingDataSnapshot();
        snapshot.Publish(source);
        var packet = new ReplyUpdatePacket(snapshot);
        var completed = packet.GetBytes();
        source.Eye = TrackingTestData.Frame(true).Eye;
        source.Shapes[0].Weight = .9f;
        source.Head.HeadPosZ = .9f;

        Assert.Equal(completed, packet.GetBytes());
        snapshot.Publish(source);
        Assert.NotEqual(completed, packet.GetBytes());
    }

    [Fact]
    public async Task SerializationFinishesWhileTheProducerIsPausedMidUpdate()
    {
        using var state = new TrackingTestData.GlobalState();
        var source = TrackingTestData.Frame();
        source.Eye.Left.PupilDiameter_MM = source.Eye.Right.PupilDiameter_MM = 4f;
        source.Eye._minDilation = -4f;
        source.Eye._maxDilation = 12f;
        UnifiedTracking.Data = source;
        var snapshot = new TrackingDataSnapshot();
        snapshot.Publish(source);
        var packet = new ReplyUpdatePacket(snapshot);
        var completed = packet.GetBytes();
        using var halfway = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        var producer = Task.Run(() =>
        {
            source.Eye._maxDilation = 8f;
            halfway.Set();
            Assert.True(resume.Wait(TrackingTestData.Timeout));
            source.Eye._minDilation = 0f;
            snapshot.Publish(source);
        });
        try
        {
            Assert.True(halfway.Wait(TrackingTestData.Timeout));
            var liveBytes = new ReplyUpdatePacket().GetBytes();
            Assert.Equal(8f, BitConverter.ToSingle(liveBytes, 12));
            Assert.Equal(-4f, BitConverter.ToSingle(liveBytes, 16));
            Assert.Equal(completed, await Task.Run(packet.GetBytes).WaitAsync(TrackingTestData.Timeout));
        }
        finally
        {
            resume.Set();
            await producer.WaitAsync(TrackingTestData.Timeout);
        }

        Assert.Equal(8f, BitConverter.ToSingle(packet.GetBytes(), 12));
        Assert.Equal(0f, BitConverter.ToSingle(packet.GetBytes(), 16));
    }

    [Fact]
    public async Task ConcurrentPublicationAndSerializationNeverMixCompletedFrames()
    {
        var first = TrackingTestData.Frame();
        var second = TrackingTestData.Frame(true);
        var firstBytes = TrackingTestData.Serialize(first);
        var secondBytes = TrackingTestData.Serialize(second);
        var source = TrackingTestData.Frame();
        var snapshot = new TrackingDataSnapshot();
        snapshot.Publish(source);
        using var start = new ManualResetEventSlim();
        var producer = Task.Run(() =>
        {
            Assert.True(start.Wait(TrackingTestData.Timeout));
            for (var i = 0; i < 3000; i++)
            {
                var next = (i & 1) == 0 ? second : first;
                source.Eye.Left = next.Eye.Left;
                Thread.Yield();
                source.Eye.Right = next.Eye.Right;
                source.Eye._maxDilation = next.Eye._maxDilation;
                Thread.Yield();
                source.Eye._minDilation = next.Eye._minDilation;
                source.Head = next.Head;
                Array.Copy(next.Shapes, source.Shapes, source.Shapes.Length);
                snapshot.Publish(source);
            }
        });
        var reader = Task.Run(() =>
        {
            var packet = new ReplyUpdatePacket(snapshot);
            start.Set();
            for (var i = 0; i < 3000; i++)
            {
                var bytes = packet.GetBytes();
                Assert.True(bytes.AsSpan().SequenceEqual(firstBytes) || bytes.AsSpan().SequenceEqual(secondBytes));
                Thread.Yield();
            }
        });
        await Task.WhenAll(producer, reader).WaitAsync(TrackingTestData.Timeout);
    }
}
