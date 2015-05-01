namespace AST.ContentConveyor.Utilities
{
    using System.Web.Hosting;

    public static class FileHelpers
    {
        /// <summary>
        /// Checks the local file system to see if a file exists
        /// </summary>
        /// <param name="file">Relative file path, e.g. "/media/1079/my_image.jpg"</param>
        /// <returns>True if the file exists, and false if it does not</returns>
        public static bool FileExists(string file)
        {
            var fullPath = HostingEnvironment.MapPath(file);
            return System.IO.File.Exists(fullPath);
        }
    }
}
