using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Xamarin.UITest.Utils;
using Xamarin.UITest.XDB.Enums;

namespace Xamarin.UITest.XDB.Services
{
    interface IiOSDeviceAgentService
    {
        Task DeleteSessionAsync(string deviceAddress);

        Task DoubleTouchAsync(string deviceAddress, PointF point);

        Task DragAsync(string deviceAddress, PointF from, PointF to, TimeSpan? duration, TimeSpan? holdTime, bool allowInertia = true);

        Task EnterTextAsync(string deviceAddress, string text);

        Task FlickAsync(string deviceAddress, PointF from, PointF to);

        Task GestureAsync(string deviceAddress, string gesture, object options = null, object specifiers = null);

        Task LaunchTestAsync(
            string deviceId,
            string deviceAddress);

        Task PinchAsync(
            string deviceAddress,
            PointF point, 
            PinchDirection direction, 
            float? amount = null, 
            TimeSpan? duration = null);

        Task PingAsync(
            string deviceAddress, 
            int attempts = 1, 
            TimeSpan? retryInterval = null,
            bool logErrors = true);

        Task<object> QueryAsync(string deviceAddress, object query);

        Task SetOrientationAsync(string deviceAddress, DeviceOrientation orientation);

        Task StartAppAsync(
            string deviceAddress, 
            string bundleId, 
            IEnumerable<string> launchArgs = null,
            IDictionary<string, string> environmentVars = null);

        Task StartAppAsync(
            string deviceAddress, 
            string bundleId, 
            string launchArgs,
            string environmentVars);

        Task ShutdownAsync(string deviceAddress, bool ignoreUnavailable);
        
        Task DismissSpringboardAlertsAsync(string deviceAddress);

        Task SetInputViewPickerWheelValueAsync(string deviceAddress, int pickerIndex, int wheelIndex, string value);

        Task TouchAndHoldAsync(string deviceAddress, PointF point, TimeSpan? duration);

        Task TouchAsync(string deviceAddress, PointF point);

        Task<UIElement> DumpElements(string deviceAddress);

        Task TwoFingerTouchAsync(string deviceAddress, PointF point);
        
        Task VolumeAsync(string deviceAddress, VolumeDirection direction);
    }
}
