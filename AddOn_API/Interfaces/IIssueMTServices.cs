using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.DTOs.Picking;
using AddOn_API.Entities;

namespace AddOn_API.Interfaces;

public interface IIssueMTServices
{
    Task<IssueMaterialH> FindById(long id);

    Task<IEnumerable<ProductionOrderH>> GetProductionH(IssueMaterialH issueMaterialH);

    Task Update(IssueMaterialH issueMaterialH,IssueMaterialLog issueMaterialLogNew);

    Task UpdateStatusLot(IssueMaterialH issueMaterialH);

    Task<(string errorMessage,IssueMaterialH issueMaterialHs)>VerifyDataIssue(IssueMaterialH issueMaterialH);

    Task<IEnumerable<IssueMTGroupD>> GetissueMTListD(long id);

     Task<(string errorMessage,IssueMaterialH issueMaterialHs)>VerifyDeleteIssue(IssueMaterialH issueMaterialH);



    
}
