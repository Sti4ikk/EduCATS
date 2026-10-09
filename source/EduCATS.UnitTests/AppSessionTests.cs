using EduCATS.Configuration;
using EduCATS.Constants;
using EduCATS.Data.Caching;
using EduCATS.Data.User;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Forms.Pages;
using EduCATS.Helpers.Forms.Settings;
using MonkeyCache.FileStore;
using Moq;
using NUnit.Framework;

namespace EduCATS.UnitTests
{
	[TestFixture]
	public class AppSessionTests
	{
		const int _userId = 42;
		const string _username = "TestUsername";
		const string _token = "Bearer token";

		[SetUp]
		public void SetUp()
		{
			Barrel.ApplicationId = GlobalConsts.AppId;
			DataCaching<object>.RemoveCache();
			AppUserData.Clear();
			AppUserData.IsProfileLoaded = false;
		}

		[Test]
		public void TryRestoreReturnsFalseWhenNotLoggedInTest()
		{
			var services = createServices(isLoggedIn: false, token: _token);
			Assert.IsFalse(AppSession.TryRestore(services));
			Assert.AreEqual(0, AppUserData.UserId);
		}

		[Test]
		public void TryRestoreReturnsFalseWithoutTokenTest()
		{
			var services = createServices(isLoggedIn: true, token: string.Empty);
			Assert.IsFalse(AppSession.TryRestore(services));
		}

		[Test]
		public void TryRestoreRestoresUserDataTest()
		{
			DataCaching<string>.Save(
				GlobalConsts.DataProfileKey, "{ \"Name\": \"Ivanov Ivan\", \"UserType\": \"1\" }");
			var services = createServices(isLoggedIn: true, token: _token);

			Assert.IsTrue(AppSession.TryRestore(services));
			Assert.AreEqual(_userId, AppUserData.UserId);
			Assert.AreEqual(_username, AppUserData.Username);
			Assert.AreEqual("Ivanov Ivan", AppUserData.Name);
			Assert.AreEqual(UserTypeEnum.Professor, AppUserData.UserType);
		}

		[Test]
		public void TryRestoreWorksWithoutCachedProfileTest()
		{
			var services = createServices(isLoggedIn: true, token: _token);

			Assert.IsTrue(AppSession.TryRestore(services));
			Assert.AreEqual(_userId, AppUserData.UserId);
			Assert.IsFalse(AppUserData.IsProfileLoaded);
		}

		[Test]
		public void LogoutClearsSessionAndOpensLoginTest()
		{
			var services = new Mock<IPlatformServices>();
			var preferences = new Mock<IPreferences>();
			var navigation = new Mock<IPages>();
			services.Setup(s => s.Preferences).Returns(preferences.Object);
			services.Setup(s => s.Navigation).Returns(navigation.Object);
			AppUserData.UserId = _userId;
			AppUserData.Username = _username;

			AppSession.Logout(services.Object);

			preferences.Verify(p => p.ResetPrefs(), Times.Once);
			navigation.Verify(n => n.OpenLogin(), Times.Once);
			Assert.AreEqual(0, AppUserData.UserId);
			Assert.IsNull(AppUserData.Username);
		}

		static IPlatformServices createServices(bool isLoggedIn, string token) =>
			Mock.Of<IPlatformServices>(ps =>
				ps.Preferences.IsLoggedIn == isLoggedIn &&
				ps.Preferences.UserId == _userId &&
				ps.Preferences.UserLogin == _username &&
				ps.Preferences.AccessToken == token);
	}
}
