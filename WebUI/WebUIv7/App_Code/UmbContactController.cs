using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Overflow.Controllers
{
    public class UmbContactController : UmbracoApiController
    {
        // POST umbraco/api/umbcontact/post
        public HttpResponseMessage Post([FromBody]UmbContactMail message)
        {
            // Return errors if the model validation fails
            // The model defines validations for empty or invalid email addresses
            // See the UmbContactMail class below 
            if (ModelState.IsValid == false)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState.First().Value.Errors.First().ErrorMessage);

            // In order to allow editors to configure the email address where contact 
            // mails will be sent, we require that to be set in a property with the
            // alias umbEmailTo - This property needs to be sent into this API call
            var umbraco = new UmbracoHelper(UmbracoContext);
            var content = umbraco.TypedContent(message.SettingsNodeId);

            if (content == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                          "Please provide a valid node Id on which the umbEmailTo property is defined.");

            var mailTo = content.GetPropertyValue<string>("umbEmailTo");

            if (string.IsNullOrWhiteSpace(mailTo))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                          string.Format("The umbEmailTo property on node {0} (Id {1}) does not exists or has not been filled in.",
                          content.Name, content.Id));

            // If we have a valid email address to send the email to, we can try to 
            // send it. If the is an error, it's most likely caused by a wrong SMTP configuration
            return TrySendMail(message, mailTo)
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                          "Could not send email. Make sure the server settings in the mailSettings section of the Web.config file are configured correctly. For a detailed error, check ~/App_Data/Logs/UmbracoTraceLog.txt.");
        }

        private static bool TrySendMail(UmbContactMail message, string mailTo)
        {
            try
            {
                var content = string.Empty;
                content += string.Format("You have a new contact mail from {0}", string.IsNullOrWhiteSpace(message.Name) ? "[no name given]" : message.Name);
                content += "\r\n";
                content += "They said:";
                content += "\r\n";
                content += string.Format("{0}", string.IsNullOrWhiteSpace(message.Message) ? "[no message entered]" : message.Message);

                var mailFrom = new System.Net.Mail.MailAddress(message.Email, message.Name);

                var mailMsg = new System.Net.Mail.MailMessage
                {
                    From = mailFrom,
                    Subject = "Contact mail",
                    Body = content,
                    IsBodyHtml = false
                };

                mailMsg.To.Add(new System.Net.Mail.MailAddress(mailTo));
                mailMsg.ReplyToList.Add(mailFrom);

                var smtpClient = new System.Net.Mail.SmtpClient();
                smtpClient.Send(mailMsg);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbContactController>("Error sending contact mail", ex);
            }

            return false;
        }

        public class UmbContactMail
        {
            public int SettingsNodeId { get; set; }

            public string Name { get; set; }

            [Required(ErrorMessage = "Please provide a valid e-mail address")]
            [RegularExpression(@"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?",
                ErrorMessage = "Please provide a valid e-mail address")]
            public string Email { get; set; }

            public string Message { get; set; }
        }
    }
}
