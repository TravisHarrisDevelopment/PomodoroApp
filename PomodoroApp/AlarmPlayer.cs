using System.Media;

namespace PomodoroApp;

public class AlarmPlayer : IDisposable
{
    private SoundPlayer? _player;
    private bool _disposed;

    public AlarmPlayer()
    {
        var assembly = typeof(AlarmPlayer).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Alarm04.wav"));
        if (resourceName is not null)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                _player = new SoundPlayer(stream);
                _player.Load();
            }
        }
    }

    public void Play()
    {
        if (_player is not null)
            _player.Play();
        else
            SystemSounds.Exclamation.Play();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _player?.Dispose();
            _disposed = true;
        }
    }
}