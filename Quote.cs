using Newtonsoft.Json;

namespace ConsoleApp1.JsonObjects
{
    public class Quote
    {
        [JsonConstructor]
        public Quote()
        {
        }

        public string rs;
        public bool isActive;
        public string T;
        public long t;
        public long f;
        public long y;
        public decimal P;
        public decimal S;
        public decimal p;
        public decimal s;
    }
}
