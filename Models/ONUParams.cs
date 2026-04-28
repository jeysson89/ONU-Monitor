namespace BDCOM.OLT.Manager.Models
{
    public class ONUParams
    {
        public string Slot { get; }
        public string Port { get; }
        public string OnuId { get; }

        public string FullId => $"EPON {Slot}/{Port}:{OnuId}";

        public ONUParams(string slot, string port, string onuId)
        {
            Slot = slot ?? "0";
            Port = port ?? "";
            OnuId = onuId ?? "";
        }
    }
}