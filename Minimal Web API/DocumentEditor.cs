﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Syncfusion.EJ2.DocumentEditor;
using WDocument = Syncfusion.DocIO.DLS.WordDocument;
using WFormatType = Syncfusion.DocIO.FormatType;
using Syncfusion.EJ2.SpellChecker;
using Newtonsoft.Json;

namespace DocumentEditorCore
{
    public class DocumentEditor
    {
        internal List<Syncfusion.EJ2.SpellChecker.DictionaryData>? spellDictCollection;
        internal string? path;
        internal string? personalDictPath;
        public DocumentEditor()
        {
            //check the spell check dictionary path environment variable value and assign default data folder
            //if it is null.
            path = Path.Combine("\\App_Data\\");
            //Set the default spellcheck.json file if the json filename is empty.
            string jsonFileName = Path.Combine("\\App_Data\\spellcheck.json");
            if (System.IO.File.Exists(jsonFileName))
            {
                string jsonImport = System.IO.File.ReadAllText(jsonFileName);
                List<DictionaryData> spellChecks = JsonConvert.DeserializeObject<List<DictionaryData>>(jsonImport);
                spellDictCollection = new List<DictionaryData>();
                //construct the dictionary file path using customer provided path and dictionary name
                foreach (var spellCheck in spellChecks)
                {
                    spellDictCollection.Add(new DictionaryData(spellCheck.LanguadeID, Path.Combine(path, spellCheck.DictionaryPath), Path.Combine(path, spellCheck.AffixPath)));
                    personalDictPath = Path.Combine(path, spellCheck.PersonalDictPath);
                }
            }
        }

        public string SystemClipboard(CustomParameter param)
        {
            if (param.content != null && param.content != "")
            {
                try
                {

                    Syncfusion.EJ2.DocumentEditor.WordDocument document = Syncfusion.EJ2.DocumentEditor.WordDocument.LoadString(param.content, GetFormatType(param.type.ToLower()));
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
                    document.Dispose();
                    return json;
                }
                catch (Exception)
                {
                    return "";
                }
            }
            return "";
        }

        public string? Import(IFormCollection data)
        {
            if (data.Files.Count == 0)
                return null;
            Stream stream = new MemoryStream();
            IFormFile file = data.Files[0];
            int index = file.FileName.LastIndexOf('.');
            string type = index > -1 && index < file.FileName.Length - 1 ?
                file.FileName.Substring(index) : ".docx";
            file.CopyTo(stream);
            stream.Position = 0;

            Syncfusion.EJ2.DocumentEditor.WordDocument document = Syncfusion.EJ2.DocumentEditor.WordDocument.Load(stream, GetFormatType(type.ToLower()));
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return json;
        }
        public string[]? RestrictEditing(CustomRestrictParameter param)
        {
            if (param.passwordBase64 == "" && param.passwordBase64 == null)
                return null;
            return Syncfusion.EJ2.DocumentEditor.WordDocument.ComputeHash(param.passwordBase64, param.saltBase64, param.spinCount);
        }

        public string LoadDefault()
        {
            string fileUrl = "https://www.syncfusion.com/downloads/support/directtrac/general/doc/Getting_Started_(5)1937944689.docx";
            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    MemoryStream stream = new MemoryStream(client.DownloadData(fileUrl));


                    stream.Position = 0;

                    Syncfusion.EJ2.DocumentEditor.WordDocument document = Syncfusion.EJ2.DocumentEditor.WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
                    document.Dispose();
                    return json;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string SpellCheck(SpellCheckJsonData spellChecker)
        {
            try
            {
                SpellChecker spellCheck = new SpellChecker(spellDictCollection, personalDictPath);
                spellCheck.GetSuggestions(spellChecker.LanguageID, spellChecker.TexttoCheck, spellChecker.CheckSpelling, spellChecker.CheckSuggestion, spellChecker.AddWord);
                return Newtonsoft.Json.JsonConvert.SerializeObject(spellCheck);
            }
            catch
            {
                return "{\"SpellCollection\":[],\"HasSpellingError\":false,\"Suggestions\":null}";
            }
        }
        public string SpellCheckByPage(SpellCheckJsonData spellChecker)
        {
            try
            {
                SpellChecker spellCheck = new SpellChecker(spellDictCollection, personalDictPath);
                spellCheck.CheckSpelling(spellChecker.LanguageID, spellChecker.TexttoCheck);
                return Newtonsoft.Json.JsonConvert.SerializeObject(spellCheck);
            }
            catch
            {
                return "{\"SpellCollection\":[],\"HasSpellingError\":false,\"Suggestions\":null}";
            }
        }



        public string MailMerge(ExportData exportData)
        {
            Byte[] data = Convert.FromBase64String(exportData.documentData.Split(',')[1]);
            MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            try
            {
                Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
                document.MailMerge.RemoveEmptyGroup = true;
                document.MailMerge.RemoveEmptyParagraphs = true;
                document.MailMerge.ClearFields = true;
                document.MailMerge.Execute(CustomerDataModel.GetAllRecords());
                document.Save(stream, Syncfusion.DocIO.FormatType.Docx);
            }
            catch (Exception ex)
            { }
            string sfdtText = "";
            Syncfusion.EJ2.DocumentEditor.WordDocument document1 = Syncfusion.EJ2.DocumentEditor.WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(document1);
            document1.Dispose();
            return sfdtText;
        }
        public class CustomerDataModel
        {
            public static List<Customer> GetAllRecords()
            {
                List<Customer> customers = new List<Customer>();
                customers.Add(new Customer("9072379", "50%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "2000", "19072379", "Folk och fä HB", "100000", "440", "32.34", "472.34", "28023", "12000", "2020-11-07 00:00:00", "2020-12-07 00:00:00"));
                customers.Add(new Customer("9072378", "20%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "", "2", "19072369", "Maersk", "140000", "245", "20", "265", "28024", "12400", "2020-11-31 00:00:00", "2020-12-22300:00:00"));
                customers.Add(new Customer("9072377", "30%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "100", "19072879", "Mediterranean Shipping Company", "104000", "434", "50.43", "484.43", "28025", "10000", "2020-11-07 00:00:00", "2020-12-02 00:00:00"));
                customers.Add(new Customer("9072393", "10%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "2050", "19072378", "China Ocean Shipping Company", "175000", "500", "32", "532", "28026", "17000", "2020-09-23 00:00:00", "2020-10-09 00:00:00"));
                customers.Add(new Customer("9072377", "14%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "2568", "19072380", "CGM", "155000", "655", "20.54", "675.54", "28027", "13000", "2020-10-11 00:00:00", "2020-11-17 00:00:00"));
                customers.Add(new Customer("9072376", "0%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "1532", "19072345", " Hapag-Lloyd", "106500", "344", "30", "374", "28028", "14500", "2020-06-17 00:00:00", "2020-07-07 00:00:00"));
                customers.Add(new Customer("9072369", "05%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "4462", "190723452", "Ocean Network Express", "100054", "541", "50", "591", "28029", "16500", "2020-04-07 00:00:00", "2020-05-07 00:00:00"));
                customers.Add(new Customer("9072359", "4%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "27547", "190723713", "Evergreen Line", "124000", "800", "10.23", "810.23", "28030", "12500", "2020-03-07 00:00:00", "2020-04-07 00:00:00"));
                customers.Add(new Customer("9072380", "20%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "7582", "19072312", "Yang Ming Marine Transport", "1046000", "290", "10", "300", "27631", "12670", "2020-11-10 00:00:00", "2020-12-13 00:00:00"));
                customers.Add(new Customer("9072381", "42%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "862", "19072354", "Hyundai Merchant Marine", "145000", "800", "10.23", "810.23", "28032", "45000", "2020-10-17 00:00:00", "2020-12-23 00:00:00"));
                customers.Add(new Customer("9072391", "84%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "82", "19072364", "Pacific International Line", "10094677", "344", "30", "374", "28033", "16500", "2020-11-14 00:00:00", "2020-12-21 00:00:00"));
                customers.Add(new Customer("9072392", "92%", "C/ Araquil, 67", "Madrid", "22020-08-10 00:00:00", "Spain", "Brittania", "82", "19072385", "Österreichischer Lloyd", "104270", "500", "32", "532", "28034", "156500", "2020-06-07 00:00:00", "2020-07-07 00:00:00"));
                return customers;
            }
        }
        public class Customer
        {
            public string CustomerID { get; set; }
            public string ProductName { get; set; }
            public string Quantity { get; set; }
            public string ShipName { get; set; }
            public string UnitPrice { get; set; }
            public string Discount { get; set; }
            public string ShipAddress { get; set; }
            public string ShipCity { get; set; }
            public string OrderDate { get; set; }
            public string ShipCountry { get; set; }
            public string OrderId { get; set; }
            public string Subtotal { get; set; }
            public string Freight { get; set; }
            public string Total { get; set; }
            public string ShipPostalCode { get; set; }
            public string RequiredDate { get; set; }
            public string ShippedDate { get; set; }
            public string ExtendedPrice { get; set; }
            public Customer(string orderId, string discount, string shipAddress, string shipCity, string orderDate, string shipCountry, string productName, string quantity, string customerID, string shipName, string unitPrice, string subtotal, string freight, string total, string shipPostalCode, string extendedPrice, string requiredDate, string shippedDate)
            {
                this.CustomerID = customerID;
                this.ProductName = productName;
                this.Quantity = quantity;
                this.ShipName = shipName;
                this.UnitPrice = unitPrice;
                this.Discount = discount;
                this.ShipAddress = shipAddress;
                this.ShipCity = shipCity;
                this.OrderDate = orderDate;
                this.ShipCountry = shipCountry;
                this.OrderId = orderId;
                this.Subtotal = subtotal;
                this.Freight = freight;
                this.Total = total;
                this.ShipPostalCode = shipPostalCode;
                this.ShippedDate = shippedDate;
                this.RequiredDate = requiredDate;
                this.ExtendedPrice = extendedPrice;
            }
        }


        public string LoadDocument(UploadDocument uploadDocument)
        {
            string documentPath = Path.Combine(path, uploadDocument.DocumentName);
            Stream? stream = null;
            if (System.IO.File.Exists(documentPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                stream = new MemoryStream(bytes);
            }
            else
            {
                bool result = Uri.TryCreate(uploadDocument.DocumentName, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result && uploadDocument.DocumentName != null)
                {
                    stream = GetDocumentFromURL(uploadDocument.DocumentName).Result;
                    if (stream != null)
                        stream.Position = 0;
                }
            }
            WordDocument document = WordDocument.Load(stream, FormatType.Docx);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return json;
        }
        async Task<MemoryStream?> GetDocumentFromURL(string url)
        {
            var client = new HttpClient(); ;
            var response = await client.GetAsync(url);
            var rawStream = await response.Content.ReadAsStreamAsync();
            if (response.IsSuccessStatusCode)
            {
                MemoryStream docStream = new MemoryStream();
                rawStream.CopyTo(docStream);
                return docStream;
            }
            else { return null; }
        }

        internal static FormatType GetFormatType(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            switch (format.ToLower())
            {
                case ".dotx":
                case ".docx":
                case ".docm":
                case ".dotm":
                    return FormatType.Docx;
                case ".dot":
                case ".doc":
                    return FormatType.Doc;
                case ".rtf":
                    return FormatType.Rtf;
                case ".txt":
                    return FormatType.Txt;
                case ".xml":
                    return FormatType.WordML;
                case ".html":
                    return FormatType.Html;
                default:
                    throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            }
        }
        internal static WFormatType GetWFormatType(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            switch (format.ToLower())
            {
                case ".dotx":
                    return WFormatType.Dotx;
                case ".docx":
                    return WFormatType.Docx;
                case ".docm":
                    return WFormatType.Docm;
                case ".dotm":
                    return WFormatType.Dotm;
                case ".dot":
                    return WFormatType.Dot;
                case ".doc":
                    return WFormatType.Doc;
                case ".rtf":
                    return WFormatType.Rtf;
                case ".html":
                    return WFormatType.Html;
                case ".txt":
                    return WFormatType.Txt;
                case ".xml":
                    return WFormatType.WordML;
                case ".odt":
                    return WFormatType.Odt;
                default:
                    throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            }
        }


        public void Save(SaveParameter data)
        {
            string name = data.FileName != null ? data.FileName : "Saveddoc.docx";

            string format = RetrieveFileType(name);

            if (string.IsNullOrEmpty(name))
            {
                name = "Document1.doc";
            }
            WDocument document = WordDocument.Save(data.Content);
            FileStream fileStream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            document.Save(fileStream, GetWFormatType(format));
            document.Close();
            fileStream.Close();
        }


        public FileStreamResult ExportSFDT(SaveParameter data)
        {

            string name = data.FileName != null ? data.FileName : "Saveddoc.docx";

            string format = RetrieveFileType(name);
            if (string.IsNullOrEmpty(name))
            {
                name = "Document1.doc";
            }
            WDocument document = WordDocument.Save(data.Content);
            return SaveDocument(document, format, name);
        }

        private string RetrieveFileType(string name)
        {
            int index = name.LastIndexOf('.');
            string format = index > -1 && index < name.Length - 1 ?
                name.Substring(index) : ".doc";
            return format;
        }



        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Export")]
        public FileStreamResult? Export(IFormCollection data)
        {
            if (data.Files.Count == 0)
                return null;
            string fileName = this.GetValue(data, "filename");
            string name = fileName;
            string format = RetrieveFileType(name);
            if (string.IsNullOrEmpty(name))
            {
                name = "Document1";
            }
            WDocument document = this.GetDocument(data);
            return SaveDocument(document, format, fileName);
        }

        private FileStreamResult SaveDocument(WDocument document, string format, string fileName)
        {
            Stream stream = new MemoryStream();
            string contentType = "";
            if (format == ".pdf")
            {
                contentType = "application/pdf";
            }
            else
            {
                WFormatType type = GetWFormatType(format);
                switch (type)
                {
                    case WFormatType.Rtf:
                        contentType = "application/rtf";
                        break;
                    case WFormatType.WordML:
                        contentType = "application/xml";
                        break;
                    case WFormatType.Html:
                        contentType = "application/html";
                        break;
                    case WFormatType.Dotx:
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                        break;
                    case WFormatType.Docx:
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;
                    case WFormatType.Doc:
                        contentType = "application/msword";
                        break;
                    case WFormatType.Dot:
                        contentType = "application/msword";
                        break;
                }
                document.Save(stream, type);
            }
            document.Close();
            stream.Position = 0;
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private string GetValue(IFormCollection data, string key)
        {
            if (data.ContainsKey(key))
            {
                string[] values = data[key];
                if (values.Length > 0)
                {
                    return values[0];
                }
            }
            return "";
        }
        private WDocument GetDocument(IFormCollection data)
        {
            Stream stream = new MemoryStream();
            IFormFile file = data.Files[0];
            file.CopyTo(stream);
            stream.Position = 0;

            WDocument document = new WDocument(stream, WFormatType.Docx);
            stream.Dispose();
            return document;
        }

    }
    public class CustomParameter
    {
        public string? content { get; set; }
        public string type { get; set; }
    }
    public class CustomRestrictParameter
    {
        public string? passwordBase64 { get; set; }
        public string? saltBase64 { get; set; }
        public int spinCount { get; set; }
    }

    public class SaveParameter
    {
        public string? Content { get; set; }
        public string? FileName { get; set; }
    }
    public class SpellCheckJsonData
    {
        public int LanguageID { get; set; }
        public string? TexttoCheck { get; set; }
        public bool CheckSpelling { get; set; }
        public bool CheckSuggestion { get; set; }
        public bool AddWord { get; set; }

    }
    public class ExportData
    {
        public string? fileName { get; set; }
        public string? documentData { get; set; }
    }
    public class UploadDocument
    {
        public string? DocumentName { get; set; }
    }

}