using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.IO;

namespace SPRView.Net.ViewModel;

/// <summary>
/// One queued image: the frame order is the list order, so the index is part
/// of the row instead of being implied by position.
/// </summary>
public sealed record ImageEntry(int Index, string Path, string Name);

/// <summary>
/// Image list editing and the animated preview built on it.
/// </summary>
public partial class CreateNewViewModel
{
    private List<ImageEntry> _entries = [];

    /// <summary>Rows for the list: position, file name, full path as a tip.</summary>
    public IReadOnlyList<ImageEntry> Entries => _entries;

    public async void AddImage()
    {
        var files = await Parent.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang?.CreateNew_AddImage_Title,
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });
        if (files.Count >= 1)
        {
            foreach (var file in files)
            {
                string? local = file.TryGetLocalPath();
                if (local != null)
                    m_aryImagePaths.Add(local);
            }
            RaiseListChanged();
        }
        ResetPreview();
    }

    public void RemoveImage()
    {
        if (m_aryImagePaths.Count <= 0)
            return;

        int removed = PathSelected;
        m_aryImagePaths.RemoveAt(removed);
        RebuildRows();
        // Keep the selection on the item that took the removed one's place.
        SetSelection(removed < m_aryImagePaths.Count ? removed : 0);
        NotifyListChanged();

        if (!ResetPreview())
            PreviewImage = null;
    }

    public void MoveupImage()
    {
        if (PathSelected <= 0)
            return;
        Swap(PathSelected, PathSelected - 1);
        SetSelection(PathSelected - 1);
        ResetPreview();
    }

    public void MovedownImage()
    {
        if (PathSelected >= m_aryImagePaths.Count - 1)
            return;
        Swap(PathSelected, PathSelected + 1);
        SetSelection(PathSelected + 1);
        ResetPreview();
    }

    private void Swap(int a, int b)
    {
        (m_aryImagePaths[a], m_aryImagePaths[b]) = (m_aryImagePaths[b], m_aryImagePaths[a]);
        RebuildRows();
    }

    /// <summary>
    /// Moves the selection without touching the preview frame; used by the list
    /// operations, which refresh the preview themselves afterwards.
    /// </summary>
    private void SetSelection(int index)
    {
        m_iPathSelected = Math.Clamp(index, 0, Math.Max(0, m_aryImagePaths.Count - 1));
        OnPropertyChanged(nameof(PathSelected));
    }

    private void RebuildRows()
    {
        _entries = [.. m_aryImagePaths.Select((path, i) =>
            new ImageEntry(i + 1, path, Path.GetFileName(path)))];
    }

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(Entries));
        OnPropertyChanged(nameof(ImageCountLabel));
        OnPropertyChanged(nameof(Preview_MaxFrame));
        OnPropertyChanged(nameof(SaveValid));
    }

    /// <summary>Rebuilds the rows and notifies everything that depends on them.</summary>
    private void RaiseListChanged()
    {
        RebuildRows();
        // The list may have shrunk past the current selection.
        SetSelection(PathSelected);
        NotifyListChanged();
    }

    #region Preview
    private readonly Avalonia.Threading.DispatcherTimer animation_timer;

    public void StartPreview()
    {
        if (m_aryImagePaths.Count <= 1)
            return;
        if (!animation_timer.IsEnabled)
        {
            animation_timer.Interval = TimeSpan.FromSeconds(1.0 / Math.Max(1, PlaySpeed));
            ShowFrame(0);
            animation_timer.Start();
        }
        else
            StopPreview();
    }

    public void StopPreview()
    {
        if (animation_timer.IsEnabled)
            animation_timer.Stop();
    }

    private bool ResetPreview()
    {
        StopPreview();
        if (m_aryImagePaths.Count == 0)
            return false;
        ShowFrame(0);
        return true;
    }

    /// <summary>
    /// Shows one frame without moving the list selection, so the preview slider
    /// and the list can be used independently.
    /// </summary>
    private void ShowFrame(int frame)
    {
        int index = Math.Clamp(frame, 0, Math.Max(0, m_aryImagePaths.Count - 1));
        m_Preview_Frame = index;
        if (m_aryImagePaths.Count > 0)
            PreviewImage = new Bitmap(m_aryImagePaths[index]);
        OnPropertyChanged(nameof(Preview_Frame));
        OnPropertyChanged(nameof(PreviewFrameLabel));
    }

    private Bitmap? m_previewImage;
    public Bitmap? PreviewImage
    {
        get => m_previewImage;
        set
        {
            m_previewImage?.Dispose();
            m_previewImage = value;
            OnPropertyChanged(nameof(PreviewImage));
        }
    }

    private int m_Preview_Frame = 0;
    public int Preview_Frame
    {
        get => m_Preview_Frame;
        set => ShowFrame(value);
    }

    public int Preview_MaxFrame => Math.Max(0, m_aryImagePaths.Count - 1);

    public string PreviewFrameLabel => $"{m_Preview_Frame + 1} / {Math.Max(1, m_aryImagePaths.Count)}";
    #endregion
}
