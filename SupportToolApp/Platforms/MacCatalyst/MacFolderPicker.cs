using Foundation;
using UIKit;

namespace SupportToolApp.Helpers;

internal static class MacFolderPicker
{
    public static Task<string?> PickAsync(string title)
    {
        var tcs = new TaskCompletionSource<string?>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // UIDocumentPickerViewController with public.folder picks directories on Mac Catalyst
#pragma warning disable CA1422
            var picker = new UIDocumentPickerViewController(
                new[] { "public.folder" }, UIDocumentPickerMode.Open);
#pragma warning restore CA1422
            picker.AllowsMultipleSelection = false;

            picker.DidPickDocumentAtUrls += (_, e) =>
            {
                var url = e.Urls?.FirstOrDefault();
                if (url != null)
                {
                    url.StartAccessingSecurityScopedResource();
                    tcs.TrySetResult(url.Path);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            };

            picker.WasCancelled += (_, _) => tcs.TrySetResult(null);

            var vc = UIApplication.SharedApplication.KeyWindow?.RootViewController;
            vc?.PresentViewController(picker, true, null);
        });

        return tcs.Task;
    }
}
