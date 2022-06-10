using System.Text.RegularExpressions;

namespace CarShop.Services.GeneratePhoneNumber
{
    public static class PhoneNumber
    {
        public static string GeneratePhoneNumber(string str)
        {
            var phone = str.Trim().ToLower();


            phone = Regex.Replace(phone, @"[^0-9]", "");
            return phone;
        }
    }
}
