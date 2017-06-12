﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZendeskApi.Client.Models;
using ZendeskApi.Client.Responses;

namespace ZendeskApi.Client.Resources
{
    public class TicketCommentsResource : ITicketCommentsResource
    {
        private const string ResourceUri = "api/v2/tickets/{0}/comments";

        private readonly IZendeskApiClient _apiClient;
        private readonly ILogger _logger;

        private Func<ILogger, string, IDisposable> _loggerScope =
            LoggerMessage.DefineScope<string>("TicketsResource: {0}");

        private ITicketsResource _ticketsResource;

        public TicketCommentsResource(IZendeskApiClient apiClient,
            ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;

            _ticketsResource = new TicketsResource(apiClient, logger);
        }

        public async Task<IEnumerable<TicketComment>> GetAllAsync(long ticketId)
        {
            using (_loggerScope(_logger, $"GetAllAsync({ticketId})"))
            using (var client = _apiClient.CreateClient())
            {
                var response = await client.GetAsync(string.Format(ResourceUri, ticketId)).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadAsAsync<TicketCommentsResponse>()).Item;
            }
        }

        public async Task AddComment(long ticketId, TicketComment ticketComment)
        {
            var ticket = await _ticketsResource.GetAsync(ticketId);

            if (ticket == null)
            {
                throw new Exception($"Ticket {ticketId} not found");
            }

            ticket.Comment = ticketComment;

            await _ticketsResource.PutAsync(ticket);
        }
    }
}
