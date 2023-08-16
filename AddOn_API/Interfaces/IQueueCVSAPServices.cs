using AddOn_API.Entities;

namespace AddOn_API.Interfaces
{
    public interface IQueueCVSAPServices
    {
        Task<IEnumerable<QueueConvertToSap>> FindAll();

        Task<QueueConvertToSap> FindById(long id);

        Task Create(List<QueueConvertToSap> queueConvertToSap);

         Task Update(QueueConvertToSap QueueConvertToSap);

         Task Delete(List<QueueConvertToSap> queueConvertToSap);

        Task<(string errorMessage,List<QueueConvertToSap> queueConvertToSapsRespon)> VerifyDataDeleteQueue (List<QueueConvertToSap> queueConvertToSap);



    }
}