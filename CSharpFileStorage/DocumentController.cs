using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpFileStorage
{
    public class DocumentController : Controller
    {
        // GET: Document
        [HttpGet]
        public void Index(string id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    DocumentService service = new DocumentService();
                    string[] splitId = id.Split('.');

                    Guid serialNumber = Guid.Parse(splitId[0]);

                    AppDocument doc = service.GetDocument(serialNumber);

                    if (doc != null)
                    {

                        byte[] fileBytes = doc.Data;

                        Response.BinaryWrite(fileBytes);

                        Response.ContentType = doc.ContentType;
                        Response.Flush();

                        try { Response.End(); }

                        catch { }
                    }
                    else
                    {
                        throw new ArgumentException("This Document does not exist");
                    }
                }
            }
            catch (Exception)
            {
                Response.Redirect("/Document/NotFound");
            }
        }

    }
}
