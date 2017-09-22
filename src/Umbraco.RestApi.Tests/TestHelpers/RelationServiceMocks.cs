using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    internal class RelationServiceMocks
    {
        internal static void SetupMocksForPost(ServiceContext serviceContext)
        {
            var relType = ModelMocks.SimpleMockedRelationType();
            var rel = ModelMocks.SimpleMockedRelation(1234, 567, 8910, relType);
            var mockRelationService = Mock.Get(serviceContext.RelationService);
            mockRelationService.Setup(x => x.GetRelationTypeByAlias(It.IsAny<string>())).Returns(() => relType);
            mockRelationService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => rel);
            mockRelationService.Setup(x => x.Relate(It.IsAny<IUmbracoEntity>(), It.IsAny<IUmbracoEntity>(), It.IsAny<IRelationType>()))
                .Returns(() => rel);            
        }
    }
}