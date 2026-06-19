using Android.Runtime;
using Javax.Net.Ssl;
using Java.Security;
using System.Net.Http;

namespace EduCATS.MAUI.Platforms.Android
{
	public class CustomAndroidHandler : Xamarin.Android.Net.AndroidMessageHandler
	{
		readonly ITrustManager[] _trustManagers;

		public CustomAndroidHandler()
		{
			_trustManagers = new ITrustManager[] { new TrustAllManager() };
		}

		protected override SSLSocketFactory ConfigureCustomSSLSocketFactory(HttpsURLConnection connection)
		{
			var sslContext = SSLContext.GetInstance("TLS");
			sslContext.Init(null, _trustManagers, new SecureRandom());
			connection.SSLSocketFactory = sslContext.SocketFactory;
			return sslContext.SocketFactory;
		}

		protected override IHostnameVerifier GetSSLHostnameVerifier(HttpsURLConnection connection)
		{
			return new AllowAllHostnameVerifier();
		}
	}

	class AllowAllHostnameVerifier : Java.Lang.Object, IHostnameVerifier
	{
		public bool Verify(string hostname, ISSLSession session) => true;
	}

	class TrustAllManager : Java.Lang.Object, IX509TrustManager
	{
		public void CheckClientTrusted(Java.Security.Cert.X509Certificate[] chain, string authType) { }
		public void CheckServerTrusted(Java.Security.Cert.X509Certificate[] chain, string authType) { }
		public Java.Security.Cert.X509Certificate[] GetAcceptedIssuers() => Array.Empty<Java.Security.Cert.X509Certificate>();
	}
}