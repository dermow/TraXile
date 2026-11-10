using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Support section of the Help page (classic DebugInfo window): shows the support text, copies it to the
    /// clipboard and creates the support bundle.
    /// </summary>
    public sealed class SupportViewModel : ObservableObject
    {
        private readonly Func<SupportContext> _context;
        private readonly Action<string> _copy;

        /// <param name="context">Collects the current state of the app (called on the UI thread).</param>
        /// <param name="copy">Puts text on the clipboard.</param>
        public SupportViewModel(string appDataPath, string appDir, Func<SupportContext> context, Action<string> copy)
        {
            AppDataPath = appDataPath;
            AppDir = appDir;
            _context = context;
            _copy = copy;
        }

        public string AppDataPath { get; }
        public string AppDir { get; }
        public string SupportFolder => SupportInfo.Folder(AppDataPath);

        private string _text = "";
        /// <summary>The support text (refreshed when the page opens and before it is copied).</summary>
        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        private string _status = "";
        /// <summary>Result of the last action (copied, bundle created, error).</summary>
        public string Status { get => _status; private set => SetProperty(ref _status, value); }

        private bool _statusIsError;
        public bool StatusIsError { get => _statusIsError; private set => SetProperty(ref _statusIsError, value); }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set { if (SetProperty(ref _isBusy, value)) OnPropertyChanged(nameof(IsIdle)); }
        }

        public bool IsIdle => !IsBusy;

        /// <summary>Path of the bundle created last (null before).</summary>
        public string LastBundle { get; private set; }

        public void Refresh() => Text = SupportInfo.BuildText(_context());

        public void CopyToClipboard()
        {
            Refresh();
            try
            {
                _copy(Text);
                SetStatus("Copied to the clipboard.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Could not copy: {ex.Message}", true);
            }
        }

        /// <summary>Creates the bundle in the background; true on success.</summary>
        public async Task<bool> CreateBundleAsync(DateTime now)
        {
            if (IsBusy) return false;
            Refresh();
            SupportContext ctx = _context();
            string text = Text;
            IsBusy = true;
            SetStatus("Creating the support bundle…", false);
            try
            {
                string zip = await Task.Run(() => SupportInfo.CreateBundle(AppDataPath, AppDir, ctx.ClientTxtPath, text, now));
                LastBundle = zip;
                long kb = new FileInfo(zip).Length / 1024;
                SetStatus($"Created {Path.GetFileName(zip)} ({kb:N0} KB).", false);
                return true;
            }
            catch (Exception ex)
            {
                SetStatus($"The support bundle could not be created: {ex.Message}", true);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetStatus(string text, bool error)
        {
            StatusIsError = error;
            Status = text;
        }
    }
}
