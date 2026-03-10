using Microsoft.Extensions.Logging;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Events;
using Wyoming.Net.Core.Server;

namespace Wyoming.Net.Satellite;

internal sealed class PingPongTask : TaskLoopRunner, IAsyncDisposable
{
    private static readonly Event CachedPing = new Ping().ToEvent();

    private const int PingDelaySeconds = 2;
    private const int PongDelaySeconds = 5;

    private readonly WyomingStreamWriter writer;
    private readonly SemaphoreSlim pongSignal = new SemaphoreSlim(0, 1);

    public PingPongTask(WyomingStreamWriter writer, ILogger<PingPongTask> logger) : base(logger)
    {
        this.writer = writer;
    }

    public void Pong()
    {
        Release();
    }

    protected override async Task LoopAsync()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(PingDelaySeconds));

            try
            {
                if (CancellationTokenSource is null || CancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                await writer.WriteEventAsync(CachedPing);

                bool received = await pongSignal.WaitAsync(
                    TimeSpan.FromSeconds(PongDelaySeconds),
                    CancellationTokenSource.Token);

                if (!received)
                {
                    logger.LogInformation("Timeout waiting pong from server");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred on PingPong task");
                throw;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        pongSignal.Dispose();
    }

    protected override ValueTask OnStopAsync()
    {
        Release();
        return base.OnStopAsync();
    }

    private void Release()
    {
        try
        {
            pongSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            // Already signaled; ignore duplicate pong
        }
    }
}
