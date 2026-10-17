using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Tests;

public class PupilUpdateTests
{
    [Fact]
    public void UnsetPupilsRetainThePreviousRangeWhileOtherEyeFieldsUpdate()
    {
        using var state = new TrackingTestData.GlobalState();
        var update = TrackingTestData.UnsetFrame();
        update.Eye.Left.Gaze.x = .9f;
        update.Eye.Right.Openness = .7f;
        update.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight = .6f;
        TrackingTestData.Packet(update).UpdateGlobalEyeState();

        Assert.Equal(.9f, UnifiedTracking.Data.Eye.Left.Gaze.x);
        Assert.Equal(.7f, UnifiedTracking.Data.Eye.Right.Openness);
        Assert.Equal(.6f, UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight);
        TrackingTestData.AssertEyeFrame(UnifiedTracking.Data);
    }

    [Fact]
    public void PartialPupilUpdatePreservesUnsetFieldsAndAcceptsZero()
    {
        using var state = new TrackingTestData.GlobalState();
        var update = TrackingTestData.UnsetFrame();
        update.Eye.Left.PupilDiameter_MM = 0f;
        update.Eye._maxDilation = 10f;
        TrackingTestData.Packet(update).UpdateGlobalEyeState();

        Assert.Equal(0f, UnifiedTracking.Data.Eye.Left.PupilDiameter_MM);
        Assert.Equal(5f, UnifiedTracking.Data.Eye.Right.PupilDiameter_MM);
        Assert.Equal(0f, UnifiedTracking.Data.Eye._minDilation);
        Assert.Equal(10f, UnifiedTracking.Data.Eye._maxDilation);
    }

    [Fact]
    public void InvertedExplicitRangeRejectsTheEntireEyeUpdate()
    {
        using var state = new TrackingTestData.GlobalState();
        var update = TrackingTestData.Frame(true);
        update.Eye._minDilation = 10f;
        update.Eye._maxDilation = 2f;
        TrackingTestData.Packet(update).UpdateGlobalEyeState();

        TrackingTestData.AssertEyeFrame(UnifiedTracking.Data);
        Assert.Equal(.1f, UnifiedTracking.Data.Eye.Left.Gaze.x);
        Assert.Equal(.25f, UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AutomaticCalibrationIsRetainedAcrossEmittedSnapshots(bool mutationsEnabled)
    {
        using var state = new TrackingTestData.GlobalState();
        state.Mutator.Enabled = mutationsEnabled;
        UnifiedTracking.Data = new UnifiedTrackingData();
        var dilations = new List<float>();
        UnifiedTracking.OnUnifiedDataUpdated = data => dilations.Add(data.Eye.Combined().PupilDiameter_MM);
        foreach (var diameter in new[] { 4f, 2f, 3f })
        {
            var update = TrackingTestData.UnsetFrame();
            update.Eye.Left.PupilDiameter_MM = update.Eye.Right.PupilDiameter_MM = diameter;
            TrackingTestData.Packet(update).UpdateGlobalEyeState();
            UnifiedTracking.UpdateDataSync();
        }

        Assert.Equal(new[] { 1f, 0f, .5f }, dilations);
        Assert.Equal(2f, UnifiedTracking.Data.Eye._minDilation);
        Assert.Equal(4f, UnifiedTracking.Data.Eye._maxDilation);
        Assert.Equal(3f, UnifiedTracking.Data.Eye._leftDiameter);
        Assert.Equal(3f, UnifiedTracking.Data.Eye._rightDiameter);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConcurrentHostApplicationAndEmissionKeepPupilFramesTogether(bool mutationsEnabled)
    {
        using var state = new TrackingTestData.GlobalState();
        state.Mutator.Enabled = mutationsEnabled;
        var first = TrackingTestData.Packet(TrackingTestData.Frame());
        var second = TrackingTestData.Packet(TrackingTestData.Frame(true));
        var emitted = 0;
        UnifiedTracking.OnUnifiedDataUpdated = data =>
        {
            TrackingTestData.AssertEyeFrame(data);
            Interlocked.Increment(ref emitted);
        };
        using var start = new ManualResetEventSlim();
        var writer = Task.Run(() =>
        {
            Assert.True(start.Wait(TrackingTestData.Timeout));
            for (var i = 0; i < 4000; i++)
            {
                ((i & 1) == 0 ? second : first).UpdateGlobalEyeState();
                Thread.Yield();
            }
        });
        var emitter = Task.Run(() =>
        {
            start.Set();
            for (var i = 0; i < 4000; i++)
                UnifiedTracking.UpdateDataSync();
        });
        await Task.WhenAll(writer, emitter).WaitAsync(TrackingTestData.Timeout);

        Assert.Equal(4000, emitted);
    }
}
