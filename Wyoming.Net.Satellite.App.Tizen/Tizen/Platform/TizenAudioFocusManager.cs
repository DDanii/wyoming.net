using System;
using Microsoft.Extensions.Logging;
using Tizen.Multimedia;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class TizenAudioFocusManager : IDisposable
{
    private readonly AudioStreamPolicy streamPolicy;
    private readonly ILogger logger;
    private bool hasFocus;

    public TizenAudioFocusManager(ILogger logger)
    {
        this.logger = logger;

        streamPolicy = new AudioStreamPolicy(AudioStreamType.Media)
        {
            FocusReacquisitionEnabled = false,
        };

        streamPolicy.FocusStateChanged += OnFocusStateChanged;
    }

    public AudioStreamPolicy Policy => streamPolicy;

    public void RequestTransientFocus()
    {
        AbandonFocus();

        try
        {
            streamPolicy.AcquireFocus(AudioStreamFocusOptions.Playback, AudioStreamBehaviors.Fading, null);
            hasFocus = true;
            logger.LogInformation("Acquired transient playback focus");
        }
        catch (AudioPolicyException ex)
        {
            logger.LogWarning(ex, "Failed to acquire playback focus");
        }
    }

    public void AbandonFocus()
    {
        if (!hasFocus)
        {
            return;
        }

        try
        {
            streamPolicy.ReleaseFocus(AudioStreamFocusOptions.Playback, AudioStreamBehaviors.Fading, null);
            logger.LogInformation("Released playback focus");
        }
        catch (AudioPolicyException ex)
        {
            logger.LogWarning(ex, "Failed to release playback focus");
        }
        finally
        {
            hasFocus = false;
        }
    }

    private void OnFocusStateChanged(object? sender, AudioStreamPolicyFocusStateChangedEventArgs args)
    {
        // We only hold transient focus for brief sounds, so we don't need to
        // react to focus loss -- we'll abandon focus when playback completes.
        logger.LogInformation("Playback focus changed: {options} -> {state}", args.FocusOptions, args.FocusState);
    }

    public void Dispose()
    {
        AbandonFocus();
        streamPolicy.FocusStateChanged -= OnFocusStateChanged;
        streamPolicy.Dispose();
    }
}
