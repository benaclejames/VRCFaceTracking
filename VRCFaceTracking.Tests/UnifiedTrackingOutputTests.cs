using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.Tests;

public class UnifiedTrackingOutputTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmittedDataIsIndependentOfTheMutableHostData(bool mutationsEnabled)
    {
        using var state = new TrackingTestData.GlobalState();
        state.Mutator.Enabled = mutationsEnabled;
        UnifiedTracking.OnUnifiedDataUpdated = output =>
        {
            Assert.NotSame(UnifiedTracking.Data, output);
            Assert.NotSame(UnifiedTracking.Data.Eye, output.Eye);
            Assert.NotSame(UnifiedTracking.Data.Shapes, output.Shapes);
            TrackingTestData.Packet(TrackingTestData.Frame(true)).UpdateGlobalEyeState();
            TrackingTestData.AssertEyeFrame(output);
            Assert.Equal(3f, output.Eye.Left.PupilDiameter_MM);
            output.Shapes[0].Weight = .9f;
            Assert.Equal(.75f, UnifiedTracking.Data.Shapes[0].Weight);
        };

        UnifiedTracking.UpdateDataSync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BlockedSubscriberKeepsItsFrameWhileAnotherEmissionCompletes(bool asynchronousSecond)
    {
        using var state = new TrackingTestData.GlobalState();
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        UnifiedTrackingData? firstOutput = null;
        var callbacks = 0;
        UnifiedTracking.OnUnifiedDataUpdated = output =>
        {
            if (Interlocked.Increment(ref callbacks) == 1)
            {
                firstOutput = output;
                entered.Set();
                Assert.True(resume.Wait(TrackingTestData.Timeout));
                Assert.Equal(3f, output.Eye.Left.PupilDiameter_MM);
                Assert.Equal(5f, output.Eye.Right.PupilDiameter_MM);
                Assert.Equal(.25f, output.Shapes[0].Weight);
                Assert.Equal(.5f, output.Eye.Combined().PupilDiameter_MM);
            }
            else
            {
                Assert.NotSame(firstOutput, output);
                Assert.NotSame(firstOutput!.Eye, output.Eye);
                Assert.NotSame(firstOutput.Shapes, output.Shapes);
                Assert.Equal(5f, output.Eye.Left.PupilDiameter_MM);
                TrackingTestData.AssertEyeFrame(output);
            }
        };
        var first = Task.Run(UnifiedTracking.UpdateDataSync);
        try
        {
            Assert.True(entered.Wait(TrackingTestData.Timeout));
            TrackingTestData.Packet(TrackingTestData.Frame(true)).UpdateGlobalEyeState();
            var second = asynchronousSecond
                ? UnifiedTracking.UpdateData(CancellationToken.None)
                : Task.Run(UnifiedTracking.UpdateDataSync);
            await second.WaitAsync(TrackingTestData.Timeout);
            Assert.Equal(2, Volatile.Read(ref callbacks));
        }
        finally
        {
            resume.Set();
            await first.WaitAsync(TrackingTestData.Timeout);
        }
    }

    [Fact]
    public async Task CanceledAsyncEmissionDoesNotInvokeSubscribersOrPreventTheNextEmission()
    {
        using var state = new TrackingTestData.GlobalState();
        var callbacks = 0;
        UnifiedTracking.OnUnifiedDataUpdated = _ => callbacks++;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => UnifiedTracking.UpdateData(cancellation.Token));
        Assert.Equal(0, callbacks);
        await UnifiedTracking.UpdateData(CancellationToken.None);
        Assert.Equal(1, callbacks);
    }

    [Fact]
    public void SubscriberFailureDoesNotCorruptTheNextEmission()
    {
        using var state = new TrackingTestData.GlobalState();
        UnifiedTracking.OnUnifiedDataUpdated = _ => throw new InvalidOperationException("subscriber failed");
        Assert.Throws<InvalidOperationException>(UnifiedTracking.UpdateDataSync);
        TrackingTestData.Packet(TrackingTestData.Frame(true)).UpdateGlobalEyeState();
        var callbacks = 0;
        UnifiedTracking.OnUnifiedDataUpdated = output =>
        {
            callbacks++;
            TrackingTestData.AssertEyeFrame(output);
            Assert.Equal(5f, output.Eye.Left.PupilDiameter_MM);
        };

        UnifiedTracking.UpdateDataSync();
        Assert.Equal(1, callbacks);
    }

    [Fact]
    public void EnabledMutationChangesOnlyTheEmittedSnapshot()
    {
        using var state = new TrackingTestData.GlobalState();
        state.Mutator.Enabled = true;
        state.Mutator._mutations.Add(new TestMutation());
        UnifiedTracking.OnUnifiedDataUpdated = output => Assert.Equal(.5f, output.Shapes[0].Weight);

        UnifiedTracking.UpdateDataSync();
        Assert.Equal(.25f, UnifiedTracking.Data.Shapes[0].Weight);
    }

    [Fact]
    public void MutationMayReplaceItsInputWithoutExposingItsOwnedDataToSubscribers()
    {
        using var state = new TrackingTestData.GlobalState();
        var replacement = TrackingTestData.Frame(true);
        state.Mutator.Enabled = true;
        state.Mutator._mutations.Add(new TestMutation(replacement));
        UnifiedTracking.OnUnifiedDataUpdated = output =>
        {
            Assert.NotSame(replacement, output);
            Assert.NotSame(replacement.Eye, output.Eye);
            Assert.NotSame(replacement.Shapes, output.Shapes);
            TrackingTestData.AssertEyeFrame(output);
            Assert.Equal(5f, output.Eye.Left.PupilDiameter_MM);
            output.Shapes[0].Weight = .9f;
            Assert.Equal(.75f, replacement.Shapes[0].Weight);
        };

        UnifiedTracking.UpdateDataSync();
        Assert.Equal(3f, UnifiedTracking.Data.Eye.Left.PupilDiameter_MM);
    }

    private sealed class TestMutation : TrackingMutation
    {
        private readonly UnifiedTrackingData? _replacement;
        public override string Name => "Test";
        public override string Description => "Test";
        public override MutationPriority Step => MutationPriority.None;

        internal TestMutation(UnifiedTrackingData? replacement = null)
        {
            _isActive = true;
            _replacement = replacement;
        }

        public override void MutateData(ref UnifiedTrackingData data)
        {
            if (_replacement is not null)
                data = _replacement;
            else
                data.Shapes[0].Weight += .25f;
        }
    }
}
