namespace BDCOM.OLT.Manager.Models
{
    public class Device
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 16);
        public string Name { get; set; } = "";
        public string Ip { get; set; } = "";
        public int Port { get; set; } = 23;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string EnablePassword { get; set; } = "";
    }
}