﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Extensions.DependencyInjection;

namespace Microsoft.Health.Dicom.Core.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonDefault<T>(this IServiceCollection services)
        {
            services.Add<T>()
                  .Singleton()
                  .AsSelf()
                  .AsImplementedInterfaces();
            return services;
        }

    }
}
