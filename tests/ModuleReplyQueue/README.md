# Module reply queue regression

Requires the .NET 10 SDK. From the repository root, run:

```sh
dotnet run --project tests/ModuleReplyQueue/ModuleReplyQueue.csproj --configuration Release
```

The console exits with code 0 on success and 1 on failure. No headset, tracking
module, running host, network socket, or external test framework is required.

The test binds the actual private `ModuleProcessMain._packetsToSend` queue's
Enqueue/TryDequeue methods. It does not replace the queue with a test instance
or rely on a queue-type assertion. Separate producer and consumer threads start
together and transfer 1,200,000 uniquely numbered packets across nine runs:

- Six single-producer runs check exact FIFO order.
- Three four-producer runs check FIFO order within each producer. Concurrent
  producers have no predefined total order.
- Every run checks unique IDs, complete totals, an empty queue afterward,
  and the absence of null packets or exceptions. A 30-second per-run timeout
  prevents a lost reply from hanging the test indefinitely.

The test can also run against the original ordinary Queue declaration. Race
failures are scheduling-dependent; a passing stress run does not establish
thread safety. This test covers concurrent queue access, not IPC serialization,
transport, module lifecycle, or tracking performance.
