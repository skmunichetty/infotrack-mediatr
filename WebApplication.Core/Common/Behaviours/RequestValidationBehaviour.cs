using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WebApplication.Core.Common.Behaviours
{
    public class RequestValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<IPipelineBehavior<TRequest, TResponse>> _logger;
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        /// <inheritdoc />
        /// 

        public RequestValidationBehaviour(ILogger<IPipelineBehavior<TRequest, TResponse>> logger, IEnumerable<IValidator<TRequest>> validators)
        {
            _logger = logger;
            _validators = validators;
        }


        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // TODO: throw a validation exception if there are any validation errors
            // NOTE: the validation exception should contain all failures

            TResponse response;
            var requestName = request.GetType().Name;
            var stopwatch = Stopwatch.StartNew();


            response = await next();

            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var failures = _validators.Select(x => x.Validate(context))
                                       .SelectMany(r => r.Errors)
                                       .Where(y => y != null)
                                       .ToList();

                if (failures.Any())
                    throw new ValidationException(failures);

                //var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                //var failures = validationResults.SelectMany(e => e.Errors).Where(i => i != null).ToList();
                                
                //if (failures.Any())
                //{
                //    throw new ValidationException("List of failures", failures);
                //}
            }


            stopwatch.Stop();
            _logger.LogInformation($" {requestName} request execution time : {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
    }
}
