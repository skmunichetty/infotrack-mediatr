using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using WebApplication.Core.Common.Exceptions;
using WebApplication.Core.Common.Models;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Queries
{
    public class ListUsersQuery : IRequest<PaginatedDto<IEnumerable<UserDto>>>
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; } = 10;

        public class Validator : AbstractValidator<ListUsersQuery>
        {
            public Validator()
            {
                RuleFor(x => x.PageNumber)
                  .GreaterThan(0);
                // TODO: Create a validation rule so that PageNumber is always greater than 0
            }
        }

        public class Handler : IRequestHandler<ListUsersQuery, PaginatedDto<IEnumerable<UserDto>>>
        {
            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }

            /// <inheritdoc />
            public async Task<PaginatedDto<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<User> users = await _userService.GetPaginatedAsync(request.PageNumber, request.ItemsPerPage, cancellationToken);

                if (users == null)
                    throw new NotFoundException($"There is no user records on this page.");

                var totalRecords = await _userService.CountAsync();
                int roundedTotalPages = Convert.ToInt32(Math.Ceiling((double) totalRecords / request.ItemsPerPage));
                var hasNextPage = request.PageNumber >= 1 && request.PageNumber < roundedTotalPages;               

                return new PaginatedDto<IEnumerable<UserDto>>() { Data = users.Select(user => _mapper.Map<UserDto>(user)), HasNextPage = hasNextPage };

                //throw new NotImplementedException("Implement a way to get a paginated list of all the users in the database.");
            }
        }
    }
}
