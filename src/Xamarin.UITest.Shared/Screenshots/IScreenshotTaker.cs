using System.IO;

namespace Xamarin.UITest.Shared.Screenshots
{
	public interface IScreenshotTaker
	{
		FileInfo Screenshot(string title);
	}
}