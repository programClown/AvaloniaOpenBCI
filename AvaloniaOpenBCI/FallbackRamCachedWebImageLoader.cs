using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;
using AvaloniaOpenBCI.Extensions;
using AvaloniaOpenBCI.Helper;

namespace AvaloniaOpenBCI;

public readonly record struct ImageLoadFailedEventArgs(string Url, Exception Exception);

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class FallbackRamCachedWebImageLoader : RamCachedWebImageLoader
{
    readonly private WeakEventManager<ImageLoadFailedEventArgs> _loadFailEventHandler = new();

    public event EventHandler<ImageLoadFailedEventArgs> LoadFailed
    {
        add => _loadFailEventHandler.AddEventHandler(value);
        remove => _loadFailEventHandler.RemoveEventHandler(value);
    }

    protected void OnLoadFailed(string url, Exception exception) =>
        _loadFailEventHandler.RaiseEvent(
            this,
            new ImageLoadFailedEventArgs(url, exception),
            nameof(LoadFailed)
        );

    /// <summary>
    ///     Attempts to load bitmap
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Bitmap</returns>
    protected override async Task<Bitmap?> LoadAsync(string url)
    {
        // Try to load from local file first
        if (File.Exists(url))
        {
            try
            {
                if (!url.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    return new Bitmap(url);
                }

                using MemoryStream? stream = ImageMetadata.BuildImageWithoutMetadata(url);
                return stream == null ? new Bitmap(url) : new Bitmap(stream);
            }
            catch (Exception e)
            {
                OnLoadFailed(url, e);
                return null;
            }
        }

        Bitmap? internalOrCachedBitmap =
            await LoadFromInternalAsync(url).ConfigureAwait(false)
            ?? await LoadFromGlobalCache(url).ConfigureAwait(false);

        if (internalOrCachedBitmap != null)
        {
            return internalOrCachedBitmap;
        }

        try
        {
            byte[]? externalBytes = await LoadDataFromExternalAsync(url).ConfigureAwait(false);
            if (externalBytes == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream(externalBytes);
            var bitmap = new Bitmap(memoryStream);
            await SaveToGlobalCache(url, externalBytes).ConfigureAwait(false);
            return bitmap;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public void RemovePathFromCache(string filePath)
    {
        var cache =
            this.GetPrivateField<ConcurrentDictionary<string, Task<Bitmap?>>>("_memoryCache")
            ?? throw new NullReferenceException("Memory cache not found");
    }

    public void RemoveAllNamesFromCache(string fileName)
    {
        var cache =
            this.GetPrivateField<ConcurrentDictionary<string, Task<Bitmap?>>>("_memoryCache")
            ?? throw new NullReferenceException("Memory cache not found");

        foreach ((string key, var _) in cache)
        {
            if (Path.GetFileName(key) == fileName)
            {
                cache.TryRemove(key, out _);
            }
        }
    }
}