using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InExchangePartnerApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string apiKey = ""; //obtained by contacting InExchange

            /*
             * --- Company/token management ---
             * 
             * Create a company object, use CreateCompany() to register/update
             * use CreateClientToken() to get a valid token and RevokeClientToken() to remove
             * 
             * First run, use Create() function to add the company, only done once or if company details are updated.
             * 
             */

            Company erp1 = new("ErpId01", "ERP v. 1.0", "Test Company 2", "test@test.se", "SE", "2233115592", "SE223311559291", "SendInvoices");
            //Company.Create(erp1, apiKey);
            string token = await Company.CreateClientToken(erp1.ErpId, apiKey);


            /* --- Create, upload and send invoice ---
             * 
             * defines transport type´, loads invoice into Byte[],
             * uploads file, gets locationID from api, and sends the document
             * 
             */

            //creates an invoice object
            Invoice i = new()
            {
                InvoiceNumber = "F003",
                IssueDate = new DateTime(2022, 12, 10),
                DueDate = new DateTime(2023, 01, 10),
                InvoiceCurrencyCode = "SEK",
                ReferenceCode = "Recipients refcode",
                SellerOrderNumber = "SellerOrder12345",
                ContractNumber = "C12345",
                InvoiceNote = "test message",

                SellerORG = "1122334455",
                SellerVAT = "SE112233445501",
                SellerCountryCode = "SE",
                SellerCompanyName = "Seller Company AB",
                SellerStreetName = "gatuadress 1",
                SellerPostalZone = "12345",
                SellerContactName = "Contact Name Here",
                BankGiro = "1234567",
                Iban = "SE12323232323232323",
                Bic = "SWEDSESS",
                BankAccount = "2222222222",


                BuyerCompanyName = "Buyer AB",
                BuyerCountryCode = "SE",
                BuyerInternalIdentifier = "Kundnummer1234",
                BuyerContactElectronicMail = "emailInvoice@buyer.se",

                LineTotalAmount = 54.5F,
                TaxExclusiveAmount = 54.5F,
                TaxInclusiveAmount = 68.13F,
                RoundingAmount = -0.13F,
                PayableAmount = 68F
            };
            i.AddInvoiceLine("description 1", 1F, "EA", 10F, 10F, 25);
            i.AddInvoiceLine("description 2", 2F, "EA", 20F, 40F, 25);
            i.AddInvoiceLine("description 3", 3F, "EA", 1.50F, 4.50F, 25);
            _ = new InvoiceBuilder(i);

            //define transport service
            var transport = await DefineTransportService(i, token);

            //upload created invoice with defined transport type
            Byte[] invoice = System.IO.File.ReadAllBytes("Invoice_" + i.InvoiceNumber + ".xml"); // file to be sent
            UploadAndSendInvoice(token, invoice, i.InvoiceNumber, transport.GetValueOrDefault(key: "TransportType"), transport.GetValueOrDefault(key: "Identifier"));



            //revoke token and exit
            Company.RevokeClientToken(token, apiKey);
            Console.ReadLine();


        }


        private static void UploadAndSendInvoice(string clientToken, Byte[] file, string erpDocumentId, string? deliveryMethod, string? deliveryIdentifier)
        {
            string documentUrl = "https://testapi.inexchange.se/v1/api/documents"; //url to upload documents, invoices, attachments etc
            string sendUrl = "https://testapi.inexchange.se/v1/api/documents/outbound"; //url to push invoice to customer

            if (clientToken != null)
            {
                HttpClient c = new HttpClient();
                //post as multipart/form-data
                var invoicePayload = new ByteArrayContent(file);
                invoicePayload.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                invoicePayload.Headers.ContentDisposition = new ContentDispositionHeaderValue("upload")
                {
                    FileName = erpDocumentId + ".xml",

                };

                c.DefaultRequestHeaders.Clear();
                c.DefaultRequestHeaders.Add("ClientToken", clientToken);
                var documentLocation = c.PostAsync(documentUrl, invoicePayload).Result.Headers.ToList().SingleOrDefault(x => x.Key == "Location").Value.FirstOrDefault();
                Console.WriteLine("Document location: " + documentLocation);

                if (deliveryMethod == "Electronic")
                {
                    var sendInvoicePayload = new
                    {
                        SendDocumentAs = new
                        {
                            Type = "Electronic",
                            Electronic = new
                            {
                                RecipientId = deliveryIdentifier
                            },
                        },
                        Document = new
                        {
                            DocumentFormat = "bis3", //format as given by InExchange, hardcoded
                            ErpDocumentId = erpDocumentId, //unique id for the invoice from ERP, used in error handling
                            DocumentUri = documentLocation, //Location from previous upload
                            Language = "sv", // sv / en / fi / dk
                            Culture = "sv" // sv / en / fi / dk
                        }
                    };

                    var sendResult = c.PostAsJsonAsync(sendUrl, sendInvoicePayload).Result;
                    Console.WriteLine(sendResult);
                }
                else if (deliveryMethod == "Pdf")
                {
                    var sendInvoicePayload = new
                    {
                        SendDocumentAs = new
                        {
                            Type = "Pdf",
                            Pdf = new
                            {
                                RecipientEmail = deliveryIdentifier
                            },
                        },
                        Document = new
                        {
                            DocumentFormat = "bis3", //format as given by InExchange, hardcoded
                            ErpDocumentId = erpDocumentId, //unique id for the invoice from ERP, used in error handling
                            DocumentUri = documentLocation, //Location from previous upload
                            Language = "sv", // sv / en / fi / dk
                            Culture = "sv" // sv / en / fi / dk
                        }
                    };

                    var sendResult = c.PostAsJsonAsync(sendUrl, sendInvoicePayload).Result;
                    Console.WriteLine(sendResult);

                }
                else
                {
                    var sendInvoicePayload = new
                    {
                        SendDocumentAs = new
                        {
                            Type = "Paper",
                        },
                        Document = new
                        {
                            DocumentFormat = "bis3", //format as given by InExchange, hardcoded
                            ErpDocumentId = erpDocumentId, //unique id for the invoice from ERP, used in error handling
                            DocumentUri = documentLocation, //Location from previous upload
                            Language = "sv", // sv / en / fi / dk
                            Culture = "sv" // sv / en / fi / dk
                        }
                    };

                    var sendResult = c.PostAsJsonAsync(sendUrl, sendInvoicePayload).Result;
                    Console.WriteLine(sendResult);
                }
                

            }

        }

        private static async Task<Dictionary<string, string?>> DefineTransportService(Invoice i, string token)
        {

            /*
            * Checks if recipient can handle e-invoice based on OrgNo
            * if one company is found, electronic delivery is set
            * if none or multiple are found, check if email address is available and set Pdf delivery
            * if electronic/pdf fails, set paper delivery
            */

            string buyerLookupUrl = "https://testapi.inexchange.se/v1/api/buyerparties/lookup";

            string? identifier, transportType;
            var payload = new
            {
                PartyId = "1",
                OrgNo = i.BuyerORG

            };

            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("ClientToken", token);
            var req = c.PostAsJsonAsync(buyerLookupUrl, payload).Result;
            var resp = await req.Content.ReadFromJsonAsync<BuyerLookup>();

            if (resp?.exactHit == true && resp?.parties?[0].receiveElectronicInvoiceCapability == "ReceivingElectronicInvoices" && resp?.parties[0].companyId != null)
            {
                transportType = "Electronic";
                identifier = resp.parties[0].companyId;
            }
            else if (i.BuyerContactElectronicMail != null)
            {
                transportType = "Pdf";
                identifier = i.BuyerContactElectronicMail;
            }
            else
            {
                transportType = "Paper";
                identifier = null;
            }

            Console.WriteLine("Delivery method: " + transportType + " | Delivery identifier: " + identifier);










            var transportDefined = new Dictionary<string, string?>()
            {
                {"TransportType", transportType },
                {"Identifier", identifier}
            };


            return transportDefined;
        }

    }



}















