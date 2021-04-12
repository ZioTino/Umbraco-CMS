using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Services.Notifications;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Compose
{
    public sealed class NotificationsComposer : ICoreComposer
    {
        public void Compose(IUmbracoBuilder builder) =>
            builder.AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>();
    }
}