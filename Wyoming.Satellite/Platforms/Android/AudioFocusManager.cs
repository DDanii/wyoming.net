using Android.Content;
using Android.Media;

namespace Wyoming.Net.Satellite;

public sealed class AudioFocusManager : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
{
    private readonly AudioManager audioManager;
    private AudioFocusRequestClass? currentRequest;

    public AudioFocusManager()
    {
        audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService)!;
    }

    public void RequestTransientFocus()
    {
        AbandonFocus();
        
        var audioAttributes = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Media)!
            .SetContentType(AudioContentType.Speech)!
            .Build()!;

        currentRequest = new AudioFocusRequestClass.Builder(AudioFocus.GainTransientExclusive)
            .SetAudioAttributes(audioAttributes)
            .SetOnAudioFocusChangeListener(this)
            .Build();

        audioManager.RequestAudioFocus(currentRequest!);
    }

    public void AbandonFocus()
    {
        if (currentRequest is null)
        {
            return;
        }

        audioManager.AbandonAudioFocusRequest(currentRequest);
        currentRequest = null;
    }

    public void OnAudioFocusChange(AudioFocus focusChange)
    {
        // We only hold transient focus for brief sounds, so we don't need to
        // react to focus loss -- we'll abandon focus when playback completes.
    }
}
