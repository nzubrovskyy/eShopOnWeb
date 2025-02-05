﻿using Ardalis.ApiEndpoints;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints
{
    public class ListPaged : BaseAsyncEndpoint
        .WithRequest<ListPagedCatalogItemRequest>
        .WithResponse<ListPagedCatalogItemResponse>
    {
        private readonly IAsyncRepository<CatalogItem> _itemRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IMapper _mapper;
        private readonly ILogger<string> _logger;

        public ListPaged(IAsyncRepository<CatalogItem> itemRepository,
            IUriComposer uriComposer,
            IMapper mapper,
            ILogger<string> logger)
        {
            _itemRepository = itemRepository;
            _uriComposer = uriComposer;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("api/catalog-items")]
        [SwaggerOperation(
            Summary = "List Catalog Items (paged)",
            Description = "List Catalog Items (paged)",
            OperationId = "catalog-items.ListPaged",
            Tags = new[] { "CatalogItemEndpoints" })
        ]
        public override async Task<ActionResult<ListPagedCatalogItemResponse>> HandleAsync([FromQuery] ListPagedCatalogItemRequest request, CancellationToken cancellationToken)
        {
            var response = new ListPagedCatalogItemResponse(request.CorrelationId());

            var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
            int totalItems = await _itemRepository.CountAsync(filterSpec, cancellationToken);

            var pagedSpec = new CatalogFilterPaginatedSpecification(
                skip: request.PageIndex * request.PageSize,
                take: request.PageSize,
                brandId: request.CatalogBrandId,
                typeId: request.CatalogTypeId);

            var items = await _itemRepository.ListAsync(pagedSpec, cancellationToken);

            //bool.TryParse(GetAppSetting("EnableCpuIntensiveOperation"), out bool enableCpuIntensiveOperation);
            //int.TryParse(GetAppSetting("cc"), out int cpuIntensiveOperationCount);


            if (true)
            {
                for (int j = 0; j < 20; j++)
                {
                    double result = 0;

                    for (var i = Math.Pow(8, 7); i >= 0; i--)
                    {
                        result += Math.Atan(i) * Math.Tan(i);
                    }
                }
            }

            response.CatalogItems.AddRange(items.Select(_mapper.Map<CatalogItemDto>));
            foreach (CatalogItemDto item in response.CatalogItems)
            {
                item.PictureUri = _uriComposer.ComposePicUri(item.PictureUri);
            }
            response.PageCount = int.Parse(Math.Ceiling((decimal)totalItems / request.PageSize).ToString());

            return Ok(response);
        }

        public static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
