using System.Reflection;
using VRCFaceTracking.Core.Sandboxing.IPC;
using VRCFaceTracking.ModuleProcess;

namespace VRCFaceTracking.Tests.ModuleReplyQueue;

internal static class Program
{
    private delegate bool TryDequeue(out IpcPacket? packet);

    private sealed class NumberedPacket(int producer, int sequence) : IpcPacket
    {
        public int Producer { get; } = producer;
        public int Sequence { get; } = sequence;
    }

    private static async Task<int> Main()
    {
        try
        {
            // Exercise the actual production queue without exposing it in the module API.
            // Binding its methods also lets this regression run against the original Queue.
            var field = typeof(ModuleProcessMain).GetField("_packetsToSend", BindingFlags.NonPublic | BindingFlags.Static)!;
            var queue = field.GetValue(null)!;
            var type = queue.GetType();
            var enqueue = type.GetMethod("Enqueue")!.CreateDelegate<Action<IpcPacket>>(queue);
            var dequeue = type.GetMethod("TryDequeue")!.CreateDelegate<TryDequeue>(queue);

            for (var round = 0; round < 6; round++)
            {
                await ExerciseQueue(enqueue, dequeue, 1, 100_000);
                Console.WriteLine($"PASS single-producer FIFO round {round + 1}: 100000 unique replies");
            }

            for (var round = 0; round < 3; round++)
            {
                await ExerciseQueue(enqueue, dequeue, 4, 50_000);
                Console.WriteLine($"PASS multiple-producer FIFO round {round + 1}: 200000 unique replies");
            }

            Console.WriteLine("PASS: 1200000 replies; exact counts, unique IDs, FIFO, no nulls or exceptions");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static async Task ExerciseQueue(Action<IpcPacket> enqueue, TryDequeue dequeue, int producerCount, int repliesPerProducer)
    {
        Check(!dequeue(out _), "Queue must be empty before each run");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var start = new ManualResetEventSlim();
        var token = timeout.Token;
        var total = producerCount * repliesPerProducer;
        var seen = new bool[total];
        var nextSequence = new int[producerCount];
        var tasks = new List<Task>();

        for (var producer = 0; producer < producerCount; producer++)
        {
            var producerId = producer;
            tasks.Add(Task.Factory.StartNew(() =>
            {
                start.Wait(token);
                for (var sequence = 0; sequence < repliesPerProducer; sequence++)
                {
                    token.ThrowIfCancellationRequested();
                    enqueue(new NumberedPacket(producerId, sequence));
                    if ((sequence & 255) == 0)
                    {
                        Thread.Yield();
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default));
        }

        tasks.Add(Task.Factory.StartNew(() =>
        {
            start.Wait(token);
            var received = 0;
            var spin = new SpinWait();
            while (received < total)
            {
                token.ThrowIfCancellationRequested();
                if (!dequeue(out var packet))
                {
                    spin.SpinOnce();
                    continue;
                }

                spin.Reset();
                Check(packet is NumberedPacket, "Dequeued an unexpected or null packet");
                var numbered = (NumberedPacket)packet!;
                var id = numbered.Producer * repliesPerProducer + numbered.Sequence;
                Check(!seen[id], $"Duplicate reply ID {id}");
                Check(numbered.Sequence == nextSequence[numbered.Producer], $"FIFO violation for producer {numbered.Producer}");
                seen[id] = true;
                nextSequence[numbered.Producer]++;
                received++;
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default));

        start.Set();
        await Task.WhenAll(tasks).WaitAsync(token);
        Check(!dequeue(out _), "Unexpected replies after the expected total");
        Check(seen.All(value => value), "Missing reply IDs");
        Check(nextSequence.All(value => value == repliesPerProducer), "Incorrect per-producer totals");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
