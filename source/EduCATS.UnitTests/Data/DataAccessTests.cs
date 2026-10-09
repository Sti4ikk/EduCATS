using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using EduCATS.Constants;
using EduCATS.Data;
using EduCATS.Data.Interfaces;
using EduCATS.Demo;
using EduCATS.Helpers.Forms;
using MonkeyCache.FileStore;
using Moq;
using NUnit.Framework;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.UnitTests
{
	[TestFixture]
	public class DataAccessTests
	{
		const string _key = "key";
		const string _message = "error";
		const string _nonJsonSuccessResponse = "\"Ok\"";

		IPlatformServices _mockedOffline;
		IPlatformServices _mockedConnected;

		[SetUp]
		public void SetUp()
		{
			_mockedOffline = Mock.Of<IPlatformServices>(ps => ps.Device.CheckConnectivity() == false);
			_mockedConnected = Mock.Of<IPlatformServices>(ps => ps.Device.CheckConnectivity() == true);

			var assembly = typeof(GlobalConsts).GetTypeInfo().Assembly;
			CrossLocalization.Initialize(
				assembly,
				GlobalConsts.RunNamespace,
				GlobalConsts.LocalizationDirectory);

			CrossLocalization.AddLanguageSupport(Languages.EN);
			CrossLocalization.SetDefaultLanguage(Languages.EN.LangCode);
			CrossLocalization.SetLanguage(Languages.EN.LangCode);

			Barrel.ApplicationId = GlobalConsts.AppId;
		}

		[Test]
		public async Task GetSingleTest()
		{
			var mockedConnected = Mock.Of<IPlatformServices>(ps => ps.Device.CheckConnectivity() == true);
			var dataAccess = new DataAccess<object>(_message, null, "1", mockedConnected);
			var actual = await dataAccess.GetSingle();
			Assert.IsNotNull(actual);
		}

		[Test]
		public async Task GetListTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "2", _mockedConnected);
			var actual = await dataAccess.GetList();
			Assert.IsNotNull(actual);
		}

		[Test]
		public void CheckConnectionTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "3", _mockedConnected);
			var actual = dataAccess.CheckConnectionEstablished();
			Assert.AreEqual(true, actual);
			AppDemo.Instance.IsDemoAccount = true;
			var demoActual = dataAccess.CheckConnectionEstablished();
			Assert.AreEqual(true, actual);
			var dataAccessOffline = new DataAccess<object>(_message, null, "4", _mockedOffline);
			var demoOfflineActual = dataAccessOffline.CheckConnectionEstablished();
			Assert.AreEqual(true, demoOfflineActual);
			AppDemo.Instance.IsDemoAccount = false;
			var offlineActual = dataAccessOffline.CheckConnectionEstablished();
			Assert.AreEqual(false, offlineActual);
		}

		[Test]
		public async Task GetSingleNoConnectionTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "5", _mockedOffline);
			var actual = await dataAccess.GetSingle();
			Assert.IsNotNull(actual);
		}

		[Test]
		public async Task GetListNoConnectionTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "6", _mockedOffline);
			var actual = await dataAccess.GetList();
			Assert.IsNotNull(actual);
		}

		[Test]
		public void ResetDataTest()
		{
			try {
				DataAccess.ResetData();
				return;
			} catch (Exception ex) {
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public async Task GetDataSingleObjectTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "7", _mockedConnected);
			var result = await DataAccess.GetSingleData(dataAccess);
			Assert.NotNull(result.Data);
		}

		[Test]
		public async Task GetDataListObjectTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "8", _mockedConnected);
			var result = await DataAccess.GetListData(dataAccess);
			Assert.NotNull(result.Data);
		}

		[Test]
		public async Task ConcurrentInstancesOfSameTypeUseOwnCallbacksTest()
		{
			var firstResponse = new TaskCompletionSource<object>();
			var secondResponse = new TaskCompletionSource<object>();
			var first = new DataAccess<object>(_message, firstResponse.Task, null, _mockedConnected);
			var second = new DataAccess<object>(_message, secondResponse.Task, null, _mockedConnected);

			var firstTask = first.GetSingle();
			var secondTask = second.GetSingle();
			secondResponse.SetResult(new KeyValuePair<string, HttpStatusCode>("{ \"id\": 2 }", HttpStatusCode.OK));
			firstResponse.SetResult(new KeyValuePair<string, HttpStatusCode>("{ \"id\": 1 }", HttpStatusCode.OK));

			Assert.That((await firstTask).ToString(), Does.Contain("1"));
			Assert.That((await secondTask).ToString(), Does.Contain("2"));
		}

		[Test]
		public async Task ConcurrentRequestsKeepOwnErrorsTest()
		{
			var failedResponse = Task.FromResult<object>(
				new KeyValuePair<string, HttpStatusCode>(string.Empty, HttpStatusCode.BadRequest));
			var successResponse = Task.FromResult<object>(
				new KeyValuePair<string, HttpStatusCode>("{ \"data\": \"test\" }", HttpStatusCode.OK));

			var failedTask = DataAccess.GetSingleData(
				new DataAccess<object>("login_error", failedResponse, null, _mockedConnected));
			var successTask = DataAccess.GetSingleData(
				new DataAccess<object>("login_error", successResponse, null, _mockedConnected));

			var failed = await failedTask;
			var success = await successTask;

			Assert.IsTrue(failed.IsError);
			Assert.AreEqual(CrossLocalization.Translate("login_error"), failed.ErrorMessage);
			Assert.IsFalse(success.IsError);
			Assert.IsNull(success.ErrorMessage);
		}

		[Test]
		public void GetComplexKeyTest()
		{
			var id_1 = 1;
			var id_2 = 2;
			var actual = DataAccess.GetKey(_key, id_1, id_2);
			Assert.AreEqual($"{_key}/{id_1}/{id_2}", actual);
		}

		[Test]
		public void GetKeyTest()
		{
			var id = 123;
			var actual = DataAccess.GetKey(_key, id);
			Assert.AreEqual($"{_key}/{id}", actual);
		}

		[Test]
		public void CreateResultWithoutErrorTest()
		{
			var dataAccess = Mock.Of<IDataAccess<object>>(d =>
				d.ErrorMessageKey == null && d.IsConnectionError == true);
			var result = DataAccess.CreateResult(new object(), dataAccess);

			Assert.AreEqual(false, result.IsError);
			Assert.AreEqual(false, result.IsConnectionError);
			Assert.IsNull(result.ErrorMessage);
		}

		[Test]
		public void CreateResultWithErrorTest()
		{
			var message = "Error message";
			var dataAccess = Mock.Of<IDataAccess<object>>(d =>
				d.ErrorMessageKey == message && d.IsConnectionError == true && d.IsRawErrorMessage == true);
			var result = DataAccess.CreateResult(new object(), dataAccess);

			Assert.AreEqual(message, result.ErrorMessage);
			Assert.AreEqual(true, result.IsError);
			Assert.AreEqual(true, result.IsConnectionError);
		}

		[Test]
		public void CreateResultTranslatesLocalizationKeyTest()
		{
			var dataAccess = Mock.Of<IDataAccess<object>>(d =>
				d.ErrorMessageKey == "base_connection_error" && d.IsRawErrorMessage == false);
			var result = DataAccess.CreateResult(new object(), dataAccess);

			Assert.AreEqual(CrossLocalization.Translate("base_connection_error"), result.ErrorMessage);
			Assert.AreNotEqual("base_connection_error", result.ErrorMessage);
		}

		[Test]
		public void DataResultMapKeepsErrorTest()
		{
			var result = new DataResult<string>("text", "error", true, true);
			var mapped = result.Map(text => text.Length);

			Assert.AreEqual(4, mapped.Data);
			Assert.AreEqual("error", mapped.ErrorMessage);
			Assert.IsTrue(mapped.IsConnectionError);
			Assert.IsTrue(mapped.IsSessionExpiredError);
		}

		[Test]
		public void GetListAccessValidJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "9", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>("[ { \"data\": \"test\" } ]", HttpStatusCode.OK);
			var actual = dataAccess.GetListAccess(kvp);
			Assert.NotNull(actual);
		}

		[Test]
		public void GetListAccessNonValidJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "10", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>("response", HttpStatusCode.OK);
			var actual = dataAccess.GetListAccess(kvp);
			Assert.AreEqual(null, actual);
		}

		[Test]
		public void GetListAccessSuccessJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "11", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>(_nonJsonSuccessResponse, HttpStatusCode.OK);
			var actual = dataAccess.GetListAccess(kvp);
			Assert.AreEqual(string.Empty, actual);
		}

		[Test]
		public void GetSingleAccessValidJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "12", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>("{ \"data\": \"test\" }", HttpStatusCode.OK);
			var actual = dataAccess.GetAccess(kvp);
			Assert.NotNull(actual);
		}

		[Test]
		public void GetSingleAccessNonValidJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "13", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>("response", HttpStatusCode.OK);
			var actual = dataAccess.GetAccess(kvp);
			Assert.AreEqual(null, actual);
		}

		[Test]
		public void GetSingleAccessSuccessJsonTest()
		{
			var dataAccess = new DataAccess<object>(_message, null, "14", _mockedConnected);
			var kvp = new KeyValuePair<string, HttpStatusCode>(_nonJsonSuccessResponse, HttpStatusCode.OK);
			var actual = dataAccess.GetAccess(kvp);
			Assert.NotNull(actual);
		}
	}
}
