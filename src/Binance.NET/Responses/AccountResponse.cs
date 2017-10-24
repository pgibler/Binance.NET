using System.Collections.Generic;
using Binance.NET.Data;

namespace Binance.NET.Responses
{
    public class AccountResponse
    {
        public double MakerCommission { get; set; }
        public double TakerCommission { get; set; }
        public double BuyerCommission { get; set; }
        public double SellerCommission { get; set; }
        public bool CanTrade { get; set; }
        public bool CanWithdraw { get; set; }
        public bool CanDeposit { get; set; }
        public List<Balance> Balances { get; set; }
    }
}
