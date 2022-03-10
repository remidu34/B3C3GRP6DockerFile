using B3C3GRP6.API.Models;

namespace B3C3GRP6.Helper
{
    public sealed class OtpHelper
    {
        #region FIELDS
        private static OtpHelper? _instance;
        #endregion
        private readonly List<OtpModel> _otpmodel = new List<OtpModel>();
        #region CONSTRUCTOR
        private OtpHelper()
        {
        }
        #endregion

        #region METHODS - PUBLIC
        public static OtpHelper GetInstance()
        {
            if (_instance == null)
            {
                _instance = new OtpHelper();
            }
            return _instance;
        }
        public string GenerateOtp(string email)
        {
            Random rnd = new Random();
            int otp = rnd.Next(1000, 9999);
            OtpModel otpModel = new OtpModel();
            otpModel.Code = otp;
            _otpmodel.Add(new OtpModel() { Email = email, Code = otpModel.Code });
            return Convert.ToString(otpModel.Code);
        }
        public bool VerifyOtp(int? otp, string? email)
        {
            return _otpmodel.Any(x => x.Code == otp && x.Email == email);
        }
        #endregion
    }
}
