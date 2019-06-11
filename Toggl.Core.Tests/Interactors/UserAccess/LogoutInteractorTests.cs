using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Interactors.UserAccess;
using Toggl.Core.Sync;
using Xunit;

namespace Toggl.Core.Tests.Interactors.UserAccess
{
    public class LogoutInteractorTests : BaseInteractorTests
    {
        private readonly IInteractor<IObservable<Unit>> interactor;

        public LogoutInteractorTests()
        {
            interactor = new LogoutInteractor(
                AnalyticsService,
                NotificationService,
                ApplicationShortcutCreator,
                SyncManager,
                Database,
                UserPreferences,
                PrivateSharedStorageService,
                UserAccessManager,
                InteractorFactory,
                LogoutSource.Settings);
        }

        [Fact, LogIfTooSlow]
        public async Task ClearsTheDatabase()
        {
            await interactor.Execute();

            await Database.Received(1).Clear();
        }

        [Fact, LogIfTooSlow]
        public async Task FreezesTheSyncManager()
        {
            await interactor.Execute();

            SyncManager.Received().Freeze();
        }

        [Fact, LogIfTooSlow]
        public void DoesNotClearTheDatabaseBeforeTheSyncManagerCompletesFreezing()
        {
            var scheduler = new TestScheduler();
            SyncManager.Freeze().Returns(Observable.Never<SyncState>());

            var observable = interactor.Execute().SubscribeOn(scheduler).Publish();
            observable.Connect();
            scheduler.AdvanceBy(TimeSpan.FromDays(1).Ticks);

            Database.DidNotReceive().Clear();
        }

        [Fact, LogIfTooSlow]
        public async Task UnschedulesAllNotifications()
        {
            await interactor.Execute();

            await NotificationService.Received().UnscheduleAllNotifications();
        }

        [Fact, LogIfTooSlow]
        public void ClearTheDatabaseOnlyOnceTheSyncManagerFreezeEmitsAValueEvenThoughItDoesNotComplete()
        {
            var freezingSubject = new Subject<SyncState>();
            SyncManager.Freeze().Returns(freezingSubject.AsObservable());

            var observable = interactor.Execute().Publish();
            observable.Connect();

            Database.DidNotReceive().Clear();

            freezingSubject.OnNext(SyncState.Sleep);

            Database.Received().Clear();
        }

        [Fact, LogIfTooSlow]
        public void EmitsUnitValueAndCompletesWhenFreezeAndDatabaseClearEmitSingleValueButDoesNotComplete()
        {
            var clearingSubject = new Subject<Unit>();
            SyncManager.Freeze().Returns(_ => Observable.Return(SyncState.Sleep));
            Database.Clear().Returns(clearingSubject.AsObservable());
            bool emitsUnitValue = false;
            bool completed = false;

            var observable = interactor.Execute();
            observable.Subscribe(
                _ => emitsUnitValue = true,
                () => completed = true);
            clearingSubject.OnNext(Unit.Default);

            emitsUnitValue.Should().BeTrue();
            completed.Should().BeTrue();
        }

        [Fact, LogIfTooSlow]
        public async Task NotifiesShortcutCreatorAboutLogout()
        {
            await interactor.Execute();

            ApplicationShortcutCreator.Received().OnLogout();
        }

        [Fact, LogIfTooSlow]
        public async Task ResetsUserPreferences()
        {
            await interactor.Execute();

            UserPreferences.Received().Reset();
        }

        [Fact, LogIfTooSlow]
        public async Task TracksLogoutEvent()
        {
            await interactor.Execute();

            AnalyticsService.Logout.Received().Track(Analytics.LogoutSource.Settings);
        }

        [Fact, LogIfTooSlow]
        public async Task ClearsPrivateSharedStorage()
        {
            await interactor.Execute();

            PrivateSharedStorageService.Received().ClearAll();
        }

        [Fact, LogIfTooSlow]
        public async Task ClearsPushNotificationsToken()
        {
            await interactor.Execute();

            PushNotificationsTokenService.Received().InvalidateCurrentToken();
            KeyValueStorage.Received().Remove(PushNotificationTokenKeys.TokenKey);
        }
    }
}
