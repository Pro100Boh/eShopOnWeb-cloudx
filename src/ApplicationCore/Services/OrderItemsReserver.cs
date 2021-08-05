using Microsoft.Azure.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
	class OrderItemsReserver
	{
		const string ServiceBusConnectionString = "Endpoint=sb://cloudx-bv-final-task.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=k0hBuKxetOmqYxerPaJUmpvZGDVf7cwpfIBjaSOOOF8=";
		const string QueueName = "cloudx-bv-final-task-queue";

		public async Task Reserve(Order order)
		{
			string json = JsonConvert.SerializeObject(order);

			await SendMessageToQueueAsync(json);
		}

		private async Task SendMessageToQueueAsync(string messageBody)
		{
			var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
			var message = new Message(Encoding.UTF8.GetBytes(messageBody));

			await queueClient.SendAsync(message);
			await queueClient.CloseAsync();
		}
	}
}