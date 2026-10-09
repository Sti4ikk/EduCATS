using System.Collections.Generic;
using System.IO;
using System.Linq;
using EduCATS.Constants;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EduCATS.UnitTests
{
	[TestFixture]
	public class LocalizationTests
	{
		/// <summary>
		/// Every language must have all keys of the Russian one:
		/// a missing key is shown to the user as the raw key name.
		/// </summary>
		[Test]
		public void AllLanguagesHaveAllKeysTest()
		{
			var languages = loadLanguages();
			var russianKeys = languages["ru"];

			Assert.That(languages.Count, Is.GreaterThan(1));

			foreach (var language in languages.Where(l => l.Key != "ru"))
			{
				var missing = russianKeys.Except(language.Value).ToList();
				Assert.That(missing, Is.Empty, $"{language.Key}.json misses keys: {string.Join(", ", missing)}");
			}
		}

		static Dictionary<string, HashSet<string>> loadLanguages()
		{
			var assembly = typeof(GlobalConsts).Assembly;
			var languages = new Dictionary<string, HashSet<string>>();

			foreach (var resource in assembly.GetManifestResourceNames()
				.Where(name => name.Contains(".Localization.") && name.EndsWith(".json")))
			{
				using var stream = assembly.GetManifestResourceStream(resource);
				using var reader = new StreamReader(stream);
				var json = JObject.Parse(reader.ReadToEnd());
				var code = Path.GetFileNameWithoutExtension(resource).Split('.').Last();
				languages[code] = json.Properties().Select(p => p.Name).ToHashSet();
			}

			return languages;
		}
	}
}
