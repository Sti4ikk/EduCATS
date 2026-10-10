using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EduCATS.Helpers.Forms;
using EduCATS.Helpers.Files;
using EduCATS.Helpers.Forms.Settings;
using EduCATS.Helpers.Logs;
using EduCATS.Networking;
using Moq;
using NUnit.Framework;

namespace EduCATS.UnitTests
{
	[TestFixture]
	public class RequestControllerTests
	{
		static readonly BindingFlags _privateInstance =
			BindingFlags.Instance | BindingFlags.NonPublic;
		static readonly BindingFlags _privateStatic =
			BindingFlags.Static | BindingFlags.NonPublic;

		[SetUp]
		public void SetUp()
		{
			RequestController.ResetHttpClient();
		}

		[Test]
		public void UriNullTest()
		{
			var controller = new RequestController(null);
			Assert.IsNull(controller.Uri);
		}

		[Test]
		public void UriTest()
		{
			const string url = "https://educats-api-test.com/";
			var controller = new RequestController(url);
			Assert.AreEqual(url, controller.Uri.ToString());
		}

		[Test]
		public void TimeoutConstantTest()
		{
			Assert.GreaterOrEqual(RequestController.RequestTimeoutSeconds, 30);
			Assert.AreEqual(
				RequestController.RequestTimeoutSeconds * 1000,
				RequestController.RequestTimeoutMilliseconds);
		}

		[Test]
		public void SetPostContentSetsPostBody()
		{
			var controller = createController("https://educats.by/Profile/GetNews", "token");
			controller.SetPostContent("{\"value\":1}", Encoding.UTF8, "application/json");

			var content = (StringContent)typeof(RequestController)
				.GetField("_postContent", _privateInstance)
				.GetValue(controller);
			Assert.NotNull(content);
			Assert.AreEqual("application/json", content.Headers.ContentType.MediaType);
		}

		[Test]
		public void SetPostContentSkipsEmptyBody()
		{
			var controller = createController("https://educats.by/Profile/GetNews", "token");
			controller.SetPostContent(string.Empty, Encoding.UTF8, "application/json");

			var content = typeof(RequestController)
				.GetField("_postContent", _privateInstance)
				.GetValue(controller);
			Assert.IsNull(content);
		}

		[Test]
		public void HttpClientIsSharedAcrossControllers()
		{
			var first = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var second = createController("https://educats.by/Profile/GetNews", "token");

			Assert.AreSame(getHttpClient(first), getHttpClient(second));
		}

		[Test]
		public void ResetHttpClientHandlesDisposeException()
		{
			var logsMock = new Mock<IFileManager>();
			logsMock.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
			logsMock.Setup(m => m.Append(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
			AppLogs.FileManager = logsMock.Object;
			AppLogs.Initialize(string.Empty);

			setSharedHttpClient(new ThrowingDisposeHttpClient());
			Assert.DoesNotThrow(RequestController.ResetHttpClient);
			logsMock.Verify(
				m => m.Append(It.IsAny<string>(), It.Is<string>(s => s.Contains("Exception:"))),
				Times.AtLeastOnce);
		}

		[Test]
		public void ResetHttpClientCreatesNewInstance()
		{
			var first = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var oldClient = getHttpClient(first);

			RequestController.ResetHttpClient();

			var second = createController("https://educats.by/Profile/GetNews", "token");
			var newClient = getHttpClient(second);
			Assert.AreNotSame(oldClient, newClient);
		}

		[Test]
		public void ShouldAttachAuthorizationHeaderReturnsFalseForLoginEndpoint()
		{
			var controller = createController("https://educats.by/Account/LoginJWT", "token");
			var shouldAttach = invokeShouldAttachAuthorizationHeader(controller);

			Assert.IsFalse(shouldAttach);
		}

		[Test]
		public void ShouldAttachAuthorizationHeaderReturnsFalseForRemoteApiEndpoint()
		{
			var controller = createController("https://educats.by/RemoteApi/Login", "token");
			var shouldAttach = invokeShouldAttachAuthorizationHeader(controller);

			Assert.IsFalse(shouldAttach);
		}

		[Test]
		public void ShouldAttachAuthorizationHeaderReturnsTrueForAuthorizedEndpoint()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var shouldAttach = invokeShouldAttachAuthorizationHeader(controller);

			Assert.IsTrue(shouldAttach);
		}

		[Test]
		public void ShouldAttachAuthorizationHeaderReturnsFalseWhenAccessTokenMissing()
		{
			var preferences = Mock.Of<IPreferences>(p => p.AccessToken == string.Empty);
			var services = Mock.Of<IPlatformServices>(s => s.Preferences == preferences);
			var controller = new RequestController("https://educats.by/Profile/GetProfileInfo", services);
			var shouldAttach = invokeShouldAttachAuthorizationHeader(controller);

			Assert.IsFalse(shouldAttach);
		}

		[Test]
		public void SetAuthorizationHeaderSetsHeaderForAuthorizedEndpoint()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var request = invokeSetAuthorizationHeader(controller);

			Assert.NotNull(request.Headers.Authorization);
			Assert.AreEqual("token", request.Headers.Authorization.Scheme);
		}

		[Test]
		public void SetAuthorizationHeaderSetsSchemeAndParameterWhenTokenContainsSeparator()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "Bearer abc.def");
			var request = invokeSetAuthorizationHeader(controller);

			Assert.NotNull(request.Headers.Authorization);
			Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
			Assert.AreEqual("abc.def", request.Headers.Authorization.Parameter);
		}

		[Test]
		public void SetAuthorizationHeaderSkipsWhenTrimmedAccessTokenIsEmpty()
		{
			var preferences = new Mock<IPreferences>();
			preferences.SetupSequence(p => p.AccessToken)
				.Returns("token")
				.Returns("   ");
			var services = Mock.Of<IPlatformServices>(s => s.Preferences == preferences.Object);
			var controller = new RequestController("https://educats.by/Profile/GetProfileInfo", services);
			var request = invokeSetAuthorizationHeader(controller);

			Assert.IsNull(request.Headers.Authorization);
		}

		[Test]
		public void SetAuthorizationHeaderHandlesInvalidSchemeException()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "Bad: token");
			Assert.DoesNotThrow(() => invokeSetAuthorizationHeader(controller));
		}

		[Test]
		public void SetAuthorizationHeaderClearsHeaderForLoginEndpoint()
		{
			var controller = createController("https://educats.by/Account/LoginJWT", "token");
			var request = invokeSetAuthorizationHeader(controller);

			Assert.IsNull(request.Headers.Authorization);
		}

		[Test]
		public async Task SendRequestSetsHeadersPerRequestWithoutTouchingSharedClient()
		{
			var seenAuthorization = new System.Collections.Concurrent.ConcurrentBag<string>();
			var client = createHttpClient((request, __) =>
			{
				seenAuthorization.Add(request.Headers.Authorization?.ToString());
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
			});
			setSharedHttpClient(client);

			var withToken = createController("https://educats.by/Profile/GetNews", "Bearer abc");
			withToken.SetPostContent("{}", Encoding.UTF8, "application/json");
			var loginEndpoint = createController("https://educats.by/Account/LoginJWT", "Bearer abc");

			await Task.WhenAll(
				withToken.SendRequest(HttpMethod.Post),
				loginEndpoint.SendRequest(HttpMethod.Get));

			Assert.That(seenAuthorization, Is.EquivalentTo(new[] { "Bearer abc", null }));
			Assert.IsNull(client.DefaultRequestHeaders.Authorization);
			Assert.IsFalse(client.DefaultRequestHeaders.Contains("Origin"));
		}

		[Test]
		public async Task SendRequestReturnsNullForUnsupportedMethod()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var response = await controller.SendRequest(HttpMethod.Delete);
			Assert.IsNull(response);
		}

		[Test]
		public async Task SendRequestGetReturnsUnauthorizedErrorResponse()
		{
			setSharedHttpClient(createHttpClient((_, __) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized))));
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");

			var response = await controller.SendRequest(HttpMethod.Get);
			var content = await response.Content.ReadAsStringAsync();

			Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
			Assert.AreEqual(string.Empty, content);
		}

		[Test]
		public async Task SendRequestPostReturnsUnauthorizedErrorResponse()
		{
			setSharedHttpClient(createHttpClient((_, __) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized))));
			var controller = createController("https://educats.by/Profile/GetNews", "token");
			controller.SetPostContent("{}", Encoding.UTF8, "application/json");

			var response = await controller.SendRequest(HttpMethod.Post);
			var content = await response.Content.ReadAsStringAsync();

			Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
			Assert.AreEqual(string.Empty, content);
		}

		[Test]
		public async Task SendRequestGetReturnsTimeoutOnTaskCanceledException()
		{
			setSharedHttpClient(createHttpClient((_, __) => throw new TaskCanceledException()));
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");

			var response = await controller.SendRequest(HttpMethod.Get);

			Assert.AreEqual(HttpStatusCode.RequestTimeout, response.StatusCode);
		}

		[Test]
		public async Task SendRequestPostReturnsTimeoutOnTaskCanceledException()
		{
			setSharedHttpClient(createHttpClient((_, __) => throw new TaskCanceledException()));
			var controller = createController("https://educats.by/Profile/GetNews", "token");
			controller.SetPostContent("{}", Encoding.UTF8, "application/json");

			var response = await controller.SendRequest(HttpMethod.Post);

			Assert.AreEqual(HttpStatusCode.RequestTimeout, response.StatusCode);
		}

		[Test]
		public async Task SendRequestGetReturnsBadRequestOnUnhandledException()
		{
			setSharedHttpClient(createHttpClient((_, __) => throw new HttpRequestException("failed")));
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");

			var response = await controller.SendRequest(HttpMethod.Get);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public async Task SendRequestPostReturnsBadRequestOnUnhandledException()
		{
			setSharedHttpClient(createHttpClient((_, __) => throw new HttpRequestException("failed")));
			var controller = createController("https://educats.by/Profile/GetNews", "token");
			controller.SetPostContent("{}", Encoding.UTF8, "application/json");

			var response = await controller.SendRequest(HttpMethod.Post);

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public async Task SendRequestGetReturnsBadRequestForInvalidUrl()
		{
			var controller = createController("not-a-valid-url", "token");
			var response = await controller.SendRequest(HttpMethod.Get);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public async Task SendRequestPostReturnsBadRequestForInvalidUrl()
		{
			var controller = createController("not-a-valid-url", "token");
			controller.SetPostContent("{}", Encoding.UTF8, "application/json");
			var response = await controller.SendRequest(HttpMethod.Post);
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Test]
		public void ErrorResponseMessageReturnsExpectedStatusAndEmptyContent()
		{
			var controller = createController("https://educats.by/Profile/GetProfileInfo", "token");
			var method = typeof(RequestController).GetMethod("errorResponseMessage", _privateInstance);
			var response = (HttpResponseMessage)method.Invoke(controller, new object[] { HttpStatusCode.Forbidden });
			var content = response.Content.ReadAsStringAsync().Result;

			Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
			Assert.AreEqual(string.Empty, content);
		}

		RequestController createController(string url, string accessToken)
		{
			var preferences = Mock.Of<IPreferences>(p => p.AccessToken == accessToken);
			var services = Mock.Of<IPlatformServices>(s => s.Preferences == preferences);

			var controller = new RequestController(url) {
				_services = services
			};

			return controller;
		}

		static bool invokeShouldAttachAuthorizationHeader(RequestController controller)
		{
			var method = typeof(RequestController).GetMethod(
				"shouldAttachAuthorizationHeader",
				_privateInstance);
			return (bool)method.Invoke(controller, null);
		}

		static HttpRequestMessage invokeSetAuthorizationHeader(RequestController controller)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, controller.Uri);
			var method = typeof(RequestController).GetMethod(
				"setAuthorizationHeader",
				_privateInstance);
			method.Invoke(controller, new object[] { request });
			return request;
		}

		static HttpClient getHttpClient(RequestController controller)
		{
			var field = typeof(RequestController).GetField("_client", _privateInstance);
			return (HttpClient)field.GetValue(controller);
		}

		static void setSharedHttpClient(HttpClient client)
		{
			typeof(RequestController)
				.GetField("_sharedClient", _privateStatic)
				.SetValue(null, client);
		}

		static HttpClient createHttpClient(
			Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
		{
			return new HttpClient(new StubHttpMessageHandler(send));
		}

		class StubHttpMessageHandler : HttpMessageHandler
		{
			readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

			public StubHttpMessageHandler(
				Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
			{
				_send = send;
			}

			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				return _send(request, cancellationToken);
			}
		}

		class ThrowingDisposeHttpClient : HttpClient
		{
			protected override void Dispose(bool disposing)
			{
				throw new Exception("Dispose failed");
			}
		}
	}
}
