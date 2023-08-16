using AddOn_API.Data;
using AddOn_API.Entities;
using AddOn_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AddOn_API.Services
{
    public class QueueCVSAPService : IQueueCVSAPServices
    {
        private readonly DatabaseContext databaseContext;
        private readonly ISapSDKService sapSDKService;
        public QueueCVSAPService(DatabaseContext databaseContext, ISapSDKService sapSDKService)
        {
            this.sapSDKService = sapSDKService;
            this.databaseContext = databaseContext;
        }

        public async Task Create(List<QueueConvertToSap> queueConvertToSap)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.QueueConvertToSaps.AddRange(queueConvertToSap);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task Delete(List<QueueConvertToSap> queueConvertToSap)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.QueueConvertToSaps.RemoveRange(queueConvertToSap);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<QueueConvertToSap>> FindAll()
        {
            return await databaseContext.QueueConvertToSaps
                    .Where( w=> w.Status == 1 && w.Complete == 0).OrderBy(d => d.CreateDate).ToListAsync();
            
        }

        public async Task<QueueConvertToSap> FindById(long id)
        {
            return await databaseContext.QueueConvertToSaps
            .Where( w=> w.Status == 1 
                   && w.Complete == 0 &&  (w.Id == id || w.DocEntry == id )).FirstOrDefaultAsync();


        }

        public async Task Update(QueueConvertToSap queueConvertToSap)
        {
            using var transaction = databaseContext.Database.BeginTransaction();
            try{
                databaseContext.QueueConvertToSaps.Update(queueConvertToSap);
                await databaseContext.SaveChangesAsync();

                transaction.Commit();

            }catch(Exception ex){
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<(string errorMessage, List<QueueConvertToSap> queueConvertToSapsRespon)> VerifyDataDeleteQueue(List<QueueConvertToSap> queueConvertToSap)
        {
           string errorMessage = string.Empty;
           List<QueueConvertToSap> queueConvertToSapsRespon = new List<QueueConvertToSap>();

            var _chk = await databaseContext.QueueConvertToSaps.Where( w => queueConvertToSap.Select(s => s.Id).ToList().Contains(w.Id) && w.Complete == 1 ).ToListAsync();

            if (_chk.Count > 0){
                errorMessage = "There are the document has been convert to SAP.";
                queueConvertToSapsRespon = _chk;
            }

           return (errorMessage,queueConvertToSap);

        }
    }
}