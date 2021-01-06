// -----------------------------------------------------------------------
// <copyright file="Entrypoint.cs" company="Wahine Kai">
// Copyright (c) Wahine Kai. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace WahineKai.MemberDatabase.SeedDatabase.Host
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using WahineKai.Common.Contracts;
    using WahineKai.MemberDatabase.Dto.Properties;

    /// <summary>
    /// Entrypoint for SeedDatabaseHost console application
    /// </summary>
    public sealed class Entrypoint : IAsyncEntrypoint
    {
        private readonly CosmosConfiguration cosmosConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entrypoint"/> class.
        /// </summary>
        public Entrypoint()
        {
            var builder = new ConfigurationBuilder();

            // Tell the builder to look for the appsettings.json file
            builder.AddJsonFile("Properties/appsettings.json", optional: false, reloadOnChange: true);

            // Add user secrets
            builder.AddUserSecrets<Entrypoint>();

            // Build configuration
            var configuration = builder.Build();

            // Build and validate Cosmos Configuration
            this.cosmosConfiguration = CosmosConfiguration.BuildFromConfiguration(configuration);
        }

        /// <summary>
        /// Main method entrypoint
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main()
        {
            var program = new Entrypoint();
            await program.StartAsync();
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var seeder = new DatabaseSeeder(this.cosmosConfiguration, loggerFactory);

            await seeder.ClearAsync();
            await seeder.SeedAsync();
        }
    }
}
