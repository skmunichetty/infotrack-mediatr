using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using WebApplication.Core.Common.Exceptions;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Contexts;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Commands
{

    public class UpdateUserCommand : IRequest<UserDto>
    {
        public int Id { get; set; }
        public string GivenNames { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;


        public class Validator : AbstractValidator<UpdateUserCommand>
        {
            public Validator()
            {
                // TODO: Create validation rules for UpdateUserCommand so that all properties are required.
                // If you are feeling ambitious, also create a validation rule that ensures the user exists in the database.
                RuleFor(x => x.GivenNames)
                   .NotEmpty();

                RuleFor(x => x.LastName)
                    .NotEmpty();

                RuleFor(x => x.EmailAddress)
                    .NotEmpty();

                RuleFor(x => x.MobileNumber)
                    .NotEmpty();

                RuleFor(x => x.Id)
                    .Must(id =>
                    {
                        using (var db = new InMemoryContext())
                        {
                            var user = db.Users.Find(id);
                            var result = user!.Id == id;
                            return result;
                        }
                    })
                    .WithErrorCode("UserNotFound")
                    .WithMessage("Cannot find the user");
            }
        }

        public class Handler : IRequestHandler<UpdateUserCommand, UserDto>
        {
            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }

            /// <inheritdoc />
            public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                var userEntity = await _userService.GetAsync(request.Id, cancellationToken);
                if(userEntity == null)
                    throw new NotFoundException($"The user '{request.Id}' could not be found.");

                userEntity.LastName = request.LastName;
                userEntity.GivenNames = request.GivenNames;
                if (userEntity.ContactDetail == null && (!string.IsNullOrEmpty(request.EmailAddress) || !string.IsNullOrEmpty(request.MobileNumber)))
                    userEntity.ContactDetail = new ContactDetail();

                userEntity.ContactDetail.MobileNumber = request.MobileNumber;
                userEntity.ContactDetail.EmailAddress = request.EmailAddress;

                var updatedUser = await _userService.UpdateAsync(userEntity, cancellationToken);
                UserDto result = _mapper.Map<UserDto>(updatedUser);

                return result;
                //throw new NotImplementedException("Implement a way to update the user associated with the provided Id.");
            }
        }
    }
}
