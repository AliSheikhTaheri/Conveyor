namespace AST.ContentConveyor
{
    using System.Web.Mvc;
    using Ionic.Zip;

    public class ZipFileResult : ActionResult
    {
        public ZipFileResult(ZipFile zip)
        {
            Zip = zip;
            Filename = null;
        }

        public ZipFileResult(ZipFile zip, string filename)
        {
            Zip = zip;
            Filename = filename;
        }

        public ZipFile Zip { get; set; }

        public string Filename { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            response.ContentType = "application/zip";
            response.AddHeader("Content-Disposition", "attachment;" + (string.IsNullOrEmpty(Filename) ? string.Empty : "filename=" + Filename));

            Zip.Save(response.OutputStream);

            response.End();
        }
    }
}