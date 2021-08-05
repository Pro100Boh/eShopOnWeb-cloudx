using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
	class DeliveryOrderProcessor
	{
		const string AzureFunctionURL = "https://deliveryorderprocessor.azurewebsites.net/api/DeliveryOrderProcessor";

		public async Task<bool> Process(Order order)
		{
			var deliveryOrder = new
			{
				Id = order.Id,
				ShipToAddress = order.ShipToAddress,
				OrderItems = order.OrderItems.Select(oi => new
				{
					ItemId = oi.ItemOrdered.CatalogItemId,
					ProductName = oi.ItemOrdered.ProductName,
					Units = oi.Units
				}),
				FinalPrice = order.OrderItems.Select(oi => oi.UnitPrice * oi.Units).Sum()
			};

			using (var client = new HttpClient())
			using (var request = new HttpRequestMessage(HttpMethod.Post, AzureFunctionURL))
			using (HttpContent httpContent = CreateHttpContent(deliveryOrder))
			{
				request.Content = httpContent;

				using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
				{
					return response.IsSuccessStatusCode;
				}
			}
		}

		private HttpContent CreateHttpContent(object content)
		{
			var ms = new MemoryStream();
			SerializeJsonIntoStream(content, ms);
			ms.Seek(0, SeekOrigin.Begin);
			var httpContent = new StreamContent(ms);
			httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return httpContent;
		}

		private void SerializeJsonIntoStream(object value, Stream stream)
		{
			using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
			using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
			{
				var js = new JsonSerializer();
				js.Serialize(jtw, value);
				jtw.Flush();
			}
		}
	}
}