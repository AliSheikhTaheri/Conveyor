namespace WebUI.DotMailer
{
    public class ApiContact
    {
        public int Id
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public ApiContactOptInTypes OptInType
        {
            get;
            set;
        }

        public ApiContactEmailTypes EmailType
        {
            get;
            set;
        }

        public ApiContactData[] DataFields
        {
            get;
            set;
        }
    }

}
