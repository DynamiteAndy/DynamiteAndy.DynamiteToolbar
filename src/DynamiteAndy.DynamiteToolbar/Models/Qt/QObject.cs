namespace DynamiteAndy.DynamiteToolbar.Models.Qt;

public abstract class QObject(IntPtr ptr) : IDisposable
{
    private bool _disposed;

    protected IntPtr Handle { get; private set; } = ptr;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (Handle != IntPtr.Zero)
        {
            Handle = IntPtr.Zero;
        }

        _disposed = true;
    }
}