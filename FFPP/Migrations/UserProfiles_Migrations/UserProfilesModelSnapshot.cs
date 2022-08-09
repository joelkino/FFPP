﻿// <auto-generated />
using System;
using FFPP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FFPP.Migrations.UserProfiles_Migrations
{
    [DbContext(typeof(UserProfiles))]
    partial class UserProfilesModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("FFPP.Data.UserProfiles+UserProfile", b =>
                {
                    b.Property<Guid>("userId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("defaultPageSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("defaultUseageLocation")
                        .HasColumnType("TEXT");

                    b.Property<string>("identityProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("lastTenantCustomerId")
                        .HasColumnType("TEXT");

                    b.Property<string>("lastTenantDomainName")
                        .HasColumnType("TEXT");

                    b.Property<string>("lastTenantName")
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .HasColumnType("TEXT");

                    b.Property<string>("photoData")
                        .HasColumnType("TEXT");

                    b.Property<string>("theme")
                        .HasColumnType("TEXT");

                    b.Property<string>("userDetails")
                        .HasColumnType("TEXT");

                    b.HasKey("userId");

                    b.ToTable("_userprofiles");
                });
#pragma warning restore 612, 618
        }
    }
}
