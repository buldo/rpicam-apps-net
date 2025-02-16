using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DrmPreview.Interop;

public static partial class libdrm
{
    private const string LibraryName = "libdrm";

    [LibraryImport(
        LibraryName,
        EntryPoint = "drmOpen",
        StringMarshalling = StringMarshalling.Utf8)]
    public static partial int drmOpen(string name, string? busid);

    [LibraryImport(
        LibraryName,
        EntryPoint = "drmIsMaster")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool drmIsMaster(int fd);
}