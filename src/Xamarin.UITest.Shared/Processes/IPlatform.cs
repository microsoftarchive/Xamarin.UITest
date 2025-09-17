namespace Xamarin.UITest.Shared.Processes
{
	internal interface IPlatform
	{
		bool IsWindows { get; }
		bool IsOSXOrUnix { get; }
		bool IsUnix { get; }
		bool IsOSX { get; }
	}
}