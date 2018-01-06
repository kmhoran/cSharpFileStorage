using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CSharpFileStorage
{
    public class DocumentService
    {
        private string BASEPATH = AppDomain.CurrentDomain.BaseDirectory;

        public AppDocument AddDocument(HttpPostedFileBase file)
        {
            try
            {
                if (file == null)
                    throw new Exception("No documet Attached.");

                if (file.ContentLength == 0)
                    throw new Exception("Empty Document Detected.");

                Guid guid = Guid.NewGuid();

                string[] splitName = file.FileName.Split('.');

                if (splitName.Length < 2)
                {
                    throw new ArgumentException("Invalid file name.");
                }

                string extention = "." + splitName.Last();
                IEnumerable<string> shortsplit = splitName.Take(splitName.Length - 1);
                string title = String.Join(".", shortsplit);

                string newFileName = guid.ToString() + extention;

                string tempFilePath = BASEPATH + String.Format("Content\\Shared\\Temp\\{0}", newFileName);

                // Temporarily store files locally
                file.SaveAs(tempFilePath);

                byte[] fileBinary = System.IO.File.ReadAllBytes(tempFilePath);

                string contentType = file.ContentType;

                int id = 0;

                using (var db = new MyEntities())
                {
                    var doc = new document_table()
                    {
                        ContentType = contentType,
                        Extention = extention,
                        UploadDate = DateTime.Now,
                        SerialNumber = guid,
                        Title = title,
                        FileName = newFileName,
                        Data = fileBinary
                    };

                    db.document_table.Add(doc);
                    db.SaveChanges();



                    id = doc.DocumentId;
                }

                // clean up local directory
                File.Delete(tempFilePath);

                if (id == 0)
                    throw new Exception("Unable to add file.");

                return new AppDocument(
                    title: title,
                    documentId: id,
                    fileName: newFileName,
                    serialNumber: guid,
                    extention: extention,
                    contentType: contentType,
                    data: fileBinary);
            }
            catch (Exception)
            {
                throw;
            }
        }




        public AppDocument GetDocument(Guid serialNumber)
        {
            using (var db = new MyEntities())
            {
                document_table dbDocument = db.document_table.SingleOrDefault(d => d.SerialNumber == serialNumber);

                if (dbDocument == null)
                    return null;

                string fileName = dbDocument.SerialNumber.ToString() + dbDocument.Extention;

                return new AppDocument(
                    title: dbDocument.Title,
                    documentId: dbDocument.DocumentId,
                    hostId: 0,
                    serialNumber: dbDocument.SerialNumber,
                    extention: dbDocument.Extention,
                    fileName: fileName,
                    contentType: dbDocument.ContentType,
                    data: dbDocument.Data);
            }
        }




        public bool RemoveDocument(Guid serialNumber)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    document_table dbDocument = db.document_table.SingleOrDefault(d => d.SerialNumber == serialNumber);

                    if (dbDocument == null)
                        return false;

                    db.document_table.Remove(dbDocument);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }


    #region DTO
    public class AppDocument
    {
        public AppDocument(
            string title,
            int documentId,
            Guid serialNumber,
            string extention,
            string fileName,
            string contentType,
            byte[] data)
        {
            Title = title;
            DocumentId = documentId;
            SerialNumber = serialNumber;
            Extention = extention;
            FileName = fileName;
            ContentType = contentType;
            Data = data;

        }
        public string Title { get; set; }
        public int DocumentId { get; set; }
        public Guid SerialNumber { get; set; }
        public string Extention { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }

    #endregion
}
