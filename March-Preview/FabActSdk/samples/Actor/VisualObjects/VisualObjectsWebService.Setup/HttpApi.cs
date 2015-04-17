using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace VisualObjectsWebService.Setup
{
    [Flags]
    internal enum HttpInitializeFlags : uint
    {
        Server = 0x01,
        Config = 0x02
    }

    internal static class Http
    {
        [DllImport("httpapi.dll", SetLastError=true)]
        static extern int HttpInitialize(
            HttpApiVersion version,
            HttpInitializeFlags flags,
            IntPtr reserved
            );

        [DllImport("httpapi.dll", SetLastError = true, ExactSpelling=true, PreserveSig=true)]
        static extern int HttpSetServiceConfiguration(
            IntPtr handle,
            HttpServiceConfigId configId,
            IntPtr info,
            int length,
            IntPtr overlapped
            );

        [DllImport("httpapi.dll", SetLastError = true, ExactSpelling = true, PreserveSig = true, EntryPoint="HttpSetServiceConfiguration")]
        static extern int SetAcl(
            [In] IntPtr handle,
            [In] HttpServiceConfigId configId,
            [In] UrlAcl urlAcl,
            [In] int length,
            [In] IntPtr overlapped
            );


        [DllImport("httpapi.dll")]
        static extern int HttpTerminate(HttpInitializeFlags flags, IntPtr mustBeZero);

        public static int Initialize()
        {
            HttpApiVersion v = new HttpApiVersion();
            v.HttpApiMajorVersion = 1;
            v.HttpApiMinorVersion = 0;

            return HttpInitialize(v, HttpInitializeFlags.Config, IntPtr.Zero);
        }

        public static int SetAcl(string url, string acl)
        {
            UrlAcl u = new UrlAcl();
            u.Prefix = url;
            u.Acl = acl;
            int rc = SetAcl(IntPtr.Zero, HttpServiceConfigId.UrlAclInfo, u, UrlAcl.Length, IntPtr.Zero);
            return rc;
        }

        public static int Terminate()
        {
            return HttpTerminate(HttpInitializeFlags.Config, IntPtr.Zero);
        }
    }

    enum HttpServiceConfigId
    {
      IPListenList = 0,
      SSLCertInfo = 1,
      UrlAclInfo = 2,
      Max = 3
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct HttpApiVersion
    {
        public ushort HttpApiMajorVersion;
        public ushort HttpApiMinorVersion;

        public HttpApiVersion(ushort majorVersion, ushort minorVersion)
        {
            HttpApiMajorVersion = majorVersion;
            HttpApiMinorVersion = minorVersion;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    class UrlAcl
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Prefix;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string Acl;

        public static int Length = Marshal.SizeOf(typeof(UrlAcl));
    }

}
