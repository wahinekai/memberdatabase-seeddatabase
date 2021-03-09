// -----------------------------------------------------------------------
// <copyright file="DatabaseSeeder.cs" company="Wahine Kai">
// Copyright (c) Wahine Kai. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace WahineKai.MemberDatabase.SeedDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using WahineKai.Common;
    using WahineKai.MemberDatabase.Dto;
    using WahineKai.MemberDatabase.Dto.Contracts;
    using WahineKai.MemberDatabase.Dto.Enums;
    using WahineKai.MemberDatabase.Dto.Models;
    using WahineKai.MemberDatabase.Dto.Properties;
    using WahineKai.MemberDatabase.SeedDatabase.Contracts;

    /// <summary>
    /// Project for seeding the development database with data
    /// </summary>
    public sealed class DatabaseSeeder : IDatabaseSeeder
    {
        private static readonly string[] Boards = { "5'10\" custom", "8'2\" funboard" };

        private static readonly string[] SurfSpots = { "Bolsa Chica", "Blackies" };

        private static readonly Dto.Models.Position[] Positions =
        {
            new Dto.Models.Position()
            {
                Name = Dto.Enums.Position.DirectorOfCommunityServices,
                Started = new DateTime(2014, 01, 01),
                Ended = new DateTime(2015, 12, 31),
            },
            new Dto.Models.Position()
            {
                Name = Dto.Enums.Position.ChapterDirector,
                Started = new DateTime(2016, 01, 01),
            },
        };

        private static readonly AdminUser[] UsersArray =
            {
                new AdminUser
                {
                    Admin = false,
                    FirstName = "Test",
                    LastName = "User",
                    Status = MemberStatus.ActivePaying,
                    FacebookName = "Test User",
                    PayPalName = "test-user",
                    Email = "user@user.com",
                    PhoneNumber = "1234567890",
                    StreetAddress = "1234 Test Drive",
                    City = "Orange",
                    Region = "California",
                    Country = Country.UnitedStates,
                    Occupation = "Software Testing",
                    Chapter = Chapter.OrangeCountyLosAngeles,
                    Birthdate = new DateTime(1982, 09, 05),
                    Level = Level.Intermediate,
                    Boards = Boards.ToList(),
                    SurfSpots = SurfSpots.ToList(),
                    PhotoUrl = string.Empty,
                    Biography = "I am a test user",
                    StartedSurfing = new DateTime(1989, 05, 01),
                    JoinedDate = new DateTime(2000, 08, 15),
                    RenewalDate = new DateTime(2021, 01, 22),
                    EnteredInFacebookChapter = EnteredStatus.Entered,
                    EnteredInFacebookWki = EnteredStatus.NotAccepted,
                    NeedsNewMemberBag = true,
                    WonSurfboard = true,
                    DateSurfboardWon = new DateTime(2019, 12, 25),
                    PostalCode = 92804,
                },
                new AdminUser
                {
                    Admin = true,
                    FirstName = "Admin",
                    LastName = "User",
                    Status = MemberStatus.ActiveNonPaying,
                    FacebookName = "Admin User",
                    PayPalName = "admin-a-user",
                    Email = "admin@admin.com",
                    PhoneNumber = "2345678901",
                    StreetAddress = "1234 Admin Circle",
                    City = "Vancouver",
                    Region = "British Columbia",
                    Country = Country.Canada,
                    Occupation = "Hardware Administration",
                    Chapter = Chapter.Washington,
                    Birthdate = new DateTime(1964, 02, 18),
                    Level = Level.Expert,
                    Boards = Boards.ToList(),
                    SurfSpots = SurfSpots.ToList(),
                    PhotoUrl = string.Empty,
                    Biography = "I am an administrator",
                    StartedSurfing = new DateTime(1977, 11, 01),
                    JoinedDate = new DateTime(1994, 01, 22),
                    EnteredInFacebookChapter = EnteredStatus.Entered,
                    EnteredInFacebookWki = EnteredStatus.Entered,
                    Positions = Positions.ToList(),
                    PostalCode = 98607,
                },
            };

        private readonly IUserRepository userRepository;
        private readonly ILogger logger;

        private bool databaseCleared;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="cosmosConfiguration">Configuration to access Cosmos DB</param>
        /// <param name="loggerFactory">Logger factory to create loggers</param>
        /// <param name="databaseCleared">Whether the database is currently clear.  Defaults to false.</param>
        public DatabaseSeeder(CosmosConfiguration? cosmosConfiguration, ILoggerFactory loggerFactory, bool databaseCleared = false)
        {
            this.databaseCleared = databaseCleared;

            // Validate cosmos configuration
            cosmosConfiguration = Ensure.IsNotNull(() => cosmosConfiguration);
            cosmosConfiguration.Validate();

            // Validate logger factory and set logger
            loggerFactory = Ensure.IsNotNull(() => loggerFactory);
            this.logger = loggerFactory.CreateLogger<DatabaseSeeder>();

            // Set user repository
            this.userRepository = new CosmosUserRepository(cosmosConfiguration, loggerFactory);

            this.logger.LogDebug("Database seeder construction complete");
        }

        /// <summary>
        /// Gets public collection of users to be seeded into the database
        /// </summary>
        public static ICollection<AdminUser> Users { get => UsersArray.ToList(); }

        /// <inheritdoc/>
        public async Task SeedAsync()
        {
            // If database is not cleared yet, we can't seed it
            if (!this.databaseCleared)
            {
                throw new InvalidOperationException("Database is not clear");
            }

            this.logger.LogDebug($"Adding {Users.Count} users to the database");

            // Add users to the container
            foreach (var user in Users)
            {
                user.Validate();
                this.logger.LogTrace($"Creating user with id {user.Id} and email {user.Email} in the database");

                await this.userRepository.CreateUserAsync(user);
            }

            this.logger.LogInformation("Database seeding complete");
        }

        /// <inheritdoc/>
        public async Task ClearAsync()
        {
            var users = await this.userRepository.GetAllUsersAsync();

            this.logger.LogDebug($"Removing {users.Count} users from the database");

            foreach (var user in users)
            {
                this.logger.LogTrace($"Removing user with id {user.Id} from the database");
                await this.userRepository.DeleteUserByIdAsync(user.Id);
            }

            // Set database cleared to be true
            this.databaseCleared = true;

            this.logger.LogInformation($"Database clearing complete");
        }
    }
}
