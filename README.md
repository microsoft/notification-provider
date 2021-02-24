# Notification Provider
![Integration Build](https://github.com/microsoft/notification-provider/workflows/Integration%20Build/badge.svg)

Notification Provider is an implementation to send Email Notifications using the [Graph APIs](https://docs.microsoft.com/en-us/graph/api/resources/mail-api-overview?view=graph-rest-1.0)/[Direct Send](https://docs.microsoft.com/en-us/exchange/mail-flow-best-practices/how-to-set-up-a-multifunction-device-or-application-to-send-email-using-microsoft-365-or-office-365#option-2-send-mail-directly-from-your-printer-or-application-to-microsoft-365-or-office-365-direct-send), and supports sending more than 10k emails in a day. This service has robust retry mechanisms and telemetry hooks to ensure proper tracking of email notifications. The library is extensible, providing the users the option to use Graph/DirectSend as Notification Providers to send the email Notifications and Table Storage or CosmosDB to store the Notification History and Templates.

## Benefits of Notification Provider
1. Uses Asynchronous processing of emails
2. Accepts email in batches
3. Extendible solution for your choice of storage and Notification Providers
4. Already supports GraphAPI/DirectSend as NotificationProviders
5. Already supports Azure Table Storage(recommended) and Cosmos DB as storage for email tracking
6. Uses Application Insights for logging, log level can be configured
7. Have different endpoints for resend/getting history/sending a single email/sending emails in batches etc
8. Supports Attachments/Templates and more to come.
9. Differnt API Endpoints for Template management

_Please find further details in the [WIKI here](https://github.com/microsoft/notification-provider/wiki/Notification-Provider)._

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmicrosoft%2Fnotification-provider%2Fmain%2FNotificationService%2FNotificationService.IaaC%2Fcspnotification.json)

## Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
