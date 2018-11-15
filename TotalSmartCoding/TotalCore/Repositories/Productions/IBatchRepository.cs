using System.Collections.Generic;

using TotalBase;
using TotalModel.Models;

namespace TotalCore.Repositories.Productions
{
    public interface IBatchRepository : IGenericRepository<Batch>
    {
        void CommonUpdate(int batchID, string nextPackNo, string nextCartonNo, string nextPalletNo);
        bool ExtendedUpdate(int batchID, string batchPackNo, string batchCartonNo, string batchPalletNo);
    }

    public interface IBatchAPIRepository : IGenericAPIRepository
    {
        List<BatchMaxNo> GetBatchMaxNoByCode(int? fillingLineID, int? commodityID, string code);
        List<BatchMaxNo> GetBatchMaxNoByEntryMonthID(int? fillingLineID, int? commodityID, int? entryMonthID);

        List<BatchAvailable> GetBatchAvailables(int? locationID, int? deliveryAdviceID, int? transferOrderID, int? commodityID, bool withNullRow);
    }
}
