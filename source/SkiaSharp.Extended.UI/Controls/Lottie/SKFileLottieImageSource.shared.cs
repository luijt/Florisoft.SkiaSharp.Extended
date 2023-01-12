using System.Reflection;
using Microsoft.Maui.Storage;

namespace SkiaSharp.Extended.UI.Controls;

public class SKFileLottieImageSource : SKLottieImageSource
{
	public static BindableProperty FileProperty = BindableProperty.Create(
		nameof(File), typeof(string), typeof(SKFileLottieImageSource),
		propertyChanged: OnSourceChanged);

	public string? File
	{
		get => (string?)GetValue(FileProperty);
		set => SetValue(FileProperty, value);
	}

	public override bool IsEmpty =>
		string.IsNullOrEmpty(File);

	internal override async Task<Skottie.Animation?> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || string.IsNullOrEmpty(File))
			return null;

		try
		{
			using var stream = await LoadFile(File);

			if (stream is null)
				return null;

			return Skottie.Animation.Create(stream);
		}
		catch (Exception ex)
		{
			throw new ArgumentException($"Unable to load Lottie animation \"{File}\".", ex);
		}
	}

	private static async Task<Stream?> LoadFile(string filename)
	{
		Stream? result = null;
		try
		{
			result = await FileSystem.OpenAppPackageFileAsync(filename).ConfigureAwait(false);
		}
		catch (FileNotFoundException)
		{
			result = LoadEmbeddedResource("ResourceLib", filename);
		}
		return result;
	}

	const string RESOURCES_FOLDER = "Resources.Raw";
	static Stream? LoadEmbeddedResource(string assemblyName, string fileName)
	{
		// Inladen van een assembly wordt gecached, dus eenmaal ingeladen, dan kunnen we deze weer loaden uit de cache.
		var resource = Assembly.Load(assemblyName);

		// Handigheidje om alle resources te zien die ingeladen zijn
		// Let op!: Resources dienen als Embedded Resource gemarkeerd te worden in de assembly (en niet als MauiAsset)
		//var names = resource.GetManifestResourceNames();
		return resource.GetManifestResourceStream($"{assemblyName}.{RESOURCES_FOLDER}.{fileName}");
	}
}
