# InExchangePartnerApi

Sample implementation of InExchange's API for Partners to register clients and send invoices. This implementation shows how to create an invoice, define delivery method, upload and send, with temporary token management.

- Contact InExchange to obtain an API key
- Create a company with the clients details, first run use Create() to add the company to InExchange
- Create an invoice in Peppol Bis Billing 3 format (solution contains a basic sample invoice), or use one that you already have. If you have one already, and the InvoiceBuilder isn't used, make sure to update the "DefineTransportService" function where to fetch the buyer org/email.

Happy flow:
1) Token is created
2) Invoice is created
3) Delivery method is defined:
- If OrgNo results in one (1) hit, Electronic delivery is set
- If OrgNO resulits in 0 or >1, and email address is available, PDf delivery is set
- If both fail, Paper delivery is set
4) Invoice is uploaded, and a location id is returned
5) Invoice is then sent, using the location id and defined delivery  method.


Documentation:<br>
https://test.inexchange.se/docs/Home
