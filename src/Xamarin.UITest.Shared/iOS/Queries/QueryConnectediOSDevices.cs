using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.iOS.Queries
{
    public class QueryConnectediOSDevices : IQuery<DeviceInfo[]>
    {
        readonly string _targetDeviceIdentifier;

        public QueryConnectediOSDevices(string targetDeviceIdentifier)
        {
            _targetDeviceIdentifier = targetDeviceIdentifier;
        }

        public DeviceInfo[] Execute()
        {
            try
            {
                return new DeviceFinder().GetConnectedDevices(_targetDeviceIdentifier);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to find connected iOS devices.", ex);
            }
        }

        class DeviceFinder
        {
            private DeviceNotificationDelegate DeviceNotificationDelegate;
            readonly List<DeviceInfo> deviceList = new List<DeviceInfo>();

            public DeviceInfo[] GetConnectedDevices(string targetDeviceIdentifier)
            {
                deviceList.Clear();

                IntPtr context;

                DeviceNotificationDelegate = new DeviceNotificationDelegate((ref am_device_notification_callback_info x) =>
                {
                    // For some reason trying to access deviceList may sometimes throw a null reference exception
                    // where even checking to see if it is null (e.x. deviceList == null) will throw a null
                    // reference exception.
                    lock (deviceList)
                    {
                        var deviceInfo = GetDeviceInfo(ref x);

                        // Make sure that a device with this UDID is not already in the list
                        if (deviceInfo != null && !deviceList.Any(d => d != null && d.GetUUID().Equals(deviceInfo.GetUUID())))
                        {
                            deviceList.Add(deviceInfo);
                        }
                    }
                });

                uint ret = NativeMethods.AMDeviceNotificationSubscribe(DeviceNotificationDelegate, 0, 0, 0, out context);

                if (ret != 0)
                {
                    throw new Exception("Call to AMDeviceNotificationSubscribe failed with error code: " + ret);
                }

                var wait = new ManualResetEvent(false);
                IntPtr handle = NativeMethods.CFRunLoopGetCurrent();

                if (!targetDeviceIdentifier.IsNullOrWhiteSpace())
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        for (var i = 0; i < 50; i++)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(50));

                            lock (deviceList)
                            {
                                if (deviceList.Any(x => x.GetUUID().Equals(targetDeviceIdentifier)))
                                {
                                    break;
                                }
                            }
                        }

                        NativeMethods.CFRunLoopStop(handle);
                        wait.Set();
                    });
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(50));
                        NativeMethods.CFRunLoopStop(handle);
                        wait.Set();
                    });
                }

                NativeMethods.CFRunLoopRun();
                WaitHandle.WaitAll(new WaitHandle[] { wait });
                NativeMethods.AMDeviceNotificationUnsubscribe(context);

                return deviceList.ToArray();
            }

            DeviceInfo GetDeviceInfo(ref am_device_notification_callback_info info)
            {
                if (info.message == 1)
                {
                    NativeMethods.AMDeviceConnect(info.am_device);
                    IntPtr setting = NativeMethods.CreateString("BuildVersion");
                    string buildVersion = NativeMethods.FetchString(NativeMethods.AMDeviceCopyValue(info.am_device, 0, setting));
                    Marshal.FreeCoTaskMem(setting);
                    string UUID = NativeMethods.FetchString(NativeMethods.AMDeviceCopyDeviceIdentifier(info.am_device));
                    NativeMethods.AMDeviceDisconnect(info.am_device);

                    // If the buildVersion is null, it may signify that the device it found isn't currently connected to the PC,
                    // and it may just be picking the device up due to iTunes WiFi Sync
                    if (UUID != null && buildVersion != null)
                    {
                        return new DeviceInfo(UUID, buildVersion);
                    }
                }

                return null;
            }
        }

        static class NativeMethods
        {
            public const string MobileDeviceLibrary = "/System/Library/PrivateFrameworks/MobileDevice.framework/MobileDevice";
            public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

            [DllImport(CoreFoundationLibrary)]
            public extern static IntPtr CFRunLoopGetCurrent();

            [DllImport(CoreFoundationLibrary)]
            public extern static void CFRunLoopRun();

            [DllImport(CoreFoundationLibrary)]
            public extern static void CFRunLoopStop(IntPtr loop);


            [DllImport(CoreFoundationLibrary, CharSet = CharSet.Unicode)]
            extern static IntPtr CFStringCreateWithCharacters(IntPtr allocator, string str, long count);

            [DllImport(CoreFoundationLibrary, CharSet = CharSet.Unicode)]
            extern static long CFStringGetLength(IntPtr handle);

            [DllImport(CoreFoundationLibrary, CharSet = CharSet.Unicode)]
            extern static IntPtr CFStringGetCharactersPtr(IntPtr handle);

            [DllImport(CoreFoundationLibrary, CharSet = CharSet.Unicode)]
            extern static IntPtr CFStringGetCharacters(IntPtr handle, range range, IntPtr buffer);


            public static IntPtr CreateString(string s)
            {
                return CFStringCreateWithCharacters(IntPtr.Zero, s, s.Length);
            }

            public static string FetchString(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                    return null;

                string str;

                int l = (int)CFStringGetLength(handle);
                IntPtr u = CFStringGetCharactersPtr(handle);
                IntPtr buffer = IntPtr.Zero;
                if (u == IntPtr.Zero)
                {
                    range r = new range((IntPtr)0, (IntPtr)l);
                    buffer = Marshal.AllocCoTaskMem(l * 2);
                    CFStringGetCharacters(handle, r, buffer);
                    u = buffer;
                }

                str = Marshal.PtrToStringUni(u);

                if (str.Length > l)
                {
                    str = str.Substring(0, l);
                }

                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(buffer);
                }

                return str;
            }

            [DllImport(MobileDeviceLibrary)]
            public static extern uint AMDeviceNotificationSubscribe(DeviceNotificationDelegate callback, uint unused0, uint unused1, uint dn_unknown3, out IntPtr context);

            [DllImport(MobileDeviceLibrary)]
            public static extern uint AMDeviceNotificationUnsubscribe(IntPtr context);

            [DllImport(MobileDeviceLibrary)]
            public static extern IntPtr AMDeviceCopyDeviceIdentifier(IntPtr device);

            [DllImport(MobileDeviceLibrary)]
            public static extern IntPtr AMDeviceCopyValue(IntPtr device, uint domain, IntPtr devicesetting);

            [DllImport(MobileDeviceLibrary)]
            public static extern IntPtr AMDeviceConnect(IntPtr device);

            [DllImport(MobileDeviceLibrary)]
            public static extern IntPtr AMDeviceDisconnect(IntPtr device);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct range
        {
            public IntPtr loc;
            public IntPtr len;

            public range(IntPtr loc, IntPtr len)
            {
                this.loc = loc;
                this.len = len;
            }
        }

        struct am_device_notification_callback_info
        {
            public IntPtr am_device;
            public uint message;

            public am_device_notification_callback_info(IntPtr amDevice, uint message)
                : this()
            {
                am_device = amDevice;
                this.message = message;
            }
        }

        delegate void DeviceNotificationDelegate(ref am_device_notification_callback_info info);
    }
}
