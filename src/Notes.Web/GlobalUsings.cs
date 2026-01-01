// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : NotesApp
// Project Name :  Web
// =======================================================

global using Auth0.AspNetCore.Authentication;

global using Blazored.LocalStorage;

global using MediatR;

global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.OutputCaching;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using Notes.Web.Components;
global using Notes.Web.Data;
global using Notes.Web.Data.Repositories;
global using Notes.Web.Features.Notes.CreateNote;
global using Notes.Web.Features.Notes.DeleteNote;
global using Notes.Web.Features.Notes.GetNoteDetails;
global using Notes.Web.Features.Notes.ListNotes;
global using Notes.Web.Features.Notes.UpdateNote;
global using Notes.Web.Services.Notes;

global using Shared.Abstractions;
global using Shared.Entities;
global using Shared.Interfaces;

global using static Shared.Constants.Constants;