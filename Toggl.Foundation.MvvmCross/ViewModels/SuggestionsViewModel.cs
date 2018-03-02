﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SuggestionsViewModel : MvxViewModel
    {
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;
        private readonly ISuggestionProviderContainer suggestionProviders;

        private IDisposable emptyDatabaseDisposable;

        private bool areStartButtonsEnabled = true;

        public MvxObservableCollection<Suggestion> Suggestions { get; }
            = new MvxObservableCollection<Suggestion>();

        public bool IsEmpty => !Suggestions.Any();

        public MvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        public SuggestionsViewModel(
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService,
            ISuggestionProviderContainer suggestionProviders,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.suggestionProviders = suggestionProviders;

            StartTimeEntryCommand = new MvxAsyncCommand<Suggestion>(startTimeEntry, _ => areStartButtonsEnabled);
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            emptyDatabaseDisposable = dataSource
                .TimeEntries
                .IsEmpty
                .FirstAsync()
                .Subscribe(fetchSuggestions);
        }

        private void fetchSuggestions(bool databaseIsEmpty)
        {
            Suggestions.Clear();

            if (databaseIsEmpty) return;

            suggestionProviders
                .Providers
                .Select(provider => provider.GetSuggestions())
                .Aggregate(Observable.Merge)
                .Subscribe(addSuggestions);
        }

        private void addSuggestions(Suggestion suggestions)
        {
            Suggestions.Add(suggestions);

            RaisePropertyChanged();
        }

        private async Task startTimeEntry(Suggestion suggestion)
        {
            areStartButtonsEnabled = false;
            StartTimeEntryCommand.RaiseCanExecuteChanged();

            await dataSource.User
                .Current
                .Select(user => new StartTimeEntryDTO
                {
                    UserId = user.Id,
                    TaskId = suggestion.TaskId,
                    ProjectId = suggestion.ProjectId,
                    Description = suggestion.Description,
                    WorkspaceId = suggestion.WorkspaceId,
                    StartTime = timeService.CurrentDateTime
                })
                .SelectMany(dataSource.TimeEntries.Start)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Do(_ =>
                {
                    areStartButtonsEnabled = true;
                    StartTimeEntryCommand.RaiseCanExecuteChanged();
                });

            analyticsService.TrackStartedTimeEntry(TimeEntryStartOrigin.Suggestion);
        }
    }
}
