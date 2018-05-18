using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Docker.Registry.DotNet.Tests
{
	public class RegistryClientTests
	{

		[Fact]
		public async Task BlobUploadOperationTest()
		{
			var config = new RegistryClientConfiguration(new Uri("http://172.16.0.175:3000"));
			var client = config.CreateClient();
			using (var fs = File.OpenRead(""))
			{
				await client.BlobUploads.UploadBlobAsync("Test", (int)fs.Length, fs, "");
			}
		}

	}

}